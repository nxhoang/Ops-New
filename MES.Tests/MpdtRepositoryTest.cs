using System;
using System.Threading.Tasks;
using MES.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MES.Tests
{
    [TestClass]
    public class MpdtRepositoryTest
    {
        [TestMethod]
        public async Task GetMesPackagesByDateAsync_Test()
        {
            var repo = new MES.Repositories.MpdtRepository();

            var result = await repo.GetMesPackagesByDateAsync("PKI3", new DateTime(2015, 7, 8));

            Assert.IsNotNull(result);
        }


        [TestMethod]
        public async Task GetLineDashBoardDtoAsync_Test()
        {
            var repo = new MES.Repositories.MpdtRepository();

            var result = await repo.GetLineDashBoardDtoAsync("M_LFM-0034_2_LFM0147RGL001001_01_03");

            Assert.IsNotNull(result);
        }
    }
}
