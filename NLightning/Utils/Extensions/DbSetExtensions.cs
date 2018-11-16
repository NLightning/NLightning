using Microsoft.EntityFrameworkCore;

namespace NLightning.Utils.Extensions
{
    public static class DbSetExtensions
    {
        public static void InsertOrUpdate<T>(this DbSet<T> dbSet, T entity, DbContext db) where T : class
        {
            if (db.Entry(entity).State == EntityState.Detached)
            {
                dbSet.Add(entity);
            }
        }
    }
}