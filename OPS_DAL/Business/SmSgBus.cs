

using OPS_DAL.DAL;
using OPS_DAL.Entities;
using OPS_DAL.Responsive;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace OPS_DAL.Business
{
    public class SmSgBus : IGenericRepository<SmSg>
    {
        #region implement interface
        public int Delete(SmSg item)
        {
            string sql = @"UPDATE T_CM_SMSG SET STATUS = 0, UPDATEID = :UPDATEID, UPDATEDATE = SYSDATE WHERE SYSTEMID = :P_SYSTEMID AND  MENUID = :P_MENUID 
                           AND FUNCTION = :P_FUNCTION AND MESSAGETYPE = :P_TYPE AND MESSAGECONTEXT = :P_CONTEXT";
            var prams = new List<OracleParameter>
            {
                new OracleParameter("UPDATEID", item.UpdateId),
                new OracleParameter("P_SYSTEMID", item.SystemId),
                new OracleParameter("P_MENUID", item.MenuId),
                new OracleParameter("P_FUNCTION", item.Function),
                new OracleParameter("P_TYPE", item.MessageType),
                new OracleParameter("P_CONTEXT", item.MessageContext)
            };
            OracleDbManager.ExecuteQuery(sql, prams.ToArray(), CommandType.Text);
            return 1;
        }

        public int DeleteById(string id)
        {
            string sql = @"UPDATE T_CM_SMSG SET STATUS = 0 WHERE CONTEXTSERIAL =  :CONTEXTSERIAL ";
            var prams = new List<OracleParameter>
            {
                new OracleParameter("CONTEXTSERIAL", id)
            };
            OracleDbManager.ExecuteQuery(sql, prams.ToArray(), CommandType.Text);
            return 1;
        }

        public SmSg GetByID(string id)
        {
            string sql = @"SELECT * FROM T_CM_SMSG WHERE CONTEXTSERIAL = :CONTEXTSERIAL";
            var prams = new List<OracleParameter>
            {
                new OracleParameter("CONTEXTSERIAL", id)
            };
            return OracleDbManager.GetObjects<SmSg>(sql, prams.ToArray()).FirstOrDefault();
        }

        public SmSg GetByItem(SmSg item)
        {
            string sql = @"SELECT * FROM T_CM_SMSG 
                           WHERE CONTEXTSERIAL = :CONTEXTSERIAL AND SYSTEMID = :SYSTEMID AND MENUID = :MENUID AND FUNCTION = :FUNCTION 
                           AND MESSAGETYPE = :MESSAGETYPE AND MESSAGECONTEXT = :MESSAGECONTEXT";
            var prams = new List<OracleParameter>
            {
                new OracleParameter("CONTEXTSERIAL", item.ContextSerial),
                new OracleParameter("SYSTEMID", item.SystemId),
                new OracleParameter("MENUID", item.MenuId),
                new OracleParameter("FUNCTION", item.Function),
                new OracleParameter("MESSAGETYPE", item.MessageType),
                new OracleParameter("MESSAGECONTEXT", item.MessageContext),
            };
            return OracleDbManager.GetObjects<SmSg>(sql, prams.ToArray()).FirstOrDefault();
        }

        public IEnumerable<SmSg> GetAll()
        {
            string sql = @"SELECT SG.*,M.CODE_NAME ContextDesc FROM T_CM_SMSG SG
                        LEFT JOIN (SELECT T.* FROM T_CM_MCMT t WHERE t.m_code ='SystemMessage') M
                        ON SG.MESSAGECONTEXT = M.S_CODE
                        ORDER BY SG.CONTEXTSERIAL DESC";
            return OracleDbManager.GetObjects<SmSg>(sql, null);
        }

        public int Add(SmSg smSg)
        {
            string sql = @" INSERT INTO T_CM_SMSG(CONTEXTSERIAL,SYSTEMID, MENUID, FUNCTION, MESSAGETYPE, MESSAGECONTEXT, TITLE, ENGLISH, VIETNAMESE, KOREAN, INDONESIAN, MYANMAR, AMHARIC,STATUS, REGISTRYDATE, UPDATEDATE, REGISTERID, UPDATEID)
                            VALUES (:CONTEXTSERIAL,:SYSTEMID, :MENUID, :FUNCTION, :MESSAGETYPE, :MESSAGECONTEXT, :TITLE, :ENGLISH, :VIETNAMESE, :KOREAN, :INDONESIAN, :MYANMAR,  :AMHARIC,1, SYSDATE, SYSDATE, :REGISTERID, :UPDATEID) ";
            var prams = new List<OracleParameter>
            {
                new OracleParameter("CONTEXTSERIAL", smSg.ContextSerial),
                new OracleParameter("SYSTEMID", smSg.SystemId),
                new OracleParameter("MENUID", smSg.MenuId),
                new OracleParameter("FUNCTION", smSg.Function),
                new OracleParameter("MESSAGETYPE", smSg.MessageType),
                new OracleParameter("MESSAGECONTEXT", smSg.MessageContext),
                new OracleParameter("TITLE", smSg.Title),
                new OracleParameter("ENGLISH", smSg.English),
                new OracleParameter("VIETNAMESE", smSg.Vietnamese),
                new OracleParameter("KOREAN", smSg.Korean),
                new OracleParameter("INDONESIAN", smSg.Indonesian),
                new OracleParameter("MYANMAR", smSg.Myanmar),
                new OracleParameter("AMHARIC", smSg.Amharic),
                new OracleParameter("REGISTERID", smSg.RegisterId),
                new OracleParameter("UPDATEID", smSg.RegisterId)
            };
            OracleDbManager.ExecuteQuery(sql, prams.ToArray(), CommandType.Text);
            return 1;
        }

        public int Update(SmSg smSg)
        {
            string sql = @" UPDATE T_CM_SMSG  
                            SET  TITLE = :TITLE
                                ,ENGLISH = :ENGLISH,VIETNAMESE = :VIETNAMESE
                                ,KOREAN = :KOREAN,INDONESIAN = :INDONESIAN
                                ,MYANMAR = :MYANMAR 
                                ,AMHARIC = :AMHARIC
                                ,UPDATEID = :UPDATEID
                                ,UPDATEDATE = SYSDATE
                            WHERE CONTEXTSERIAL = :CONTEXTSERIAL
                            AND SYSTEMID = :SYSTEMID AND MENUID = :MENUID
                            AND FUNCTION = :FUNCTION AND MESSAGETYPE = :MESSAGETYPE AND MESSAGECONTEXT=:MESSAGECONTEXT";
            var prams = new List<OracleParameter>
            {
                new OracleParameter("TITLE", smSg.Title),
                new OracleParameter("ENGLISH", smSg.English),
                new OracleParameter("VIETNAMESE", smSg.Vietnamese),
                new OracleParameter("KOREAN", smSg.Korean),
                new OracleParameter("INDONESIAN", smSg.Indonesian),
                new OracleParameter("MYANMAR", smSg.Myanmar),
                new OracleParameter("AMHARIC", smSg.Amharic),
                new OracleParameter("UPDATEID", smSg.UpdateId),
                new OracleParameter("CONTEXTSERIAL", smSg.ContextSerial),
                new OracleParameter("SYSTEMID", smSg.SystemId),
                new OracleParameter("MENUID", smSg.MenuId),
                new OracleParameter("FUNCTION", smSg.Function),
                new OracleParameter("MESSAGETYPE", smSg.MessageType),
                new OracleParameter("MESSAGECONTEXT", smSg.MessageContext)
            };
            OracleDbManager.ExecuteQuery(sql, prams.ToArray(), CommandType.Text);
            return 1;
        }
        #endregion region implement interface

        /// <summary>
        /// GetAllSystem
        /// </summary>
        /// <returns></returns>
        /// Author: VitHV
        public List<SmSg> GetAllSystem()
        {
            string sql = "SELECT DISTINCT SYSTEM_ID SYSTEMID FROM T_CM_MENU";
            return OracleDbManager.GetObjects<SmSg>(sql, null);
        }

        /// <summary>
        /// GetEvent
        /// </summary>
        /// <returns></returns>
        /// Author: VitHV
        public List<SmSg> GetFunction()
        {
            string sql = "SELECT DISTINCT FUNCTION FROM T_CM_SMSG";
            return OracleDbManager.GetObjects<SmSg>(sql, null);
        }
        

        /// <summary>
        /// GetAllMenu
        /// </summary>
        /// <returns></returns>
        /// Author: VitHV
        public List<SmSg> GetAllMenu()
        {
            string sql = "SELECT DISTINCT MENU_ID MENUID, SYSTEM_ID SYSTEMID FROM T_CM_MENU";
            return OracleDbManager.GetObjects<SmSg>(sql, null);
        }

        /// <summary>
        /// GetMenuBySys
        /// </summary>
        /// <param name="sysId"></param>
        /// <returns></returns>
        /// Author: VitHV
        public List<SmSg> GetMenuBySys(string sysId)
        {
            string sql = "SELECT DISTINCT MENU_ID MENUID FROM T_CM_MENU WHERE SYSTEM_ID = :SYSTEMID";
            var prams = new List<OracleParameter>
            {
                new OracleParameter("SYSTEMID", sysId)
            };
            return OracleDbManager.GetObjects<SmSg>(sql, prams.ToArray());
        }

        public string GetMaxId(SmSg item)
        {
            string sql = @"SELECT TO_CHAR(MAX(TO_NUMBER(CONTEXTSERIAL))) CONTEXTSERIAL
                           FROM T_CM_SMSG G 
                           WHERE G.SYSTEMID = :SYSTEMID
                           AND G.MENUID = :MENUID 
                           AND G.FUNCTION = :FUNCTION 
                           AND G.MESSAGETYPE = :MESSAGETYPE
                           AND G.MESSAGECONTEXT = :MESSAGECONTEXT";
            var prams = new List<OracleParameter>
            {
                new OracleParameter("SYSTEMID", item.SystemId),
                new OracleParameter("MENUID", item.MenuId),
                new OracleParameter("FUNCTION", item.Function),
                new OracleParameter("MESSAGETYPE", item.MessageType),
                new OracleParameter("MESSAGECONTEXT", item.MessageContext)
            };
            var result =  OracleDbManager.GetObjects<SmSg>(sql, prams.ToArray()).FirstOrDefault();
            int contextSerial = 1;
            if(result != null && !string.IsNullOrEmpty(result.ContextSerial))
            {
                contextSerial = int.Parse(result.ContextSerial)+1;
            }
            string strSerial = contextSerial.ToString("D3");
            return strSerial;
        }
        
    }
}
