using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Xunit;

namespace Domain.Tests
{
    public class OperationTests
    {
        [Fact]
        public void Add_Operation_ShouldSaveEntity()
        {
            using var connection = TestStorageFactory.CreateConnection();

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var repository = new Repository<Operation>(context);
                var operation = new Operation
                {
                    OperationCode = 1,
                    WorkshopNumber = 5,
                    DurationHours = 2,
                    Cost = 150.50m
                };

                repository.Add(operation);
                repository.Save();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var repository = new Repository<Operation>(context);
                var entity = repository.GetById(1);

                Assert.NotNull(entity);
                Assert.Equal(5, entity.WorkshopNumber);
            }
        }

        [Fact]
        public void GetAll_Operation_ShouldReturnAllEntities()
        {
            using var connection = TestStorageFactory.CreateConnection();

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                context.Operations.AddRange(
                    new Operation { OperationCode = 1, WorkshopNumber = 1, DurationHours = 2, Cost = 100 },
                    new Operation { OperationCode = 2, WorkshopNumber = 2, DurationHours = 3, Cost = 200 }
                );
                context.SaveChanges();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var repository = new Repository<Operation>(context);
                var operations = repository.GetAll().ToList();

                Assert.Equal(2, operations.Count);
            }
        }

        [Fact]
        public void Update_Operation_ShouldChangeEntity()
        {
            using var connection = TestStorageFactory.CreateConnection();

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                context.Operations.Add(new Operation
                {
                    OperationCode = 1,
                    WorkshopNumber = 5,
                    DurationHours = 2,
                    Cost = 100
                });
                context.SaveChanges();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var repository = new Repository<Operation>(context);
                var operation = repository.GetById(1);

                operation.Cost = 300;
                repository.Update(operation);
                repository.Save();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var updated = context.Operations.Find(1);

                Assert.Equal(300, updated.Cost);
            }
        }

        [Fact]
        public void Delete_Operation_ShouldRemoveEntity()
        {
            using var connection = TestStorageFactory.CreateConnection();

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                context.Operations.Add(new Operation
                {
                    OperationCode = 1,
                    WorkshopNumber = 5,
                    DurationHours = 2,
                    Cost = 100
                });
                context.SaveChanges();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var repository = new Repository<Operation>(context);
                var operation = repository.GetById(1);

                repository.Delete(operation);
                repository.Save();
            }

            using (var context = TestStorageFactory.CreateContext(connection))
            {
                var deleted = context.Operations.Find(1);

                Assert.Null(deleted);
            }
        }
    }
}