using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKERP.MES.Domain.Interface.Dto
{
    public class DefectMediaDto
    {
        public int ID { get; set; }
        public int DEFECT_ITEM_ID { get; set; }
        public int OBJECT_ID { get; set; }
        public string MEDIA_TYPE { get; set; }
        public string OBJECTKEY { get; set; }

        //need calc
        public string URL { get; set; }
    }
}
