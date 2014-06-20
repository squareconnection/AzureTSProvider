using System;
using System.Collections.Generic;

namespace AzureTSProvider
{
    public abstract class TSContext
    {
        private string tableName { get; set; }
        private string connectionString { get; set; }
        private List<ITableSet> internalContext { get; set; }

        public TSContext(string connectionString, string tableName)
        {
            this.tableName = tableName;
            this.connectionString = connectionString;
            internalContext = new List<ITableSet>();
        }

        public virtual TableSet<TEntity> Set<TEntity>()
            where TEntity : class, new()
        {
            var set = new TableSet<TEntity>(connectionString, tableName);
            internalContext.Add(set);

            return set;
        }

        public int SaveChanges()
        {
            int count=0;
            foreach (var set in internalContext)
            {
                count += set.Commit();
            }

            return count;
        }
    }
}
