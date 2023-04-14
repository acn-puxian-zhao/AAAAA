using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using System;
using System.Configuration;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Intelligent.OTC.Common.Attr
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class UserAuthorizeFilterAttribute : AuthorizationFilterAttribute, IAuthorizationFilter
    {
        public UserAuthorizeFilterAttribute()
        {

        }
        public UserAuthorizeFilterAttribute(string actionSet)
        {
            this.actionSet = actionSet;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.Request.RequestUri.Equals(ConfigurationManager.AppSettings["FilterSkipe"])) { 
                return;
            }
            if (Authorize(actionContext))
            {
                return;
            }
            throw new NoPermissionException();
        }

        private string actionSet { get; set; }

        private bool Authorize(HttpActionContext actionContext)
        {
            // 1, logic to determine if you are authenticated.
            //TODO: separate the authenticate from authorize logics.
            bool isDev = false;
            if (!Boolean.TryParse(ConfigurationManager.AppSettings["IsDev"] as string, out isDev))
            {
                throw new OTCServiceException("Missing config 'IsDev'.");
            }

            if (AppContext.Current == null)
            {
                throw new UserNotLoginException();
            }
            else 
            {
                if (AppContext.Current.User == null)
                {
                    lock (actionSet)
                    {
                        if (AppContext.Current.User == null)
                        {
                            // check if dev then pass though
                            if (isDev)
                            {
                                // simulate login for dev code. 
                                AppContext.Current.User = new SysUser()
                                {
                                    EID = "shuhan.liu",
                                    Name = "OTC dev account",
                                    Email = "cindy.zhu@ap.averydennison.com",
                                    TimeZone = 8,
                                    Deal = "Arrow",
                                    DealId = "1",
                                    Region = "China",
                                    RegionId = "1",
                                    Center = "Dalian",
                                    CenterId = "7",
                                    Group = "OTC",
                                    GroupId = "10",
                                    Team = "Collection",
                                    TeamId = "12",
                                    ActionPermissions = "",

                                };
                            }
                            else
                            {
                                throw new UserNotLoginException();
                            }
                        }
                    }
                }
            }

            // 2, logic to determine if you are authorized.  
            bool hasPermission = true;
            string controller = string.Empty;
            if (actionContext.ControllerContext.RouteData.Values.Count > 0)
            {
                controller = actionContext.ControllerContext.RouteData.Values["controller"].ToString();
            }
            else
            {
                return true;
            }
             
            var apiName = actionContext.ActionDescriptor.ActionName;

            if (!string.IsNullOrEmpty(actionSet))
            {
                // if actionset is provided, Check permission using the action set setting.
                apiName = actionSet;
            }

            Helper.Log.Info("******* AppContext.Current.User.ActionPermissions ****** " + AppContext.Current.User.ActionPermissions);
            
            if (AppContext.Current.User.ActionPermissions == null)
            {
                Helper.Log.Info("******* ActionPermissions is null ****** ");
                hasPermission = false;
            }
            else if (!AppContext.Current.User.ActionPermissions.Contains(String.Format(",/{0}/{1}", controller.ToLower(), apiName.ToLower())))
            {
                // check if current action is assiged by using user permission list.
                Helper.Log.Info("ActionPermissions: " + AppContext.Current.User.ActionPermissions);
                Helper.Log.Info("Controller: " + controller + " ApiName:" + apiName);
                hasPermission = false;
            }
            else
            {
                Helper.Log.Info("Permisson match successed! Controller: " + controller + " ApiName:" + apiName);
            }

            if (isDev)
            {
                // alway give permission in dev environment.
                hasPermission = true;
            }

            return hasPermission;
        }
    }
}
