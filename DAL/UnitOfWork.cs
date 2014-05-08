using System;

namespace DAL
{
    public class UnitOfWork: IDisposable
    {
        AzureContext context;

        private CustomerRepository customerRepository;


        public UnitOfWork(string tenantId)
        {
            string connectionString = "UseDevelopmentStorage=true;";//DevelopmentStorageProxyUri=http://myProxyUri

            this.context = new AzureContext(connectionString, tenantId);
        }

        public CustomerRepository CustomerRepository
        {
            get
            {
                if (customerRepository == null)
                    customerRepository = new CustomerRepository(context);

                return customerRepository;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
        }
    }
}
