using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Detail
    {
        [Key]
        public int DetailCode { get; set; }

        public string DecimalNumber { get; set; }

        public string DetailName { get; set; }

        public string AlloyGrade { get; set; }

        public decimal Mass { get; set; }

        public virtual ICollection<Production> Productions { get; set; } = new List<Production>();
    }
}
