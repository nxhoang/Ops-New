using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OPS_DAL.CuttingPlanEntity;
using OPS_DAL.DAL;

using OPS_Utils;


//3rd party
using Oracle.ManagedDataAccess.Client;


namespace OPS_DAL.CuttingPlanRepository
{
	public interface ICutTicketRepository
	{
		//Task<bool> CreateTicketAsync(string pMaterialDataString, string pProductionPackageDataString, string pCuttingSettingDataString);

		Task<CutTicket> FindIDAsync(string pCutTicketID);

		//Task<bool> NewIDAsync();

		Task<List<ProductionPackage>> FindAllPackagesAsync();
	}


	public class CutTicketRepository : CuttingBaseRepo, ICutTicketRepository
	{
		public async Task<List<ProductionPackage>> FindAllPackagesAsync()
		{
			throw new NotImplementedException();
		}

		public async Task<CutTicket> FindIDAsync(string pCutTicketID)
		{
			//var strSQL = $"Select * From T_CT_CTMT Where TicketID = '{pCutTicketID}' ";
			var strSQL = $"Select Factory as TicketID From T_QC_QCFR Where Factory = '{pCutTicketID}' ";
			var result = await OracleDbManager.GetObjectsAsync<CutTicket>(strSQL, System.Data.CommandType.Text, null, _mySQLMESConnString);
			return result[0];
		}

	}
}
