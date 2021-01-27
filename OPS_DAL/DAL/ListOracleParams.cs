using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;

namespace OPS_DAL.DAL
{
    /// <summary>
    /// Initialize list of oracle parameters
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    /// <seealso cref="System.Collections.Generic.List{OracleParameter}" />
    public class OracleParamsBase : List<OracleParameter>
    {
        public OracleParamsBase(object styleCode, object styleSize, object styleColorSerial, object revNo)
        {
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_STYLESIZE", styleSize),
                new OracleParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new OracleParameter("P_REVNO", revNo)
            };

            AddRange(oracleParams);
        }
    }

    /// <summary>
    /// Initialize list of oracle parameters for part of ops layout
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    /// <seealso cref="List{OracleParameter}" />
    public class OpsOracleParams : OracleParamsBase
    {
        public OpsOracleParams(object styleCode, object styleSize, object styleColorSerial, object revNo) :
            base(styleCode, styleSize, styleColorSerial, revNo)
        { }

        public OpsOracleParams(object styleCode, object styleSize, object styleColorSerial, object revNo, 
            object opRevNo) : base(styleCode, styleSize, styleColorSerial, revNo)
        {
            Add(new OracleParameter("P_OPREVNO", opRevNo));
        }

        public OpsOracleParams(object edition, object styleCode, object styleSize, object styleColorSerial, 
            object revNo, object opRevNo) : base(styleCode, styleSize, styleColorSerial, revNo)
        {
            Insert(0, new OracleParameter("P_EDITION", edition));
            Add(new OracleParameter("P_OPREVNO", opRevNo));
        }

        public OpsOracleParams(object edition, object styleCode, object styleSize, object styleColorSerial,
            object revNo, object opRevNo, object opSerial) : base(styleCode, styleSize, styleColorSerial, revNo)
        {
            Insert(0, new OracleParameter("P_EDITION", edition));
            Add(new OracleParameter("P_OPREVNO", opRevNo));
            Add(new OracleParameter("P_OPSERIAL", opSerial));
        }

        public void AddCursor()
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            Add(cursor);
        }
    }

    public class OpsParams : OracleParamsBase
    {
        public OpsParams(object styleCode, object styleSize, object styleColorSerial, object revNo) : base(styleCode, styleSize, styleColorSerial, revNo)
        {
        }
        public OpsParams(object styleCode, object styleSize, object styleColorSerial, object revNo, object opRevNo) : base(styleCode, styleSize, styleColorSerial, revNo)
        {
            Add(new OracleParameter("P_OPREVNO", opRevNo));
        }
        public OpsParams(object styleCode, object styleSize, object styleColorSerial,
            object revNo, object opRevNo, int opSerial) : base(styleCode, styleSize, styleColorSerial, revNo)
        {
            Add(new OracleParameter("P_OPREVNO", opRevNo));
            Add(new OracleParameter("P_OPSERIAL", opSerial));
        }
        public OpsParams(object styleCode, object styleSize, object styleColorSerial,
            object revNo, object opRevNo, int opSerial, object itemcode) : base(styleCode, styleSize, styleColorSerial, revNo)
        {
            Add(new OracleParameter("P_OPREVNO", opRevNo));
            Add(new OracleParameter("P_OPSERIAL", opSerial));
            Add(new OracleParameter("P_ITEMCODE", itemcode));
        }
        public void ReplacePbyEmpty()
        {
            foreach (var item in this)
            {
                item.ParameterName = item.ParameterName.Replace("P_", string.Empty);
            }
        }
        public void AddCusor()
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            Add(cursor);
        }

    }
}
