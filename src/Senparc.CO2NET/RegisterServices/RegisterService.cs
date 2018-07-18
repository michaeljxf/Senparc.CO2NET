﻿/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：RegisterService.cs
    文件功能描述：Senparc.CO2NET SDK 快捷注册流程


    创建标识：Senparc - 20180222

    修改标识：Senparc - 20180517
    修改描述：完善 .net core 注册流程

    修改标识：Senparc - 20180517
    修改描述：v0.1.1 修复 RegisterService.Start() 的 isDebug 设置始终为 true 的问题

    修改标识：Senparc - 20180517
    修改描述：v0.1.9 1、RegisterService 取消 public 的构造函数，统一使用 RegisterService.Start() 初始化
                     2、.net framework 和 .net core 版本统一强制在构造函数中要求提供 SenparcSetting 参数

----------------------------------------------------------------*/


#if NETCOREAPP2_0 || NETCOREAPP2_1
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
#endif

namespace Senparc.CO2NET.RegisterServices
{
    /// <summary>
    /// 快捷注册接口
    /// </summary>
    public interface IRegisterService
    {

    }

    /// <summary>
    /// 快捷注册类，IRegisterService的默认实现
    /// </summary>
    public class RegisterService : IRegisterService
    {
        //private RegisterService() : this(null) { }

        private RegisterService(SenparcSetting senparcSetting)
        {
            //Senparc.CO2NET SDK 配置
            Senparc.CO2NET.Config.SenparcSetting = senparcSetting ?? new SenparcSetting();
        }

#if NETCOREAPP2_0 || NETCOREAPP2_1

        /// <summary>
        /// 单个实例引用全局的 ServiceCollection
        /// </summary>
        public IServiceCollection ServiceCollection => SenparcDI.GlobalServiceCollection;

        /// <summary>
        /// 开始 Senparc.CO2NET SDK 初始化参数流程（.NET Core）
        /// </summary>
        /// <param name="env">IHostingEnvironment，控制台程序可以输入null，</param>
        /// <param name="senparcSetting"></param>
        /// <returns></returns>
        public static RegisterService Start(IHostingEnvironment env, SenparcSetting senparcSetting)
        {

            //提供网站根目录
            if (env != null && env.ContentRootPath != null)
            {
                Senparc.CO2NET.Config.RootDictionaryPath = env.ContentRootPath;
            }

            var register = new RegisterService(senparcSetting);

            //如果不注册此线程，则AccessToken、JsTicket等都无法使用SDK自动储存和管理。
            register.RegisterThreads();//默认把线程注册好

            return register;
        }

#else
        /// <summary>
        /// 开始 Senparc.CO2NET SDK 初始化参数流程
        /// </summary>
        /// <returns></returns>
        public static RegisterService Start(SenparcSetting senparcSetting)
        {
            var register = new RegisterService(senparcSetting);

            //如果不注册此线程，则AccessToken、JsTicket等都无法使用SDK自动储存和管理。
            register.RegisterThreads();//默认把线程注册好

            return register;
        }
#endif
    }
}
