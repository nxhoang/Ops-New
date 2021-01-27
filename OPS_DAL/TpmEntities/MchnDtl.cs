using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.TpmEntities
{
    public class MchnDtl
    {
        public string MCHN_DTL_CD { get; set; }
        public string MCHN_DTL_NM { get; set; }
        public string MCHN_MST_CD { get; set; }
        public string SRL_NO { get; set; }
        public decimal AIR_CTRL { get; set; }
        public decimal HEAT_CTRL { get; set; }
        public string SPPL_CD { get; set; }
        public string PRCH_DT { get; set; }
        public string VALD_DT { get; set; }
        public decimal PRC_AMT { get; set; }
        public string MCHN_STATUS { get; set; }
        public string PRCH_PURPOSE { get; set; }
        public string APPL_PROCESS { get; set; }
        public string REMARKS { get; set; }
        public string CRPR_CD { get; set; }
        public string FCTR_CD { get; set; }
        public string LINE_NO { get; set; }
        public string PRNT_QR_YN { get; set; }
        public string CRT_USR_ID { get; set; }
        public string UPDT_USR_ID { get; set; }
        public DateTime CRT_DTTM { get; set; }
        public DateTime UPDT_DTTM { get; set; }
        public string DEL_YN { get; set; }
        public string IOT_STS { get; set; }
        public string MAC { get; set; }
        public string PLAN_STS { get; set; }
        public string IOT_SETUP_STS { get; set; }
        public string IOT_SPPL { get; set; }
        public string CUSTOMS_TYPE { get; set; }
        public string CUSTOMS_NO { get; set; }
        public string CUSTOMS_DATE { get; set; }
        public string REGISTRY_NO { get; set; }
        public string INVOICE_NO { get; set; }
        public string INVOICE_DATE { get; set; }
        public string TAX_INVOICE_NO { get; set; }

        //Machine master name
        public string MCHN_MST_NM { get; set; }

    }
}
