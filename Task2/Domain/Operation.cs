using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Operation
    {
        [Key]
        public int OperationCode { get; set; }

        public int WorkshopNumber { get; set; }

        public int DurationHours { get; set; }

        public decimal Cost { get; set; }

        public virtual ICollection<Production> Productions { get; set; } = new List<Production>();
    }
}