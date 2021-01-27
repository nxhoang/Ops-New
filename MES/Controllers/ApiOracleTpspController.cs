using MES.Repositories;
using OPS_DAL.MesEntities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MES.Controllers
{
    [SessionTimeout]
    [RoutePrefix("Api/ApiOracleTpsp")]
    public class ApiOracleTpspController : ApiController
    {
        private static readonly TableSpaceRepository TableSpaceRepository = new TableSpaceRepository();

        public IEnumerable<TableSpaceEntity> Get(string factory)
        {
            return TableSpaceRepository.OracleGet(factory);
        }

        public HttpResponseMessage Post([FromBody]TableSpaceEntity item)
        {
            item = TableSpaceRepository.OracleAdd(item);
            var response = Request.CreateResponse(HttpStatusCode.Created, item);

            string uri = Url.Link("DefaultApi", new { id = item.TableId });
            response.Headers.Location = new Uri(uri);
            return response;
        }
    }
}