using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.RegisterServices;
using Senparc.CO2NET.Tests.Trace;
using Senparc.CO2NET.Threads;
using Senparc.CO2NET.Trace;

namespace Senparc.CO2NET.Tests
{
    [TestClass]
    public class RegisterTests : BaseTest
    {
        [TestMethod]
        public void ChangeDefaultCacheNamespaceTest()
        {
            var guid = Guid.NewGuid().ToString();

            Config.DefaultCacheNamespace = guid;
            Assert.AreEqual(guid, Config.DefaultCacheNamespace);

            //���Ի�����ʵ�ʵ�key
            var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            var cacheKey = cache.GetFinalKey("key");
            Console.WriteLine(cacheKey);
            Assert.IsTrue(cacheKey.Contains($":{guid}:"));

            Config.DefaultCacheNamespace = null;
            Assert.AreEqual("DefaultCache", Config.DefaultCacheNamespace);//����Ĭ��ֵ
        }

        [TestMethod]
        public void RegisterThreads()
        {
            var registerService = new RegisterService();
            Register.RegisterThreads(registerService);

            Assert.IsTrue(ThreadUtility.AsynThreadCollection.Count > 0);
        }

        #region RegisterTraceLogTest

        [TestMethod]
        public void RegisterTraceLogTest()
        {
            var registerService = new RegisterService();
            Register.RegisterTraceLog(registerService, RegisterTraceLogAction);
            Assert.IsTrue(registerTraceLogActionRun);
        }

        bool registerTraceLogActionRun = false;

        private void RegisterTraceLogAction()
        {
            registerTraceLogActionRun = true;

            SenparcTrace.SendCustomLog("Testϵͳ��־", "Testϵͳ����");//ֻ��Senparc.Weixin.Config.IsDebug = true���������Ч

            //�Զ�����־��¼�ص�
            SenparcTrace.OnLogFunc = () =>
            {
                //����ÿ�δ���Log����Ҫִ�еĴ���
            };
        }

        #endregion

   }
}
