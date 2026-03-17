using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Domain;
using Xunit;

namespace Domain.Tests
{
    public class DatabaseCreationTests
    {
        [Fact]
        public void DatabaseFile_ShouldBeCreated_InDataFolder()
        {
            using var context = new Storage();
            context.Database.EnsureCreated();

            var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (currentDirectory != null &&
                   !currentDirectory.GetFiles("*.sln").Any())
            {
                currentDirectory = currentDirectory.Parent;
            }

            var solutionPath = currentDirectory.FullName;
            var dbPath = Path.Combine(solutionPath, "Data", "production.db");

            Assert.True(File.Exists(dbPath));
        }
    }
}