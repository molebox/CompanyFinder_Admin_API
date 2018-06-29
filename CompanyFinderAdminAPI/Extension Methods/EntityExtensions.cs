using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyFinderAdminAPI.Extension_Methods
{
    /// <summary>
    /// Static class for removing all rows from db table
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        /// Removes all rows from a table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbSet"></param>
        public static void Clear<T>(this DbSet<T> dbSet) where T : class
        {
            dbSet.RemoveRange(dbSet);
        }
    }
}
