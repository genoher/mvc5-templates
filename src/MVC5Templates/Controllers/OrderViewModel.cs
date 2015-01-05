using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVC5Templates.Models
{
    public class OrderViewModel
    {
        public List<Customer> Customers { get; set; }

        public List<Employee> Employees { get; set; }

        public List<Shipper> Shippers { get; set; }

        public Order Order { get; set; }
    }
}
