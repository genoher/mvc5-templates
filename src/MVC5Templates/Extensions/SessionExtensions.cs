using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace DuetGroup.WebsiteUtils.Extensions
{
    public static class SessionExtensions
    {
        private static object _sync = new object();

        public static T GetOrStore<T>(this HttpSessionStateBase session, string key, Func<T> generator)
        {
            return session.GetOrStore(key, (session[key] == null && generator != null) ? generator() : default(T));
        }

        public static T GetOrStore<T>(this HttpSessionStateBase session, string key, T obj)
        {
            var result = session[key];
            if (result == null)
            {
                lock (_sync)
                {
                    if (result == null)
                    {
                        result = obj != null ? obj : default(T);
                        session[key] = obj;
                    }
                }
            }

            return (T)result;
        }

        public static T Get<T>(this HttpSessionState session, string key)
        {
            var result = session[key] ?? default(T);
            return (T)result;
        }
    }
}
