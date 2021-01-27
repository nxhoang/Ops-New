using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKBugClient
{
    public class TelegramBugClient : IBugClient
    {
        private readonly string _telegramApiKey = "733502191:AAE6DRzR-Pfp70cvOrdONN4rE1IflAhg4mM";
        private readonly long _groupChatId = -1001381381816;
        public TelegramBugClient()
        {
        }
        public async Task SendBugAsync(string applicationName, string functionName, string message, string detail = "")
        {
            try
            {
                var client = new Telegram.Bot.TelegramBotClient(_telegramApiKey);

                var text = BuildMessage(applicationName, functionName, message, detail);
                if (text.Length > 4000)
                    text = text.Substring(0, 4000);

                await client.SendTextMessageAsync(_groupChatId, text, Telegram.Bot.Types.Enums.ParseMode.Default, true, false);
            }
            catch (Exception)
            {
                //avoid throw exception
                //throw;
            }
        }

        private string BuildMessage(string applicationName, string functionName, string message, string detail = "")
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($@"👉 {applicationName} has new log");
            builder.AppendLine($@"{functionName} operate on machine {System.Environment.MachineName} with:");
            builder.AppendLine("- - - - - - - - - - - - - - - - - - - - ");
            builder.AppendLine(message);
            if(string.IsNullOrWhiteSpace(detail) == false)
            {
                builder.AppendLine("- - - - - - - - - - - - - - - - - - - - ");
                builder.AppendLine(detail);
            }

            return builder.ToString();
        }
    }
}
