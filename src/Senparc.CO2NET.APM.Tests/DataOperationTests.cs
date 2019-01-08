using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Tests;
using System;
using Moq;

namespace Senparc.CO2NET.APM.Tests
{
    [TestClass]
    public class DataOperationTests : BaseTest
    {
    public    const string domainPrefix = "CO2NET_Test_";

        public DataOperationTests()
        {

        }

        private void BuildTestData(DataOperation dataOperation)
        {
            dataOperation.Set("CPU", .65, dateTime: SystemTime.Now.AddMinutes(-2));
            dataOperation.Set("CPU", .78, dateTime: SystemTime.Now.AddMinutes(-2));
            dataOperation.Set("CPU", .75, dateTime: SystemTime.Now.AddMinutes(-2));
            dataOperation.Set("CPU", .92, dateTime: SystemTime.Now.AddMinutes(-1));
            dataOperation.Set("CPU", .48, dateTime: SystemTime.Now.AddMinutes(-1));

            dataOperation.Set("������", 1, dateTime: SystemTime.Now.AddMinutes(-3));
            dataOperation.Set("������", 1, dateTime: SystemTime.Now.AddMinutes(-3));
            dataOperation.Set("������", 1, dateTime: SystemTime.Now.AddMinutes(-2));
            dataOperation.Set("������", 1, dateTime: SystemTime.Now.AddMinutes(-2));
            dataOperation.Set("������", 1, dateTime: SystemTime.Now.AddMinutes(-1));
            dataOperation.Set("������", 1, dateTime: SystemTime.Now.AddMinutes(-1));

            dataOperation.Set("������", 1, dateTime: SystemTime.Now);//��ǰ���ӣ��������ռ�

        }

        [TestMethod]
        public void SetAndGetTest()
        {
            DataOperation dataOperation = new DataOperation(domainPrefix+ "SetAndGetTest");
            BuildTestData(dataOperation);

            var cpuData = dataOperation.GetDataItemList("CPU");
            Assert.AreEqual(5, cpuData.Count);

            var viewData = dataOperation.GetDataItemList("������");
            Assert.AreEqual(7, viewData.Count);
        }

     
        [TestMethod]
        public void ReadAndCleanDataItemsTest()
        {
            DataOperation dataOperation = new DataOperation(domainPrefix + "ReadAndCleanDataItemsTest");
            BuildTestData(dataOperation);
            var result = dataOperation.ReadAndCleanDataItems();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);//CPU����������������
            Console.WriteLine(result.ToJson());
            Console.WriteLine("===============");

            //������ȡ������Ƿ��Ѿ���յ�ǰ����֮ǰ������
            var cpuData = dataOperation.GetDataItemList("CPU");
            Assert.AreEqual(0, cpuData.Count);

            var viewData = dataOperation.GetDataItemList("������");
            Assert.AreEqual(1, viewData.Count);//��ǰ���ӵĻ��治�ᱻ���

            //ģ�⵱ǰʱ��

        }
    }
}
