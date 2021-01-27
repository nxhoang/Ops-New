using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PKERP.MES.Services.Implement.Services;

namespace PKERP.MES.Services.Impltement.Tests
{
    [TestClass]
    public class ConfigServiceTest
    {
        [TestMethod]
        public async Task GetConfigValueAsync_Test()
        {
            var service = new ConfigService();

            var result = await service.GetIntConfigValueAsync("mqtt.port");

            Assert.AreNotEqual(result, 0);
        }
    }
}
