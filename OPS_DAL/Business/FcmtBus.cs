using OPS_DAL.DAL;
using OPS_DAL.Entities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Business
{
    //Author: Ha Nguyen
    public class FcmtBus
    {
        public static List<Fcmt> GetVideoComment(string fileId)
        {
            var strSql = @"SELECT * FROM T_PB_FCMT WHERE FILEID = :P_FILEID ORDER BY COMMENTDATE DESC";

            var oracleParams = new List<OracleParameter> {

                new OracleParameter("P_FILEID", fileId),
            };

            return OracleDbManager.GetObjects<Fcmt>(strSql, oracleParams.ToArray());
        }

        public static string GetMaxCommentId(string fileId)
        {
            var strSql = @"SELECT max(COMMENTID) MAXCOMMENTID FROM T_PB_FCMT WHERE FILEID = :P_FILEID ";

            var oracleParams = new List<OracleParameter> {

                new OracleParameter("P_FILEID", fileId),
            };

            var fcmt = OracleDbManager.GetObjects<Fcmt>(strSql, oracleParams.ToArray()).FirstOrDefault();

            string maxCommentId = "";
            if (fcmt.MaxCommentId == null)
            {
                maxCommentId = "00001";
            }
            else
            {
                var maxId = Convert.ToInt32(fcmt.MaxCommentId) + 1;

                string commentId = maxId.ToString();
            
                maxCommentId = commentId.PadLeft(5, '0');
            }
            return maxCommentId;
        }

        public static bool AddVideoComment(Fcmt videoComment)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    AddNewVideoComment(videoComment, connection, trans);

                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public static bool AddNewVideoComment(Fcmt videoComment, OracleConnection oraConn, OracleTransaction trans)
        {
            string sql = @"INSERT INTO T_PB_FCMT (FILEID, COMMENTID, COMMENTNOTE, COMMENTDATE, REGISTERID) 
                            VALUES (:P_FILEID, :P_TEMPID, :P_COMMENTNOTE, :P_COMMENTDATE, :P_REGISTERID)";

            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_ACTIONCODE", videoComment.FileId),
                new OracleParameter("P_TEMPID", videoComment.CommentId),
                new OracleParameter("P_COMMENTNOTE", videoComment.CommentNote),
                new OracleParameter("P_COMMENTDATE", videoComment.CommentDate),
                new OracleParameter("P_REGISTERID", videoComment.RegisterId),
            };
            var result = OracleDbManager.ExecuteQuery(sql, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;
        }
    }
}
