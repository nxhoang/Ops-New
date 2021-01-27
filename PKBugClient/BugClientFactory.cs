using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKBugClient
{
    public class BugClientFactory
    {
        public static IBugClient CreateBugClient()
        {
            IBugClient client = new TelegramBugClient();


            return client;
        }
    }
}   
