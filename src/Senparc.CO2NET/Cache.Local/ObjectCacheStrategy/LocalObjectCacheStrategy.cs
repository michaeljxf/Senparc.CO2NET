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

    文件名：LocalContainerCacheStrategy.cs
    文件功能描述：本地容器缓存。


    创建标识：Senparc - 20160308

    修改标识：Senparc - 20160812
    修改描述：v4.7.4  解决Container无法注册的问题

    修改标识：Senparc - 20170205
    修改描述：v0.2.0 重构分布式锁

    修改标识：Senparc - 20181226
    修改描述：v0.4.3 修改 DateTime 为 DateTimeOffset

 ----------------------------------------------------------------*/


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Exceptions;
#if NET35 || NET40 || NET45
using System.Web;
#else
using Microsoft.Extensions.Caching.Memory;
#endif


namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// 全局静态数据源帮助类
    /// </summary>
    public static class LocalObjectCacheHelper
    {
#if NET35 || NET40 || NET45
        /// <summary>
        /// 所有数据集合的列表
        /// </summary>
        public static System.Web.Caching.Cache LocalObjectCache { get; set; }

        static LocalObjectCacheHelper()
        {
            LocalObjectCache = System.Web.HttpRuntime.Cache;
        }
#else

        private static IMemoryCache _localObjectCache;

        /// <summary>
        /// 所有数据集合的列表
        /// </summary>
        public static IMemoryCache LocalObjectCache
        {
            get
            {
                if (_localObjectCache == null)
                {
                    _localObjectCache = SenparcDI.GetService<IMemoryCache>();

                    if (_localObjectCache == null)
                    {
                        throw new CacheException("IMemoryCache 依赖注入未设置！请在 Startup.cs 中使用 serviceCollection.AddMemoryCache() 进行设置！");
                    }
                }
                return _localObjectCache;
            }
        }

        /// <summary>
        /// .NET Core 的 MemoryCache 不提供遍历所有项目的方法，因此这里做一个储存Key的地方
        /// </summary>
        public static Dictionary<string, DateTimeOffset> Keys { get; set; } = new Dictionary<string, DateTimeOffset>();

        static LocalObjectCacheHelper()
        {

        }

        /// <summary>
        /// 获取储存Keys信息的缓存键
        /// </summary>
        /// <param name="cacheStrategy"></param>
        /// <returns></returns>
        public static string GetKeyStoreKey(BaseCacheStrategy cacheStrategy)
        {
            var keyStoreFinalKey = cacheStrategy.GetFinalKey("CO2NET_KEY_STORE");
            return keyStoreFinalKey;
        }
#endif
    }

    /// <summary>
    /// 本地容器缓存策略
    /// </summary>
    public class LocalObjectCacheStrategy : BaseCacheStrategy, IBaseObjectCacheStrategy
    {
        #region 数据源

#if NET35 || NET40 || NET45
        private System.Web.Caching.Cache _cache = LocalObjectCacheHelper.LocalObjectCache;
#else
        private IMemoryCache _cache = LocalObjectCacheHelper.LocalObjectCache;

#endif

        #endregion

        #region 单例

        ///<summary>
        /// LocalCacheStrategy的构造函数
        ///</summary>
        LocalObjectCacheStrategy()
        {
        }

        //静态LocalCacheStrategy
        public static LocalObjectCacheStrategy Instance
        {
            get
            {
                return Nested.instance;//返回Nested类中的静态成员instance
            }
        }


        class Nested
        {
            static Nested()
            {
            }
            //将instance设为一个初始化的LocalCacheStrategy新实例
            internal static readonly LocalObjectCacheStrategy instance = new LocalObjectCacheStrategy();
        }


        #endregion

        #region IObjectCacheStrategy 成员

        //public IContainerCacheStrategy ContainerCacheStrategy
        //{
        //    get { return LocalContainerCacheStrategy.Instance; }
        //}

        [Obsolete("此方法已过期，请使用 Set(TKey key, TValue value) 方法")]
        public void InsertToCache(string key, object value, TimeSpan? expiry = null)
        {
            Set(key, value, expiry, false);
        }

        public void Set(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            if (key == null || value == null)
            {
                return;
            }

            var finalKey = base.GetFinalKey(key, isFullKey);

#if NET35 || NET40 || NET45
            _cache[finalKey] = value;
#else
            var newKey = !CheckExisted(finalKey, true);

            if (expiry.HasValue)
            {
                _cache.Set(finalKey, value, expiry.Value);
            }
            else
            {
                _cache.Set(finalKey, value);
            }

            //由于MemoryCache不支持遍历Keys，所以需要单独储存
            if (newKey)
            {
                var keyStoreFinalKey = LocalObjectCacheHelper.GetKeyStoreKey(this);
                List<string> keys;
                if (!CheckExisted(keyStoreFinalKey, true))
                {
                    keys = new List<string>();
                }
                else
                {
                    keys = _cache.Get<List<string>>(keyStoreFinalKey);
                }
                keys.Add(finalKey);
                _cache.Set(keyStoreFinalKey, keys);
            }

#endif
        }

        public void RemoveFromCache(string key, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key, isFullKey);
            _cache.Remove(cacheKey);

#if !NET35 && !NET40 && !NET45
            //移除key
            var keyStoreFinalKey = LocalObjectCacheHelper.GetKeyStoreKey(this);
            if (CheckExisted(keyStoreFinalKey, true))
            {
                var keys = _cache.Get<List<string>>(keyStoreFinalKey);
                keys.Remove(cacheKey);
                _cache.Set(keyStoreFinalKey, keys);
            }
#endif

        }

        public object Get(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            if (!CheckExisted(key, isFullKey))
            {
                return null;
                //InsertToCache(key, new ContainerItemCollection());
            }

            var cacheKey = GetFinalKey(key, isFullKey);

#if NET35 || NET40 || NET45
            return _cache[cacheKey];
#else
            return _cache.Get(cacheKey);
#endif
        }

        /// <summary>
        /// 返回指定缓存键的对象，并强制指定类型
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="isFullKey">是否已经是完整的Key，如果不是，则会调用一次GetFinalKey()方法</param>
        /// <returns></returns>
        public T Get<T>(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return default(T);
            }

            if (!CheckExisted(key, isFullKey))
            {
                return default(T);
                //InsertToCache(key, new ContainerItemCollection());
            }

            var cacheKey = GetFinalKey(key, isFullKey);

