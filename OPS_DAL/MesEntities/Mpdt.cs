using OPS_DAL.Entities;
using System;

namespace OPS_DAL.MesEntities
{
    public class Mpdt : StyleMaster
    {
        public string PackageGroup { get; set; }
        public int SeqNo { get; set; } //decimal
        public string LineNo { get; set; }
        public string MxPackage { get; set; }
        public int MxTarget { get; set; }//decimal
        public string PlnStartDate { get; set; }
        public string PlnEndDate { get; set; }
        public DateTime? PlnActStartDate { get; set; }
        public DateTime? PlnActEndDate { get; set; }
        public string Factory { get; set; }
        public int FinishedQty { get; set; }//decimal
        public DateTime? LastProductionSync { get; set; }
        public int TaktTime { get; set; }//decimal
        public decimal WorkingHours { get; set; }
        public string MxStatus { get; set; }
        public string StatusName { get; set; }
        public string ConfirmedId { get; set; }
        public DateTime ConfirmedDate { get; set; }
        public int LineSerial { get; set; }

        public int MX_IOT_Completed { get; set; }
        public int MX_Manual_Completed { get; set; }
        public int MX_IOT_COMPLETED_DGS { get; set; }//2020-12-15 Tai Le(Thomas)

        //User information
        public DateTime CreateDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string UpdatedId { get; set; }
        public string Registrar { get; set; }
        
        public string StyleInf { get; set; }
        public string AoNo { get; set; }       
        public string Buyer { get; set; }
        public string CreateNew { get; set; }
        public string LineCombination { get; set; }
        public string LineName { get; set; }

        //Working process
        public string ModuleName { get; set; }
        public string ModuleId { get; set; }
        public string OpGroupName { get; set; }
        public string IoTType { get; set; }
        public int TotalMade { get; set; }//decimal
        public int Balance { get; set; }//decimal
        public int Shipped { get; set; }//decimal
        public int Issued { get; set; }//decimal

        public int Workers { get; set; }
        public string IsActive { get; set; }
        public DateTime Last_Iot_Data_Receive_Time { get; set; } //ADD - SON) 17/Mar/2020

        public string factoryName { get; set; } //Dinh Van - Add 2020-10 
        public int Output { get; set; } //Dinh Van - Add 2020-12-08
        public int TotalDefect { get; set; } //Dinh Van - Add 2020-12-08


    }
}
