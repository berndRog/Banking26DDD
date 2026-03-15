using System.Text.Json.Serialization;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;

// Address is a value object without identity.
// It is immutable and fully replaced on change.
public sealed record AddressVo {
   // Properties
   public string Street     { get; }
   public string PostalCode { get; }
   public string City       { get; }
   public string? Country   { get; }

   [JsonConstructor]
   public AddressVo(
      string street,
      string postalCode,
      string city,
      string? country = null
   ) {
      var normalized = Normalize(street, postalCode, city, country, out var error);
      if (error is not null)
         throw new ArgumentException(error.Message, nameof(street));

      Street = normalized.Street;
      PostalCode = normalized.PostalCode;
      City = normalized.City;
      Country = normalized.Country;
   }

   //--- Static factory method -------------------------------------------------
   public static Result<AddressVo> Create(
      string street,
      string postalCode,
      string city,
      string? country = null
   ) {
      var normalized = Normalize(street, postalCode, city, country, out var error);
      if (error is not null)
         return Result<AddressVo>.Failure(error);

      return Result<AddressVo>.Success(new AddressVo(
         street: normalized.Street,
         postalCode: normalized.PostalCode,
         city: normalized.City,
         country: normalized.Country
      ));
   }

   private static (string Street, string PostalCode, string City, string? Country) Normalize(
      string street,
      string postalCode,
      string city,
      string? country,
      out DomainErrors? error
   ) {
      street = street?.Trim() ?? string.Empty;
      postalCode = postalCode?.Trim() ?? string.Empty;
      city = city?.Trim() ?? string.Empty;
      country = country?.Trim();

      if (string.IsNullOrWhiteSpace(street)) {
         error = CommonErrors.StreetIsRequired;
         return default;
      }
      if (street.Length is < 2 or > 80) {
         error = CommonErrors.InvalidStreet;
         return default;
      }

      if (string.IsNullOrWhiteSpace(postalCode)) {
         error = CommonErrors.PostalCodeIsRequired;
         return default;
      }
      if (postalCode.Length is < 2 or > 10) {
         error = CommonErrors.InvalidPostalCode;
         return default;
      }

      if (string.IsNullOrWhiteSpace(city)) {
         error = CommonErrors.CityIsRequired;
         return default;
      }
      if (city.Length is < 2 or > 80) {
         error = CommonErrors.InvalidCity;
         return default;
      }

      if (country?.Length is < 2 or > 80) {
         error = CommonErrors.InvalidCountry;
         return default;
      }

      error = null;
      return (street, postalCode, city, country);
   }
}
