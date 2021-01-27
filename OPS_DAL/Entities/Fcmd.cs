using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    /// <summary>
    /// Refer to t_pb_fcmd table
    /// </summary>
    /// Author: Ha Nguyen.
    public class Fcmd
    {
        public string FileId { get; set; }
        public Decimal CommentId { get; set; }
        public decimal? ParentId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public string ComContent { get; set; }
        public string CreatorId { get; set; }
        public string FileURL { get; set; }
        public string FileType { get; set; }
        public decimal UpvoteCount { get; set; }
        public string UserHasUpvote { get; set; }
        public string UserName { get; set; }
        public string FileName { get; set; }
        public Decimal MaxCommentId { get; set; }
        public Decimal ComAtSecond { get; set; }
    }
}
