using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC5Templates.Models
{
    public class Connection
    {
        [Key]
        public int ConnectionId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name="Connection String")]
        public string ConnectionString { get; set; }

        public int UserIdUpdated { get; set; }
        public DateTimeOffset Updated { get; set; }
    }
}
