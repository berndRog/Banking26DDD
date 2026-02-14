using BankingApi._2_Modules.Employees._3_Domain.Enums;
using BankingApi._2_Modules.Employees._3_Domain.Errors;
using BankingApi._2_Modules.Owners._3_Domain.Errors;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._3_Domain;
using BankingApi._4_BuildingBlocks._3_Domain.Entities;
using BankingApi._4_BuildingBlocks._4_Infrastructure;
namespace BankingApi._2_Modules.Employees._3_Domain.Aggregates;

/// <summary>
/// Employee aggregate root.
///
/// Represents an employee of the organization and defines
/// all domain rules related to employee lifecycle and administration.
///
/// Responsibilities:
/// - Holds identity and personal data (via Person base class)
/// - Manages administrative rights
/// - Controls activation and deactivation lifecycle
///
/// Invariants:
/// - Personnel number must be present
/// - Creation timestamp must be defined
/// - Admin rights must only contain allowed flag values
///
/// Notes:
/// - This aggregate contains no persistence or application logic
/// - All state changes are enforced via domain methods
/// </summary>
public sealed class Employee : AggregateRoot<Guid> {
   
   public string  Firstname { get; private set; } = string.Empty;
   public string  Lastname  { get; private set; } = string.Empty;
   public string  Email     { get; private set; } = default!;
   public string? Phone { get; private set; } = null;
  
   public string  Subject { get; private set; } = default!; // IdentityAccessServer
   
   public string PersonnelNumber { get; private set; } = string.Empty;
   public AdminRights AdminRights { get; private set; } = AdminRights.ViewReports;
   public bool IsAdmin => AdminRights != AdminRights.None;
   public bool IsActive { get; private set; }
   public DateTimeOffset? DeactivatedAt { get; private set; }

   private const AdminRights AllowedRights =
      AdminRights.ViewReports |
      AdminRights.ViewOwners | AdminRights.ManageOwners |
      AdminRights.ViewAccounts | AdminRights.ManageAccounts |
      AdminRights.ViewTransfers | AdminRights.ManageTransfers |
      AdminRights.ViewEmployees | AdminRights.ManageEmployees; 
   
   // EF Core constructor
   private Employee(): base(new BankingSystemClock()) { }

   // Domain constructor
   private Employee(
      IClock clock,
      Guid id,
      string firstname,
      string lastname,
      string email,
      string? phone,
      string subject,
      string personnelNumber,
      AdminRights adminRights,
      bool isActive
   ): base(clock) {
      Id = id;
      Firstname = firstname;
      Lastname  = lastname;
      Email     = email;
      Phone = phone;
      Subject = subject;
      PersonnelNumber = personnelNumber;
      AdminRights = adminRights;
      IsActive = isActive;
   }

   // ---------- Factory (Result-based) ----------
   /// </summary>
   public static Result<Employee> Create(
      IClock clock,
      string firstname,
      string lastname,
      string email,
      string? phone,
      string subject,
      string personnelNumber,
      AdminRights adminRights,
      bool isActive = true,
      string? id = null
   ) {
      // Normalize input early
      firstname = firstname.Trim();
      lastname = lastname.Trim();
      email= email.Trim();
      phone = phone?.Trim();
      personnelNumber = personnelNumber.Trim();

      // required firstname
      if (string.IsNullOrWhiteSpace(firstname))
         return Result<Employee>.Failure(EmployeeErrors.FirstnameIsRequired);
      if (firstname.Length is < 2 or > 100)
         return Result<Employee>.Failure(EmployeeErrors.InvalidFirstname);
      
      // required lastname
      if (string.IsNullOrWhiteSpace(lastname))
         return Result<Employee>.Failure(EmployeeErrors.LastnameIsRequired);
      if (lastname.Length is < 2 or > 100)
         return Result<Employee>.Failure(EmployeeErrors.InvalidFirstname);

      // required email
      if (string.IsNullOrWhiteSpace(email))
         return Result<Employee>.Failure(EmployeeErrors.EmailIsRequired);
      var resultEmail = EmailAddress.Check(email);
      if(resultEmail.IsFailure) 
         return Result<Employee>.Failure(EmployeeErrors.InvalidEmail);

      // optional phone
      if (!string.IsNullOrWhiteSpace(phone)) {
         var resultPhone = PhoneNumber.Check(phone);
         if (resultPhone.IsFailure)
            return Result<Employee>.Failure(resultPhone.Error);
         phone = resultPhone.Value!;
      }

      var subjectResult = IdentitySubject.Check(subject);
      if (subjectResult.IsFailure)
         return Result<Employee>.Failure(subjectResult.Error);
      
      // required personnel number
      if (string.IsNullOrWhiteSpace(personnelNumber))
         return Result<Employee>.Failure(EmployeeErrors.PersonnelNumberIsRequired);

      var result = EntityId.Resolve(id, EmployeeErrors.InvalidId);
      if (result.IsFailure)
         return Result<Employee>.Failure(result.Error);

      var employee = new Employee(
         clock: clock,
         id: result.Value, 
         firstname: firstname,
         lastname: lastname,
         email: email,
         phone: phone,
         subject: subject,
         personnelNumber: personnelNumber,
         adminRights: adminRights,
         isActive: isActive
      );
      return Result<Employee>.Success(employee);
   }

