using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._4_Infrastructure.Repositories;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace BankingApiTest._2_Modules.Owners.Infrastructure;

public sealed class OwnersRepositoryIntT : TestBase, IAsyncLifetime {
   private TestSeed _seed = null!;
   private SqliteConnection _dbConnection = null!;
   private BankingDbContext _dbContext = null!;
   private IOwnerRepository _repository = null!;
   private IUnitOfWork _unitOfWork = null!;

   public async Task InitializeAsync() {
      _seed = new TestSeed();

      _dbConnection = new SqliteConnection("Filename=:memory:");
      await _dbConnection.OpenAsync();

      var options = new DbContextOptionsBuilder<BankingDbContext>()
         .UseSqlite(_dbConnection)
         .EnableSensitiveDataLogging()
         .Options;

      _dbContext = new BankingDbContext(options);
      await _dbContext.Database.EnsureCreatedAsync();

      _repository = new OwnerRepositoryEf(_dbContext);
      _unitOfWork = new UnitOfWork(_dbContext, CreateLogger<UnitOfWork>());
   }

   public async Task DisposeAsync() {
      if (_dbContext != null) {
         await _dbContext.DisposeAsync();
         _dbContext = null!;
      }

      if (_dbConnection != null) {
         await _dbConnection.CloseAsync();
         await _dbConnection.DisposeAsync();
         _dbConnection = null!;
      }
   }

   
   [Fact]
   public async Task Add_returns_owner1() {
      // Arrange
      var owner = _seed.Owner1;
      
      // Act
      _repository.Add(owner);
      await _unitOfWork.SaveAllChangesAsync();
      _dbContext.ChangeTracker.Clear();
      
      // Assert
      var actual = await _repository.FindByIdAsync(owner.Id, CancellationToken.None);
      Assert.NotNull(actual);
      Assert.Equal(_seed.Owner1.Id, actual!.Id);
      Assert.Equal(_seed.Owner1.Firstname, actual.Firstname);
      Assert.Equal(_seed.Owner1.Lastname, actual.Lastname);
      Assert.Equal(_seed.Owner1.Email, actual.Email); 
      Assert.Equal(_seed.Owner1.Subject, actual.Subject);
   }
   
   
   [Fact]
   public async Task FindByIdAsync_returns_owner1() {
      // Arrange
      _dbContext.Owners.AddRange(_seed.Owners);
      await _unitOfWork.SaveAllChangesAsync();
      _dbContext.ChangeTracker.Clear();
      
      var id = _seed.Owner1.Id;
      
      // Act
      var actual = await _repository.FindByIdAsync(id, CancellationToken.None);

      // Assert
      Assert.NotNull(actual);
      Assert.Equal(id, actual!.Id);
      Assert.Equal(_seed.Owner1.Firstname, actual.Firstname);
      Assert.Equal(_seed.Owner1.Lastname, actual.Lastname);
      Assert.Equal(_seed.Owner1.Email, actual.Email); 
      Assert.Equal(_seed.Owner1.Subject, actual.Subject);
   }

