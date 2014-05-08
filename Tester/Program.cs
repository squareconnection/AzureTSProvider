using DAL;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            UnitOfWork proxy = new UnitOfWork("test001");
            Customer customer = new Customer()
            {
                Company = "Square Connection Ltd",
                Email = "Bretthargreaves@hotmail.com",
                Name = "Brett Hargreaves",
                Telephone = "12345 12345678",
                VATNumber = "123 456789 GB"
            };
            proxy.CustomerRepository.Insert(customer);

            Console.WriteLine("Done!");
            Console.ReadKey();
        }
    }
}
