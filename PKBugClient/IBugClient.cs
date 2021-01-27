using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKBugClient
{
    public interface IBugClient
    {
        Task SendBugAsync(string applicationName, string functionName, string message, string detail = "");
    }
}
