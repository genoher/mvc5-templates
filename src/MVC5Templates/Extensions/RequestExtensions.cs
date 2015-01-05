using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DuetGroup.WebsiteUtils.Extensions
{
    public static class RequestExtensions
    {
        public static string GetUrlRoot(this HttpRequestBase request)
        {
            return request.Url.GetLeftPart(UriPartial.Authority);
        }
    }
}
