using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    /// <summary>
    /// T_PB_UFMT
    /// </summary>
    public class Ufmt
    {
        public string FileId { get; set; }
        public string Corporation { get; set; }
        public string Department { get; set; }
        public string PrivateCheck { get; set; }
        public string DepartmentCheck { get; set; }
        public string PublicCheck { get; set; }
        public string FileNameSys { get; set; }
        public string FileName { get; set; }
        public string ContenType { get; set; }
        public string FileType { get; set; }
        public DateTime? UploadDate { get; set; }
        public string UploadId { get; set; }
        public string Product { get; set; }
        public string FileKind { get; set; }
        public string VideoLink { get; set; }
    }
}
