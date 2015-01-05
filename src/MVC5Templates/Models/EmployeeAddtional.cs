using System.ComponentModel.DataAnnotations.Schema;

namespace MVC5Templates.Models
{
    public partial class Employee
    {
        [NotMapped]
        public string Name { get { return string.Format("{0} {1}", FirstName, LastName); } }
    }
}