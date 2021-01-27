using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace OPS_DAL.DAL
{
    /// <summary>
    /// Initialize list of MySql parameters
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    /// <seealso cref="List{MySqlParameter}" />
    public class MySqlParamsBase : List<MySqlParameter>
    {
        public MySqlParamsBase(object styleCode, object styleSize, object styleColorSerial, object revNo)
        {
            var mySqlParams = new List<MySqlParameter>
            {
                new MySqlParameter("P_STYLECODE", styleCode),
                new MySqlParameter("P_STYLESIZE", styleSize),
                new MySqlParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new MySqlParameter("P_REVNO", revNo)
            };

            AddRange(mySqlParams);
        }
    }

    /// <summary>
    /// Initialize list of MySql parameters for part of ops layout
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    /// <seealso cref="List{MySqlParameter}" />
    public class OpsMySqlParams : MySqlParamsBase
    {
        public OpsMySqlParams(object styleCode, object styleSize, object styleColorSerial, object revNo) :
            base(styleCode, styleSize, styleColorSerial, revNo)
        { }

        public OpsMySqlParams(object styleCode, object styleSize, object styleColorSerial, object revNo, 
            object opRevNo) : base(styleCode, styleSize, styleColorSerial, revNo)
        {
            Add(new MySqlParameter("P_OPREVNO", opRevNo));
        }

        public OpsMySqlParams(object edition, object styleCode, object styleSize, object styleColorSerial, 
            object revNo, object opRevNo) : base(styleCode, styleSize, styleColorSerial, revNo)
        {
            Insert(0, new MySqlParameter("P_EDITION", edition));
            Add(new MySqlParameter("P_OPREVNO", opRevNo));
        }

        public OpsMySqlParams(object edition, object styleCode, object styleSize, object styleColorSerial,
            object revNo, object opRevNo, object opSerial) : base(styleCode, styleSize, styleColorSerial, revNo)
        {
            Insert(0, new MySqlParameter("P_EDITION", edition));
            Add(new MySqlParameter("P_OPREVNO", opRevNo));
            Add(new MySqlParameter("P_OPSERIAL", opSerial));
        }
    }
}
