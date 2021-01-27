using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_Utils
{
    public class ApplicationVersionControl
    {
        public static string BuildVersion() {
            int Year, Month, Day, Hour=0, Minute=0, Second=0;

            Year = 2019;
            Month = 8;
            Day = 20;
            Hour = 12;
            Minute = 52;
            var _version = new DateTime(Year, Month, Day, Hour, Minute, Second).ToString("o");

            return _version;
        }
    }
}
