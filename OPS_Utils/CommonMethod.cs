using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace OPS_Utils
{
    public class CommonMethod
    {
        #region FTP Method


        /// <summary>
        /// Creates the folder.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool CreateFolder(string folderPath)
        {
            try
            {
                //Check selection style
                if (string.IsNullOrEmpty(folderPath))
                    return false;

                //If folder is not exist then create it
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        /// <summary>
        /// Checks the and create new path.
        /// </summary>
        /// <param name="pathfile">The pathfile.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static string CheckAndCreateNewPath(string pathfile)
        {
            var i = 1;
            string newPathFile;
            var filename = Path.GetFileNameWithoutExtension(pathfile);
            var extFile = Path.GetExtension(pathfile);
            var pathFolder = Path.GetDirectoryName(pathfile);
            do
            {
                var newFileName = filename + i + extFile;
                newPathFile = pathFolder + "/" + newFileName;
                i++;
            } while (File.Exists(newPathFile));

            return newPathFile;
        }

        #endregion

        /// <summary>
        /// Gets the table name master by edition.
        /// </summary>
        /// <param name="edition">The edition.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static string GetTableNameMasterByEdition(string edition)
        {
            switch (edition)
            {
                case ConstantGeneric.EditionOps:
                case ConstantGeneric.EditionPdm:
                    return ConstantGeneric.TableSdOpmt;
                case ConstantGeneric.EditionAom:
                    return ConstantGeneric.TableMtOpmt;
                case ConstantGeneric.EditionMes:
                    return ConstantGeneric.TableMxOpmt;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Gets the table name OPMT by edition.
        /// </summary>
        /// <param name="edition">The edition.</param>
        /// <returns></returns>
        /// Author: VitHV
        public static string GetOpmtNameByEdition(string edition)
        {
            string tableName;
            switch (edition)
            {
                case ConstantGeneric.EditionOps:
                    tableName = ConstantGeneric.TableOpOpmt;
                    break;
                case ConstantGeneric.EditionPdm:
                    tableName = ConstantGeneric.TableSdOpmt;
                    break;
                case ConstantGeneric.EditionAom:
                    tableName = ConstantGeneric.TableMtOpmt;
                    break;
                case ConstantGeneric.EditionMes:
                    tableName = ConstantGeneric.TableMxOpmt;
                    break;
                default:
                    tableName = "";
                    break;
            }

            return tableName;
        }

        /// <summary>
        /// Gets the table name detail by edition.
        /// </summary>
        /// <param name="edition">The edition.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static string GetTableNameDetailByEdition(string edition)
        {
            if (string.IsNullOrEmpty(edition))
                return string.Empty;
            edition = edition.Substring(0, 1);
            string tableName;
            switch (edition)
            {
                case ConstantGeneric.EditionPdm:
                    tableName = ConstantGeneric.TableSdOpdt;
                    break;
                case ConstantGeneric.EditionOps:
                    tableName = ConstantGeneric.TableOpOpdt;
                    break;
                case ConstantGeneric.EditionAom:
                    tableName = ConstantGeneric.TableMtOpdt;
                    break;
                case ConstantGeneric.EditionMes:
                    tableName = ConstantGeneric.TableMxOpdt;
                    break;
                default:
                    return string.Empty;
            }

            return tableName;
        }

        /// <summary>
        /// Get table optl by edition, if edition MES then get table name from PKMES schema.
        /// </summary>
        /// <param name="edition"></param>
        /// <returns></returns>
        public static string GetTableNameOptlByEdition(string edition)
        {
            if (string.IsNullOrEmpty(edition)) return string.Empty;
            edition = edition.Substring(0, 1);
            string tableName;
            if(edition == ConstantGeneric.EditionMes)
            {
                tableName = ConstantGeneric.TableOptlMes;
            }
            else
            {
                tableName = ConstantGeneric.TableOptlErp;
            }
            
            return tableName;
        }

        public static string GetTableNameProtByEdition(string edition)
        {
            if (string.IsNullOrEmpty(edition)) return string.Empty;
            edition = edition.Substring(0, 1);
            string tableName;
            if (edition == ConstantGeneric.EditionMes)
            {
                tableName = ConstantGeneric.TableProtMes;
            }
            else
            {
                tableName = ConstantGeneric.TableProtErp;
            }

            return tableName;
        }

        public static string GetTableNameOPNTByEdition(string edition)
        {
            if (string.IsNullOrEmpty(edition)) return string.Empty;
            edition = edition.Substring(0, 1);
            string tableName;
            if (edition == ConstantGeneric.EditionMes)
            {
                tableName = ConstantGeneric.TableOpntMes;
            }
            else
            {
                tableName = ConstantGeneric.TableOpntErp;
            }

            return tableName;
        }

        public static string GetCharProject(string edition)
        {
            return string.IsNullOrEmpty(edition) ? string.Empty : edition.Substring(0, 1);
        }

        public static string GetCharBuyerCode(string buyerCode)
        {
            return string.IsNullOrEmpty(buyerCode) ? string.Empty : buyerCode.Substring(0, 3);
        }

        /// <summary>
        /// Checks the role.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool CheckRole(string role)
        {
            return role == ConstantGeneric.RoleTrue;
        }

        /// <summary>
        /// Converts the datetime to string.
        /// </summary>
        /// <param name="inputDate">The input date.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static string ConvertDatetimeToString(DateTime inputDate)
        {
            return inputDate.ToString("dd/MM/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Checks the style master key code.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColorSerial">The style color serial.</param>
        /// <param name="revNo">The rev no.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool CheckStyleMasterKeyCodeValid(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            if (string.IsNullOrEmpty(styleCode))
                return false;
            if (string.IsNullOrEmpty(styleSize))
                return false;
            if (string.IsNullOrEmpty(styleColorSerial))
                return false;
            return !string.IsNullOrEmpty(revNo);
        }

        /// <summary>
        /// Checks the key code op master.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColorSerial">The style color serial.</param>
        /// <param name="revNo">The rev no.</param>
        /// <param name="opRevNo">The op rev no.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool CheckOpMasterKeyCodeValid(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo)
        {
            if (!CheckStyleMasterKeyCodeValid(styleCode, styleSize, styleColorSerial, revNo))
                return false;

            return !string.IsNullOrEmpty(opRevNo);
        }

        /// <summary>
        /// Checks the key code op detail.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColorSerial">The style color serial.</param>
        /// <param name="revNo">The rev no.</param>
        /// <param name="opRevNo">The op rev no.</param>
        /// <param name="opSerial">The op serial.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool CheckKeyCodeOpDetailValid(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string opSerial)
        {
            if (!CheckOpMasterKeyCodeValid(styleCode, styleSize, styleColorSerial, revNo, opRevNo))
                return false;

            if (string.IsNullOrEmpty(opSerial))
                return false;

            return true;
        }

        /// <summary>
        /// Maps edition to table name.
        /// </summary>
        /// <param name="cmdTextName">Command text name</param>
        /// <param name="tableName">Name of table</param>
        /// <param name="edition">Edition</param>
        /// <param name="cmdText">command text</param>
        /// Author: Nguyen Xuan Hoang
        public static void MapEditionToTable(string cmdTextName, string tableName, ref string edition, ref string cmdText)
        {
            switch (edition)
            {
                case "A":
                    cmdText = $"{cmdTextName}_MT_{tableName}";
                    edition = "AOM";
                    break;
                case "M":
                    cmdText = $"{cmdTextName}_MX_{tableName}";
                    edition = "MES";
                    break;
                case "O":
                    cmdText = $"{cmdTextName}_OP_{tableName}";
                    edition = "OPS";
                    break;
                case "P":
                    cmdText = $"{cmdTextName}_SD_{tableName}";
                    edition = "PDM";
                    break;
            }
        }


        /// <summary>
        /// By Tai-Le-Huu (Thomas)
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="digit"></param>
        /// <returns></returns>
        public static string GetLeft(string expression, int digit)
        {
            if (String.IsNullOrEmpty(expression))
                return expression;

            if (expression.Length <= digit)
                return expression;

            return expression.Substring(0, digit);
        }

        /// <summary>
        /// By Tai-Le-Huu (Thomas)
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="digit"></param>
        /// <returns></returns>
        public static string GetRight(string expression, int digit)
        {
            if (String.IsNullOrEmpty(expression))
                return expression;

            if (expression.Length <= digit)
                return expression;

            return expression.Substring(expression.Length - digit, digit);
            ;
        }

        public static string GetXMLNodeValue(string XMLPath, string Node)
        {
            string sReturn = string.Empty;

            XmlDocument doc = new XmlDocument();

            if (System.IO.File.Exists(XMLPath))
            {
                doc.Load(XMLPath);

                if (doc.SelectSingleNode(Node) != null)
                {
                    sReturn = doc.SelectSingleNode(Node).InnerText;
                } 
                //if (doc.SelectSingleNode("/config/Machine/ID") != null)
                //{
                //    sReturn = doc.SelectSingleNode("/config/Machine/ID").InnerText;
                //}
            }


            return sReturn;
        }

        /// <summary>
        /// Getting condition string for where clause
        /// </summary>
        /// <typeparam name="T">An object</typeparam>
        /// <param name="list">List of objects</param>
        /// <param name="tableName">Alias of table name</param>
        /// <param name="propName">Property name</param>
        /// <returns>string</returns>
        public static string GetWhereCondition<T>(List<T> list, string tableName, string propName)
        {
            var whereCon = "";

            if (list != null && list.Count > 0)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    var propValue = list[i].GetType().GetProperty(propName)?.GetValue(list[i]);
                    whereCon += i == 0 ? $"'{propValue}'" : $",'{propValue}'";
                }

                whereCon = $"{tableName}.{propName} IN ({whereCon})";
            }
            else
            {
                whereCon = "1=1";
            }

            return whereCon;
        }

        public static string GetWhereCondition<T>(List<T> list, string tableName, string propName, string inputPropName)
        {
            var whereCon = "";

            if (list != null && list.Count > 0)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    var propValue = list[i].GetType().GetProperty(inputPropName)?.GetValue(list[i]);
                    whereCon += i == 0 ? $"'{propValue}'" : $",'{propValue}'";
                }

                whereCon = $"{tableName}.{propName} IN ({whereCon})";
            }
            else
            {
                whereCon = "1=1";
            }

            return whereCon;
        }

        /// <summary>
        /// Getting condition string for where clause
        /// </summary>
        /// <param name="arr">String array</param>
        /// <returns>string</returns>
        public static string GetWhereCondition(string[] arr)
        {
            var cdnStr = "";
            if (arr == null || arr.Length == 0)
            {
                cdnStr = "1=1";
            }
            else
            {
                for (var i = 0; i < arr.Length; i++)
                {
                    cdnStr += i == 0 ? $"'{arr[i]}'" : $",'{arr[i]}'";
                }
            }

            return cdnStr;
        }
    }
}
