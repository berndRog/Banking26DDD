using BankingApi._4_BuildingBlocks._3_Domain;
namespace BankingApiTest._4_BuildingBlocks._3_Domain;
public class IbanValidationUt {
   // [Fact]
   // public void Create_IBAN() {
   //    //var givenIban = "DE10 1000 0000 0000 0000 42";
   //    var givenIban = "DE60 1000 0000 0000 0000 XX";
   //    // Act
   //    var iban =IbanValidation.GenerateByFixingCheckDigits(givenIban);
   //    var result = IbanValidation.IsValid(iban);
   //    // Assert
   //    True(result.IsSuccess);
   // }
   //
   [Fact]
   public void Create_with_valid_IBAN() {
      var iban = "DE10 1000 0000 0000 0000 42";
      // Act
      var result = IbanValidation.IsValid(iban);
      // Assert
      True(result.IsSuccess);
   }
   
   [Fact]
   public void Create_with_invalid_IBAN() {
      var givenIban = "DE10 1000 0000 0000 0000 00";
      // Act
      var result = IbanValidation.IsValid(givenIban);
      // Assert
      True(result.IsFailure);
   }
   
   
   [Fact]
   public void Create_fails_invalid_IBAN() {
      // Act
      var result = IbanValidation.IsValid(null);
      // Assert
      True(result.IsFailure);
   }
   
}