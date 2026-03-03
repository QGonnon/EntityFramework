# ANALYSE.md - Conception du Système de Gestion Hospitalière

## Nouvelles Fonctionnalités Implémentées

### 1. Gestion des Séjours Hospitaliers (HospitalStay)

**Contexte**: Le cahier des charges mentionnait qu'un patient peut avoir plusieurs séjours hospitaliers.

**Implémentation**:
```csharp
public class HospitalStay
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public DateTime AdmissionDate { get; set; }
    public DateTime? DischargeDate { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}
```

**Bénéfices**:
- Historique complet des admissions de chaque patient
- Traçabilité de la durée de séjour par département
- DischargeDate nullable pour les séjours en cours
- Support des requêtes analytiques (taux d'occupation, durée moyenne)

### 2. Hiérarchie du Personnel Médical (Héritage)

**Contexte**: L'étape 6 demandait d'implémenter différents types de personnel avec informations communes.

**Implémentation** (Table-Per-Hierarchy):
```csharp
// Classe de base abstraite
public abstract class MedicalStaff
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
}

// Classes dérivées
public class Nurse : MedicalStaff
{
    public string Service { get; set; } = null!;
    public string Grade { get; set; } = null!;
}

public class Administrator : MedicalStaff
{
    public string Function { get; set; } = null!;
}
```

**Stratégie**: TPH (Table-Per-Hierarchy) - une seule table `MedicalStaff` avec discriminateur
- ✅ Requêtes polymorphiques simples (`_context.MedicalStaff.ToList()`)
- ✅ Performance optimale (pas de JOIN)
- ⚠️ Colonnes nullables pour propriétés spécifiques

### 3. Hiérarchie des Départements (Auto-référencement)

**Contexte**: L'étape 6 mentionnait une organisation en sous-départements (ex: "Cardiologie adulte" sous "Cardiologie").

**Implémentation**:
```csharp
public class Department
{
    // ... autres propriétés
    
    public int? ParentDepartmentId { get; set; }
    public Department? ParentDepartment { get; set; }
    public ICollection<Department> SubDepartments { get; set; } = new List<Department>();
}
```

**Configuration EF Core**:
```csharp
modelBuilder.Entity<Department>()
    .HasOne(d => d.ParentDepartment)
    .WithMany(d => d.SubDepartments)
    .HasForeignKey(d => d.ParentDepartmentId)
    .OnDelete(DeleteBehavior.Restrict);  // Empêche suppression en cascade
```

**Bénéfices**:
- Structure organisationnelle flexible
- Requêtes hiérarchiques possibles (département parent → enfants)
- DeleteBehavior.Restrict évite la suppression accidentelle de sous-départements

### 4. Validation de la Date de Naissance

**Contexte**: L'étape 1 demandait comment valider que la date de naissance est dans le passé.

**Implémentation**:
```csharp
// Validation/PastDateAttribute.cs
public class PastDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime dateValue && dateValue >= DateTime.Now)
        {
            return new ValidationResult("The date must be in the past.");
        }
        return ValidationResult.Success;
    }
}

// Application sur Patient
[Required, PastDate]
public DateTime BirthDate { get; set; }
```

**Avantages**:
- Validation côté serveur avant persistence
- Réutilisable sur d'autres entités si nécessaire
- Messages d'erreur clairs pour API

### 5. Comportements de Suppression Explicites

**Contexte**: L'étape 2 posait la question "si on supprime un département, que deviennent les médecins ?"

**Décisions implémentées**:
```csharp
// Doctor → Department: Restrict
modelBuilder.Entity<Doctor>()
    .HasOne(d => d.Department)
    .WithMany(dept => dept.Doctors)
    .OnDelete(DeleteBehavior.Restrict);

// MedicalStaff → Department: Restrict
modelBuilder.Entity<MedicalStaff>()
    .HasOne(ms => ms.Department)
    .WithMany(dept => dept.MedicalStaff)
    .OnDelete(DeleteBehavior.Restrict);

// Department hierarchy: Restrict
modelBuilder.Entity<Department>()
    .HasOne(d => d.ParentDepartment)
    .WithMany(d => d.SubDepartments)
    .OnDelete(DeleteBehavior.Restrict);
```

**Justification**:
- **Restrict** empêche la suppression accidentelle de départements contenant du personnel
- Force une décision explicite (réaffecter le personnel avant suppression)
- Préserve l'intégrité des données historiques

### 6. Couverture de Tests Élargie

**Nouveaux tests ajoutés**:
- `DepartmentTests.cs`: Hiérarchie, contraintes de suppression, statistiques
- `MedicalStaffTests.cs`: Héritage TPH, queries polymorphiques
- `HospitalStayTests.cs`: Création, relations, multiples séjours
- `PatientTests.cs`: Validation birth date (future vs past)

**Couverture actuelle**:
| Module | Tests | Couverture |
|--------|-------|------------|
| Patient | Concurrency, Validation | ✅ |
| Consultation | Duplicate prevention | ✅ |
| Department | Hierarchy, Delete behavior, Stats | ✅ |
| MedicalStaff | Inheritance, Polymorphism | ✅ |
| HospitalStay | Creation, Relationships | ✅ |

---

## Questions de Synthèse

### 1. Quels sont les avantages et inconvénients de votre modèle ?

#### Avantages

**Modélisation**
- ✅ **Clarté des relations**: Relations Un-à-Plusieurs et Un-à-Un explicitement modélisées
- ✅ **Intégrité des données**: Contraintes uniques et composites au niveau BD
- ✅ **Flexibilité des adresses**: Objet de valeur `Address` réutilisable pour Patient et Département
- ✅ **Énumération des statuts**: Le statut de Consultation en tant qu'énumération prévient les états invalides
- ✅ **Séparation des préoccupations**: Le modèle Repository isole l'accès aux données

**Performance**
- ✅ **Indexation stratégique**: Index sur les colonnes fréquemment recherchées
- ✅ **Index composite**: (PatientId, DoctorId, Date) prévient les doublons à la BD
- ✅ **Chargement avancé**: Include/ThenInclude prévient les requêtes N+1
- ✅ **Pagination intégrée**: Support des grands ensembles de données sans surcharge mémoire

**Concurrence**
- ✅ **Verrouillage optimiste**: RowVersion permet la détection des modifications concurrentes
- ✅ **Pas de deadlocks**: Verrouillage pessimiste évité; conflits détectés avec grâce
- ✅ **Convivial**: Les réponses HTTP 409 informent les clients des conflits

**Testabilité**
- ✅ **Interfaces de Repository**: Faciles à simuler pour les tests unitaires
- ✅ **Modèle Unit of Work**: Gestion centralisée des transactions
- ✅ **Intégration DI**: Le DI ASP.NET Core rend les tests directs
- ✅ **Base de données en mémoire**: Tests isolés sans BD externe

#### Inconvénients

**Modélisation**
- ❌ **Responsable du Département**: Conception auto-référentielle (Médecin référençant Département) pourrait causer une dépendance circulaire si non gérée correctement
- ❌ **Réutilisabilité des adresses**: Les entités possédées sont copiées sur chaque propriétaire; pas d'enregistrements Address partagés
- ❌ **Pas de piste d'audit**: Les dates créées/modifiées ne sont pas suivies automatiquement
- ❌ **Stockage des énumérations**: Le statut de Consultation est stocké en tant qu'entier; les changements de nom nécessitent une migration

**Performance**
- ❌ **Surcharge de chargement avancé**: Les grands chargements avec de nombreux enregistrements peuvent être gourmands en mémoire
- ❌ **Pas de couche de cache**: Chaque requête frappe la base de données (pas de Redis/memcache)
- ❌ **Recherche de texte**: LINQ Contains() effectue un scan complet de la table; pas de recherche en texte intégral
- ❌ **Scalabilité en écriture**: Limitation mono-writer de SQLite pour les mises à jour concurrentes

**Concurrence**
- ❌ **Limité à Patient**: RowVersion uniquement sur Patient; les autres entités pourraient avoir le même problème
- ❌ **Pas de verrous distribués**: Fonctionne uniquement pour une seule instance de base de données
- ❌ **Résolution de conflits côté client**: L'application décide comment gérer les conflits

**Maintenabilité**
- ❌ **Pas de suppressions logiques**: Les enregistrements supprimés sont perdus définitivement
- ❌ **Pas d'authentification**: N'importe quel client peut modifier n'importe quel enregistrement
- ❌ **Validation limitée**: Annotations de données basiques uniquement; règles complexes dans les contrôleurs
- ❌ **DTOs manuels**: Pas de mappage automatique entité-vers-DTO (travail futur)

---

### 2. Quelles optimisations feriez-vous si l'hôpital avait 100 000 patients ?

#### Optimisations Recommandées

**1. Infrastructure Technique**
```
Alternative d'accès aux données:
- Remplacer SQLite par PostgreSQL/SQL Server pour support multi-writer
- Ajouter Redis pour caching des requêtes fréquentes
- Implémenter CQRS: Commandes (writes) vs Requêtes (reads)
- Partitionnement de tables: Consultations par année pour les historiques anciennes
```

**2. Performance des Requêtes**
```sql
-- Index supplémentaires nécessaires:
CREATE INDEX IX_Patient_BirthDate ON Patients(BirthDate);  -- Pour requêtes basées sur l'âge
CREATE INDEX IX_Consultation_PatientId ON Consultations(PatientId, Date DESC);  -- Consultations récentes
CREATE INDEX IX_Doctor_DepartmentId ON Doctors(DepartmentId);  -- Recherche de département
CREATE INDEX IX_Consultation_Status ON Consultations(Status) WHERE Status = 'Planned';  -- Rendez-vous planifiés

-- Partitionnement:
-- Table Consultations partitionnée par année de Date
-- Archivage des anciens enregistrements dans une table séparée
```

**3. Optimisation des Requêtes LINQ**
```csharp
// Avant (Problème N+1 avec 100k patients)
var patients = await _context.Patients.ToListAsync();
foreach(var p in patients) {
    var count = p.Consultations.Count;  // Une requête par patient!
}

// Après (Une seule requête groupée)
var consultationCounts = await _context.Consultations
    .GroupBy(c => c.PatientId)
    .Select(g => new { PatientId = g.Key, Count = g.Count() })
    .ToDictionaryAsync(x => x.PatientId, x => x.Count);

// Pagination obligatoire
var page1 = await _context.Patients
    .OrderBy(p => p.LastName)
    .Skip(0)
    .Take(50)
    .ToListAsync();
```

**4. Stratégie de Caching**
```csharp
// Cache Redis pour requêtes lourdes en lecture
public async Task<Patient> GetPatientAsync(int id) {
    var cacheKey = $"patient_{id}";
    var cached = await _cache.GetAsync<Patient>(cacheKey);
    if (cached != null) return cached;
    
    var patient = await _context.Patients.FindAsync(id);
    await _cache.SetAsync(cacheKey, patient, TimeSpan.FromHours(1));
    return patient;
}

// Invalidation du cache à la mise à jour
public async Task UpdatePatientAsync(Patient patient) {
    _context.Patients.Update(patient);
    await _context.SaveChangesAsync();
    await _cache.RemoveAsync($"patient_{patient.Id}");
}
```

**5. Rapports/Analytique séparés**
```csharp
// Base de données en lecture seule (réplica en lecture)
// Les statistiques de département sont interrogées sur réplica, pas sur BD principale
var statsDb = new ApplicationDbContext(_replicaConnection);
var stats = await statsDb.DepartmentStatistics
    .FromSqlInterpolated($"SELECT ... FROM analytics_view")
    .ToListAsync();
```

**6. Async/Await + Traitement par lots**
```csharp
// Insertion par lots de 10 000 patients
var batch = patients.Skip(i * 1000).Take(1000);
await _context.Patients.AddRangeAsync(batch);
await _context.SaveChangesAsync();

// Opérations en masse (Extensions EF Core)
var toUpdate = await _context.Patients
    .Where(p => p.BirthDate.Year < 2000)
    .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsUrgentPriority, true));
```

**7. Suppressions Logiques + Partitionnement**
```csharp
// Modèle de suppression logique avec drapeau IsDeleted
public class Patient {
    public bool IsDeleted { get; set; }
}

// Filtre de requête dans DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.Entity<Patient>()
        .HasQueryFilter(p => !p.IsDeleted);
}

// Archivage des anciennes données
var oldPatients = await _context.Patients
    .Where(p => p.CreatedDate < DateTime.Now.AddYears(-5))
    .ToListAsync();
// Déplacer vers table d'archive
```

**8. Limitation de débit API + Pagination**
```csharp
// Appliquer pagination stricte
[HttpGet]
public async Task<IActionResult> GetPatients(
    [Range(1, 100)] int pageSize = 20,  // Max 100 par page
    [Range(1, int.MaxValue)] int page = 1)
{
    var skip = (page - 1) * pageSize;
    var total = await _context.Patients.CountAsync();
    var patients = await _context.Patients
        .Skip(skip)
        .Take(pageSize)
        .ToListAsync();
    
    return Ok(new { patients, total, page, pageSize });
}
```

**Résumé de l'impact:**
| Défi | Solution | Gain |
|-----------|----------|------|
| Concurrence multi-writer | PostgreSQL + Verrouillage optimiste | Support million+ événements |
| Requête lente (scan) | Index + partitionnement | 100x recherches plus rapides |
| Requêtes N+1 | Requêtes par lots + caching | 10000x moins de requêtes |
| Mémoire serveur | Pagination stricte | Utilisation mémoire constante |
| Statistiques | CQRS + réplica en lecture | Zéro impact production |
| Données anciennes | Archivage + partitionnement | Table principale 90% plus petite |

---

### 3. Comment implémenteriez-vous un système de rendez-vous en ligne ?

#### Architecture Proposée

```
Flux du Système de Rendez-vous:
Patient → Application Web → API → Logique Métier → Base de données
         ↓                    ↓
       Notifications (email/SMS)
       Traitement de paiement
       Service de rappel
```

**1. Entités Additionnelles**
```csharp
public class Timeslot {
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; }
    
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    public bool IsAvailable { get; set; }
    public int MaxPatients { get; set; }  // Plusieurs patients par créneau
    
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

public class Appointment : Consultation {
    // Hérite PatientId, DoctorId, Date de Consultation
    
    public int TimeslotId { get; set; }
    public Timeslot Timeslot { get; set; }
    
    public AppointmentStatus Status { get; set; }  // Booked, Cancelled, Completed
    public DateTime BookedAt { get; set; }
    
    public string? Notes { get; set; }
    public AppointmentSource Source { get; set; }  // Online, Phone, Walk-in
    
    public decimal? Fee { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
}

public enum AppointmentStatus {
    Pending,      // En attente de confirmation
    Booked,       // Confirmé
    Cancelled,    // Annulé par patient/médecin
    NoShow,       // Patient absent
    Completed     // Terminé
}

public enum PaymentStatus {
    None,
    PendingPayment,
    Paid,
    Refunded
}
```

**2. Repository Spécialisé**
```csharp
public interface IAppointmentRepository : IRepository<Appointment> {
    // Trouver les créneaux disponibles pour un médecin
    Task<IEnumerable<Timeslot>> GetAvailableTimeslotsAsync(
        int doctorId, 
        DateTime from, 
        DateTime to);
    
    // Obtenir les rendez-vous à venir d'un patient
    Task<IEnumerable<Appointment>> GetPatientUpcomingAsync(
        int patientId,
        DateTime from);
    
    // Obtenir les rendez-vous réservés d'un médecin pour un jour
    Task<IEnumerable<Appointment>> GetDoctorDailyAppointmentsAsync(
        int doctorId,
        DateTime date);
    
    // Vérifier la disponibilité d'un créneau
    Task<bool> IsTimeslotAvailableAsync(int timeslotId);
}

public class AppointmentRepository : IAppointmentRepository {
    public async Task<IEnumerable<Timeslot>> GetAvailableTimeslotsAsync(
        int doctorId, DateTime from, DateTime to) {
        return await _context.Timeslots
            .Where(t => t.DoctorId == doctorId 
                && t.StartTime >= from 
                && t.EndTime <= to
                && t.IsAvailable
                && t.Appointments.Count < t.MaxPatients)
            .OrderBy(t => t.StartTime)
            .ToListAsync();
    }
    
    public async Task<bool> IsTimeslotAvailableAsync(int timeslotId) {
        var timeslot = await _context.Timeslots.FindAsync(timeslotId);
        return timeslot?.IsAvailable == true 
            && timeslot.Appointments.Count < timeslot.MaxPatients;
    }
}
```

**3. Service Métier**
```csharp
public class AppointmentService {
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<AppointmentService> _logger;
    
    public async Task<Result<Appointment>> BookAppointmentAsync(
        int patientId,
        int timeslotId,
        Payment payment) {
        
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try {
            // 1. Vérifier la disponibilité du créneau
            var isAvailable = await _unitOfWork.Appointments
                .IsTimeslotAvailableAsync(timeslotId);
            if (!isAvailable) {
                return Result.Failure("Le créneau n'est plus disponible");
            }
            
            // 2. Créer le rendez-vous
            var appointment = new Appointment {
                PatientId = patientId,
                TimeslotId = timeslotId,
                Status = AppointmentStatus.Pending,
                BookedAt = DateTime.UtcNow,
                Source = AppointmentSource.Online
            };
            await _unitOfWork.Appointments.AddAsync(appointment);
            
            // 3. Traiter le paiement
            var paymentResult = await _paymentService.ProcessAsync(payment);
            if (!paymentResult.IsSuccess) {
                return Result.Failure("Paiement échoué: " + paymentResult.Error);
            }
            appointment.PaymentStatus = PaymentStatus.Paid;
            appointment.Fee = paymentResult.Amount;
            
            // 4. Confirmer le rendez-vous
            appointment.Status = AppointmentStatus.Booked;
            await _unitOfWork.CompleteAsync();
            
            // 5. Envoyer email de confirmation
            var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
            await _emailService.SendConfirmationAsync(patient, appointment);
            
            // 6. Planifier rappel (24 heures avant)
            _ = _reminderService.ScheduleReminderAsync(appointment);
            
            await transaction.CommitAsync();
            return Result.Success(appointment);
        }
        catch (Exception ex) {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Échec de la réservation");
            return Result.Failure("La réservation a échoué");
        }
    }
    
    public async Task<Result> CancelAppointmentAsync(
        int appointmentId,
        string reason) {
        
        var appointment = await _unitOfWork.Appointments
            .GetByIdAsync(appointmentId);
        
        if (appointment.Status != AppointmentStatus.Booked) {
            return Result.Failure("Impossible d'annuler un rendez-vous non réservé");
        }
        
        appointment.Status = AppointmentStatus.Cancelled;
        await _unitOfWork.CompleteAsync();
        
        // Traiter le remboursement
        if (appointment.PaymentStatus == PaymentStatus.Paid) {
            await _paymentService.RefundAsync(appointment.Id);
        }
        
        // Notifier le patient
        await _emailService.SendCancellationAsync(appointment);
        
        return Result.Success();
    }
}
```

**4. Points d'Extrémité API**
```csharp
[ApiController]
[Route("api/[controller]")]
public class AppointmentsController {
    private readonly AppointmentService _service;
    
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableSlots(
        int doctorId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to) {
        
        var slots = await _service.GetAvailableSlotsAsync(
            doctorId, from, to);
        return Ok(slots);
    }
    
    [HttpPost("book")]
    [Authorize]  // Le patient doit être authentifié
    public async Task<IActionResult> BookAppointment(
        [FromBody] BookAppointmentRequest request) {
        
        var result = await _service.BookAppointmentAsync(
            getUserId(),  // À partir des réclamations
            request.TimeslotId,
            request.Payment);
        
        if (!result.IsSuccess) {
            return BadRequest(result.Error);
        }
        
        return CreatedAtAction(nameof(GetAppointment),
            new { id = result.Value.Id },
            result.Value);
    }
    
    [HttpPost("{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelAppointment(
        int id,
        [FromBody] CancelRequest request) {
        
        var result = await _service.CancelAppointmentAsync(
            id, request.Reason);
        
        return Ok(result);
    }
}
```

**5. API Frontend (Application Web)**
```javascript
// JavaScript (Vue/React/Angular)
async function bookAppointment(doctorId, date) {
  const availableSlots = await fetch(
    `/api/appointments/available?doctorId=${doctorId}&from=${date}`
  ).then(r => r.json());
  
  displaySlots(availableSlots);
  
  const selected = await getUserSelection();
  
  const response = await fetch('/api/appointments/book', {
    method: 'POST',
    body: JSON.stringify({
      timeslotId: selected.id,
      payment: { method: 'card', token: stripeToken }
    })
  });
  
  if (response.ok) {
    showConfirmation('Rendez-vous réservé!');
  }
}
```

**6. Tâches d'Arrière-Plan (Rappels)**
```csharp
// Service hébergé pour rappels de rendez-vous
public class AppointmentReminderService : BackgroundService {
    private readonly ISendGridClient _emailClient;
    
    protected override async Task ExecuteAsync(CancellationToken ct) {
        while (!ct.IsCancellationRequested) {
            // Trouver les rendez-vous de demain
            var tomorrow = DateTime.UtcNow.AddDays(1);
            var appointments = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Booked
                    && a.TimeslotId.StartTime.Date == tomorrow.Date)
                .include(a => a.Patient)
                .ToListAsync();
            
            foreach (var apt in appointments) {
                await _emailClient.SendEmailAsync(new SendGridMessage {
                    Subject = "Rappel: Votre rendez-vous demain",
                    HtmlContent = $"N'oubliez pas votre rendez-vous avec Dr. {apt.Doctor.FirstName}"
                });
            }
            
            // S'exécuter toutes les 6 heures
            await Task.Delay(TimeSpan.FromHours(6), ct);
        }
    }
}
```

**Impact sur le Modèle Existant:**
- ✅ Hérite de `Consultation` pour compatibilité
- ✅ Ajoute `Timeslot` pour gestion des créneaux
- ✅ Séparation: Consultation (historique) vs Appointment (futur)
- ✅ Transactions pour cohérence du paiement

---

### 4. Quel impact sur le modèle si on ajoutait la facturation ?

#### Changements Recommandés

**1. Nouvelles Entités**
```csharp
public class Invoice {
    public int Id { get; set; }
    public string InvoiceNumber { get; set; }  // Comme INV-2026-001
    
    public int PatientId { get; set; }
    public Patient Patient { get; set; }
    
    public DateTime IssuedDate { get; set; }
    public DateTime DueDate { get; set; }
    
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public InvoiceStatus Status { get; set; }  // Draft, Issued, Paid, Overdue, Cancelled
    
    public string? Description { get; set; }  // Services médicaux fournis
    public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
    
    public int? PrimaryInsuranceId { get; set; }
    public Insurance? PrimaryInsurance { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class InvoiceLineItem {
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    
    public int? ConsultationId { get; set; }  // Lien vers consultation
    public Consultation? Consultation { get; set; }
    
    public string Description { get; set; }  // p.ex., "Consultation générale", "Tests de labo"
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total => Quantity * UnitPrice;
    
    public string? MedicalCode { get; set; }  // Code ICD-10 pour services médicaux
}

public class Payment {
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; }
    
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod Method { get; set; }  // Carte, Banque, Espèces, Assurance
    
    public string TransactionId { get; set; }
    public PaymentStatus Status { get; set; }  // Pending, Success, Failed, Refunded
    
    public string? Notes { get; set; }
}

public class Insurance {
    public int Id { get; set; }
    public string Name { get; set; }
    public string PolicyNumber { get; set; }
    
    public int PatientId { get; set; }
    public Patient Patient { get; set; }
    
    public InsuranceType Type { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    
    public decimal CoveragePercentage { get; set; }
    public decimal AnnualMaximum { get; set; }
}

public enum InvoiceStatus {
    Draft,      // Pas encore envoyée
    Issued,     // Envoyée au patient
    PartiallyPaid,
    Paid,       // Entièrement payée
    Overdue,    // En retard
    Cancelled
}

public enum PaymentMethod {
    CreditCard,
    BankTransfer,
    Cash,
    Insurance,
    Check
}
```

**2. Modification de Patient**
```csharp
public class Patient {
    // ... propriétés existantes ...
    
    // NOUVEAU: Informations de facturation
    public DateTime? LastInvoiceDate { get; set; }
    public decimal TotalOutstanding { get; set; }  // En cache pour performance
    
    // L'adresse pourrait être divisée en HomeAddress + BillingAddress
    public Address? BillingAddress { get; set; }
    
    // Référence d'assurance
    public ICollection<Insurance> Insurances { get; set; } = new List<Insurance>();
    public int? PrimaryInsuranceId { get; set; }
    
    // Factures
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
```

**3. Modification de Consultation**
```csharp
public class Consultation {
    // ... propriétés existantes ...
    
    // NOUVEAU: Association facturation
    public int? InvoiceLineItemId { get; set; }  // FK vers InvoiceLineItem
    public InvoiceLineItem? InvoiceLineItem { get; set; }
    
    // Codage médical
    public string? DiagnosisCode { get; set; }  // ICD-10
    public string? ProcedureCode { get; set; }  // Code CPT
    
    // Informations de coût
    public decimal BaseRate { get; set; }  // Tarif du médecin
    public decimal AppliedDiscount { get; set; }
}
```

**4. Repositories Nécessaires**
```csharp
public interface IInvoiceRepository : IRepository<Invoice> {
    Task<Invoice?> GetByNumberAsync(string invoiceNumber);
    Task<IEnumerable<Invoice>> GetByPatientAsync(int patientId);
    Task<IEnumerable<Invoice>> GetOverdueAsync();
    Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status);
    Task<decimal> GetPatientTotalOutstandingAsync(int patientId);
}

public interface IPaymentRepository : IRepository<Payment> {
    Task<IEnumerable<Payment>> GetByInvoiceAsync(int invoiceId);
    Task<decimal> GetTotalPaidAsync(int invoiceId);
}

public interface IInsuranceRepository : IRepository<Insurance> {
    Task<Insurance?> GetActiveAsync(int patientId);
    Task<IEnumerable<Insurance>> GetPatientInsurancesAsync(int patientId);
}
```

**5. Service pour Générer les Factures**
```csharp
public class InvoiceService {
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    
    public async Task<Invoice> GenerateFromConsultationsAsync(
        int patientId,
        DateTime from,
        DateTime to) {
        
        // Obtenir les consultations non facturées
        var consultations = await _unitOfWork.Consultations
            .Where(c => c.PatientId == patientId
                && c.Date >= from
                && c.Date <= to
                && c.InvoiceLineItemId == null)  // Pas encore facturées
            .ToListAsync();
        
        if (!consultations.Any()) {
            return null;  // Rien à facturer
        }
        
        // Créer la facture
        var invoice = new Invoice {
            InvoiceNumber = GenerateInvoiceNumber(),
            PatientId = patientId,
            IssuedDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = InvoiceStatus.Draft
        };
        
        // Ajouter les éléments de ligne
        decimal total = 0;
        foreach (var consultation in consultations) {
            var price = consultation.Consultation.BaseRate 
                - consultation.Consultation.AppliedDiscount;
            
            var lineItem = new InvoiceLineItem {
                ConsultationId = consultation.Id,
                Description = $"Consultation avec Dr. {consultation.Doctor.LastName}",
                Quantity = 1,
                UnitPrice = price,
                MedicalCode = consultation.ProcedureCode
            };
            
            invoice.LineItems.Add(lineItem);
            total += lineItem.Total;
        }
        
        invoice.Amount = total;
        
        // Appliquer la couverture d'assurance
        var insurance = await _unitOfWork.Insurances
            .GetActiveAsync(patientId);
        if (insurance != null) {
            invoice.PrimaryInsuranceId = insurance.Id;
            // L'assurance couvre une partie
            var coveredAmount = total * (insurance.CoveragePercentage / 100m);
            invoice.PaidAmount = coveredAmount;  // Prérempli
        }
        
        await _unitOfWork.Invoices.AddAsync(invoice);
        await _unitOfWork.CompleteAsync();
        
        // Envoyer au patient
        var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
        await _emailService.SendInvoiceAsync(patient, invoice);
        
        return invoice;
    }
    
    public async Task<Payment> ProcessPaymentAsync(
        int invoiceId,
        decimal amount,
        PaymentMethod method) {
        
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
        if (invoice == null) {
            throw new InvalidOperationException("Facture non trouvée");
        }
        
        var payment = new Payment {
            InvoiceId = invoiceId,
            Amount = amount,
            Method = method,
            PaymentDate = DateTime.UtcNow,
            Status = PaymentStatus.Pending
        };
        
        try {
            // Traiter avec la passerelle de paiement
            var result = await _paymentGateway.ChargeAsync(amount, method);
            payment.TransactionId = result.TransactionId;
            payment.Status = PaymentStatus.Success;
            
            // Mettre à jour la facture
            invoice.PaidAmount += amount;
            
            if (invoice.PaidAmount >= invoice.Amount) {
                invoice.Status = InvoiceStatus.Paid;
            } else {
                invoice.Status = InvoiceStatus.PartiallyPaid;
            }
            
            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.CompleteAsync();
        }
        catch (PaymentException ex) {
            payment.Status = PaymentStatus.Failed;
            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.CompleteAsync();
            throw;
        }
        
        return payment;
    }
    
    private string GenerateInvoiceNumber() {
        var year = DateTime.UtcNow.Year;
        var lastInvoice = _unitOfWork.Invoices
            .GetAll()
            .Where(i => i.InvoiceNumber.StartsWith($"INV-{year}"))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefault();
        
        var number = lastInvoice != null
            ? int.Parse(lastInvoice.InvoiceNumber.Split('-')[2]) + 1
            : 1;
        
        return $"INV-{year}-{number:D6}";
    }
}
```

**6. Points d'Extrémité API pour Facturation**
```csharp
[ApiController]
[Route("api/[controller]")]
public class InvoicesController {
    private readonly InvoiceService _service;
    
    [HttpPost("generate")]
    [Authorize(Roles = "Admin,Billing")]
    public async Task<IActionResult> GenerateInvoices(
        int patientId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to) {
        
        var invoice = await _service.GenerateFromConsultationsAsync(
            patientId, from, to);
        
        return CreatedAtAction(nameof(GetInvoice),
            new { id = invoice.Id }, invoice);
    }
    
    [HttpGet("{id}/outstanding")]
    public async Task<IActionResult> GetOutstanding(int id) {
        var outstanding = await _service
            .GetTotalOutstandingAsync(id);
        return Ok(new { outstanding });
    }
    
    [HttpPost("{id}/pay")]
    public async Task<IActionResult> RecordPayment(
        int id,
        [FromBody] PaymentRequest request) {
        
        var payment = await _service.ProcessPaymentAsync(
            id, request.Amount, request.Method);
        
        return Ok(payment);
    }
}
```

**7. Impact sur les Migrations**
```
Migration 4: Ajout Infrastructure de Facturation
- Créer table Invoice
- Créer table InvoiceLineItem
- Créer table Payment
- Créer table Insurance
- Ajouter BillingAddress à Patient
- Ajouter FK de Consultation à InvoiceLineItem

Migration 5: Ajout des Champs de Facturation Médicale
- Ajouter DiagnosisCode, ProcedureCode à Consultation
- Ajouter BaseRate, AppliedDiscount à Consultation
- Créer index sur Invoice(PatientId, Status)
- Créer index sur Payment(InvoiceId, Status)

Migration 6: Ajout Intégration Assurance
- Ajouter PrimaryInsuranceId à Patient
- Ajouter CoveragePercentage, AnnualMaximum à Insurance
- Créer index sur Insurance(PatientId, ValidFrom, ValidTo)
```

**8. Implications Architecturales**
| Aspect | Impact | Solution |
|--------|--------|----------|
| **Concurrence** | Plusieurs paiements sur même facture | Verrous distribués ou idempotence de passerelle |
| **Conformité** | PCI DSS pour cartes de crédit | Ne jamais stocker données de carte; utiliser passerelle |
| **Audit** | Historique des modifications de facture | Piste d'audit sur tables Invoice, Payment |
| **Rapport** | Rapports fiscaux, revenus, soldes | Base de données rapports séparée (CQRS) |
| **Scalabilité** | Millions de factures | Partitionner par année; archiver anciennes |
| **Performance** | Requêtes de solde impayé | TotalOutstanding en cache sur Patient |

---

## Résumé Exécutif

Ce modèle de gestion hospitalière fournit une base solide avec:
- ✅ Architecture propre séparant données et métier
- ✅ Performance optimisée pour milliers de patients
- ✅ Concurrence gérée via versioning optimiste
- ✅ Testabilité avec repositories et DI
- ✅ **Gestion complète des séjours hospitaliers** (HospitalStay)
- ✅ **Hiérarchie du personnel médical** (MedicalStaff avec héritage TPH)
- ✅ **Organisation hiérarchique des départements** (sous-départements)
- ✅ **Validation métier robuste** (dates de naissance, contraintes)
- ✅ **Comportements de suppression explicites** (protection des données)

**Couverture des 8 étapes du TP**:
1. ✅ Modélisation initiale (Patient, Department)
2. ✅ Intégration médecins + responsable département
3. ✅ Gestion consultations avec contraintes
4. ✅ Opérations CRUD + pagination
5. ✅ Chargement données liées (Include/ThenInclude)
6. ✅ Modélisation avancée (Address owned type, héritage MedicalStaff, hiérarchie)
7. ✅ Performance (indexes) + concurrence (RowVersion)
8. ✅ Architecture testable (Repository, UnitOfWork, tests unitaires)

Pour **100k patients**: PostgreSQL, caching Redis, et partitionnement requis
Pour **rendez-vous en ligne**: Ajouter Timeslot, Appointment, et traitement de paiement
Pour **facturation**: Ajouter Invoice, Payment, Insurance avec gestion de conformité

---

**Document créé**: 3 mars 2026  
**Dernière mise à jour**: 3 mars 2026 (ajout fonctionnalités Step 6+)  
**Niveau de couverture**: Complet - 8/8 étapes implémentées  
