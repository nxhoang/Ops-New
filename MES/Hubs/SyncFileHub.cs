using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using OPS_DAL.Business;

namespace MES.Hubs
{
    public class SyncFileHub : Hub
    {
        public void SyncFile(string buyer, string ao, string style, string overwrite, List<string> fileTypeList, List<string> countryList)
        {
            try
            {
                if (string.IsNullOrEmpty(buyer) || string.IsNullOrEmpty(ao))
                {
                    Clients.All.updateProcess("Please select buyer and enter AO number");
                    return;
                }

                var listStyle = FileSdBus.GetFilesByAo(buyer, ao, style, fileTypeList);//AdsmBus.GetListStyleCodeByBuyer(buyer, ao, style);

                //var ftp = FtpInfoBus.GetFtpInfo();
                var i = 1;
                var totalFiles = listStyle.Count;
                foreach (var fileType in listStyle)
                {
                    var percent = ((double)i / totalFiles) * 100;
                    Clients.All.updateProcess((int)percent);
                    System.Threading.Thread.Sleep(500);
                    i++;
                }

                Clients.All.syncFileComplete("Synchronization completed");

            }
            catch (Exception ex)
            {
                Clients.All.syncFileError(ex.Message);
                return ;
            }
        }

        public void Hello()
        {
            Clients.All.hello();
        }
    }
}