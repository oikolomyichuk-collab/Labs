using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class DB
    {

        Storage storage;
        public IRepository<Detail> repoDetail;
        public IRepository<Production> repoProduction;
        public IRepository<Operation> repoOperation;
        public DB()
        {
            storage = new Storage();
            storage.Database.EnsureCreated();
            repoDetail = new Repository<Detail>(storage);
            repoProduction = new Repository<Production>(storage);
            repoOperation = new Repository<Operation>(storage);
        }
    }
}
