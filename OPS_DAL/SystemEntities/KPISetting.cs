using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.SystemEntities
{
    public class KPISetting
    {
        public string MasterID { get; set; }

        public string SYSTEM_ID { get; set; }
        public string SYSTEM_NAME { get; set; }

        public string TEAM { get; set; }
        public string TEAM_NAME { get; set; }

        public string CORPORATION { get; set; }
        public string CORP_NAME { get; set; }

        public string DIRECTOR { get; set; }
        public string DIRE_NAME { get; set; }

        public decimal KPIP_SEQNO { get; set; }

        public string BUYER { get; set; }
        public string BUYER_NAME { get; set; }

        public string FACTORY { get; set; }
        public string FACTORY_NAME { get; set; }

        public string MENU { get; set; }
        public string MENU_NAME { get; set; }

        public decimal SEQNO { get; set; }

        public string POSITION { get; set; }

        public string USERID { get; set; }
        public string UserName { get; set; }

        public string USE_YN { get; set; }

        public DateTime? START_DATE { get; set; }
        public DateTime? EXPIRY_DATE { get; set; }

    }
}
