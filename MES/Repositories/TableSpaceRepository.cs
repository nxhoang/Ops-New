using System;
using OPS_DAL.MesBus;
using OPS_DAL.MesEntities;
using System.Collections.Generic;

namespace MES.Repositories
{
    public class TableSpaceRepository : ITableSpaceRepository
    {
        public TableSpaceEntity OracleAdd(TableSpaceEntity item)
        {
            return TableSpaceBus.OracleInsert(item);
        }

        public TableSpaceEntity MySqlAdd(TableSpaceEntity item)
        {
            return TableSpaceBus.MySqlInsert(item);
        }

        public IEnumerable<TableSpaceEntity> MySqlGet(string factory)
        {
            return TableSpaceBus.MySqlGetByFactory(factory);
        }

        public IEnumerable<TableSpaceEntity> OracleGet(string factory)
        {
            return TableSpaceBus.OracleGetByFactory(factory);
        }
         
        /// <summary>
        /// 2019-08-06 Tai Le (Thomas): Get the Table Space based on Factory & MX Package LineSerial
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public IEnumerable<TableSpaceEntity> MySqlGet(string factory, string lineSerial)
        {
            var _tableSpace = TableSpaceBus.MySqlGetByFactory(factory);
            //return TableSpaceBus.MySqlGetByFactory(factory);
            return _tableSpace.FindAll(x=>x.LineSerial == Convert.ToDecimal(lineSerial));
        }
         
        /// <summary>
        /// 2019-08-06 Tai Le (Thomas): Get the Table Space based on Factory & MX Package LineSerial
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="lineSerial"></param>
        /// <returns></returns>
        public IEnumerable<TableSpaceEntity> OracleGet(string factory, string lineSerial)
        {
            //return TableSpaceBus.OracleGetByFactory(factory);
            var _tableSpace = TableSpaceBus.OracleGetByFactory(factory);
            return _tableSpace.FindAll(x => x.LineSerial == Convert.ToDecimal(lineSerial));
        }

    }
}