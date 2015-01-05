using System.ComponentModel.DataAnnotations.Schema;

namespace MVC5Templates.Models
{
    public partial class Customer
    {
        [NotMapped]
        public string Name { get { return CompanyName; } }
    }
}