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
    [RoutePrefix("Api/ApiTableSpace")]
    public class ApiTableSpaceController : ApiController
    {
        private static readonly TableSpaceRepository TableSpaceRepository = new TableSpaceRepository();

        public IEnumerable<TableSpaceEntity> Get(string factory)
        {
            var _TableSpace = TableSpaceRepository.MySqlGet(factory);
            return _TableSpace;
        }


        /// <summary>
        /// 2019-08-06 Tai Le (Thomas): Get the Table Space based on Factory & MX Package LineSerial 
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="lineserial"></param>
        /// <returns></returns>
        public IEnumerable<TableSpaceEntity> Get(string factory, string lineserial)
        {
            var _TableSpace = TableSpaceRepository.MySqlGet(factory, lineserial);
            return _TableSpace;
        }

        public HttpResponseMessage Post([FromBody]TableSpaceEntity item)
        {
            item = TableSpaceRepository.MySqlAdd(item);

            var response = Request.CreateResponse(HttpStatusCode.Created, item);

            string uri = Url.Link("DefaultApi", new { id = item.TableId });
            response.Headers.Location = new Uri(uri);
            return response;
        }
    }
}