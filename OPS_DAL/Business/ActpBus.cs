using OPS_DAL.Entities;
using OPS_Utils;
using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OPS_DAL.DAL;
using System.Data;

namespace OPS_DAL.Business
{
    //Author: HA NGUYEN
    public class ActpBus
    {
        public static bool AddTemplateToActioncode(List<Actp> lstTemplate)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    foreach (var actp in lstTemplate)
                    {
                        AddTemplate(actp, connection, trans);
                    }
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

        public static bool AddTemplate(Actp lstTemplate, OracleConnection oraConn, OracleTransaction trans)
        {
            string sql = @"INSERT INTO T_OP_ACTP (ACTIONCODE, TEMPID) VALUES (:P_ACTIONCODE, :P_TEMPID)";
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_ACTIONCODE", lstTemplate.ActionCode),
                new OracleParameter("P_TEMPID", lstTemplate.TempId),
            };
            var result = OracleDbManager.ExecuteQuery(sql, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;
        }

        //Get Weekly Report
        public static List<Report> GetWeeklyReport(string startDate, string finishDate)
        {
            var strSql = @" select  sf_mastercode('Buyer',substr(pdmt.adno,4,3),'code_name') as buyername
                            ,pdmt.sos
                            , T_CM_SSCM.NATION 
                            , t_cm_mcmt.CODE_NAME
                            ,sf_sosname(pdmt.sos) as fullname
                            ,pdmt.adno
                            ,pdmt.pdno
                            ,pdim.itemcode
                            ,sf_itemname(pdim.itemcode) as itemname
                            ,pdim.itemcolorserial
                            ,sf_itemcolorname(pdim.itemcode,pdim.itemcolorserial) as itemcolor
                            ,pdim.consumpunit
                            ,pdim.qtyconsumption
                            ,pdim.rprqty
                            ,pdim.prprice
                            ,pdim.currcode
                            ,(pdim.rprqty * pdim.prprice) as poamount
                            , (case when pdim.currcode = 'USD' 
                            then pdim.prprice else sf_crate('MP',pdmt.issueddate,pdim.currcode,'USD') * pdim.prprice end) * pdim.rprqty as poamount_usd,
                            c.reasonname,
                            fcmt.name as factory 
          
                            ,(Select Name From T_CM_USMT Where UserID = pdmt.REGISTER) as PORegistryName 
         
                            from t_pd_pdmt pdmt left join t_pd_pdim pdim on pdmt.adno = pdim.adno and pdmt.pdno = pdim.pdno
                            left outer join ( select a.pdno  , a.pdcd , b.reasonname
                                        from ( select pdno , pdcd,
                                                    dense_rank() over (partition by pdno  order by  pdno, pdcd asc ) as rankseq
                                                from (  select pdno  , pdcd, count(*)
                                                        from t_pd_pdar
                                                        where pdno is not null
                                                        group by  pdno  , pdcd
                                                      )
                                              ) a left outer join (select s_code, code_name as reasonname
                                            from t_cm_mcmt where m_code = 'PDCD' and s_code <> '000') b on a.pdcd = b.s_code 
                                        where a.rankseq = 1
                                        ) c on pdmt.pdno = c.pdno
                           left outer join t_cm_fcmt fcmt on  
                           pdim.delifactory = fcmt.factory

                            Left join      T_CM_SSCM ON
                            pdmt.sos = T_CM_SSCM.SOS
     
                            LEFT JOIN t_cm_mcmt ON 
                            T_CM_SSCM.NATION = T_CM_MCMT.S_CODE
                            AND  m_Code='Nation'
                            where 
                            to_char(pdmt.issueddate,'yyyy-mm-dd') between :P_STARTDATE and :P_FINISHDATE
                            order by pdmt.sos, pdmt.adno, pdmt.pdno, pdim.itemcode, pdim.itemcolorserial";

            var oracleParams = new List<OracleParameter> {
                new OracleParameter("P_STARTDATE", startDate),
                new OracleParameter("P_FINISHDATE", finishDate)
            };

            return OracleDbManager.GetObjects<Report>(strSql, oracleParams.ToArray());
        }

        public static bool DeleteTemplateActioncode(Actp objActp)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    var optp = new Optp() { ActionCode = objActp.ActionCode, TempId = objActp.TempId };

                    if (OptpBus.DeleteActionCodeTemplate(optp, trans, connection))
                    {
                        if (DeleteActionCodeTemplateProcess(objActp, trans, connection))
                        {
                            trans.Commit();
                            return true;
                        };
                    }
                    else
                    {
                        trans.Rollback();
                        return false;
                    }
                                        
                    return false;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
            
        }

        public static bool DeleteActionCodeTemplateProcess(Actp template, OracleTransaction trans, OracleConnection oraConn)
        {
            string sql = @"DELETE FROM T_OP_ACTP where TEMPID = :P_TEMPID and ACTIONCODE =:P_ACTIONCODE";
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_TEMPID", template.TempId),
                new OracleParameter("P_ACTIONCODE", template.ActionCode),
            };
            var result = OracleDbManager.ExecuteQuery(sql, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;
        }

        public static List<Actp> GetActionCodeTable(string actionCode)
        {
            if (actionCode == "999")
            {
                string sql = @"select prth.tempname, prth.tempid, mcmt.code_name actioncode, mcmt.s_code actioncodeid
                            from T_CM_MCMT mcmt join t_op_actp actp 
                            on mcmt.s_code = actp.actioncode join t_op_prth prth
                            on actp.tempid = prth.tempid where mcmt.m_code = 'ActionCode'";

                return OracleDbManager.GetObjects<Actp>(sql, null);
            }
            else
            {
                string sql = @"select prth.tempname, prth.tempid, mcmt.code_name actioncode, mcmt.s_code actioncodeid 
                            from T_CM_MCMT mcmt join t_op_actp actp 
                            on mcmt.s_code = actp.actioncode join t_op_prth prth
                            on actp.tempid = prth.tempid where mcmt.m_code = 'ActionCode' and actp.actioncode = :P_ACTIONCODE";

                var oracleParams = new List<OracleParameter> {
                new OracleParameter("P_ACTIONCODE", actionCode),
                };

                return OracleDbManager.GetObjects<Actp>(sql, oracleParams.ToArray());
            }
        }
    }
}
