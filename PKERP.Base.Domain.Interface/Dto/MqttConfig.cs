using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKERP.Base.Domain.Interface.Dto
{
    public class MqttConfig
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public int WsPort { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int AutoReconnectDelay { get; set; }//miliseconds
    }
}
