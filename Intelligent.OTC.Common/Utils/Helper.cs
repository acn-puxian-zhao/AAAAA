using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Reflection;
using Intelligent.OTC.Common.Attr;
using System.Collections;
using System.Web;
using Intelligent.OTC.Common.Exceptions;

namespace Intelligent.OTC.Common.Utils
{
    public class Helper
    {
        public static T CodeToEnum<T>(string propStr) 
        {
            foreach (FieldInfo pi in typeof(T).GetFields())
            {
                foreach (Attribute typeAtt in Attribute.GetCustomAttributes(pi))
                {
                    if (typeAtt is EnumCodeAttribute)
                    {
                        if (((EnumCodeAttribute)typeAtt).VariableType == propStr)
                        {
                            return (T)Enum.Parse(typeof(T), pi.Name);
                        }
                    }
                }
            }
            return default(T);
        }

        public static string EnumToCode<T>(T type) 
        {
            string res = string.Empty;
            foreach (FieldInfo pi in type.GetType().GetFields())
            {
                if (pi.Name == (type as Enum).ToString())
                {
                    foreach (Attribute typeAtt in Attribute.GetCustomAttributes(pi))
                    {
                        if (typeAtt is EnumCodeAttribute)
                        {
                            res = ((EnumCodeAttribute)typeAtt).VariableType;
                            break;
                        }

                    }
                }
                if (!string.IsNullOrEmpty(res))
                {
                    break;
                }
            }
            return res;
        }

        /// <summary>
        /// Mapping from region to its time zone.
        /// </summary>
        /// <returns></returns>
        public static DateTime GetRegionNow(string region)
        {
            int sheft = GetRegionTimeSheft(region);
            return DateTime.UtcNow.AddHours(sheft);
        }

        /// <summary>
        /// Get time sheft from region.
        /// </summary>
        /// <param name="region"></param>
        /// <returns>by hour</returns>
        public static int GetRegionTimeSheft(string region)
        {
            //TODO: currently is hard coding, we need a mapping from region to its time zone.
            switch (region)
            {
                case "Avery":
                    return 8;
            }
            return 0;
        }


        public static string GetVirtualFullPathCommon(string fileName)
        {
            HttpRequest request = HttpContext.Current.Request;
            StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
            appUriBuilder.Append(Uri.SchemeDelimiter);
            appUriBuilder.Append(request.Url.Authority);
            if (String.Compare(request.ApplicationPath, @"/") != 0)
            {
                appUriBuilder.Append(request.ApplicationPath);
            }
            string outName = appUriBuilder.ToString();
            outName += (fileName.Trim('~'));
            return outName;
        }

        public static string GetMsgDisplayName(string originMsgDisplayName)
        {
            if (string.IsNullOrWhiteSpace(originMsgDisplayName))
            {
                return originMsgDisplayName;
            }
            else
            {
                var dn = System.Text.Encoding.UTF8.GetBytes(originMsgDisplayName);
                var dn64 = Convert.ToBase64String(dn);
                var dnRes = string.Format("=?utf-8?b?{0}?=", dn64);
                return dnRes;
            }
        }

        public class Distributor
        {
            public string DisplayName { get; set; }
            public string Address { get; set; }
        }

        /// <summary>
        /// Parse the given address and create distributor
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static Distributor AddressToDistributor(string address)
        {
            Distributor dist = new Distributor();
            dist.DisplayName = string.Empty;
            int addressStart = address.IndexOf("<");
            int addressEnd = address.IndexOf(">");

            if (addressStart >= 0 && addressEnd > 0)
            {
                // 1, Display <aaa@bbb.ccc>
                // 2, "Display" <aaa@bbb.ccc>
                // 3, <aaa@bbb.ccc>
                if (addressStart > 0)
                {
                    string display = address.Substring(0, addressStart - 1);
                    dist.DisplayName = display.Trim('"');
                }
                dist.Address = address.Substring(addressStart + 1, addressEnd - addressStart - 1);
            }
            else
            {
                if (addressStart >= 0 || addressEnd > 0)
                {
                    throw new OTCServiceException(string.Format("The given address :{0} do not meet any iligible formats.", address));
                }

                // 4, aaa@bbb.ccc
                dist.Address = address;
            }
            return dist;
        }

        public static DateTime GetDatetimeFromMailInternalTime(long internalTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(internalTime / 1000);
        }

        public static class Log
        {
            static ILog log = LogManager.GetLogger(typeof(Helper));

