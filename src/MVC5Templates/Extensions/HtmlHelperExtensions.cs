using MVC5Templates.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DuetGroup.WebsiteUtils.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static User GetCurrentUser<TModel>(this HtmlHelper<TModel> htmlHelper)
        {
            var u = HttpContext.Current.Session.Get<User>("currentUser");
            return u ?? new User { FullName = "Unknown" };
        }
    }
}
