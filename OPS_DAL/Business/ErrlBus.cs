using System;
using System.Collections.Generic;
using System.Data;
using OPS_DAL.DAL;
using Oracle.ManagedDataAccess.Client;

namespace OPS_DAL.Business
{
    public class ErrlBus
    {

        /// <summary>
        /// Insers the exception log.
        /// </summary>
        /// <param name="exc">The exc.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="menuId">The screen identifier.</param>
        /// <param name="eventId">The event identifier.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InserExceptionLog(Exception exc, string userId, string menuId, string eventId, string systemId)
        {
            string innerExcType = "";
            string innerExcSrc;
            string innerExcMessage = "";
            var stacktrace = exc.StackTrace.Length > 500 ? exc.StackTrace.Substring(0, 499) : exc.StackTrace;
           
            if (exc.InnerException != null)
            {
                innerExcType = exc.InnerException.GetType().ToString();
                innerExcMessage = exc.InnerException.Message;
                innerExcSrc = exc.InnerException.Source;
            }
            else
            {
                innerExcSrc = exc.Source;
            }
            
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) {Direction = ParameterDirection.Output},
                new OracleParameter("P_USERID", userId),
                new OracleParameter("P_SCREENID", menuId),
                new OracleParameter("P_EVENTID", eventId),
                new OracleParameter("P_ERRORDESC", exc.Message),
                new OracleParameter("P_INNEREXCEPTIONTYPE", innerExcType),
                new OracleParameter("P_INNEREXCEPTIONMESSAGE", innerExcMessage),
                new OracleParameter("P_INNEREXCEPTIONSOURCE", innerExcSrc),
                new OracleParameter("P_INNEREXCEPTIONSTACKTRACE", stacktrace),
                new OracleParameter("P_SYSTEM_ID", systemId)
            };

            var resInsert = OracleDbManager.ExecuteQuery("SP_OPS_INSERT_ERRL", oracleParams.ToArray(), CommandType.StoredProcedure);

            return resInsert != null && int.Parse(resInsert.ToString()) != 0;

        }
    }
}
