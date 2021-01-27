using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OPS_Utils;

using MySql.Data.MySqlClient;

namespace OPS_DAL.CuttingPlanRepository
{
	public class CuttingBaseRepo
	{
		public readonly string _mySQLMESConnString;

		public CuttingBaseRepo()
		{
			_mySQLMESConnString = OPS_Utils.ConstantGeneric.ConnectionStrMes;
		}

		public MySqlConnection GetConnection()
		{
			MySqlConnection mySqlConnection = new MySqlConnection(_mySQLMESConnString);
			mySqlConnection.Open();
			return mySqlConnection;
		}
	}
}
