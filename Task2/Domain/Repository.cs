using Microsoft.EntityFrameworkCore;

namespace Domain
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly Storage _context;
        private readonly DbSet<T> _dbSet;

        public Repository(Storage context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();
        }

        public T GetById(params object[] keyValues)
        {
            return _dbSet.Find(keyValues);
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}