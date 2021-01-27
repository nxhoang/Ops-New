using OPS_DAL.DAL;
using OPS_DAL.Entities;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OPS_DAL.Business
{
    public class OpnmBus
    {
        public static Opnm GetOpNameLevel(string opNameId)
        {
            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("p_opnameid", opNameId)
            };

            string strSql = $@"select opn1.opnameid, opn1.english, opn1.grouplevel, opn1.parentid, opn1.code, opn1.iconname
                                    , opn2.opnameid as opnameid2, opn2.iconname,  opn3.opnameid as opnameid3
                                from t_op_opnm opn1     
                                     left join t_op_opnm opn2 on OPN2.OPNAMEID = OPN1.parentid 
                                     left join t_op_opnm opn3 on OPN3.OPNAMEID = OPN2.PARENTID 
                                where opn1.opnameid = :p_opnameid";

            var opName = OracleDbManager.GetObjectsByType<Opnm>(strSql, CommandType.Text, oraParams.ToArray()).FirstOrDefault();

            return opName ?? new Opnm();
        }

        /// <summary>
        /// Update icon name of process name
        /// </summary>
        /// <param name="opNameId"></param>
        /// <param name="iconName"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UpdateOpNameIconName(string opNameId, string iconName)
        {
            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("p_iconname", iconName),
                new OracleParameter("p_opnameid", opNameId)
            };

            string strSql = $@"update t_op_opnm set iconname = :p_iconname where opnameid = :p_opnameid ";

            var resUpd = OracleDbManager.ExecuteQuery(strSql, oraParams.ToArray(), CommandType.Text);

            return resUpd != null;
        }

        /// <summary>
        /// Get opname by opname id
        /// </summary>
        /// <param name="opNameId"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Opnm GetOpName(string opNameId)
        {
            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("p_opnameid", opNameId)
            };

            string strSql = $@"select nm.*, ot.itemname, mc.mchgroupname
                                from t_op_opnm nm 
                                    left join t_op_otmt ot on ot.itemcode = nm.machineid 
                                    left join t_op_mcca mc on MC.OPNAMEID = nm.opnameid and MC.MCHGROUPID = nm.machinegroup
                                where nm.opnameid = :p_opnameid";

            var opName = OracleDbManager.GetObjects<Opnm>(strSql, CommandType.Text, oraParams.ToArray()).FirstOrDefault();

            return opName;
        }

        /// <summary>
        /// Update machine group
        /// </summary>
        /// <param name="opNameId"></param>
        /// <param name="machineGroup"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UpdateOpNameMachineGroup(string opNameId, string machineGroup)
        {
            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("p_machinegroup", machineGroup),
                new OracleParameter("p_opnameid", opNameId)
            };

            string strSql = $@"update t_op_opnm set machinegroup = :p_machinegroup where opnameid = :p_opnameid ";

            var resUpd = OracleDbManager.ExecuteQuery(strSql, oraParams.ToArray(), CommandType.Text);

            return resUpd != null;
        }

        /// <summary>
        /// Update machine for operation name
        /// </summary>
        /// <param name="opNameId"></param>
        /// <param name="machineId"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UpdateOpNameMachine(string opNameId, string machineId)
        {
            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("p_machineid", machineId),
                new OracleParameter("p_opnameid", opNameId)
            };

            string strSql = $@"update t_op_opnm set machineid = :p_machineid where opnameid = :p_opnameid ";

            var resUpd = OracleDbManager.ExecuteQuery(strSql, oraParams.ToArray(), CommandType.Text);

            return resUpd != null;
        }

        /// <summary>
        /// Get Operation group
        /// </summary>
        /// <param name="groupLevel"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Opnm> GetOperationGroup(string groupLevel, string parentId)
        {
            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("p_grouplevel", groupLevel)
            };

            string conParentId = string.Empty;
            if (!string.IsNullOrEmpty(parentId))
            {
                conParentId = " and parentId = :p_parentId";
                oraParams.Add(new OracleParameter("p_parentId", parentId));
            }

            string strSql = $@"select opnameid, code ||' '|| english as english, grouplevel, parentid, code, machinegroup, machineid, iconname
                            from t_op_opnm 
                            where grouplevel = :p_grouplevel {conParentId} 
                            order by to_number(regexp_substr(code, '\d+')), code ";

            var listOpName = OracleDbManager.GetObjects<Opnm>(strSql, CommandType.Text, oraParams.ToArray());

            return listOpName;
        }

        public static List<Opnm> GetOpNames(string groupLevel, string parentId)
        {
            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("p_grouplevel", groupLevel)
            };

            string conParentId = string.Empty;
            if (!string.IsNullOrEmpty(parentId))
            {
                conParentId = " and nm.parentId = :p_parentId";
                oraParams.Add(new OracleParameter("p_parentId", parentId));
            }

            string strSql = $@"select DISTINCT case when t1.opnameid is null then 'N' else 'Y' end  HasChild
                                    , nm.* , OT.ITEMNAME, mc.mchgroupname
                                from t_op_opnm nm
                                    left join (
                                        select * from t_op_opnm where grouplevel = {int.Parse(groupLevel) + 1}
                                    )t1 on t1.parentid = NM.OPNAMEID
                                left join t_op_otmt ot on ot.itemcode = nm.machineid
                                left join t_op_mcca mc on mc.mchgroupid = nm.machinegroup and mc.opnameid = nm.opnameid
                                where nm.grouplevel = :p_grouplevel {conParentId}
                                order by to_number(nm.code)  ";

            var listOpName = OracleDbManager.GetObjects<Opnm>(strSql, CommandType.Text, oraParams.ToArray());

            return listOpName;
        }

        /// <summary>
        /// Get operation name of operation detail
        /// </summary>
        /// <param name="groupLevel"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Opnm> GetOpNamesOpDetail(string groupLevel, string parentId)
        {
            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("p_grouplevel", groupLevel),
                new OracleParameter("p_parentId", parentId)
            };

            string strSql = $@"select t0.opnameid grouplevel_0, nm.*, OT.ITEMNAME
                                from t_op_opnm nm
                                -- get parent id of group level 1
                                 left join (
                                        select opnameid, parentid from t_op_opnm where grouplevel = 1
                                    )t1 on t1.opnameid = NM.parentid  
                                 -- Get opname id of group level 0
                                 left join (
                                        select opnameid, parentid from t_op_opnm where grouplevel = 0
                                    )t0 on t0.opnameid = t1.parentid    
                                left join t_op_otmt ot on ot.itemcode = nm.machineid
                                where nm.grouplevel = :p_grouplevel and nm.parentid = :p_parentId
                                order by nm.english  ";

            var listOpName = OracleDbManager.GetObjects<Opnm>(strSql, CommandType.Text, oraParams.ToArray());

            return listOpName;
        }

        /// <summary>
        /// Get opname id by operation name.
        /// </summary>
        /// <param name="opName"></param>
        /// <returns></returns>
        public static Opnm GetOpNameId(string opName)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            var oracleParams = new List<OracleParameter>
                {
                    new OracleParameter("P_OPNAME", opName),
                    cursor
                };
            var lstOpName = OracleDbManager.GetObjects<Opnm>("SP_OPS_GETPROCESSNAMEID_OPNM", CommandType.StoredProcedure, oracleParams.ToArray());

            return lstOpName.FirstOrDefault();
        }

        public static List<Opnm> SearchProcessName(string searchParam, string languageId)
        {
            string language = "";
            switch (languageId)
            {
                case "vi":
                    language = "VIETNAM";
                    break;
                case "id":
                    language = "INDONESIA";
                    break;
                case "mm":
                    language = "MYANMAR";
                    break;
                case "ko":
                    language = "KOREA";
                    break;
                case "et":
                    language = "ETHIOPIA";
                    break;
                default:
                    language = "ENGLISH";
                    break;
            }

            //string strSql = $"SELECT {language} from t_op_opnm where lower({language}) like '%{searchParam}%'";
            string strSql = $"SELECT * from t_op_opnm where lower({language}) like '%{searchParam}%'";

            var lstOpnm = OracleDbManager.GetObjects<Opnm>(strSql, null);

            return lstOpnm;

        }

        //Author: HA NGUYEN
        public static List<Opnm> GetAllProcessName(string languageId)
        {
            string sql = @"select opnm.opnameid, 
                            case when (:P_LANGUAGEID = 'vn' or :P_LANGUAGEID = 'v') then opnm.vietnam
                            when (:P_LANGUAGEID = 'id' or :P_LANGUAGEID = 'i') then opnm.indonesia
                            when (:P_LANGUAGEID = 'mm' or :P_LANGUAGEID = 'm') then opnm.myanmar
                            when (:P_LANGUAGEID = 'et' or :P_LANGUAGEID = 't') then opnm.ethiopia
                            else opnm.english end as processname from t_op_opnm opnm";

            var oracleParams = new List<OracleParameter> {
                new OracleParameter("P_LANGUAGEID", languageId?.ToLower()),
                new OracleParameter("P_LANGUAGEID", languageId?.ToLower()),
                new OracleParameter("P_LANGUAGEID", languageId?.ToLower()),
                new OracleParameter("P_LANGUAGEID", languageId?.ToLower()),
                new OracleParameter("P_LANGUAGEID", languageId?.ToLower()),
                new OracleParameter("P_LANGUAGEID", languageId?.ToLower()),
                new OracleParameter("P_LANGUAGEID", languageId?.ToLower()),
                new OracleParameter("P_LANGUAGEID", languageId?.ToLower()),
            };

            return OracleDbManager.GetObjects<Opnm>(sql, oracleParams.ToArray());
        }
    }
}
