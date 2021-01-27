using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PKBugClientTest
{
    [TestClass]
    public class TelegramBugClientTest
    {
        [TestMethod]
        public async Task SendBugAsync_Test()
        {
            var client = PKBugClient.BugClientFactory.CreateBugClient();
            await client.SendBugAsync("Test", "test", "abc", "123");
        }
    }
}
