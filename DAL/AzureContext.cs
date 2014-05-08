using AzureTSProvider;
using Model;
using System;

namespace DAL
{
    public class AzureContext : TSContext
    {
        private static string tableName;
        private static string connection;

        public AzureContext(string connectionString, string table)
            : base(connectionString, table)
        {
            tableName = table;
            connection = connectionString;
        }

        public TableSet<Customer> Customers { get; set; }
    }
}
