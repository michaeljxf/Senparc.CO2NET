﻿using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// 用于提供给缓存储存的封装对象，包含了对象类型（Type）信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    sealed internal class CacheWrapper<T>
    {
        public Type Type { get; set; }
        public T Object { get; set; }

        public CacheWrapper(T obj)
        {
            this.Object = obj;
            this.Type = typeof(T);
        }
    }

    /// <summary>
    /// 缓存序列化扩展方法，所以缓存的序列化、反序列化过程必须使用这里的方法统一读写
    /// </summary>
    public static class CacheSerializeExtension
    {
        /// <summary>
        /// 序列化到缓存可用的对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeToCache<T>(this T obj)
        {
            var cacheWarpper = new CacheWrapper<T>(obj);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(cacheWarpper);
            return json;
        }

        /// <summary>
        /// 从缓存对象反序列化到实例
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object DeserializeFromCache(this string value)
        {
            var cacheWarpper = (CacheWrapper<object>)Newtonsoft.Json.JsonConvert.DeserializeObject(value, typeof(CacheWrapper<object>));
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(cacheWarpper.Object.ToJson(), cacheWarpper.Type);
            return obj;
        }

        /// <summary>
        /// 从缓存对象反序列化到实例（效率更高，推荐）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T DeserializeFromCache<T>(this string value)
        {
            var cacheWarpper = (CacheWrapper<T>)Newtonsoft.Json.JsonConvert.DeserializeObject(value, typeof(CacheWrapper<T>));
            return cacheWarpper.Object;
        }
    }
}