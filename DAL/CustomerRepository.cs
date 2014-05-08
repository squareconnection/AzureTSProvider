using Model;
using System;

namespace DAL
{
    public class CustomerRepository : RepositoryBase<Customer>
    {

        public CustomerRepository(AzureContext context)
            : base(context)
        {
            if (context == null)
                throw new ArgumentNullException("Context cannot be null!");
        }

    }
}
