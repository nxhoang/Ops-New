
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PKQCO;
using OPS_DAL.DAL;
using OPS_Utils;

using Newtonsoft.Json;
using Serilog;
using Serilog.Sinks;

namespace TestRun
{
	static class Program
	{
		const string ConnString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=erp.pungkookvn.com)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=pkerp)));User Id=PKPCM;Password=PKPCM@)!&; ";
		const string ConnStringMES =
			"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=erp.pungkookvn.com)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=pkerp)));User Id=PKMES;Password=pkENT)%@%@)@); ";
		const string ConnStringPKERP =
					"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=118.69.170.22)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=PKERP)));User Id=PKERP;Password=PKERP@()$@K16;";
		static void Main(string[] args)
		{
			//2020-03-20 Tai Le(Thomas)
			System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
			Log.Logger = new LoggerConfiguration()
			   .WriteTo.File(@"Logs\log-.txt", rollingInterval: RollingInterval.Day)
			   .CreateLogger();
			try
			{ 
				string QCOResult_2 = "";
				PCMQCOCalculation pcmQCOCalculation = new PCMQCOCalculation(ConnStringPKERP, ConnStringMES);
				/*Run QCO*/
				pcmQCOCalculation.mEnviroment = "Console";
				pcmQCOCalculation.mUserID = "System";
				pcmQCOCalculation.mFactory = "%";

				//pcmQCOCalculation.mFactory = "PEA1";
				pcmQCOCalculation.mQCOSource = "QCO";
				QCOResult_2 = pcmQCOCalculation.QCOCalculationAllNew(pcmQCOCalculation.mFactory, pcmQCOCalculation.mUserID, "5000", "",  true).Result;
				//pcmQCOCalculation.CalculateCapaAll();
				//----------------------------------------------------------------------------
				//pcmQCOCalculation.mQCOSource = "QCOSim";
				//pcmQCOCalculation.QCOCalculationSIM(pcmQCOCalculation.mFactory, "System", "5000", out QCOResult_2);
				Log.Information(QCOResult_2);
				//Console.WriteLine($"Result: {QCOResult_2}"); 
			}
			catch (Exception ex)
			{
				Console.WriteLine("ERROR: " + ex.Message);
			}
			Console.ReadLine();
			Environment.Exit(0);
		}
	}
}