   [Fact]
   public async Task FindByIdAsync_returns_null_when_not_found() {
      // Arrange
      _dbContext.Owners.AddRange(_seed.Owners);
      await _unitOfWork.SaveAllChangesAsync();
      _dbContext.ChangeTracker.Clear();

      var nonExistentId = Guid.NewGuid();

      // Act
      var actual = await _repository.FindByIdAsync(nonExistentId, CancellationToken.None);

      // Assert
      Assert.Null(actual);
   }
  


/*
   [Fact]
   public async Task SelectAsync_returns_all_cars_when_no_filters() {
      // Arrange
      _dbContext.Owners.AddRange(_seed.Owners);
      await _unitOfWork.SaveAllChangesAsync();
      _dbContext.ChangeTracker.Clear();

      // Act
      var cars = await _repository.(
         category: null,
         status: null,
         ct: CancellationToken.None
      );

      // Assert
      Assert.Equal(20, cars.Count); // All cars from seed
   }
/*
   [Fact]
   public async Task SelectAsync_filters_by_category() {
      // Arrange
      _dbContext.Cars.AddRange(_seed.Cars);
      await _unitOfWork.SaveAllChangesAsync();

      // Act
      var cars = await _repository.SelectByAsync(
         category: CarCategory.Economy,
         status: null,
         ct: CancellationToken.None
      );

      // Assert
      Assert.Equal(5, cars.Count); // Car1-Car5
      Assert.All(cars, car => Assert.Equal(CarCategory.Economy, car.Category));
   }

   [Fact]
   public async Task SelectAsync_filters_by_status() {
      // Arrange
      _dbContext.Cars.AddRange(_seed.Cars);
      await _unitOfWork.SaveAllChangesAsync();
      _dbContext.ChangeTracker.Clear();

      // Load cars and change their status
      var car1 = await _repository.FindByIdAsync(Guid.Parse(_seed.Car1Id), CancellationToken.None);
      var car2 = await _repository.FindByIdAsync(Guid.Parse(_seed.Car2Id), CancellationToken.None);

      var result1 = car1!.SendToMaintenance();
      var result2 = car2!.SendToMaintenance();

      Assert.True(result1.IsSuccess);
      Assert.True(result2.IsSuccess);

      await _unitOfWork.SaveAllChangesAsync();
      _dbContext.ChangeTracker.Clear();

      // Act
      var cars = await _repository.SelectByAsync(
         category: null,
         status: CarStatus.Maintenance,
         ct: CancellationToken.None
      );

      // Assert
      Assert.Equal(2, cars.Count);
      Assert.All(cars, car => Assert.Equal(CarStatus.Maintenance, car.Status));
   }

   [Fact]
   public async Task SelectAsync_filters_by_category_and_status() {
      // Arrange
      _dbContext.Cars.AddRange(_seed.Cars);
      await _unitOfWork.SaveAllChangesAsync();
      _dbContext.ChangeTracker.Clear();

      var car1 = await _repository.FindByIdAsync(Guid.Parse(_seed.Car1Id), CancellationToken.None);
      var result = car1!.SendToMaintenance();
      Assert.True(result.IsSuccess);

      await _unitOfWork.SaveAllChangesAsync();
      _dbContext.ChangeTracker.Clear();

      // Act
      var cars = await _repository.SelectByAsync(
         category: CarCategory.Economy,
         status: CarStatus.Maintenance,
         ct: CancellationToken.None
      );

      // Assert
      Assert.Single(cars);
      Assert.Equal(Guid.Parse(_seed.Car1Id), cars[0].Id);
   }
   #endregion

   #region Add
   [Fact]
   public async Task Add_persists_car() { 
      // Assert
      var id = _seed.Car1.Id;
      var manufacturer = _seed.Car1.Manufacturer;
      var model = _seed.Car1.Model;
      var licensePlate = _seed.Car1.LicensePlate;
      var category = _seed.Car1.Category;
      var createdAt = _seed.Car1.CreatedAt;
      var status = _seed.Car1.Status;
      
      // Arrange
      var carResult = Car.Create(manufacturer, model, licensePlate, category, 
         createdAt, id.ToString());
      
      // Assert
      Assert.True(carResult.IsSuccess);
      var car = carResult.Value;

      // Act
      _repository.Add(car);
      await _unitOfWork.SaveAllChangesAsync();
      _dbContext.ChangeTracker.Clear();

      // Assert
      var actual = await _repository.FindByIdAsync(car.Id, CancellationToken.None);
      Assert.NotNull(actual);
      Assert.Equal(id, actual.Id);
      Assert.Equal(manufacturer, actual.Manufacturer);
      Assert.Equal(model, actual.Model);
      Assert.Equal(licensePlate, actual.LicensePlate);
      Assert.Equal(category, actual.Category);
      Assert.Equal(createdAt, actual.CreatedAt);
      Assert.Equal(status, actual.Status);
   }

   [Fact]
   public async Task Add_multiple_cars_persists_all() {
      // Arrange
      var car1 = _seed.Car1;
      var car2 = _seed.Car2;
      var car3 = _seed.Car3;

      // Act
      _repository.Add(car1);
      _repository.Add(car2);
      _repository.Add(car3);
      await _unitOfWork.SaveAllChangesAsync();
      _dbContext.ChangeTracker.Clear();

      // Assert
      var saved1 = await _repository.FindByIdAsync(car1.Id, CancellationToken.None);
      var saved2 = await _repository.FindByIdAsync(car2.Id, CancellationToken.None);
      var saved3 = await _repository.FindByIdAsync(car3.Id, CancellationToken.None);

      Assert.NotNull(saved1);
      Assert.NotNull(saved2);
      Assert.NotNull(saved3);
   }
*/
}