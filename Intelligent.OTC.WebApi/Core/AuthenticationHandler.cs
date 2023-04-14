using Common.Logging;
using Intelligent.OTC.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Web.Mvc;

namespace Intelligent.OTC.WebApi.Core
{
    /// <summary>  
    ///RarFileHandler 的摘要说明  
    /// </summary>  
    public class AuthenticationHandler : IHttpHandler, IReadOnlySessionState
    {
        private static ILog loger = LogManager.GetLogger("ServiceLogger");
        private static string isDev = ConfigurationManager.AppSettings["IsDev"];
        public AuthenticationHandler()
        {
            //  
            //TODO: 在此处添加构造函数逻辑  
            //  
        }

        #region IHttpHandler 成员  

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var fileName = context.Request.RawUrl;
            int pos = fileName.IndexOf("?");
            if (pos >= 0)
            {
                fileName = context.Request.RawUrl.Substring(0, pos);
            }
            var physicalName = context.Server.MapPath(fileName);
            string baseHref = new UrlHelper(context.Request.RequestContext).Content("~");
            string fileUrl = fileName.Substring(baseHref.Length -1);
            bool authentication = false;
            if (isDev=="true" || whitelistUrl.Exists(q => string.Equals(q, fileUrl, StringComparison.CurrentCultureIgnoreCase)))
            {
                authentication = true;
            }
            else
            {
                var session = context.Session;
                if (session != null && AppContext.Current != null && AppContext.Current.User != null)//已登录
                {
                    //loger.Debug("Authorized");
                    authentication = true;
                }
            }
            if (!authentication ||  !File.Exists(physicalName))
            {
                loger.Debug(string.Format("Unauthorized,Raw Url:{0} Base Href:{1} File Url:{2}", context.Request.RawUrl, baseHref, fileUrl));
                context.Response.StatusCode = 404;
                return;
            }
            context.Response.ContentType = MimeMapping.GetMimeMapping(Path.GetFileName(physicalName));
            context.Response.WriteFile(physicalName);
        }

        #endregion
        /// <summary>
        /// 白名单,不控制权限
        /// </summary>
        public static List<string> whitelistUrl = new List<string>()
        {
            "/favicon.ico",
            "/Error/404.html",
            "/Content/images/error_404.jpg",
        };
    }
}