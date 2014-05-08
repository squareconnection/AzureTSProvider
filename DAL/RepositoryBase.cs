using AzureTSProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAL
{
    public abstract class RepositoryBase<TEntity> where TEntity : class, new()
    {
        internal AzureContext context;
        internal TableSet<TEntity> dbset;

        public RepositoryBase(AzureContext context)
        {
            this.context = context;
            this.dbset = context.Set<TEntity>();
        }

        public virtual TEntity GetByID(object id)
        {
            return dbset.GetByID(id);
        }

        public virtual List<TEntity> GetAll()
        {
            return dbset.GetAll();
        }

        public virtual void Insert(TEntity entity)
        {
            dbset.Insert(entity);
        }

    }
}
