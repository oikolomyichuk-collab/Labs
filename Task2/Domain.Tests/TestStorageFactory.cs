 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Domain.Tests
{
    public static class TestStorageFactory
    {
        public static SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            return connection;
        }

        public static Storage CreateContext(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<Storage>()
                .UseSqlite(connection)
                .Options;

            var context = new Storage(options);
            context.Database.EnsureCreated();

            return context;
        }
    }
}