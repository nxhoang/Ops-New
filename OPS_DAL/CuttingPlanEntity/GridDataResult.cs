using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.CuttingPlanEntity
{
    public class GridDataResult
    {
        public decimal total { get; set; }
        public int page { get; set; }
        public decimal records { get; set; }
        public DataTable rows { get; set; } 
    }

    public static class GridDataResultExtensions
    {
        public static GridDataResult Minify(this GridDataResult pGridDataResult, DataTable dt)
        {
            return pGridDataResult;
        }
    }
}
