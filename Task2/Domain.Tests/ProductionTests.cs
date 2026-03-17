using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Xunit;

namespace Domain.Tests
{
    public class ProductionTests
    {
        [Fact]
        public void Add_Production_ShouldSaveEntity()
        {
            using var connection = TestStorageFactory.CreateConnection();

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                context.Details.Add(new Detail
                {
                    DetailCode = 1,
                    DecimalNumber = "A-01",
                    DetailName = "Деталь",
                    AlloyGrade = "Steel",
                    Mass = 10
                });

                context.Operations.Add(new Operation
                {
                    OperationCode = 1,
                    WorkshopNumber = 2,
                    DurationHours = 3,
                    Cost = 100
                });

                context.SaveChanges();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var repository = new Repository<Production>(context);
                var production = new Production
                {
                    DetailCode = 1,
                    OperationNumberInProcess = 1,
                    OperationCode = 1
                };

                repository.Add(production);
                repository.Save();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var repository = new Repository<Production>(context);
                var entity = repository.GetById(1, 1);

                Assert.NotNull(entity);
                Assert.Equal(1, entity.OperationCode);
            }
        }

        [Fact]
        public void GetAll_Production_ShouldReturnAllEntities()
        {
            using var connection = TestStorageFactory.CreateConnection();

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                context.Details.Add(new Detail
                {
                    DetailCode = 1,
                    DecimalNumber = "A-01",
                    DetailName = "Деталь",
                    AlloyGrade = "Steel",
                    Mass = 10
                });

                context.Operations.AddRange(
                    new Operation { OperationCode = 1, WorkshopNumber = 1, DurationHours = 2, Cost = 100 },
                    new Operation { OperationCode = 2, WorkshopNumber = 2, DurationHours = 3, Cost = 200 }
                );

                context.Productions.AddRange(
                    new Production { DetailCode = 1, OperationNumberInProcess = 1, OperationCode = 1 },
                    new Production { DetailCode = 1, OperationNumberInProcess = 2, OperationCode = 2 }
                );

                context.SaveChanges();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var repository = new Repository<Production>(context);
                var productions = repository.GetAll().ToList();

                Assert.Equal(2, productions.Count);
            }
        }

        [Fact]
        public void Update_Production_ShouldChangeEntity()
        {
            using var connection = TestStorageFactory.CreateConnection();

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                context.Details.Add(new Detail
                {
                    DetailCode = 1,
                    DecimalNumber = "A-01",
                    DetailName = "Деталь",
                    AlloyGrade = "Steel",
                    Mass = 10
                });

                context.Operations.AddRange(
                    new Operation { OperationCode = 1, WorkshopNumber = 1, DurationHours = 2, Cost = 100 },
                    new Operation { OperationCode = 2, WorkshopNumber = 2, DurationHours = 3, Cost = 200 }
                );

                context.Productions.Add(new Production
                {
                    DetailCode = 1,
                    OperationNumberInProcess = 1,
                    OperationCode = 1
                });

                context.SaveChanges();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var repository = new Repository<Production>(context);
                var production = repository.GetById(1, 1);

                production.OperationCode = 2;
                repository.Update(production);
                repository.Save();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var updated = context.Productions.Find(1, 1);

                Assert.Equal(2, updated.OperationCode);
            }
        }

        [Fact]
        public void Delete_Production_ShouldRemoveEntity()
        {
            using var connection = TestStorageFactory.CreateConnection();

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                context.Details.Add(new Detail
                {
                    DetailCode = 1,
                    DecimalNumber = "A-01",
                    DetailName = "Деталь",
                    AlloyGrade = "Steel",
                    Mass = 10
                });

                context.Operations.Add(new Operation
                {
                    OperationCode = 1,
                    WorkshopNumber = 1,
                    DurationHours = 2,
                    Cost = 100
                });

                context.Productions.Add(new Production
                {
                    DetailCode = 1,
                    OperationNumberInProcess = 1,
                    OperationCode = 1
                });

                context.SaveChanges();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var repository = new Repository<Production>(context);
                var production = repository.GetById(1, 1);

                repository.Delete(production);
                repository.Save();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var deleted = context.Productions.Find(1, 1);

                Assert.Null(deleted);
            }
        }

        [Fact]
        public void Delete_Detail_ShouldCascadeDelete_Productions()
        {
            using var connection = TestStorageFactory.CreateConnection();

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                context.Details.Add(new Detail
                {
                    DetailCode = 1,
                    DecimalNumber = "A-01",
                    DetailName = "Деталь",
                    AlloyGrade = "Steel",
                    Mass = 10
                });

                context.Operations.Add(new Operation
                {
                    OperationCode = 1,
                    WorkshopNumber = 1,
                    DurationHours = 2,
                    Cost = 100
                });

                context.Productions.Add(new Production
                {
                    DetailCode = 1,
                    OperationNumberInProcess = 1,
                    OperationCode = 1
                });

                context.SaveChanges();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var detail = context.Details.Find(1);
                context.Details.Remove(detail);
                context.SaveChanges();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var production = context.Productions.Find(1, 1);

                Assert.Null(production);
            }
        }

        [Fact]
        public void Delete_Operation_ShouldCascadeDelete_Productions()
        {
            using var connection = TestStorageFactory.CreateConnection();

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                context.Details.Add(new Detail
                {
                    DetailCode = 1,
                    DecimalNumber = "A-01",
                    DetailName = "Деталь",
                    AlloyGrade = "Steel",
                    Mass = 10
                });

                context.Operations.Add(new Operation
                {
                    OperationCode = 1,
                    WorkshopNumber = 1,
                    DurationHours = 2,
                    Cost = 100
                });

                context.Productions.Add(new Production
                {
                    DetailCode = 1,
                    OperationNumberInProcess = 1,
                    OperationCode = 1
                });

                context.SaveChanges();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var operation = context.Operations.Find(1);
                context.Operations.Remove(operation);
                context.SaveChanges();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var production = context.Productions.Find(1, 1);

                Assert.Null(production);
            }
        }
    }
}
