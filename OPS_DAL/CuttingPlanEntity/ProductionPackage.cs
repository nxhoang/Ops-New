using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.CuttingPlanEntity
{
	public class ProductionPackage
	{
		public string STYLECODE { get; set; }
		public string STYLESIZE { get; set; }
		public string STYLECOLORSERIAL { get; set; }
		public string REVNO { get; set; }
		public string AONO { get; set; }
		public string FACTORY { get; set;  }
		public string LINENO { get; set; }
		public string PRDPKG { get; set; }
		public DateTime PRDSDAT { get; set; }
		public DateTime PRDEDAT { get; set; }
		public int PLANQTY { get; set; }

	}
}