   // ---------- Domain operations ----------
   /// <summary>
   /// Create an employee on first login (provisioning).
   /// - Only identity facts are known for sure (subject, email, createdAt).
   /// - Business profile data is still missing and must be completed by the employee.
   /// </summary>
   public static Result<Employee> CreateProvisioned(
      IClock clock,
      string identitySubject,
      string email,
      DateTimeOffset createdAt,
      AdminRights adminRights = AdminRights.ViewReports,
      string? id = null
   ) {
      
      var subjectResult = IdentitySubject.Check(identitySubject);
      if (subjectResult.IsFailure)
         return Result<Employee>.Failure(subjectResult.Error);

      var emailResult = EmailAddress.Check(email);
      if (emailResult.IsFailure)
         return Result<Employee>.Failure(emailResult.Error);
      
      var idResult = EntityId.Resolve(id, EmployeeErrors.InvalidId);
      if (idResult.IsFailure)
         return Result<Employee>.Failure(idResult.Error);

      // Provisioned owner starts with empty profile fields
      var employee = new Employee(
         clock: clock,
         id: idResult.Value,
         firstname: string.Empty,
         lastname: string.Empty,
         email: emailResult.Value,
         phone: null,
         subject: subjectResult.Value,
         personnelNumber: string.Empty, 
         adminRights: adminRights,
         isActive: true
      );
      
      // Provisioning should reflect identity creation time (not "now")
      employee.SetCreatedAt(createdAt);

      return Result<Employee>.Success(employee);
   }

