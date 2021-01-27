using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MES.Models
{
    public enum OPERATION
    {
        none,
        add,
        del,
        edit,
        excel
    }

    /// <summary>
    /// The supported operations in where-extension
    /// </summary>
    public enum WhereOperation
    {
        [StringValue("eq")]
        Equal,
        [StringValue("ne")]
        NotEqual,
        [StringValue("cn")]
        Contains
    }

    [ModelBinder(typeof(GridModelBinder))]
    public class GridSettings
    {
        public int pageIndex { get; set; }
        public int pageSize { get; set; }
        public string sortColumn { get; set; }
        public string sortOrder { get; set; }
        public bool isSearch { get; set; }
        public string id { get; set; }
        public string param { get; set; }
        public string editOper { get; set; }
        public string addOper { get; set; }
        public string delOper { get; set; }
        public Filter where { get; set; }
        public OPERATION operation { get; set; }

        public string extraWhere { get; set; } //2019-08-03 Tai Le (Thomas)
    }

    public class GridModelBinder: IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            HttpRequestBase request = controllerContext.HttpContext.Request;

            /* 2019-08-02 Tai Le(Thomas): Handle auto Filter */
            string _Where = "";
            if (request.QueryString != null)
            {
                var _QueryStringKey = request.QueryString.AllKeys;
                foreach (var item in _QueryStringKey)
                {
                    if (item.Contains("_searchField"))
                    {
                        if (!String.IsNullOrEmpty(request.QueryString[item].ToString()))
                            if (item.Contains("_searchFieldDateRange"))
                            {
                                var _paramValue = request.QueryString[item].ToString();
                                var _arrParamValue = _paramValue.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

                                if (String.IsNullOrEmpty(_Where))
                                {
                                    if (!String.IsNullOrEmpty(_arrParamValue[0]) && !String.IsNullOrEmpty(_arrParamValue[1]))
                                    {
                                        _Where = " TO_CHAR(" + item.Replace("_searchFieldDateRange", "") + ",'yyyy-MM-DD HH24:mi:ss')||'Z' >= '" + DateTime.Parse(_arrParamValue[0].Trim()).ToString("u") + "' AND " +
                                                 " TO_CHAR(" + item.Replace("_searchFieldDateRange", "") + ", 'yyyy-MM-DD HH24:mi:ss')||'Z' <= '" + DateTime.Parse(_arrParamValue[1].Trim()).AddDays(1).AddSeconds(-1).ToString("u") + "' ";
                                    } else {
                                        if (!String.IsNullOrEmpty(_arrParamValue[0]) && String.IsNullOrEmpty(_arrParamValue[1]))
                                        {
                                            _Where = " TO_CHAR(" + item.Replace("_searchFieldDateRange", "") + ",'yyyy-MM-DD HH24:mi:ss')||'Z' >= '" + DateTime.Parse(_arrParamValue[0].Trim()).ToString("u") + "' ";

                                        }
                                        else if (String.IsNullOrEmpty(_arrParamValue[0]) && !String.IsNullOrEmpty(_arrParamValue[1]))
                                        {
                                            _Where = " TO_CHAR(" + item.Replace("_searchFieldDateRange", "") + ", 'yyyy-MM-DD HH24:mi:ss')||'Z' <= '" + DateTime.Parse(_arrParamValue[1].Trim()).AddDays(1).AddSeconds(-1).ToString("u") + "' ";
                                        }
                                    } 
                                }
                                else
                                {
                                    //_Where += " AND TO_CHAR(" + item.Replace("_searchFieldDateRange", "") + ",  'yyyy-MM-DD HH24:mi:ss')||'Z' >= '" + DateTime.Parse(_arrParamValue[0].Trim()).ToString("u") + "' AND " +
                                    //     " TO_CHAR(" + item.Replace("_searchFieldDateRange", "") + ",  'yyyy-MM-DD HH24:mi:ss')||'Z' <= '" + DateTime.Parse(_arrParamValue[1].Trim()).AddDays(1).AddSeconds(-1).ToString("u") + "' ";
                                    if (!String.IsNullOrEmpty(_arrParamValue[0]) && !String.IsNullOrEmpty(_arrParamValue[1]))
                                    {
                                        _Where += " AND TO_CHAR(" + item.Replace("_searchFieldDateRange", "") + ",'yyyy-MM-DD HH24:mi:ss')||'Z' >= '" + DateTime.Parse(_arrParamValue[0].Trim()).ToString("u") + "' AND " +
                                                 " TO_CHAR(" + item.Replace("_searchFieldDateRange", "") + ", 'yyyy-MM-DD HH24:mi:ss')||'Z' <= '" + DateTime.Parse(_arrParamValue[1].Trim()).AddDays(1).AddSeconds(-1).ToString("u") + "' ";
                                    }
                                    else
                                    {
                                        if (!String.IsNullOrEmpty(_arrParamValue[0]) && String.IsNullOrEmpty(_arrParamValue[1]))
                                        {
                                            _Where += " AND TO_CHAR(" + item.Replace("_searchFieldDateRange", "") + ",'yyyy-MM-DD HH24:mi:ss')||'Z' >= '" + DateTime.Parse(_arrParamValue[0].Trim()).ToString("u") + "' ";
                                        }
                                        else if (String.IsNullOrEmpty(_arrParamValue[0]) && !String.IsNullOrEmpty(_arrParamValue[1]))
                                        {
                                            _Where += " AND TO_CHAR(" + item.Replace("_searchFieldDateRange", "") + ", 'yyyy-MM-DD HH24:mi:ss')||'Z' <= '" + DateTime.Parse(_arrParamValue[1].Trim()).AddDays(1).AddSeconds(-1).ToString("u") + "' ";
                                        }
                                    }
                                }
                            }
                            else if (item.Contains("_searchFieldDateStart"))
                            {
                                if (String.IsNullOrEmpty(_Where))
                                {
                                    _Where = " TO_CHAR(" + item.Replace("_searchFieldDateStart", "") + ", 'yyyy-MM-DD HH24:mi:ss')||'Z' >= '" + DateTime.Parse(request.QueryString[item].ToString()).ToString("u") + "' ";
                                }
                                else
                                {
                                    _Where += " AND TO_CHAR(" + item.Replace("_searchFieldDateStart", "") + ", 'yyyy-MM-DD HH24:mi:ss')||'Z' >= '" + DateTime.Parse(request.QueryString[item].ToString()).ToString("u") + "' ";
                                }
                            }
                            else if (item.Contains("_searchFieldDateEnd"))
                            {
                                if (String.IsNullOrEmpty(_Where))
                                {
                                    _Where = "TO_CHAR(" + item.Replace("_searchFieldDateEnd", "") + ", 'yyyy-MM-DD HH24:mi:ss')||'Z' <= '" + DateTime.Parse(request.QueryString[item].ToString()).ToString("u") + "' ";
                                }
                                else
                                {
                                    _Where += " AND TO_CHAR(" + item.Replace("_searchFieldDateEnd", "") + ", 'yyyy-MM-DD HH24:mi:ss')||'Z' <= '" + DateTime.Parse(request.QueryString[item].ToString()).ToString("u") + "' ";
                                }
                            }
                            else if (item.Contains("_searchFieldNumBegin"))
                            {
                                if (String.IsNullOrEmpty(_Where))
                                {
                                    _Where = item.Replace("_searchFieldNumBegin", "") + " >= " + Convert.ToDecimal(request.QueryString[item].ToString());
                                }
                                else
                                {
                                    _Where += " AND " + item.Replace("_searchFieldNumBegin", "") + " >= " + Convert.ToDecimal(request.QueryString[item].ToString());
                                }
                            }
                            else if (item.Contains("_searchFieldNumEnd"))
                            {
                                if (String.IsNullOrEmpty(_Where))
                                {
                                    _Where = item.Replace("_searchFieldNumEnd", "") + " <= " + Convert.ToDecimal(request.QueryString[item].ToString());
                                }
                                else
                                {
                                    _Where += " AND " + item.Replace("_searchFieldNumEnd", "") + " <= " + Convert.ToDecimal(request.QueryString[item].ToString());
                                }
                            }

                            else if (item.Contains("_searchFieldStrBegin"))
                            {
                                if (String.IsNullOrEmpty(_Where))
                                {
                                    _Where = item.Replace("_searchFieldStrBegin", "") + " >= '" + (request.QueryString[item].ToString()) + "' ";
                                }
                                else
                                {
                                    _Where += " AND " + item.Replace("_searchFieldStrBegin", "") + " >= '" + (request.QueryString[item].ToString()) + "' ";
                                }
                            }
                            else if (item.Contains("_searchFieldStrEnd"))
                            {
                                if (String.IsNullOrEmpty(_Where))
                                {
                                    _Where = item.Replace("_searchFieldStrEnd", "") + " <= '" + (request.QueryString[item].ToString()) + "' ";
                                }
                                else
                                {
                                    _Where += " AND " + item.Replace("_searchFieldStrEnd", "") + " <= '" + (request.QueryString[item].ToString()) + "' ";
                                }
                            } else if (item.Contains("_searchFieldStrInc")) {
                                var seperator = "";
                                if (String.IsNullOrEmpty(_Where))
                                {
                                    if (request.QueryString[item].ToString().Contains(";"))
                                        seperator = ";";
                                    else if (request.QueryString[item].ToString().Contains(","))
                                        seperator = ",";

                                    if (!String.IsNullOrEmpty(seperator ))
                                        _Where = item.Replace("_searchFieldStrInc", "") + " IN ('" + request.QueryString[item].ToString().Replace(seperator, "', '") + "') ";
                                    else
                                        _Where = item.Replace("_searchFieldStrInc", "") + " IN ('" + request.QueryString[item].ToString() + "') ";

                                }
                                else
                                {
                                    if (request.QueryString[item].ToString().Contains(";"))
                                        seperator = ";";
                                    else if (request.QueryString[item].ToString().Contains(","))
                                        seperator = ",";
                                     
                                    if (!String.IsNullOrEmpty(seperator ))
                                        _Where += " AND " + item.Replace("_searchFieldStrInc", "") + " IN ('" + request.QueryString[item].ToString().Replace(seperator, "', '") + "') ";
                                    else
                                        _Where += " AND " + item.Replace("_searchFieldStrInc", "") + " IN ('" + request.QueryString[item].ToString() + "') ";
                                }
                            }
                            else
                            {
                                if (String.IsNullOrEmpty(_Where))
                                {
                                    _Where = item.Replace("_searchField", "") + " like '" + request.QueryString[item].ToString() + "' ";
                                }
                                else
                                {
                                    _Where += " AND " + item.Replace("_searchField", "") + " like '" + request.QueryString[item].ToString() + "' ";
                                }
                            }
                    }
                }
            }


            /* 2019-08-02 Tai Le(Thomas): Handle jqFilter */
            var _Filter = Filter.Create(request["filters"] ?? "");

            if (_Filter != null)
            {
                foreach (var rule in _Filter.rules)
                {
                    if (String.IsNullOrEmpty(_Where))
                        _Where = rule.ConvertoSQL();
                    else
                        _Where += " AND " + rule.ConvertoSQL();
                }
            }
            return new GridSettings()
            {
                isSearch = bool.Parse(request["_search"] ?? "false"),
                pageIndex = int.Parse(request["page"] ?? "1"),
                pageSize = int.Parse(request["rows"] ?? "50"),
                sortColumn = request["sidx"] ?? "",
                sortOrder = request["sord"] ?? "asc",
                id = request["id"] ?? "",
                param = request["oper"] ?? "",
                editOper = request["edit"] ?? "",
                addOper = request["add"] ?? "",
                delOper = request["del"] ?? "",
                where = Filter.Create(request["filters"] ?? ""),
                operation = (OPERATION)System.Enum.Parse(typeof(OPERATION), request["oper"] ?? "none"),
                extraWhere = _Where
            };
        }
    }

    [DataContract]
    public class Filter
    {
        [DataMember]
        public string groupOp { get; set; }
        [DataMember]
        public Rule[] rules { get; set; }

        public static Filter Create(string jsonData)
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(Filter));
                System.IO.StringReader reader = new System.IO.StringReader(jsonData);
                System.IO.MemoryStream ms = new System.IO.MemoryStream(Encoding.Unicode.GetBytes(jsonData.Replace("\t", "")));
                return serializer.ReadObject(ms) as Filter;
            }
            catch
            {
                return null;
            }
        }
    }

    [DataContract]
    public class Rule
    {
        //private string _myField;
        string _data;

        [DataMember]
        public string field { get; set; }
        [DataMember]
        public string op { get; set; }
        [DataMember]
        public string data
        {
            get
            {
                return _data.Replace("&", "&amp;")
                            .Replace("\"", "&quot;")
                            .Replace("'", "\'")
                            .Replace("<", "&lt;")
                            .Replace(">", "&gt;");
            }
            set
            {
                _data = value;
            }
        }

        public string ConvertoSQL()
        {
            string strOperator = "";
            switch (this.op)
            {
                case "eq":
                    strOperator = field + " = '" + data + "'  ";
                    break;
                case "ne":
                    strOperator = field + " <> N'" + data + "'  ";
                    break;
                case "lt":
                    strOperator = field + " < N'" + data + "'  ";
                    break;
                case "le":
                    strOperator = field + " <= N'" + data + "'  ";
                    break;
                case "gt":
                    strOperator = field + " > N'" + data + "'  ";
                    break;
                case "ge":
                    strOperator = field + " >= N'" + data + "'  ";
                    break;
                case "bw":
                    strOperator = field + " LIKE N'" + data + "%'  ";
                    break;
                case "bn":
                    strOperator = field + " NOT LIKE N'" + data + "%'  ";
                    break;
                case "ew":
                    strOperator = field + " LIKE N'%" + data + "'  ";
                    break;
                case "en":
                    strOperator = field + " NOT LIKE N'%" + data + "'  ";
                    break;
                case "cn":
                    strOperator = field + " LIKE N'%" + data + "%'  ";
                    break;
                case "nc":
                    strOperator = field + " NOT LIKE N'%" + data + "%'  ";
                    break;
            }
            return " " + strOperator;
        }

    }

}