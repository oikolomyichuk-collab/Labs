using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportPluginBase
{
    public interface IReportPlugin
    {
        string Name { get; }
        string Description { get; }
        void GenerateReport();
    }
}