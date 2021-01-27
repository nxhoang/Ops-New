using System.Collections.Generic;
using System.Data; 
using System.Text; 
using OPS_DAL.DAL; 
using OPS_DAL.QCOEntities;
using OPS_Utils;

namespace OPS_DAL.QCOBus
{
    public class QcopBus
    {
        public static List<Qcop> GetMasterSettings()
        {
            var sb = new StringBuilder();
            sb.AppendLine(" SELECT ROW_NUMBER() OVER(ORDER BY A.SEQNO) AS ID , A.PARAMETERNAME , A.SEQNO , A.DBFIELDNAME , A.SORTDIRECT " +
                          " FROM PKMES.T_CM_QCOP A " +
                          " WHERE A.STATUS = 'OK' "); 
            return OracleDbManager.GetObjects<Qcop>(sb.ToString(), CommandType.Text , null , EnumDataSource.OdpConnStr);
        } 
    }
}
