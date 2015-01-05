using System.ComponentModel.DataAnnotations.Schema;

namespace MVC5Templates.Models
{
    public partial class Shipper
    {
        [NotMapped]
        public string Name { get { return CompanyName; } }

        [NotMapped]
        public int ShipVia { get { return ShipperID; } }
    }
}