#if NET35 || NET40 || NET45
            return (T)_cache[cacheKey];
#else
            return _cache.Get<T>(cacheKey);
#endif
        }

        public IDictionary<string, object> GetAll()
        {
            IDictionary<string, object> data = new Dictionary<string, object>();
#if NET35 || NET40 || NET45
            IDictionaryEnumerator cacheEnum = System.Web.HttpRuntime.Cache.GetEnumerator();

            while (cacheEnum.MoveNext())
            {
                data[cacheEnum.Key as string] = cacheEnum.Value;
            }
#else
            //获取所有Key
            var keyStoreFinalKey = LocalObjectCacheHelper.GetKeyStoreKey(this);
            if (CheckExisted(keyStoreFinalKey, true))
            {
                var keys = _cache.Get<List<string>>(keyStoreFinalKey);
                foreach (var key in keys)
                {
                    data[key] = Get(key, true);
                }
            }
#endif
            return data;

        }

        public bool CheckExisted(string key, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key, isFullKey);

#if NET35 || NET40 || NET45
            return _cache.Get(cacheKey) != null;
#else
            return _cache.Get(cacheKey) != null;
#endif
        }

        public long GetCount()
        {
#if NET35 || NET40 || NET45
            return _cache.Count;
#else
            var keyStoreFinalKey = LocalObjectCacheHelper.GetKeyStoreKey(this);
            if (CheckExisted(keyStoreFinalKey, true))
            {
                var keys = _cache.Get<List<string>>(keyStoreFinalKey);
                return keys.Count;
            }
            else
            {
                return 0;
            }
#endif
        }

        public void Update(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            Set(key, value, expiry, isFullKey);
        }

        //public void UpdateContainerBag(string key, object bag, bool isFullKey = false)
        //{
        //    Update(key, bag, isFullKey);
        //}

        #endregion

        #region ICacheLock

        public override ICacheLock BeginCacheLock(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan())
        {
            return new LocalCacheLock(this, resourceName, key, retryCount, retryDelay);
        }

        #endregion

    }
}
