using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Business
{
    public class ProtBus
    {
        #region Oracle database

        #region linking Bom and Patterns with Op Name detail
        /// <summary>
        /// Get linked Bom
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColor"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <param name="edition"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Prot> GetListLinkedBom(string styleCode, string styleSize, string styleColor, string revNo, string opRevNo, string edition)
        {
            var tblName = CommonMethod.GetTableNameProtByEdition(edition);

            string strSql = $@"select t1.*, case when t1.patterncount > 1 then 'Y' else 'N' end HasPattern from (
                                select count(pro.patternserial) PatternCount, pro.stylecode, pro.stylesize, pro.stylecolorserial, pro.revno, pro.oprevno, pro.opserial, pro.opnserial
                                    , pro.itemcode, pro.itemcolorserial
                                    , mainitemcode, mainitemcolorserial, optype, edition
                                    , icm.itemname, icc.itemcolorserial ||' - '|| icc.itemcolorways as itemcolorways
                                from {tblName}  pro
                                    join t_00_icmt icm on icm.itemcode = pro.itemcode
                                    join t_00_iccm icc on icc.itemcode = pro.itemcode and icc.itemcolorserial = pro.itemcolorserial
                                where edition  = :P_EDITION and stylecode = :P_STYLECODE and stylesize = :P_STYLESIZE and stylecolorserial = :P_STYLECOLORSERIAL and revno = :P_REVNO and oprevno = :P_OPREVNO
                                group by pro.stylecode, pro.stylesize, pro.stylecolorserial, pro.revno, pro.oprevno, pro.opserial, pro.opnserial
                                    , pro.itemcode, pro.itemcolorserial
                                    , pro.mainitemcode, pro.mainitemcolorserial, pro.optype, pro.edition
                                    , icm.itemname, icc.itemcolorserial, icc.itemcolorways
                            )t1
                            ";

            var oracleParams = new List<OracleParameter>
            {
                 new OracleParameter("P_EDITION", edition),
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_STYLESIZE", styleSize),
                new OracleParameter("P_STYLECOLORSERIAL", styleColor),
                new OracleParameter("P_REVNO", revNo),
                new OracleParameter("P_OPREVNO", opRevNo)
            };

            var lstProt = OracleDbManager.GetObjects<Prot>(strSql, oracleParams.ToArray());
            return lstProt;
        }
        #endregion

        /// <summary>
        /// delete pattern linking then inserting the new again.
        /// </summary>
        /// <param name="listLinkedItem"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool AddPatternLinkingByOpSerial(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, int opSerial, string edition, List<Prot> listLinkedItem)
        {
            using (var oraCon = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                oraCon.Open();
                var trans = oraCon.BeginTransaction();
                try
                {
                    //delete all pattern by opserial
                    DeletePatternLinking(styleCode, styleSize,styleColorSerial, revNo, opRevNo, opSerial, edition, trans, oraCon);
                    foreach (var item in listLinkedItem ?? new List<Prot>())
                    {
                        AddPatternBom(item, oraCon, trans);
                    }

                    trans.Commit();
                    return true;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Get list Prot
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColor"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <returns>List Prot</returns>
        /// Author: Son Nguyen Cao
        public static List<Prot> GetListPatternLinked(string styleCode, string styleSize, string styleColor, string revNo, string opRevNo, string edition)
        {
            var tblName = CommonMethod.GetTableNameProtByEdition(edition); //ADD) SON - 1/Jul/2019

            var sb = new StringBuilder();
            sb.AppendLine(" SELECT ");
            sb.AppendLine("     PRO.STYLECODE, PRO.STYLESIZE, PRO.STYLECOLORSERIAL, PRO.REVNO, PRO.ITEMCODE, PRO.ITEMCOLORSERIAL, PRO.PATTERNSERIAL");
            sb.AppendLine("     , PRO.MAINITEMCODE, PRO.MAINITEMCOLORSERIAL, PRO.OPREVNO, PRO.OPSERIAL, PRO.OPTYPE, PRO.STATUS, PRO.EDITION ");
            sb.AppendLine(" FROM ");
            sb.AppendLine($"     {tblName} PRO ");
            sb.AppendLine(" WHERE ");
            sb.AppendLine("      PRO.STYLECODE = :P_STYLECODE AND PRO.STYLESIZE = :P_STYLESIZE ");
            sb.AppendLine("      AND PRO.STYLECOLORSERIAL = :P_STYLECOLORSERIAL AND PRO.REVNO = :P_REVNO AND PRO.OPREVNO = :P_OPREVNO AND PRO.EDITION = :P_EDITION ");

            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_STYLESIZE", styleSize),
                new OracleParameter("P_STYLECOLORSERIAL", styleColor),
                new OracleParameter("P_REVNO", revNo),
                new OracleParameter("P_OPREVNO", opRevNo),
                 new OracleParameter("P_EDITION", edition),

            };

            var lstProt = OracleDbManager.GetObjects<Prot>(sb.ToString(), oracleParams.ToArray());
            return lstProt;
        }

        /// <summary>
        /// get list linked patterns
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColor"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <param name="edition"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Prot> GetListLinkedPattern(string styleCode, string styleSize, string styleColor, string revNo, string opRevNo, string edition)
        {
            var tblName = CommonMethod.GetTableNameProtByEdition(edition);
         
            string strSql = $@"select t1.totalpieceqty - t2.totallinkingpieceqty as PieceQtyRest, t1.* from (
                                    select  icm.itemname, icc.itemcolorserial ||' - '|| icc.itemcolorways as itemcolorways, pro.* 
                                            , ptm.piece, PTM.WIDTH, ptm.height, ptm.endwise, ptm.pieceunique, ptm.pieceqty as totalpieceqty, bomh.cadfile
                                    from {tblName}  pro
                                        join t_00_icmt icm on icm.itemcode = pro.itemcode
                                        join t_00_iccm icc on icc.itemcode = pro.itemcode and icc.itemcolorserial = pro.itemcolorserial
                                        join t_sd_ptmt ptm 
                                            on ptm.stylecode = pro.stylecode and ptm.stylesize = pro.stylesize and ptm.stylecolorserial = pro.stylecolorserial and ptm.revno = pro.revno 
                                            and ptm.itemcode = pro.itemcode and ptm.itemcolorserial = pro.itemcolorserial 
                                            and ptm.mainitemcode = pro.mainitemcode and ptm.mainitemcolorserial = pro.mainitemcolorserial and ptm.patternserial = pro.patternserial  
                                        join t_sd_bomh bomh on bomh.stylecode = ptm.stylecode and bomh.stylecolorserial = ptm.stylecolorserial and bomh.stylesize = ptm.stylesize and bomh.revno = ptm.revno
                                    where pro.edition  = :P_EDITION and pro.stylecode = :P_STYLECODE and pro.stylesize = :P_STYLESIZE and pro.stylecolorserial = :P_STYLECOLORSERIAL and pro.revno = :P_REVNO and pro.oprevno = :P_OPREVNO
                                 )t1 left join 
                                 (
                                    -- sum piece qty
                                    select sum(pieceqty) as totallinkingpieceqty, stylecode, stylesize, stylecolorserial, revno, oprevno, itemcode, itemcolorserial, mainitemcode, mainitemcolorserial
                                    , patternserial, optype, edition 
                                    from {tblName} pro
                                    where pro.edition  = :P_EDITION and pro.stylecode = :P_STYLECODE and pro.stylesize = :P_STYLESIZE and pro.stylecolorserial = :P_STYLECOLORSERIAL and pro.revno = :P_REVNO and pro.oprevno = :P_OPREVNO
                                    group by stylecode, stylesize, stylecolorserial, revno, oprevno, itemcode, itemcolorserial, mainitemcode, mainitemcolorserial, patternserial, optype, edition
                                 )t2 on t1.stylecode = t2.stylecode and t1.stylesize = t2.stylesize and t1.stylecolorserial = t2.stylecolorserial and t1.revno = t2.revno and t1.oprevno = t2.oprevno
                                            and t1.itemcode = t2.itemcode and t1.itemcolorserial = t2.itemcolorserial 
                                            and t1.mainitemcode = t2.mainitemcode and t1.mainitemcolorserial = t2.mainitemcolorserial and t1.patternserial = t2.patternserial
                                            and t1.optype = t2.optype and t1.edition = t2.edition
                                    order by t1.opserial, t1.mainitemcode, t1.itemcode, t1.patternserial ";

            var oracleParams = new List<OracleParameter>
            {
                 new OracleParameter("P_EDITION", edition),
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_STYLESIZE", styleSize),
                new OracleParameter("P_STYLECOLORSERIAL", styleColor),
                new OracleParameter("P_REVNO", revNo),
                new OracleParameter("P_OPREVNO", opRevNo)
            };

            var lstProt = OracleDbManager.GetObjectsByType<Prot>(strSql, CommandType.Text, oracleParams.ToArray());
            return lstProt;
        }

        public static List<Prot> GetListLinkedItem(string styleCode, string styleSize, string styleColor, string revNo, string opRevNo, string edition)
        {
            var tblName = CommonMethod.GetTableNameProtByEdition(edition);           

            string strSql = $@"select t1.*, case when t1.patterncount > 1 then 'Y' else 'N' end HasPattern from (
                                select count(pro.patternserial) PatternCount, pro.stylecode, pro.stylesize, pro.stylecolorserial, pro.revno, pro.oprevno, pro.opserial, pro.itemcode, pro.itemcolorserial
                                    , mainitemcode, mainitemcolorserial, optype, edition
                                    , icm.itemname, icc.itemcolorserial ||' - '|| icc.itemcolorways as itemcolorways
                                from {tblName}  pro
                                    join t_00_icmt icm on icm.itemcode = pro.itemcode
                                    join t_00_iccm icc on icc.itemcode = pro.itemcode and icc.itemcolorserial = pro.itemcolorserial
                                where edition  = :P_EDITION and stylecode = :P_STYLECODE and stylesize = :P_STYLESIZE and stylecolorserial = :P_STYLECOLORSERIAL and revno = :P_REVNO and oprevno = :P_OPREVNO
                                group by pro.stylecode, pro.stylesize, pro.stylecolorserial, pro.revno, pro.oprevno, pro.opserial, pro.itemcode, pro.itemcolorserial
                                    , pro.mainitemcode, pro.mainitemcolorserial, pro.optype, pro.edition
                                    , icm.itemname, icc.itemcolorserial, icc.itemcolorways
                            )t1
                            ";

            var oracleParams = new List<OracleParameter>
            {
                 new OracleParameter("P_EDITION", edition),
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_STYLESIZE", styleSize),
                new OracleParameter("P_STYLECOLORSERIAL", styleColor),
                new OracleParameter("P_REVNO", revNo),
                new OracleParameter("P_OPREVNO", opRevNo)
            };

            var lstProt = OracleDbManager.GetObjects<Prot>(strSql, oracleParams.ToArray());
            return lstProt;
        }

        /// <summary>
        /// Get list Prot by operation revision
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColor"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <param name="edition"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Prot> GetListProt(string styleCode, string styleSize, string styleColor, string revNo, string opRevNo, string edition)
        {
            var tblName = CommonMethod.GetTableNameProtByEdition(edition);

            string strSql = $@"select  *
                            from {tblName} pro 
                            where pro.edition  = :P_EDITION and pro.stylecode = :P_STYLECODE and pro.stylesize = :P_STYLESIZE and pro.stylecolorserial = :P_STYLECOLORSERIAL and pro.revno = :P_REVNO and pro.oprevno = :P_OPREVNO ";

            var oracleParams = new List<OracleParameter>
            {
                 new OracleParameter("P_EDITION", edition),
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_STYLESIZE", styleSize),
                new OracleParameter("P_STYLECOLORSERIAL", styleColor),
                new OracleParameter("P_REVNO", revNo),
                new OracleParameter("P_OPREVNO", opRevNo)
            };

            var lstProt = OracleDbManager.GetObjects<Prot>(strSql, oracleParams.ToArray());
            return lstProt;
        }

        /// <summary>
        /// Get list Prot 
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColor"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <param name="edition"></param>
        /// <returns>List Prot</returns>
        /// Author: VitHV
        public static List<Prot> GetProt(string styleCode, string styleSize, string styleColor, string revNo, string opRevNo, string edition)
        {
            var tblName = CommonMethod.GetTableNameProtByEdition(edition); //ADD) SON - 1/Jul/2019

            var url = FtpInfoBus.GetSubUrl();
            //remember WILL DELETED LATER
            url = "http://203.113.151.204:8080/PKPDM/";
            var sb = new StringBuilder();
            //START MOD) SON - 1/Jul/2019 - change table prot
            //sb.Append(@"WITH PT AS(
            //SELECT * FROM T_SD_PTMT C
            //     WHERE C.STYLECODE = '" + styleCode + @"' 
            //     AND C.STYLESIZE = '" + styleSize + @"'
            //     AND C.STYLECOLORSERIAL = '" + styleColor + @"'
            //     AND C.REVNO = '" + revNo + @"'
            //)
            //,A AS
            //(
            //     SELECT    C.STYLECODE
            //               ,C.STYLESIZE
            //               ,C.STYLECOLORSERIAL
            //               ,C.REVNO
            //               ,C.ITEMCODE
            //               ,C.ITEMCOLORSERIAL
            //               ,NVL(P.PATTERNSERIAL,'000') PATTERNSERIAL
            //               ,C.PATTERNSERIAL CURPATTERNSERIAL
            //               ,C.MAINITEMCODE
            //               ,C.MAINITEMCOLORSERIAL
            //               ,C.OPREVNO
            //               ,C.OPSERIAL
            //               ,C.OPTYPE
            //               ,C.STATUS
            //               ,C.EDITION
            //               ,C.CONSUMPUNIT
            //               ,C.UNITCONSUMPTION
            //               ,C.PIECEQTY
            //     FROM T_SD_PROT C 
            //     LEFT JOIN PT P ON C.PATTERNSERIAL = P.PATTERNSERIAL
            //     WHERE C.STYLECODE = :STYLECODE 
            //     AND C.STYLESIZE = :STYLESIZE
            //     AND C.STYLECOLORSERIAL = :STYLECOLORSERIAL
            //     AND C.REVNO = :REVNO
            //     AND C.OPREVNO = :OPREVNO
            //     AND (C.EDITION = :EDITION OR C.EDITION = 'ALL')                            
            //)");

            sb.Append(@"WITH PT AS(
            SELECT * FROM T_SD_PTMT C
                 WHERE C.STYLECODE = '" + styleCode + @"' 
                 AND C.STYLESIZE = '" + styleSize + @"'
                 AND C.STYLECOLORSERIAL = '" + styleColor + @"'
                 AND C.REVNO = '" + revNo + @"'
            )
            ,A AS
            (
                 SELECT    C.STYLECODE
                           ,C.STYLESIZE
                           ,C.STYLECOLORSERIAL
                           ,C.REVNO
                           ,C.ITEMCODE
                           ,C.ITEMCOLORSERIAL
                           ,NVL(P.PATTERNSERIAL,'000') PATTERNSERIAL
                           ,C.PATTERNSERIAL CURPATTERNSERIAL
                           ,C.MAINITEMCODE
                           ,C.MAINITEMCOLORSERIAL
                           ,C.OPREVNO
                           ,C.OPSERIAL
                           ,C.OPTYPE
                           ,C.STATUS
                           ,C.EDITION
                           ,C.CONSUMPUNIT
                           ,C.UNITCONSUMPTION
                           ,C.PIECEQTY ");
            sb.Append($" FROM {tblName} C");
            sb.Append(@" LEFT JOIN PT P ON C.PATTERNSERIAL = P.PATTERNSERIAL
                 WHERE C.STYLECODE = :STYLECODE 
                 AND C.STYLESIZE = :STYLESIZE
                 AND C.STYLECOLORSERIAL = :STYLECOLORSERIAL
                 AND C.REVNO = :REVNO
                 AND C.OPREVNO = :OPREVNO
                 AND (C.EDITION = :EDITION OR C.EDITION = 'ALL')                            
            )");
            //START MOD) SON - 1/Jul/2019
            sb.Append("SELECT DISTINCT ");
            sb.Append("A.STYLECODE");
            sb.Append(", A.STYLESIZE");
            sb.Append(", A.STYLECOLORSERIAL");
            sb.Append(", A.Revno");
            sb.Append(", A.ITEMCODE");
            sb.Append(", I.ITEMNAME");
            sb.Append(", A.ITEMCOLORSERIAL");
            sb.Append(", A.MAINITEMCODE");
            sb.Append(", A.MAINITEMCOLORSERIAL");
            sb.Append(", A.PATTERNSERIAL");
            sb.Append(", A.CURPATTERNSERIAL");
            sb.Append(", A.OPREVNO");
            sb.Append(", A.OPSERIAL");
            sb.Append(", A.OPTYPE");
            sb.Append(",A.CONSUMPUNIT");
            sb.Append(", A.UNITCONSUMPTION");
            sb.Append(", A.EDITION");
            sb.Append(", A.PIECEQTY");
            sb.Append(", B.CAD_MATERIAL ");
            sb.Append(", C.WIDTH ");
            sb.Append(", C.HEIGHT ");
            sb.Append(", C.PIECEUNIQUE ");
            sb.Append(", C.PIECE ");
            sb.Append(@",CASE WHEN A.PATTERNSERIAL = '000' THEN '' ELSE 
                         CASE WHEN D.CADCOLORSERIAL IS NOT NULL THEN
                           '" + url + @"' ||
                           SUBSTR(C.STYLECODE, 0, 3) || '/' || C.STYLECODE || '/' || C.STYLECODE || C.STYLESIZE
                                           || D.CADCOLORSERIAL || C.REVNO || '/'
                                           || substr(D.CADFILE, 0, instr(D.CADFILE, '.') - 1)
                                           || '/' || C.PIECEUNIQUE || '.PNG'
                         ELSE '' END
                      END URL ");
            sb.Append(@"FROM A
                            LEFT JOIN T_SD_BOMT B 
                               ON A.ITEMCODE = B.ITEMCODE AND A.ITEMCOLORSERIAL = B.ITEMCOLORSERIAL 
                               AND A.MAINITEMCODE=B.MAINITEMCODE AND A.MAINITEMCOLORSERIAL = B.MAINITEMCOLORSERIAL 
                               AND A.STYLECODE = B.STYLECODE AND A.STYLESIZE = B.STYLESIZE 
                               AND A.STYLECOLORSERIAL = B.STYLECOLORSERIAL AND A.REVNO = B.REVNO AND A.PATTERNSERIAL = '000'
                            LEFT JOIN PT C 
                                 ON A.ITEMCODE = C.ITEMCODE AND A.ITEMCOLORSERIAL = C.ITEMCOLORSERIAL 
                                 AND A.MAINITEMCODE=C.MAINITEMCODE 
                                 AND A.MAINITEMCOLORSERIAL = C.MAINITEMCOLORSERIAL 
                                 AND A.STYLECODE = C.STYLECODE 
                                 AND A.STYLESIZE = C.STYLESIZE 
                                 AND A.STYLECOLORSERIAL = C.STYLECOLORSERIAL 
                                 AND A.REVNO = C.REVNO
                                 AND A.PATTERNSERIAL = C.PATTERNSERIAL
                                 AND A.PATTERNSERIAL <> '000'
                            LEFT JOIN T_SD_BOMH D 
                                 ON A.STYLECODE = D.STYLECODE AND A.STYLESIZE = D.STYLESIZE
                                 AND A.STYLECOLORSERIAL = D.STYLECOLORSERIAL AND A.REVNO = D.REVNO 
                            LEFT JOIN t_00_icmt I ON A.MAINITEMCODE = I.ITEMCODE ");
            var prams = new OpsParams(styleCode, styleSize, styleColor, revNo, opRevNo);
            prams.ReplacePbyEmpty();
            prams.Add(new OracleParameter("EDITION", CommonMethod.GetCharProject(edition)));
            var prots = OracleDbManager.GetObjects<Prot>(sb.ToString(), prams.ToArray());
            return prots;
        }

        /// <summary>
        /// Get list of pattern
        /// </summary>
        /// <param name="edition"></param>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static List<Prot> GetListProts(string edition, string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string languageId)
        {

            //var oracleParams = new OpsLayoutOracleParams(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo);
            //{
            //    new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor){Direction = ParameterDirection.Output};
            //};

            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor)
            {
                Direction = ParameterDirection.Output
            };
            var oracleParams = new List<OracleParameter>
            {
                 new OracleParameter("P_EDITION", edition),
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_STYLESIZE", styleSize),
                new OracleParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new OracleParameter("P_REVNO", revNo),
                new OracleParameter("P_OPREVNO", opRevNo),
                new OracleParameter("P_LANGUAGEID", languageId.ToLower()),
                cursor
            };

            var lstProts = OracleDbManager.GetObjects<Prot>("SP_OPS_GETPATTERNS_PROT", CommandType.StoredProcedure, oracleParams.ToArray());

            return lstProts;
        }

        /// <summary>
        /// Adds the pattern bom.
        /// </summary>
        /// <param name="prot">The prot.</param>
        /// <param name="oraConn">The ora connection.</param>
        /// <param name="trans">The trans.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool AddPatternBom(Prot prot, OracleConnection oraConn, OracleTransaction trans)
        {
            var tblName = CommonMethod.GetTableNameProtByEdition(prot.Edition); //ADD) SON - 14/Jun/2019

            var sb = new StringBuilder();
            //sb.AppendLine(" INSERT INTO ");
            //sb.AppendLine("      T_SD_PROT (STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, ITEMCODE, ITEMCOLORSERIAL, PATTERNSERIAL ");
            //sb.AppendLine(@"      , MAINITEMCODE, MAINITEMCOLORSERIAL, OPREVNO, OPSERIAL, OPTYPE
            //                      , STATUS, CONSUMPUNIT, UNITCONSUMPTION, EDITION, PIECEQTY ) ");
            //sb.AppendLine(" VALUES ( :STYLECODE, :STYLESIZE, :STYLECOLORSERIAL, :REVNO, :ITEMCODE, :ITEMCOLORSERIAL, :PATTERNSERIAL ");
            //sb.AppendLine(@"      , :MAINITEMCODE, :MAINITEMCOLORSERIAL, :OPREVNO, :OPSERIAL, :OPTYPE, :STATUS,
            //                        :CONSUMPUNIT,  :UNITCONSUMPTION, :EDITION, :PIECEQTY)");

            sb.AppendLine(" INSERT INTO ");
            sb.AppendLine($" { tblName }  (STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, ITEMCODE, ITEMCOLORSERIAL, PATTERNSERIAL ");
            sb.AppendLine(@"      , MAINITEMCODE, MAINITEMCOLORSERIAL, OPREVNO, OPSERIAL, OPTYPE
                                  , STATUS, CONSUMPUNIT, UNITCONSUMPTION, EDITION, PIECEQTY, OPNSERIAL ) ");
            sb.AppendLine(" VALUES ( :STYLECODE, :STYLESIZE, :STYLECOLORSERIAL, :REVNO, :ITEMCODE, :ITEMCOLORSERIAL, :PATTERNSERIAL ");
            sb.AppendLine(@"      , :MAINITEMCODE, :MAINITEMCOLORSERIAL, :OPREVNO, :OPSERIAL, :OPTYPE, :STATUS,
                                    :CONSUMPUNIT,  :UNITCONSUMPTION, :EDITION, :PIECEQTY, :OPNSERIAL)");

            var prams = new OracleParameter[18];
            prams[0] = new OracleParameter("STYLECODE", prot.StyleCode);
            prams[1] = new OracleParameter("STYLESIZE", prot.StyleSize);
            prams[2] = new OracleParameter("STYLECOLORSERIAL", prot.StyleColorSerial);
            prams[3] = new OracleParameter("REVNO", prot.RevNo);
            prams[4] = new OracleParameter("ITEMCODE", prot.ItemCode);
            prams[5] = new OracleParameter("ITEMCOLORSERIAL", prot.ItemColorSerial);
            prams[6] = new OracleParameter("PATTERNSERIAL", prot.PatternSerial);
            prams[7] = new OracleParameter("MAINITEMCODE", prot.MainItemCode);
            prams[8] = new OracleParameter("MAINITEMCOLORSERIAL", prot.MainItemColorSerial);
            prams[9] = new OracleParameter("OPREVNO", prot.OpRevNo);
            prams[10] = new OracleParameter("OPSERIAL", prot.OpSerial);
            prams[11] = new OracleParameter("OPTYPE", prot.OpType);
            prams[12] = new OracleParameter("STATUS", prot.Status);
            prams[13] = new OracleParameter("CONSUMPUNIT", prot.ConsumpUnit);
            prams[14] = new OracleParameter("UNITCONSUMPTION", prot.UnitConsumption);
            prams[15] = new OracleParameter("EDITION", prot.Edition);
            prams[16] = new OracleParameter("PIECEQTY", prot.PieceQty);
            prams[17] = new OracleParameter("OPNSERIAL", prot.OpnSerial);
            var result = OracleDbManager.ExecuteQuery(sb.ToString(), prams, CommandType.Text, trans, oraConn);
            return result != null;
        }

        /// <summary>
        /// Removes the pattern bom.
        /// </summary>
        /// <param name="prot">The prot.</param>
        /// <param name="oraConn">The ora connection.</param>
        /// <param name="trans">The trans.</param>
        /// <returns></returns>
        /// Author: VitHV
        public static bool RemovePatternBom(Prot prot, OracleConnection oraConn, OracleTransaction trans)
        {
            var tblName = CommonMethod.GetTableNameProtByEdition(prot.Edition); //ADD) SON - 1/Jul/2019

            var sb = new StringBuilder();
            sb.AppendLine($" DELETE {tblName}");
            sb.AppendLine(@" WHERE STYLECODE =:STYLECODE 
                             AND STYLESIZE =:STYLESIZE 
                             AND STYLECOLORSERIAL =:STYLECOLORSERIAL
                             AND REVNO =:REVNO 
                             AND OPREVNO =:OPREVNO 
                             AND OPSERIAL =: OPSERIAL 
                             AND ITEMCODE =:ITEMCODE 
                             AND ITEMCOLORSERIAL =:ITEMCOLORSERIAL ");
            sb.AppendLine(@" AND MAINITEMCODE =: MAINITEMCODE 
                             AND MAINITEMCOLORSERIAL =:MAINITEMCOLORSERIAL 
                             AND PATTERNSERIAL =: PATTERNSERIAL 
                             AND OPTYPE =: OPTYPE ");

            // var prams = new OracleParameter[12];
            var prams = new OpsParams(prot.StyleCode, prot.StyleSize, prot.StyleColorSerial, prot.RevNo, prot.OpRevNo,
                prot.OpSerial, prot.ItemCode);
            prams.ReplacePbyEmpty();
            prams.Add(new OracleParameter("ITEMCOLORSERIAL", prot.ItemColorSerial));
            prams.Add(new OracleParameter("MAINITEMCODE", prot.MainItemCode));
            prams.Add(new OracleParameter("MAINITEMCOLORSERIAL", prot.MainItemColorSerial));
            prams.Add(new OracleParameter("PATTERNSERIAL", prot.PatternSerial));
            prams.Add(new OracleParameter("OPTYPE", prot.OpType));
            var result = OracleDbManager.ExecuteQuery(sb.ToString(), prams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;
        }

        /// <summary>
        /// Delete pattern bom by operation plan detail
        /// </summary>
        /// <param name="opdt"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool DeletePatternBomByOpdt(Opdt opDetail, OracleTransaction trans, OracleConnection oraConn)
        {
            var oracleParams = new OpsOracleParams(opDetail.Edition, opDetail.StyleCode, opDetail.StyleSize, opDetail.StyleColorSerial, opDetail.RevNo, opDetail.OpRevNo)
            {
                new OracleParameter("P_OPSERIAL", opDetail.OpSerial)
            };

            var resDel = OracleDbManager.ExecuteQuery("SP_OPS_DELETEPATTERNBOM_PROT", oracleParams.ToArray(), CommandType.StoredProcedure, trans, oraConn);

            return resDel != null;
        }

        /// <summary>
        /// Delete pattern linking by OP serial
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <param name="opSerial"></param>
        /// <param name="edition"></param>
        /// <param name="trans"></param>
        /// <param name="oraCon"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool DeletePatternLinking(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, int opSerial, string edition, OracleTransaction trans, OracleConnection oraCon)
        {
            var tblName = CommonMethod.GetTableNameProtByEdition(edition);
            if (string.IsNullOrEmpty(tblName)) throw new InvalidOperationException("Cannot get table name.");
            string strSql = $@"DELETE FROM {tblName} WHERE STYLECODE = :P_STYLECODE
                                    AND STYLESIZE = :P_STYLESIZE
                                    AND STYLECOLORSERIAL = :P_STYLECOLORSERIAL
                                    AND REVNO = :P_REVNO
                                    AND OPREVNO = :P_OPREVNO
                                    AND OPSERIAL = :P_OPSERIAL
                                    AND EDITION = :P_EDITION";

            var oraParam = new List<OracleParameter>() {
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_STYLESIZE", styleSize),
                new OracleParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new OracleParameter("P_REVNO", revNo),
                new OracleParameter("P_OPREVNO", opRevNo),
                new OracleParameter("P_OPSERIAL", opSerial),
                new OracleParameter("P_EDITION", edition)
            };
            var resDel = OracleDbManager.ExecuteQuery(strSql, oraParam.ToArray(), CommandType.Text, trans, oraCon);

            return resDel != null;
        }

        /// <summary>
        /// Delete pattern and bom by id of operation master
        /// </summary>
        /// <param name="opmt"></param>
        /// <param name="trans"></param>
        /// <param name="oraConn"></param>
        /// <returns></returns>
        public static bool DeletePatternBomByOpmt(Opmt opmt, OracleTransaction trans, OracleConnection oraConn)
        {
            var oracleParams = new OpsOracleParams(opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo);
            oracleParams.Insert(0, new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) { Direction = ParameterDirection.Output });

            var resDel = OracleDbManager.ExecuteQuery("SP_OPS_DELPATTERNBYOPMT_PROT", oracleParams.ToArray(), CommandType.StoredProcedure, trans, oraConn);

            return resDel != null; // && int.Parse(resDel.ToString()) != 0;

        }

        /// <summary>
        /// UpdateProt the pattern bom.
        /// </summary>
        /// <param name="prot">The prot.</param>
        /// <returns></returns>
        /// Author: VitHV
        public static bool UpdateProt(Prot prot)
        {
            var tblName = CommonMethod.GetTableNameProtByEdition(prot.Edition); //ADD) SON - 14/Jun/2019

            var sb = new StringBuilder();
            //sb.AppendLine(@" UPDATE T_SD_PROT 
            sb.AppendLine($" UPDATE {tblName} ");
            sb.AppendLine(@" SET OPTYPE =: NEWTYPE
                                ,CONSUMPUNIT =: CONSUMPUNIT
                                ,UNITCONSUMPTION =: UNITCONSUMPTION
                                ,PIECEQTY =: PIECEQTY ");
            sb.AppendLine(@" WHERE STYLECODE =:STYLECODE 
                             AND STYLESIZE =:STYLESIZE 
                             AND STYLECOLORSERIAL =:STYLECOLORSERIAL
                             AND REVNO =:REVNO 
                             AND OPREVNO =: OPREVNO 
                             AND OPSERIAL =: OPSERIAL 
                             AND ITEMCODE =:ITEMCODE 
                             AND ITEMCOLORSERIAL =:ITEMCOLORSERIAL ");
            sb.AppendLine(@" AND MAINITEMCODE =: MAINITEMCODE 
                             AND MAINITEMCOLORSERIAL =:MAINITEMCOLORSERIAL 
                             AND PATTERNSERIAL =: PATTERNSERIAL 
                             AND OPTYPE =: OPTYPE ");
            var prams = new OpsParams(prot.StyleCode, prot.StyleSize, prot.StyleColorSerial, prot.RevNo, prot.OpRevNo,
               prot.OpSerial, prot.ItemCode);
            prams.ReplacePbyEmpty();
            prams.Add(new OracleParameter("ITEMCOLORSERIAL", prot.ItemColorSerial));
            prams.Add(new OracleParameter("MAINITEMCODE", prot.MainItemCode));
            prams.Add(new OracleParameter("MAINITEMCOLORSERIAL", prot.MainItemColorSerial));
            prams.Add(new OracleParameter("PATTERNSERIAL", prot.PatternSerial));
            prams.Add(new OracleParameter("OPTYPE", prot.OpType));
            prams.Insert(0, new OracleParameter("PIECEQTY", prot.PieceQty));
            prams.Insert(0, new OracleParameter("UNITCONSUMPTION", prot.UnitConsumption));
            prams.Insert(0, new OracleParameter("CONSUMPUNIT", prot.ConsumpUnit));
            prams.Insert(0, new OracleParameter("NEWTYPE", prot.BomOrPattern));
            var result = OracleDbManager.ExecuteQuery(sb.ToString(), prams.ToArray(), CommandType.Text);
            return result != null;
        }

        /// <summary>
        /// Gets the prot bom pattern.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColor">Color of the style.</param>
        /// <param name="revNo">The rev no.</param>
        /// <param name="opRevNo">The op rev no.</param>
        /// <param name="opSerial"></param>
        /// <param name="edtion"></param>
        /// <returns></returns>
        /// Author: VitHV
        public static List<Prot> GetProtBomPattern(string styleCode, string styleSize, string styleColor, string revNo
            , string opRevNo, int opSerial, string edtion)
        {
            var prams = new OpsParams(styleCode, styleSize, styleColor, revNo, opRevNo, opSerial)
            {
                new OracleParameter("P_EDITION", CommonMethod.GetCharProject(edtion)),
                new OracleParameter("P_URL", FtpInfoBus.GetSubUrl())
            };
            prams.AddCusor();
            var prots = OracleDbManager.GetObjects<Prot>("SP_OPS_GETBOMPATTERN_PROT", CommandType.StoredProcedure, prams.ToArray());
            return prots;
        }

        /// <summary>
        /// Adds the new List Prot proccess.
        /// </summary>
        /// <param name="prots">The prots.</param>
        /// <returns></returns>
        /// Author: VitHV
        public static bool AddListProt(List<Prot> prots)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    //Add list tool Prot
                    foreach (var item in prots)
                    {
                        item.Edition = CommonMethod.GetCharProject(item.Edition);
                        AddPatternBom(item, connection, trans);
                    }
                    trans.Commit();
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    trans.Rollback();
                    return false;
                }
            }
        }

        /// <summary>
        /// Removes the proccess.
        /// </summary>
        /// <param name="prots">The prots.</param>
        /// <returns></returns>
        /// Author: VitHV
        public static bool RemoveListProt(List<Prot> prots)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    //Add list tool Prot
                    foreach (var item in prots)
                    {
                        var newitem = (Prot)item.Clone();
                        if (!string.IsNullOrEmpty(item.CurPatternSerial) && item.Edition == "ALL")
                        {
                            newitem.PatternSerial = newitem.CurPatternSerial;
                            RemovePatternBom(newitem, connection, trans);
                        }
                        else
                        {
                            RemovePatternBom(item, connection, trans);
                        }
                    }
                    trans.Commit();
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    trans.Rollback();
                    return false;
                }

            }
        }
        /// <summary>
        /// Updates the prot.
        /// </summary>
        /// <param name="prots">The prots.</param>
        /// <returns></returns>
        /// Author: VitHV
        public static bool UpdateProt(List<Prot> prots)
        {
            //Add list tool Prot
            foreach (var item in prots)
            {
                UpdateProt(item);
            }
            return true;
        }

        /// <summary>
        /// Summarizes the bom (bill of material) by process.
        /// </summary>
        /// <param name="opmt">The operation master.</param>
        /// <param name="language">The language.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static List<Prot> SummarizeBomByProcess(Opmt opmt, string language)
        {
            var oracleParams = new OpsOracleParams(opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial,
                opmt.RevNo, opmt.OpRevNo) { new OracleParameter("P_LANGUAGEID", language) };
            oracleParams.AddCursor();

            var prots = OracleDbManager.GetObjects<Prot>("SP_OPS_SUMMARIZEBOM_PROT", CommandType.StoredProcedure,
                oracleParams.ToArray());

            return prots;
        }

        #endregion

        #region MySql database

        /// <summary>
        /// Deletes the by opdt.
        /// </summary>
        /// <param name="opDetail">The op detail.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 05-Jul-19
        public static bool DeleteByOpdt(Opdt opDetail, MySqlTransaction transaction, MySqlConnection connection)
        {
            var ps = new OpsMySqlParams(opDetail.Edition, opDetail.StyleCode, opDetail.StyleSize,
                opDetail.StyleColorSerial, opDetail.RevNo, opDetail.OpRevNo)
            {
                new MySqlParameter("P_OPSERIAL", opDetail.OpSerial)
            };

            var rs = MySqlDBManager.ExecuteQueryWithTrans("SP_OPS_DELETEBYOPDT_PROT", ps.ToArray(),
                CommandType.StoredProcedure, transaction, connection);

            return rs != null;
        }

        /// <summary>
        /// Gets the prots.
        /// </summary>
        /// <param name="edition">The edition.</param>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColorSerial">The style color serial.</param>
        /// <param name="revNo">The rev no.</param>
        /// <param name="opRevNo">The op rev no.</param>
        /// <param name="languageId">The language identifier.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 05-Jul-19
        public static List<Prot> GetProts(string edition, string styleCode, string styleSize, string styleColorSerial,
            string revNo, string opRevNo, string languageId)
        {
            var prs = new OpsMySqlParams(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo)
            {
                new MySqlParameter("P_LANGUAGEID", languageId.ToLower())
            };
            var prots = MySqlDBManager.GetAll<Prot>("SP_MES_GETBYOPERATIONPLAN_PROT", CommandType.StoredProcedure,
                prs.ToArray());

            return prots;
        }

        /// <summary>
        /// Get list BOM and Pattern 
        /// </summary>
        /// <param name="prot"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static async Task<IEnumerable<Prot>> GetProtsAsyncMySql(Prot prot)
        {
            var param = new OpsMySqlParams(prot.StyleCode, prot.StyleSize, prot.StyleColorSerial, prot.RevNo, prot.OpRevNo)
            {
                new MySqlParameter("P_EDITION", prot.Edition)
            };

            string strSql = @"select * from t_sd_prot 
                             where stylecode = @P_STYLECODE and stylesize = @P_STYLESIZE and stylecolorserial = @P_STYLECOLORSERIAL and revno = @P_REVNO and oprevno = @P_OPREVNO and edition = @P_EDITION;";

            var listProt = MySqlDBManager.GetAll<Prot>(strSql, CommandType.Text, param.ToArray());

            return await Task.FromResult<IEnumerable<Prot>>(listProt);
        }

        #endregion
    }
}
