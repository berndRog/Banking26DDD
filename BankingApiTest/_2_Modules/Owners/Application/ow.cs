using BankingApi._2_Modules.Owners._2_Application.Dtos;
using BankingApi._4_BuildingBlocks;
namespace BankingApiTest.Modules.Owners.Application.UseCases;

/*
public sealed class OwnerUseCasesUt {

   // private OwnerUcCreateProvisioned _createProvisionedUc = new(MockBehavior.Strict);
   // private OwnerUcUpsertProfile _upsertProfileUc = new(MockBehavior.Strict);
   // private OwnerUcActivate _activateUc = new(MockBehavior.Strict);
   // private OwnerUcReject _rejectUc = new(MockBehavior.Strict);
   // private OwnerUcDeactivate _deactivateUc = new(MockBehavior.Strict);
   // private OwnerUcUpdateEmail _updateEmailUc = new(MockBehavior.Strict);


   public OwnerUseCasesUt() {
      // common setup can be done here
   }


   // -----------------------------------------------------------------------------------------
   // CreateProvisionedAsync
   // -----------------------------------------------------------------------------------------
   [Fact]
   public async Task CreateProvisionedAsync_returns_result() {
      // Arrange
      var sut = CreateSut();

      string? id = "11111111-0000-0000-0000-000000000000";
      var expected = Result<Guid>.Success(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000000"));
      var ct = CancellationToken.None;
      
      // Act
      var result = await sut.CreateProvisionedAsync(id, ct);

      // Assert
      True(result.IsSuccess);
      Equal(expected.Value, result.Value);

      _createProvisionedUc.VerifyAll();
      _createUc.VerifyNoOtherCalls();
      _upsertProfileUc.VerifyNoOtherCalls();
      _activateUc.VerifyNoOtherCalls();
      _rejectUc.VerifyNoOtherCalls();
      _deactivateUc.VerifyNoOtherCalls();
      _updateEmailUc.VerifyNoOtherCalls();
   }

   // -----------------------------------------------------------------------------------------
   // UpsertProfileAsync
   // -----------------------------------------------------------------------------------------
   [Fact]
   public async Task UpsertProfileAsync_delegates_to_upsertProfileUc_and_returns_result() {
      // Arrange
      var sut = CreateSut();

      var dto = new OwnerProfileDto(
         Firstname: "Erika",
         Lastname: "Mustermann",
         CompanyName: null,
         Email: "erika@demo.de",
         Street: "Main St 1",
         PostalCode: "10115",
         City: "Berlin",
         Country: "DE"
      );

      var expected = Result<OwnerProfileDto>.Success(dto);
      var ct = CancellationToken.None;

      _upsertProfileUc
         .Setup(x => x.ExecuteAsync(dto, ct))
         .ReturnsAsync(expected);

      // Act
      var result = await sut.UpsertProfileAsync(dto, ct);

      // Assert
      True(result.IsSuccess);
      Equal(dto, result.Value);

      _upsertProfileUc.VerifyAll();
      _createUc.VerifyNoOtherCalls();
      _createProvisionedUc.VerifyNoOtherCalls();
      _activateUc.VerifyNoOtherCalls();
      _rejectUc.VerifyNoOtherCalls();
      _deactivateUc.VerifyNoOtherCalls();
      _updateEmailUc.VerifyNoOtherCalls();
   }
/*
   // -----------------------------------------------------------------------------------------
   // ActivateAsync
   // -----------------------------------------------------------------------------------------
   [Fact]
   public async Task ActivateAsync_delegates_to_activateUc_and_returns_result() {
      // Arrange
      var sut = CreateSut();

      var ownerId = Guid.Parse("cccccccc-0000-0000-0000-000000000000");
      var expected = Result.Success();
      var ct = CancellationToken.None;

      _activateUc
         .Setup(x => x.ExecuteAsync(ownerId, ct))
         .ReturnsAsync(expected);

      // Act
      var result = await sut.ActivateAsync(ownerId, ct);

      // Assert
      True(result.IsSuccess);

      _activateUc.VerifyAll();
      _createUc.VerifyNoOtherCalls();
      _createProvisionedUc.VerifyNoOtherCalls();
      _upsertProfileUc.VerifyNoOtherCalls();
      _rejectUc.VerifyNoOtherCalls();
      _deactivateUc.VerifyNoOtherCalls();
      _updateEmailUc.VerifyNoOtherCalls();
   }

*/