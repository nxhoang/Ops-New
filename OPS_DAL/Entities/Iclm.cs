using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    /// <summary>
    /// Refer to T_00_ICLM table
    /// </summary>
    /// Author: Son Nguyen Cao
    public class Iclm
    {
        public string MainLevel { get; set; }
        public string LevelNo { get; set; }
        public string LevelCode { get; set; }
        public string LevelDesc { get; set; }
        public string LevelUse { get; set; }
        public string RegistryId { get; set; }
        public string RegistryDate { get; set; }
        public string ModifyId { get; set; }
        public string ModifyDate { get; set; }
    }
}
