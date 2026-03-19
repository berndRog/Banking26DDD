using System.Net.Mail;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
namespace BankingApiTest._2_Core.BuildingBlocks;

public static class EmailCheck {

   public static Result<string> Run(string? input) {
      if (string.IsNullOrWhiteSpace(input))
         return Result<string>.Failure(CommonErrors.InvalidEmail);
      var email = input.Trim().ToLowerInvariant();
      
      if (email.Length > 254)
         return Result<string>.Failure(CommonErrors.InvalidEmail);

      // simple structural sanity
      int at = email.IndexOf('@');
      if (at <= 0 || at >= email.Length - 1) 
         return Result<string>.Failure(CommonErrors.InvalidEmail);;
      if (email.Contains(' ')) 
         return Result<string>.Failure(CommonErrors.InvalidEmail);

      // split into both parts
      var token = email.Split('@');
      token[1] = token[1].Trim();
      // check whether right part has an endig with .xyz
      var rightToken = token[1].Split('.').ToList();
      if(rightToken.Count < 2)
         return Result<string>.Failure(CommonErrors.InvalidEmail);
      
      // Pragmatic syntax validation
      // (robust enough for real-world usage)
      try {
         var emailAddress = new MailAddress(email);
         email = emailAddress.Address;
         return Result<string>.Success(email);
      }
      catch {
         return Result<string>.Failure(CommonErrors.InvalidEmail);
      }
   }
}