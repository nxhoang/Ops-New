using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    /// <summary>
    /// Refer to t_pb_fcmnt table
    /// </summary>
    /// Author: Ha Nguyen.
    public class Fcmt
    {
        public string FileId { get; set; }
        public string CommentId { get; set; }
        public string CommentNote { get; set; }
        public DateTime CommentDate { get; set; }
        public string RegisterId { get; set; }
        public string MaxCommentId { get; set; }
        
    }
}
