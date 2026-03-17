using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Xunit;

namespace Domain.Tests
{
    public class DetailTests
    {
        [Fact]
        public void Add_Detail_ShouldSaveEntity()
        {
            using var connection = TestStorageFactory.CreateConnection();

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var repository = new Repository<Detail>(context);
                var detail = new Detail
                {
                    DetailCode = 1,
                    DecimalNumber = "A-01",
                    DetailName = "Корпус",
                    AlloyGrade = "Steel",
                    Mass = 12.5m
                };

                repository.Add(detail);
                repository.Save();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var repository = new Repository<Detail>(context);
                var entity = repository.GetById(1);

                Assert.NotNull(entity);
                Assert.Equal("Корпус", entity.DetailName);
            }
        }

        [Fact]
        public void GetAll_Detail_ShouldReturnAllEntities()
        {
            using var connection = TestStorageFactory.CreateConnection();

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                context.Details.AddRange(
                    new Detail { DetailCode = 1, DecimalNumber = "A-01", DetailName = "Деталь1", AlloyGrade = "Steel", Mass = 10 },
                    new Detail { DetailCode = 2, DecimalNumber = "A-02", DetailName = "Деталь2", AlloyGrade = "Iron", Mass = 20 }
                );
                context.SaveChanges();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var repository = new Repository<Detail>(context);
                var details = repository.GetAll().ToList();

                Assert.Equal(2, details.Count);
            }
        }

        [Fact]
        public void Update_Detail_ShouldChangeEntity()
        {
            using var connection = TestStorageFactory.CreateConnection();

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                context.Details.Add(new Detail
                {
                    DetailCode = 1,
                    DecimalNumber = "A-01",
                    DetailName = "Стара назва",
                    AlloyGrade = "Steel",
                    Mass = 10
                });
                context.SaveChanges();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var repository = new Repository<Detail>(context);
                var detail = repository.GetById(1);

                detail.DetailName = "Нова назва";
                repository.Update(detail);
                repository.Save();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var updated = context.Details.Find(1);

                Assert.Equal("Нова назва", updated.DetailName);
            }
        }

        [Fact]
        public void Delete_Detail_ShouldRemoveEntity()
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
                context.SaveChanges();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var repository = new Repository<Detail>(context);
                var detail = repository.GetById(1);

                repository.Delete(detail);
                repository.Save();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var deleted = context.Details.Find(1);

                Assert.Null(deleted);
            }
        }
    }
}
