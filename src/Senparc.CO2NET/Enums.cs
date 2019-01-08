﻿#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2018 Jeffrey Su & Suzhou Senparc Network Technology Co.,Ltd.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
either express or implied. See the License for the specific language governing permissions
and limitations under the License.

Detail: https://github.com/Senparc/Senparc.CO2NET/blob/master/LICENSE

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc
  
    文件名：Enums.cs
    文件功能描述：枚举配置文件
    
    
    创建标识：Senparc - 20180602
 
----------------------------------------------------------------*/

namespace Senparc.CO2NET
{

    /// <summary>
    /// 缓存类型
    /// </summary>
    public enum CacheType
    {
        /// <summary>
        /// 本地运行时缓存（单机）
        /// </summary>
        Local,
        /// <summary>
        /// Redis缓存（支持分布式）
        /// </summary>
        Redis,
        /// <summary>
        /// Memcached（支持分布式）
        /// </summary>
        Memcached
    }

    /// <summary>
    /// 用户信息中的性别（sex）
    /// </summary>
    public enum Sex
    {
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释   
        未知 = 0,
        男 = 1,
        女 = 2,
        其他 = 3
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
    }

}
