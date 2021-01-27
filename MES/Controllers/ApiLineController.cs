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
    public class ApiLineController : ApiController
    {
        private static readonly ILineRepository LineRepository = new LineRepository();
        private static readonly ITableSpaceRepository TableSpaceRep = new TableSpaceRepository();

        public IEnumerable<LineEntity> GetMtopLinesByFactory(string factory)
        {
            return LineRepository.GetMtopLinesByFactory(factory);
        }

        public IEnumerable<LineEntity> GetMesLinesByFactory(string fac)
        {
            var tbsps = TableSpaceRep.MySqlGet(fac);
            var sumTbsp = from t in tbsps
                          group t by new { t.LineSerial, t.Factory }
                into g
                          select new LineEntity { LineSerial = g.Key.LineSerial, TotalTables = g.Count() };

            var mesLines = LineRepository.MySqlGetMesLinesByFactory(fac) ?? new LineEntity[0];
            var q = from ml in mesLines
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
            item = LineRepository.MySqlAdd(item);

            var response = Request.CreateResponse(HttpStatusCode.Created, item);

            string uri = Url.Link("DefaultApi", new { id = item.LineSerial });
            response.Headers.Location = new Uri(uri);
            return response;
        }
    }
}