   // --------------------------------------------------------------------------
   // Domain methods (mutations)
   // - Important: we accept 'now' from outside to keep tests deterministic and
   //   to avoid reliance on the internal clock after EF materialization.
   // --------------------------------------------------------------------------
   /// <summary>
   /// Employee completes or updates their profile after provisioning.
   /// </summary>
   public Result UpdateProfile(
      string firstname,
      string lastname,
      string email,
      string? phone,
      string personnelNumber,
      DateTimeOffset updatedAt
   ) {

      firstname = firstname.Trim();
      lastname  = lastname.Trim();
      email = email.Trim();
      phone = phone?.Trim();
      personnelNumber = personnelNumber.Trim();

      // Validate required profile fields
      if (string.IsNullOrWhiteSpace(firstname))
         return Result.Failure(EmployeeErrors.FirstnameIsRequired);
            
      if (firstname.Length is < 2 or > 80)
         return Result.Failure(EmployeeErrors.InvalidFirstname);

      if (string.IsNullOrWhiteSpace(lastname))
         return Result.Failure(EmployeeErrors.LastnameIsRequired);
      if (lastname.Length is < 2 or > 80)
         return Result.Failure(EmployeeErrors.InvalidLastname);
      
      // Validate email in domain (do not rely on caller)
      if (string.IsNullOrWhiteSpace(email))
         return Result.Failure(EmployeeErrors.EmailIsRequired);

      var emailResult = EmailAddress.Check(email);
      if (emailResult.IsFailure)
         return Result.Failure(emailResult.Error);

      // optional phone
      if (!string.IsNullOrWhiteSpace(phone)) {
         var resultPhone = PhoneNumber.Check(phone);
         if (resultPhone.IsFailure)
            return Result.Failure(resultPhone.Error);
         phone = resultPhone.Value!;
      }
      
      if (string.IsNullOrWhiteSpace(personnelNumber))
         return Result.Failure(EmployeeErrors.PersonnelNumberIsRequired);

      // Apply changes
      Firstname = firstname;
      Lastname  = lastname;
      Email = emailResult.Value;
      Phone = phone;
      PersonnelNumber = personnelNumber;
      
      Touch(updatedAt);
      return Result.Success();
   }

   
   /// <summary>
   /// Replaces the administrative rights of the employee.
   ///
   /// Semantics:
   /// - The provided rights replace the previous rights completely
   /// - Partial add/remove operations are intentionally not supported
   ///
   /// Returns:
   /// - Success if the rights are valid and applied
   /// - Failure if the bitmask contains unsupported flags
   /// </summary>
   public Result SetAdminRights(
      AdminRights adminRights, 
      DateTimeOffset updatedAt
   ) {
      if ((adminRights & ~AllowedRights) != 0)
         return Result.Failure(EmployeeErrors.InvalidAdminRightsBitmask);

      AdminRights = adminRights;
      
      Touch(updatedAt);
      return Result.Success();
   }

   /// <summary>
   /// Deactivates the employee.
   ///
   /// Business rules:
   /// - An employee can only be deactivated once
   ///
   /// Side effects:
   /// - Sets IsActive to false
   /// - Records the deactivation timestamp
   /// </summary>
   public Result Deactivate(
      DateTimeOffset deactivatedAt
   ) {
      if (!IsActive)
         return Result.Failure(EmployeeErrors.AlreadyDeactivated);

      IsActive = false;
      DeactivatedAt = deactivatedAt;
      
      Touch(deactivatedAt);
      return Result.Success();
   }
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Erläuterung
 * =====================================================================
 *
 * Was ist das Employee-Aggregat?
 * ------------------------------
 * Employee ist das Aggregate Root des Employees-Bounded-Contexts.
 *
 * Es modelliert:
 * - Identität und Personendaten (über die Basisklasse Person)
 * - administrative Berechtigungen (AdminRights)
 * - den fachlichen Lebenszyklus eines Mitarbeiters
 *
 *
 * Warum eine Result-basierte Factory?
 * -----------------------------------
 * Die statische Create-Methode stellt sicher, dass:
 * - alle fachlichen Invarianten beim Erzeugen geprüft werden
 * - kein ungültiges Employee-Objekt entstehen kann
 * - Fehler eindeutig als DomainErrors zurückgegeben werden
 *
 *
 * Wie werden AdminRights behandelt?
 * ---------------------------------
 * AdminRights werden IMMER als vollständiger Satz gesetzt.
 *
 * Das bedeutet:
 * - Der neue Wert ersetzt den bisherigen komplett
 * - Es gibt kein inkrementelles Hinzufügen oder Entfernen
 *
 * Vorteil:
 * - deterministischer, sicherer Rechtezustand
 * - einfache Autorisierungslogik
 * - keine schleichenden Berechtigungsreste
 *
 *
 * Aktiv / Inaktiv:
 * ----------------
 * Ein Employee ist entweder aktiv oder deaktiviert.
 * Die Deaktivierung ist:
 * - ein fachlicher Zustand
 * - irreversibel ohne expliziten Reaktivierungs-UseCase
 *
 *
 * Abgrenzung:
 * -----------
 * - Persistenz (EF Core): Infrastructure Layer
 * - Orchestrierung (Create, Deactivate, SetRights):
 *   Application UseCases
 * - Lesen / Suchen / Listen:
 *   EmployeeReadModel
 *
 *
 * Merksatz:
 * ---------
 * Aggregate schützen ihre Invarianten selbst.
 * UseCases orchestrieren – Aggregate entscheiden.
 *
 * =====================================================================
 */
