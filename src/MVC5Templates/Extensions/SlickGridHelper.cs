using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace DuetGroup.WebsiteUtils.Extensions
{
    public static class SlickGridHtmlHelperExtensions
    {
        public static SlickGridHelper SlickGrid(this HtmlHelper htmlHelper)
        {
            return SlickGridHelper.Instance;
        }
    }

    public class SlickGridHelper
    {
        private static Lazy<SlickGridHelper> _instance = new Lazy<SlickGridHelper>(() => new SlickGridHelper());
        private static Regex _regExSeperateCamelCase = new Regex("([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))");

        private SlickGridHelper()
        {
        }

        public static SlickGridHelper Instance { get { return _instance.Value; } }

        public MvcHtmlString LooseJsonColumnDefnFor<TModel>(object[] addtionalPropertyArray = null, string[] addtionalColumns = null, bool sortableColumns = true)
        {
            var sb = new StringBuilder();

            using (var jw = new JsonTextWriter(new StringWriter(sb)))
            {
                int i = 0;
                jw.WriteStartArray();
                foreach (var prpty in typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    //Check the display attribute. Ignore this property if Display(AUtoGenreatedField = faslse)
                    var dsplyAttr = (DisplayAttribute[])prpty.GetCustomAttributes(typeof(DisplayAttribute), false);

                    if (dsplyAttr.Length == 0 || dsplyAttr.Length > 0 && (dsplyAttr[0].GetAutoGenerateField() ?? true) == true)
                    {
                        var camelCaseName = ToCamelCase(prpty.Name);
                        jw.WriteStartObject();

                        jw.WritePropertyName("id");
                        jw.WriteValue(camelCaseName);

                        jw.WritePropertyName("name");
                        if (dsplyAttr.Length > 0)
                        {
                            //use custom display name
                            jw.WriteValue(dsplyAttr[0].Name);
                        }
                        else
                        {
                            //Seperate the Property name with space between caps
                            jw.WriteValue(_regExSeperateCamelCase.Replace(prpty.Name, "$1 "));
                        }
                        jw.WritePropertyName("field");
                        jw.WriteValue(camelCaseName);

                        //addtional properties
                        bool isSortable = sortableColumns;
                        if (addtionalPropertyArray != null && addtionalPropertyArray.Length > i)
                        {
                            var addtnlPrpty = addtionalPropertyArray[i];
                            foreach (var xtrPrpty in addtnlPrpty.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                            {
                                if (xtrPrpty.Name != "field")
                                {
                                    object val = xtrPrpty.GetValue(addtnlPrpty, null);

                                    if (xtrPrpty.Name == "sortable")
                                    {
                                        if (xtrPrpty.PropertyType != typeof(bool))
                                            throw new Exception("'sortable' attribute must be of type bool");

                                        isSortable = (bool)val;
                                    }
                                    else
                                    {
                                        if (xtrPrpty.PropertyType != typeof(string) || (xtrPrpty.PropertyType == typeof(string) && !String.IsNullOrEmpty((string)val)))
                                        {
                                            jw.WritePropertyName(xtrPrpty.Name);

                                            if ((new string[] { "formatter", "editor", "asyncPostREnder", "validator", "groupTotalsFormatter" }).Any(s => s == xtrPrpty.Name))
                                                jw.WriteRawValue((string)val);
                                            else
                                                jw.WriteValue(Convert.ChangeType(val, xtrPrpty.PropertyType));
                                        }
                                    }
                                }
                            }
                        }

                        //Set Sortable attribute
                        if (isSortable)
                        {
                            jw.WritePropertyName("sortable");
                            jw.WriteValue(true);
                        }

                        //close object
                        jw.WriteEndObject();
                        i++;
                    }
                }

                if (addtionalColumns != null)
                    foreach (var addtionalColumn in addtionalColumns)
                    {
                        var camelCaseName = ToCamelCase(addtionalColumn);
                        var friendlyName = addtionalColumn.Replace('_', ' ');

                        jw.WriteStartObject();

                        jw.WritePropertyName("id");
                        jw.WriteValue(camelCaseName);

                        jw.WritePropertyName("name");
                        jw.WriteValue(friendlyName);

                        if (sortableColumns)
                        {
                            jw.WritePropertyName("sortable");
                            jw.WriteValue(true);
                        }

                        jw.WriteEndObject();
                    }
                jw.WriteEndArray();
            }

            return new MvcHtmlString(sb.ToString());
        }

        public MvcHtmlString JsonGridDataFor<TModel>(IEnumerable<TModel> modelList)
        {
            return JsonGridDataFor(modelList, null);
        }

        public MvcHtmlString JsonGridDataFor<TModel>(IEnumerable<TModel> modelList, string dictionaryToFlatten)
        {
            var html = new MvcHtmlString(
                JsonConvert.SerializeObject(
                    modelList,
                    Formatting.None,
                    new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    }
                )
            );

            if (dictionaryToFlatten != null)
            {
                var camelCaseName = "\"" + dictionaryToFlatten.Substring(0, 1).ToLower() + dictionaryToFlatten.Substring(1) + "\":{";
                var htmlString = html.ToHtmlString();

                var start = htmlString.IndexOf(camelCaseName);
                while (start > 0)
                {
                    htmlString = htmlString.Remove(start, camelCaseName.Length);
                    var end = htmlString.IndexOf("}", start);
                    htmlString = htmlString.Remove(end, 1);
                    start = htmlString.IndexOf(camelCaseName);
                }
                html = new MvcHtmlString(htmlString);
            }

            return html;
        }


        public MvcHtmlString JsonColumnDefnFor<TModel>(SGColOption[] columnOptions)
        {
            if (columnOptions == null || columnOptions.Any(co => String.IsNullOrEmpty(co.field)))
                return LooseJsonColumnDefnFor<TModel>(columnOptions);

            var sb = new StringBuilder();

            using (var jw = new JsonTextWriter(new StringWriter(sb)))
            {
                jw.WriteStartArray();

                //get a dictionary of model properties
                var propertyDict = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                 .ToDictionary(pi => pi.Name);

                foreach (var colOption in columnOptions)
                {
                    //Calcuylate display Name. Use the DisplayAttribute on TModel if it exists
                    string dsplyName;

                    if (propertyDict.ContainsKey(colOption.field))
                    {
                        var prptyInfo = propertyDict[colOption.field];
                        var dsplyAttr = (DisplayAttribute[])prptyInfo.GetCustomAttributes(typeof(DisplayAttribute), false);

                        if (dsplyAttr.Length > 0 && (dsplyAttr[0].GetAutoGenerateField() ?? true) == true)
                        {
                            dsplyName = dsplyAttr[0].Name;
                        }
                        else
                        {
                            dsplyName = _regExSeperateCamelCase.Replace(colOption.field, "$1 ");
                        }
                    }
                    else
                    {
                        dsplyName = _regExSeperateCamelCase.Replace(colOption.field, "$1 ");
                    }

                    //add icon markup
                    if (colOption.bootStrapIcon != null)
                    {
                        dsplyName = String.Format("{0} <span class='glyphicon {1}' title='{2}' style='vertical-align:middle;'></span>", dsplyName, colOption.bootStrapIcon, colOption.bootstrapIconTitle);
                    }

                    var camelCaseName = ToCamelCase(colOption.field);
                    jw.WriteStartObject();

                    jw.WritePropertyName("id");
                    jw.WriteValue(camelCaseName);

                    jw.WritePropertyName("name");
                    jw.WriteValue(dsplyName);

                    jw.WritePropertyName("field");
                    jw.WriteValue(camelCaseName);

                    //Write out optional properties
                    bool isSortable = false;
                    foreach (var prpty in colOption.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (prpty.Name != "field")
                        {
                            object val = prpty.GetValue(colOption, null);

                            if (prpty.Name == "sortable")
                            {
                                if (prpty.PropertyType != typeof(bool))
                                    throw new Exception("'sortable' attribute must be of type bool");

                                isSortable = (bool)val;
                            }
                            else
                            {
                                if (prpty.PropertyType != typeof(string) || (prpty.PropertyType == typeof(string) && !String.IsNullOrEmpty((string)val)))
                                {
                                    jw.WritePropertyName(prpty.Name);

                                    if ((new string[] { "formatter", "editor", "asyncPostRender", "validator", "groupTotalsFormatter" }).Any(s => s == prpty.Name))
                                        jw.WriteRawValue((string)val);
                                    else
                                        jw.WriteValue(Convert.ChangeType(val, prpty.PropertyType));
                                }
                            }
                        }
                    }

                    //Set Sortable attribute
                    if (isSortable)
                    {
                        jw.WritePropertyName("sortable");
                        jw.WriteValue(true);
                    }

                    //close object
                    jw.WriteEndObject();
                }

                jw.WriteEndArray();
            }

            return new MvcHtmlString(sb.ToString());
        }

        public static bool PropExists(dynamic dynamicObject, string name)
        {
            return dynamicObject.GetType().GetProperty(name) != null;
        }

        private string ToCamelCase(string value)
        {
            return value.Substring(0, 1).ToLower() + value.Substring(1);
        }
    }


    public class SGColOption
    {
        public SGColOption()
        {
            bootStrapIcon = null;
            bootstrapIconTitle = String.Empty;
            cssClass = null;
            editor = null;
            field = null;
            formatter = null;
            headerCssClass = null;
            sortable = true;
            width = 120;
        }

        public string bootStrapIcon { get; set; }
        public string bootstrapIconTitle { get; set; }
        public string cssClass { get; set; }
        public string editor { get; set; }
        public string field { get; set; }
        public string formatter { get; set; }
        public string headerCssClass { get; set; }
        public bool sortable { get; set; }
        public int width { get; set; }
    }
}
