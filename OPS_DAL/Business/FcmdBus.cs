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
    public class FcmdBus
    {
        public static List<Fcmd> GetVideoComment(string fileId)
        {
            var strSql = @"SELECT USMT.NAME USERNAME, FCMD.* FROM T_PB_FCMD FCMD LEFT JOIN T_CM_USMT USMT
                            ON FCMD.CREATORID = USMT.USERID WHERE FILEID = :P_FILEID";
            
            var oracleParams = new List<OracleParameter> {

                new OracleParameter("P_FILEID", fileId),
            };

            return OracleDbManager.GetObjects<Fcmd>(strSql, oracleParams.ToArray());
        }

        public static string GetCommentId(string fileId)
        {
            var strSql = @"SELECT max(COMMENTID) MAXCOMMENTID FROM T_PB_FCMD WHERE FILEID = :P_FILEID ";

            var oracleParams = new List<OracleParameter> {

                new OracleParameter("P_FILEID", fileId),
            };

            var fcmd = OracleDbManager.GetObjects<Fcmd>(strSql, oracleParams.ToArray()).FirstOrDefault();

            decimal CommentId = 0;
            if (fcmd== null)
            //if (fcmd.MaxCommentId == null)
            {
                CommentId = 1;
            }
            else
            {
                CommentId = fcmd.MaxCommentId + 1;
            }

            return CommentId.ToString();
        }

        public static string GetTimeVideoById(string fileId ,string commentId)
        {
            var strSql = @"SELECT FCMD.COMATSECOND FROM T_PB_FCMD FCMD WHERE FILEID = :P_FILEID AND COMMENTID = :P_COMMENTID ";

            var oracleParams = new List<OracleParameter> {

                new OracleParameter("P_FILEID", fileId),
                new OracleParameter("P_COMMENTID", commentId),
            };

            var fcmd = OracleDbManager.GetObjects<Fcmd>(strSql, oracleParams.ToArray()).FirstOrDefault();
            return fcmd.ComAtSecond.ToString();
        }

        public static bool AddVideoComment(Fcmd comment)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    AddNewVideoComment(comment, connection, trans);

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

        public static bool AddNewVideoComment(Fcmd comment, OracleConnection oraConn, OracleTransaction trans)
        {
            string sql = @"INSERT INTO T_PB_FCMD (FILEID, COMMENTID, PARENTID, CREATEDATE, MODIFYDATE, COMCONTENT, CREATORID,
                            FILEURL, FILETYPE, UPVOTECOUNT, USERHASUPVOTE, COMATSECOND) 
                            VALUES (:P_FILEID, :P_COMMENTID, :P_PARENTID, :P_CREATEDATE, :P_MODIFYDATE, :P_COMCONTENT, :P_CREATORID,
                            :P_FILEURL, :P_FILETYPE, :P_UPVOTECOUNT, :P_USERHASUPVOTE, :P_COMATSECOND)";

            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_FILEID", comment.FileId),
                new OracleParameter("P_COMMENTID", comment.CommentId),
                new OracleParameter("P_PARENTID", comment.ParentId),
                new OracleParameter("P_CREATEDATE", comment.CreateDate),
                new OracleParameter("P_MODIFYDATE", comment.ModifyDate),
                new OracleParameter("P_COMCONTENT", comment.ComContent),
                new OracleParameter("P_CREATORID", comment.CreatorId),
                new OracleParameter("P_FILEURL", comment.FileURL),
                new OracleParameter("P_FILETYPE", comment.FileType),
                new OracleParameter("P_UPVOTECOUNT", comment.UpvoteCount),
                new OracleParameter("P_USERHASUPVOTE", comment.UserHasUpvote),
                new OracleParameter("P_COMATSECOND", comment.ComAtSecond),
            };
            var result = OracleDbManager.ExecuteQuery(sql, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;
        }

        public static bool EditComment(Fcmd comment)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    EditVideoComment(comment, connection, trans);

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

        public static bool EditVideoComment(Fcmd comment, OracleConnection oraConn, OracleTransaction trans)
        {
            string sql = @"UPDATE T_PB_FCMD SET MODIFYDATE = SYSDATE, COMCONTENT = :P_COMCONTENT, 
                           COMATSECOND = :P_COMATSECOND WHERE FILEID = :P_FILEID AND COMMENTID = :P_COMMENTID";

            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_COMCONTENT", comment.ComContent),
                new OracleParameter("P_COMATSECOND", comment.ComAtSecond),
                new OracleParameter("P_FILEID", comment.FileId),
                new OracleParameter("P_COMMENTID", comment.CommentId),
                
            };
            var result = OracleDbManager.ExecuteQuery(sql, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;
        }

        public static bool DeleteComment(Fcmd comment)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    DeleteVideoComment(comment, connection, trans);

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

        public static bool DeleteVideoComment(Fcmd comment, OracleConnection oraConn, OracleTransaction trans)
        {
            string sql = @"DELETE FROM T_PB_FCMD WHERE FILEID = :P_FILEID AND COMMENTID = :P_COMMENTID";

            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_FILEID", comment.FileId),
                new OracleParameter("P_COMMENTID", comment.CommentId)
            };
            var result = OracleDbManager.ExecuteQuery(sql, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;
        }

        public static bool UpvoteComment(Fcmd comment)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    UpvoteComment(comment, connection, trans);

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

        public static bool UpvoteComment(Fcmd comment, OracleConnection oraConn, OracleTransaction trans)
        {
            string sql = @"UPDATE T_PB_FCMD SET UPVOTECOUNT = :P_UPVOTECOUNT, USERHASUPVOTE = :P_USERHASUPVOTE
                            WHERE FILEID = :P_FILEID AND COMMENTID = :P_COMMENTID";

            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_UPVOTECOUNT", comment.UpvoteCount),
                new OracleParameter("P_USERHASUPVOTE", comment.UserHasUpvote),
                new OracleParameter("P_FILEID", comment.FileId),
                new OracleParameter("P_COMMENTID", comment.CommentId)
            };
            var result = OracleDbManager.ExecuteQuery(sql, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;
        }

        public static bool UploadAttachments(Fcmd comment)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    UploadAttachmentsComment(comment, connection, trans);

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

        public static bool UploadAttachmentsComment(Fcmd comment, OracleConnection oraConn, OracleTransaction trans)
        {
            string sql = @"INSERT INTO T_PB_FCMD (FILEID, COMMENTID, PARENTID, CREATEDATE, CREATORID,
                            MODIFYDATE, FILEURL, FILETYPE, FILENAME) 
                            VALUES (:P_FILEID, :P_COMMENTID, :P_PARENTID, :P_CREATEDATE, :P_CREATORID,
                            :P_MODIFYDATE, :P_FILEURL, :P_FILETYPE, :P_FILENAME)";

            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_FILEID", comment.FileId),
                new OracleParameter("P_COMMENTID", comment.CommentId),
                new OracleParameter("P_PARENTID", comment.ParentId),
                new OracleParameter("P_CREATEDATE", comment.CreateDate),
                new OracleParameter("P_CREATORID", comment.CreatorId),
                new OracleParameter("P_MODIFYDATE", comment.ModifyDate),
                new OracleParameter("P_FILEURL", comment.FileURL),
                new OracleParameter("P_FILETYPE", comment.FileType),
                new OracleParameter("P_FILENAME", comment.FileName),
            };
            var result = OracleDbManager.ExecuteQuery(sql, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;
        }
    }
}

