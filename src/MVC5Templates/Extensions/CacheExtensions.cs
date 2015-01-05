using System;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DuetGroup.WebsiteUtils
{
    public static class CacheExtensions
    {
        private static object _sync = new object();
        public const int DefaultCacheExpiration = 20;

        /// <summary>
        /// Allows Caching of typed data
        /// </summary>
        /// <example><![CDATA[
        /// var user = HttpRuntime
        ///   .Cache
        ///   .GetOrStore<User>(
        ///      string.Format("User{0}", _userId), 
        ///      () => Repository.GetUser(_userId));
        ///
        /// ]]></example>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache">calling object</param>
        /// <param name="key">Cache key</param>
        /// <param name="generator">Func that returns the object to store in cache</param>
        /// <returns></returns>
        /// <remarks>Uses a default cache expiration period as defined in <see cref="CacheExtensions.DefaultCacheExpiration"/></remarks>
        public static T GetOrStore<T>(this Cache cache, string key, Func<T> generator)
        {
            return cache.GetOrStore(key, (cache[key] == null && generator != null) ? generator() : default(T), DefaultCacheExpiration);
        }


        /// <summary>
        /// Allows Caching of typed data
        /// </summary>
        /// <example><![CDATA[
        /// var user = HttpRuntime
        ///   .Cache
        ///   .GetOrStore<User>(
        ///      string.Format("User{0}", _userId), 
        ///      () => Repository.GetUser(_userId));
        ///
        /// ]]></example>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache">calling object</param>
        /// <param name="key">Cache key</param>
        /// <param name="generator">Func that returns the object to store in cache</param>
        /// <param name="expireInMinutes">Time to expire cache in minutes</param>
        /// <returns></returns>
        public static T GetOrStore<T>(this Cache cache, string key, Func<T> generator, double expireInMinutes)
        {
            return cache.GetOrStore(key, (cache[key] == null && generator != null) ? generator() : default(T), expireInMinutes);
        }


        /// <summary>
        /// Allows Caching of typed data
        /// </summary>
        /// <example><![CDATA[
        /// var user = HttpRuntime
        ///   .Cache
        ///   .GetOrStore<User>(
        ///      string.Format("User{0}", _userId),_userId));
        ///
        /// ]]></example>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache">calling object</param>
        /// <param name="key">Cache key</param>
        /// <param name="obj">Object to store in cache</param>
        /// <returns></returns>
        /// <remarks>Uses a default cache expiration period as defined in <see cref="CacheExtensions.DefaultCacheExpiration"/></remarks>
        public static T GetOrStore<T>(this Cache cache, string key, T obj)
        {
            return cache.GetOrStore(key, obj, DefaultCacheExpiration);
        }

        /// <summary>
        /// Allows Caching of typed data
        /// </summary>
        /// <example><![CDATA[
        /// var user = HttpRuntime
        ///   .Cache
        ///   .GetOrStore<User>(
        ///      string.Format("User{0}", _userId), 
        ///      () => Repository.GetUser(_userId));
        ///
        /// ]]></example>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache">calling object</param>
        /// <param name="key">Cache key</param>
        /// <param name="obj">Object to store in cache</param>
        /// <param name="expireInMinutes">Time to expire cache in minutes</param>
        /// <returns></returns>
        public static T GetOrStore<T>(this Cache cache, string key, T obj, double expireInMinutes)
        {
            var result = cache[key];

            if (result == null)
            {

                lock (_sync)
                {
                    if (result == null)
                    {
                        result = obj != null ? obj : default(T);
                        cache.Insert(key, result, null, DateTime.Now.AddMinutes(expireInMinutes), Cache.NoSlidingExpiration);
                    }
                }
            }

            return (T)result;

        }

        public static List<CacheItem> GetAll(this Cache cache)
        {
            var idEnum = cache.GetEnumerator();
            var cacheItems = new List<CacheItem>();

            while (idEnum.MoveNext())
            {
                var key = idEnum.Key.ToString();
                if (!key.StartsWith("__") && !key.StartsWith("System.Web.Optimization") && !key.StartsWith(":ViewCacheEntry:"))
                {
                    var obj = cache.GetType().GetMethod("Get", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(cache, new object[] { key, 1 });
                    var prop = obj.GetType().GetProperty("UtcExpires", BindingFlags.NonPublic | BindingFlags.Instance);
                    var expire = (DateTime)prop.GetValue(obj, null);

                    long objectSize = -1;
                    var cacheObject = cache.Get(key);
                    if (cacheObject != null)
                    {
                        try
                        {
                            var ms = new MemoryStream();
                            var bf = new BinaryFormatter();
                            bf.Serialize(ms, cacheObject);
                            objectSize = ms.Position;
                            ms.Close();
                        }
                        catch
                        {
                        }
                    }
                    cacheItems.Add(new CacheItem { ID = key, Expires = expire, Size = objectSize });
                }
            }
            return cacheItems.OrderByDescending(ob => ob.Size).ToList();
        }

        public static void ClearAll(this Cache cache)
        {
            var e = cache.GetEnumerator();
            var cacheIds = new List<string>();

            while (e.MoveNext())
                cacheIds.Add(e.Key.ToString());

            foreach (var id in cacheIds)
                cache.Remove(id);
        }
    }

    public class CacheItem
    {
        public string ID { get; set; }
        public DateTime Expires { get; set; }
        public long Size { get; set; }
    }
}