            /// <summary>
            /// log info message
            /// </summary>
            /// <param name="message">log message</param>
            /// <param name="logProperties">log properties</param>
            /// <param name="exception">log exception</param>
            public static void Info(object message, string requestUrl = null, string tag1 = null, string tag2 = null, string tag3 = null, string tag4 = null)
            {
                if (log.IsInfoEnabled)
                {
                    var logCtx = GetLogContext(requestUrl, tag1, tag2, tag3, tag4);

                    PushLogProperties(logCtx);
                    log.Info(message);
                    PopLogProperties(logCtx);
                }
            }

            public static void Error(object message, Exception ex, string requestUrl = null, string tag1 = null, string tag2 = null, string tag3 = null, string tag4 = null)
            {
                if (log.IsErrorEnabled)
                {
                    var logCtx = GetLogContext(requestUrl, tag1, tag2, tag3, tag4);

                    PushLogProperties(logCtx);
                    log.Error(message, ex);
                    PopLogProperties(logCtx);
                }
            }

            public static void Warn(object message, Exception ex = null, string requestUrl = null, string tag1 = null, string tag2 = null, string tag3 = null, string tag4 = null)
            {
                if (log.IsWarnEnabled)
                {
                    var logCtx = GetLogContext(requestUrl, tag1, tag2, tag3, tag4);

                    PushLogProperties(logCtx);
                    if (ex !=null)
                    {
                        log.Warn(message, ex);
                    }
                    else
                    {
                        log.Warn(message);
                    }
                    PopLogProperties(logCtx);
                }
            }

            /// <summary>
            /// push log properties
            /// </summary>
            /// <param name="logProperties">log properties</param>
            private static void PushLogProperties(object logProperties)
            {
                if (logProperties != null)
                {
                    if (logProperties is IDictionary)
                    {
                        var logProps = logProperties as IDictionary;
                        foreach (object key in logProps.Keys)
                        {
                            var value = logProps[key];
                            if (key != null && value != null)
                            {
                                ThreadContext.Stacks[key.ToString()].Push(value.ToString());
                            }
                        }
                    }
                    else
                    {
                        Type attrType = logProperties.GetType();
                        PropertyInfo[] properties = attrType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        for (int i = 0; i < properties.Length; i++)
                        {
                            object value = properties[i].GetValue(logProperties, null);
                            if (value != null)
                            {
                                ThreadContext.Stacks[properties[i].Name].Push(value.ToString());
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// pop log properties
            /// </summary>
            /// <param name="logProperties">log properties</param>
            private static void PopLogProperties(object logProperties)
            {
                if (logProperties != null)
                {
                    if (logProperties is IDictionary)
                    {
                        var logProps = logProperties as IDictionary;
                        foreach (object key in logProps.Keys)
                        {
                            var value = logProps[key];
                            if (key != null && value != null)
                            {
                                ThreadContext.Stacks[key.ToString()].Pop();
                            }
                        }
                    }
                    else
                    {
                        Type attrType = logProperties.GetType();
                        PropertyInfo[] properties = attrType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        for (int i = properties.Length - 1; i >= 0; i--)
                        {
                            object value = properties[i].GetValue(logProperties, null);
                            if (value != null)
                            {
                                ThreadContext.Stacks[properties[i].Name].Pop();
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Return some basic context information: UserId, PageUrl, APId
            /// </summary>
            /// <param name="pageUrl">pageUrl, if null then use HttpContext.Current.Request.Url</param>
            /// <returns>a string dictionary</returns>
            private static Dictionary<string, string> GetLogContext(string requestUrl = null, string tag1 = null, string tag2 = null, string tag3 = null, string tag4 = null)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

                if (HttpContext.Current != null)
                {
                    //dict.Add("UserId", AppContext.Current.User.EID);
                    if (requestUrl != null)
                    {
                        dict.Add("RequestUrl", requestUrl);
                    }
                    else
                    {
                        if (HttpContext.Current != null && HttpContext.Current.Request != null)
                        {
                            dict.Add("RequestUrl", HttpContext.Current.Request.RawUrl);
                        }
                    }

                    dict.Add("Tag1", tag1 ?? string.Empty);
                    dict.Add("Tag2", tag2 ?? string.Empty);
                    dict.Add("Tag3", tag3 ?? string.Empty);
                    dict.Add("Tag4", tag4 ?? string.Empty);
                }

                return dict;
            }
        }
    }
}
