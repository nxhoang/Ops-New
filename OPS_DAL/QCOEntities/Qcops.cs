using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.QCOEntities
{
	public class Qcops
	{
		public int intNewRowSeqNo { get; set; }
		public string QCOFACTORY { get; set; }
		public int QCOYEAR { get; set; }
		public string QCOWEEKNO { get; set; }
		public int QCORANK { get; set; }
		public int RANKING { get; set; } //RowSeqNo
		public string CORANKING { get; set; }
		public string FACTORY { get; set; }
		public string LINENO { get; set; }
		public string AONO { get; set; }
		public string STYLECODE { get; set; }
		public string STYLESIZE { get; set; }
		public string STYLECOLORSERIAL { get; set; }
		public string REVNO { get; set; }
		public string PRDPKG { get; set; }
		public string PRDSDAT { get; set; }
		public string PRDEDAT { get; set; }
		public DateTime DELIVERYDATE { get; set; }
		public string NORMALIZEDPERCENT { get; set; }
		public int QCORANKINGNEW { get; set; }
		public int MAXQCORANKING { get; set; }
		public int intNewRanking { get; set; }

		public string CHANGEQCORANK { get; set; } //2019-07-25 Tai Le (Thomas) 
		public string CHANGEBY { get; set; } //2019-09-24 Tai Le (Thomas) 
		public string CHANGEON { get; set; } //2019-09-24 Tai Le (Thomas) 
		public string REASON { get; set; } //2019-09-24 Tai Le (Thomas) 

		public string QCOSOURCE { get; set; } //2019-11-07 Tai Le (Thomas) 
		public string QCOVERSION { get; set; } //2019-11-07 Tai Le (Thomas) 


		public string FINSOREADINESS { get; set; }//2020-03-31 Tai Le(Thomas)
		public string JIGREADINESS { get; set; }//2020-03-31 Tai Le(Thomas)
		public string SOPREADINESS { get; set; }//2020-03-31 Tai Le(Thomas)

		public string QCOSTARTDATE { get; set; }//2020-04-25 Tai Le(Thomas)

		public string UpdateCustomRanking(string pConnectionString, string pUpdateID)
		{
			double ReplacedRanking = double.MinValue,
				ReplacedOrigQCORanking = double.MinValue,
				tempRank = double.MinValue;
			try
			{
				//For Sample Case: just swap the CHANGEQCORank each other.

				//Swap position:
				//Find Replaced Ranking
				ReplacedRanking = Convert.ToDouble(CHANGEQCORANK); //Target Rank
				tempRank = ReplacedRanking + 0.1;

				ReplacedOrigQCORanking = GetOrigCHANGEQCORANK(pConnectionString, ReplacedRanking); //Orig CHANGEQCORank of  Target Rank
				int ReplacementOrigQCORanking = GetOrigCHANGEQCORANK(pConnectionString, QCORANK); //Orig CHANGEQCORank of Current QCORank  

				UpdateCHANGEQCORANK(pConnectionString, tempRank, ReplacedRanking, REASON, pUpdateID);

				UpdateCHANGEQCORANK(pConnectionString, ReplacedRanking, ReplacementOrigQCORanking, REASON, pUpdateID);

				UpdateCHANGEQCORANK(pConnectionString, ReplacementOrigQCORanking, tempRank, REASON, pUpdateID);

				return "QCORank[" + ReplacedOrigQCORanking.ToString() + "], QCOChangeRank[" + ReplacedRanking.ToString() + "] updated to new QCOChangeRank.";
			}
			catch (Exception ex)
			{
				var Msg = ex.Message;
				return "Fail to update for QCORank[" + ReplacedOrigQCORanking.ToString() + "], QCOChangeRank[" + ReplacedRanking.ToString() + "]: " + Msg;
			}
		}

		private int GetOrigCHANGEQCORANK(string pConnectionString, double pQCORank)
		{
			int retValue = 0;
			var strSQL =
				" Select CHANGEQCORANK From PKMES.T_QC_QUEUE " +
				" Where QCOFACTORY = '" + QCOFACTORY + "'  AND QCOYEAR = " + QCOYEAR + " AND QCOWEEKNO = '" + QCOWEEKNO + "' " +
				" And CHANGEQCORANK = " + pQCORank;

			var _dt = OPS_DAL.DAL.OracleDbManager.Query(strSQL, null, pConnectionString);

			if (_dt != null)
			{
				if (_dt.Rows.Count > 0)
					if (_dt.Rows[0][0] != null)
						retValue = Convert.ToInt32(_dt.Rows[0][0].ToString());
				_dt.Dispose();
			}

			return retValue;
		}

		public int MaxQCORank(string pConnectionString)
		{
			int retValue = 0;
			var strSQL =
				" SELECT MAX(QCORANK) " +
				" FROM PKMES.T_QC_QUEUE " +
				" WHERE QCOFACTORY = '" + QCOFACTORY + "'  AND QCOYEAR = " + QCOYEAR + " AND QCOWEEKNO = '" + QCOWEEKNO + "' "
				;
			var _dt = OPS_DAL.DAL.OracleDbManager.Query(strSQL, null, pConnectionString);

			if (_dt != null)
			{
				if (_dt.Rows.Count > 0)
					if (_dt.Rows[0][0] != null)
						retValue = Convert.ToInt32(_dt.Rows[0][0].ToString());
				_dt.Dispose();
			}
			return retValue;
		}

		public void UpdateCHANGEQCORANK(string pConnectionString, double NewChangeQCORank, double OrigChangeQCORank, string pReason, string pUpdateID)
		{
			try
			{
				//var strSQL =
				//   " Update PKMES.T_QC_QUEUE " +
				//   " Set CHANGEQCORANK = " + NewChangeQCORank +
				//   " , REASON = '"+ pReason + "' " +
				//   " , CHANGEBY = '"+ pUpdateID + "' " +
				//   " , CHANGEON = Sysdate " +
				//   " Where QCOFACTORY = '" + QCOFACTORY + "'  AND QCOYEAR = " + QCOYEAR + " AND QCOWEEKNO = '" + QCOWEEKNO + "' " +
				//   " And CHANGEQCORANK = " + OrigChangeQCORank;

				var strSQL =
				   " Update {TableName} " +
				   " Set CHANGEQCORANK = " + NewChangeQCORank +
				   " , REASON = '" + pReason + "' " +
				   " , CHANGEBY = '" + pUpdateID + "' " +
				   " , CHANGEON = Sysdate " +
				   " Where QCOFACTORY = '" + QCOFACTORY + "'  AND QCOYEAR = " + QCOYEAR + " AND QCOWEEKNO = '" + QCOWEEKNO + "' " +
				   " And CHANGEQCORANK = " + OrigChangeQCORank;

				if (QCOSOURCE.ToUpper() == "QCO")
					strSQL = strSQL.Replace("{TableName}", "PKMES.T_QC_QUEUE");
				else if (QCOSOURCE.ToUpper() == "QCOSIM")
					strSQL = strSQL.Replace("{TableName}", "PKMES.T_QC_QUEUESIM");

				OPS_DAL.DAL.OracleDbManager.ExecuteQuery(strSQL, null, System.Data.CommandType.Text, pConnectionString);
			}
			catch (Exception ex)
			{
				var Msg = ex.Message;
			}
		}

		public void UpdateCHANGEQCORANKv2(string pConnectionString, int ChangeQCORank)
		{
			try
			{
				var strSQL =
			   " Update PKMES.T_QC_QUEUE " +
			   " Set CHANGEQCORANK = CHANGEQCORANK * 1.01 " +
			   " Where QCOFACTORY = '" + QCOFACTORY + "'  AND QCOYEAR = " + QCOYEAR + " AND QCOWEEKNO = '" + QCOWEEKNO + "' " +
			   " And CHANGEQCORANK= " + ChangeQCORank;

				OPS_DAL.DAL.OracleDbManager.ExecuteQuery(strSQL, null, System.Data.CommandType.Text, pConnectionString);
			}
			catch (Exception ex)
			{
				var Msg = ex.Message;
			}
		}
	}
}
