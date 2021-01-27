using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QRCoder;

namespace PKQRGenerator
{
    public class PKQRCodeGenerate
    {
        public string mText { get; set; }
        public string[] marrText { get; set; } 
        public string mSeperator { get; set; }
        public int mPixelsPerModule { get; set; }


        public PKQRCodeGenerate() { }

        public System.Drawing.Bitmap[] TextToQRBitmap()
        {
            if (String.IsNullOrEmpty(mText))
                return null;
            try
            {
                List<System.Drawing.Bitmap> Results = new List<System.Drawing.Bitmap>();

                string[] arrText = new string[] { };

                if (!String.IsNullOrEmpty(mSeperator))
                    arrText = System.Text.RegularExpressions.Regex.Split(mText, mSeperator, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (arrText.Length == 0)
                    arrText[0] = mText;

                for (int I = 0; I < arrText.Length; I++)
                {
                    using (QRCoder.QRCodeGenerator qrCodeGenerator = new QRCoder.QRCodeGenerator())
                    {
                        using (QRCodeData qrCodeData = qrCodeGenerator.CreateQrCode(arrText[I], QRCoder.QRCodeGenerator.ECCLevel.Q))
                        {
                            using (QRCode qrCode = new QRCode(qrCodeData))
                            {
                                int _pixelPerModule = 5;

                                if (mPixelsPerModule > 0)
                                    _pixelPerModule = mPixelsPerModule;
                                Results.Add(qrCode.GetGraphic(_pixelPerModule));
                            }
                        }
                    }
                }
                return Results.ToArray();
            }
            catch
            {
                return null;
            }
        }
        public List<byte[]> TextToQRArrayByte()
        {
            try
            {
                List<byte[]> Results = new List<byte[]>();
                var QRBitmap = this.TextToQRBitmap();

                if (QRBitmap != null)
                {
                    foreach (System.Drawing.Bitmap item in QRBitmap)
                    {
                        Results.Add(BitmapToBytes(item));
                    }
                } 
                return Results;
            }
            catch
            {
                return null;
            }
        }

        public string[] TextToQCStringArrayByte()
        {
            try
            {
                List<string> Results = new List<string>();
                var _TextToQRArrayByte = this.TextToQRArrayByte();

                if (_TextToQRArrayByte != null)
                {
                    if (_TextToQRArrayByte.Count > 0)
                    {
                        foreach (byte[] item in _TextToQRArrayByte)
                        {
                            Results.Add(ArrayByteToString(item));
                        }
                    }
                }
                 
                return Results.ToArray();
            }
            catch
            { return null; }
        }

        private static byte[] BitmapToBytes(System.Drawing.Bitmap img)
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        private static string ArrayByteToString(byte[] pBytes)
        {
            return Convert.ToBase64String(pBytes, 0, pBytes.Length);
        }
    }

}
