using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Entities;
using BankingApi._2_Core.BuildingBlocks._4_IntegrationContracts._1_Ports;
using BankingApi._2_Core.BuildingBlocks.Utils;
using BankingApi._2_Core.Customers._1_Ports.Inbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Enums;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
namespace BankingApi._2_Core.Payments._2_Application.UseCases;

public sealed class AccountUcCreate(
   IIdentityGateway identityGateway,
   ICustomerContract customerContract,
   IEmployeeContract employeeContract,
   IAccountRepository accountRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<AccountUcCreate> logger
) {
   
   public async Task<Result<AccountDto>> ExecuteAsync(
      Guid customerId,
      AccountDto accountDto,
      CancellationToken ct = default
   ) {
      
      if (!await customerContract.ExistsActiveAsync(customerId, ct))
         return Result<AccountDto>.Failure(AccountErrors.OwnerIdNotFoundOrInactive);
      
      // 1) subject required
      var resultSubject = SubjectCheck.Run(identityGateway.Subject);
      if (resultSubject.IsFailure) 
         return Result<AccountDto>.Failure(resultSubject.Error);
      var subject = resultSubject.Value;
      
      // 2) load employee id
      var resultEmployee = await employeeContract.GetEmployeeBySubjectAsync(subject, ct);   
      if(resultEmployee == null)
         return Result<AccountDto>.Failure(resultSubject.Error);
      var employeeContractDto = resultEmployee.Value;
      
      // 3) domain model  
      var resultIban = IbanVo.Create(accountDto.Iban);
      if (resultIban.IsFailure)
         return Result<AccountDto>.Failure(AccountErrors.InvalidIban);
      var ibanVo = resultIban.Value;
      
      // create entity
      var result = Account.Create(
         customerId: customerId,
         ibanVo: ibanVo, 
         balance: accountDto.Balance, 
         createdByEmployeeId: employeeContractDto.Id,
         createdAt: clock.UtcNow,
         id: accountDto.Id.ToString()
      );
      if (result.IsFailure)
         return Result<AccountDto>.Failure(result.Error)
            .LogIfFailure(logger, "CustomerUcCreate.DomainRejected",
               new { accountDto });
      var account = result.Value;
      
      // add to repository
      accountRepository.Add(account);            
         
      // unit of work, save changes to database
      var rows = await unitOfWork.SaveAllChangesAsync("Add account", ct);
      
      logger.LogInformation("AccountUcCreate={id} rows={rows}", account.Id, rows);
      
      return Result<AccountDto>.Success(account.ToAccountDto());
   }
}