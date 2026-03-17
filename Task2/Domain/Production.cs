using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [PrimaryKey(nameof(DetailCode), nameof(OperationNumberInProcess))]
    public class Production
    {
        [ForeignKey(nameof(Detail))]
        public int DetailCode { get; set; }

        public int OperationNumberInProcess { get; set; }

        [ForeignKey(nameof(Operation))]
        public int OperationCode { get; set; }

        public virtual Detail Detail { get; set; }

        public virtual Operation Operation { get; set; }
    }
}
