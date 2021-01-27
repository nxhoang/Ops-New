using MES.Repositories;
using OPS_DAL.MesEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MES.Controllers
{
    [SessionTimeout]
    public class ApiOracleLineController : ApiController
    {
        private static readonly ILineRepository LineRepository = new LineRepository();
        private static readonly ITableSpaceRepository TableSpaceRep = new TableSpaceRepository();
        
        public IEnumerable<LineEntity> GetMesLinesByFactory(string fac)
        {
            var tbsps = TableSpaceRep.OracleGet(fac);
            var sumTbsp = from t in tbsps
                          group t by new { t.LineSerial, t.Factory }
                into g
                          select new LineEntity { LineSerial = g.Key.LineSerial, TotalTables = g.Count() };

            var amtopLines = LineRepository.GetMtopLinesByFactory(fac);
            var mesLines = LineRepository.GetMesLinesByFactory(fac);

            var l = from me in mesLines
                    join mt in amtopLines on me.LineNo equals mt.LineNo into temp
                    from t in temp.DefaultIfEmpty()
                    select new LineEntity
                    {
                        LineSerial = me.LineSerial,
                        LineName = me.LineName,
                        LineNo = t?.LineNo,
                        LineMan = t?.LineMan,
                        BackgroundColor = me.BackgroundColor
                    };
            var q = from ml in l
                    join t in sumTbsp on ml.LineSerial equals t.LineSerial into temp
                    from tp in temp.DefaultIfEmpty()
                    select new LineEntity
                    {
                        LineSerial = ml.LineSerial,
                        LineName = ml.LineName,
                        LineNo = ml.LineNo,
                        LineMan = ml.LineMan,
                        TotalTables = tp?.TotalTables,
                        BackgroundColor = ml.BackgroundColor
                    };

            return q;
        }

        public HttpResponseMessage PostLineEntity(LineEntity item)
        {
            if (item.Factory == null || item.LineName == null || item.BackgroundColor == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }
            item.InUse = "1";
            item = LineRepository.OracleAdd(item);

            var response = Request.CreateResponse(HttpStatusCode.Created, item);

            string uri = Url.Link("DefaultApi", new { id = item.LineSerial });
            response.Headers.Location = new Uri(uri);
            return response;
        }
    }
}
