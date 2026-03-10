using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Employees._2_Application.Dtos;
using BankingApi._2_Core.Employees._3_Domain.Enums;
namespace BankingApi._2_Core.Employees._1_Ports.Inbound;
// Contract used by other bounded contexts to access employee authorization data.
// Provides a minimal interface to verify the currently authenticated employee
// and ensure that required administrative rights are present.
public interface IEmployeeContract {

   // Returns the currently authenticated employee
   // and verifies that the employee has the required administrative rights
   Task<Result<EmployeeDto>> GetAuthorizedEmployeeAsync(
      AdminRights requiredRights,
      CancellationToken ct = default
   );

}

/*
Didaktik
--------

Dieses Interface stellt einen Contract zwischen Bounded Contexts dar.

Ein anderer Context (z.B. Owners oder Payments) kann darüber prüfen,
ob der aktuell angemeldete Mitarbeiter bestimmte Berechtigungen besitzt.

Der Zugriff erfolgt bewusst über ein Interface, damit:

- keine direkte Abhängigkeit auf das Employee-Domainmodell entsteht
- nur die benötigten Informationen freigegeben werden
- der Zugriff klar über eine definierte Schnittstelle erfolgt

Der Contract liefert ein DTO und kein Domain-Objekt zurück,
da es sich um eine Kontextübergreifende Kommunikation handelt.


Lernziele
---------

- Verständnis von Context Contracts zwischen Bounded Contexts
- Reduzierung von Kopplung zwischen Modulen
- Einsatz von Ports für modulare Architektur
- Unterschied zwischen Domain-Modell und DTO bei BC-Kommunikation
*/