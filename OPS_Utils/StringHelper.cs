using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_Utils
{
    public class StringHelper
    {
        public static Stream ConvertBase64ToStream(string base64Text)
        {
            return new MemoryStream(ConvertBase64ToByteArrays(base64Text));
        }

        public static byte[] ConvertBase64ToByteArrays(string base64Text)
        {
            return Convert.FromBase64String(base64Text);
        }
    }
}
