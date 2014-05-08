using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Customer
    {
        public Guid CustomerId { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string VATNumber { get; set; }

        public Customer()
        {
            this.CustomerId = Guid.NewGuid();
        }
    }
}
