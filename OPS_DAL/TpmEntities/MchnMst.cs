using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.TpmEntities
{
    public class MchnMst
    {
        public string MCHN_MST_CD { get; set; }
        public string MCHN_MST_NM { get; set; }
        public string MCHN_ENG_NM { get; set; }
        public string MNFC_CD { get; set; }
        public float ELCT_INPT { get; set; }
        public string ELCT_INPT_UNIT_CD { get; set; }
        public string DMNS_INFO { get; set; }
        public float ELCT_CNMSMPTN { get; set; }
        public string ELCT_CNSM_UNIT_CD { get; set; }
        public float WGT { get; set; }
        public string WGT_UNIT_CD { get; set; }
        public string THRD_CAT_CD { get; set; }
        public string FRTH_CAT_CD { get; set; }
        public string CRT_USR_ID { get; set; }
        public string UPDT_USR_ID { get; set; }
        public DateTime CRT_DTTM { get; set; }
        public DateTime UPDT_DTTM { get; set; }
        public string DEL_YN { get; set; }
        public string WORK_AREA_X_Y_Z { get; set; }
        public string IMG_PATH { get; set; }
        public decimal RPM { get; set; }
        public string RPM_UNIT_CD { get; set; }
        public string AIR { get; set; }
        public string AIR_UNIT_CD { get; set; }
        public string COUNTRY_ORIG { get; set; }
        public string HS_CD { get; set; }
        public string CATEGORY_CD { get; set; }
        public string VIDEO_URL { get; set; }
        public string CBM { get; set; }
        public string TAGS { get; set; }
    }
}
