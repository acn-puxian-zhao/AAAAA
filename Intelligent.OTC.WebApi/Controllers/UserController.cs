using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Proxy.Permission;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Intelligent.OTC.WebApi.Controllers
{
    public class UserController : Controller
    {
        public class CheckLogin
        {
            public bool Success;
            public string Message;
        }

        public ActionResult test()
        {
            return GetToken(new UserLoginDto() { UserCode = "admin", Password = "admin" });
        }

        public ActionResult test2()
        {
            string t = string.Empty;
            return LoginToken(t);
        }

        public ActionResult GetToken(UserLoginDto user)
        {
            Helper.Log.Info("GetToken called" );
            CheckLogin result = new CheckLogin();

            if (String.IsNullOrWhiteSpace(user.UserCode))
            {
                result.Success = false;
                result.Message = "User code is required.";
            }
            else if (String.IsNullOrWhiteSpace(user.Password))
            {
                result.Success = false;
                result.Message = "Password is required.";
            }
            else
            {
                var tokenId = "TK" + Guid.NewGuid().ToString().Replace("-", "");
                UserLoginResultDto loginResult = null;
                try
                {
                    HttpClient client = new HttpClient();
                    Task<HttpResponseMessage> res = client.PostAsJsonAsync<UserLoginDto>(ConfigurationManager.AppSettings["Xccelerator"]+ "/api/user", user);
                    Task.WaitAll(res);

                    res.Result.TryGetContentValue<UserLoginResultDto>(out loginResult);

                    var r = res.Result.Content.ReadAsAsync<UserLoginResultDto>();
                    Task.WaitAll(r);
                    loginResult = r.Result;
                    loginResult.UserCode = user.UserCode;
                    loginResult.tokenId = tokenId;
                    SetSession(loginResult, true);
                }
                catch (Exception ex)
                {
                    Helper.Log.Error("Failed to call Xccelerator service method: [CheckLogin].", ex);
                    result.Success = false;
                    result.Message = "Login failed because of CheckLogin service call failed.";
                    return Json(result);
                }

                if (loginResult.Success && !String.IsNullOrWhiteSpace(loginResult.UserName))
                {
                    var loginResultJson = JsonConvert.SerializeObject(loginResult);

                    var filePath = System.Web.HttpContext.Current.Server.MapPath("~/Temp");
                    var filename =  tokenId;
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }
                    System.IO.File.WriteAllText(Path.Combine(filePath, filename), loginResultJson);
                    
                    result.Success = true;
                    result.Message = filename;
                }
                else
                {
                    result.Success = false;
                    result.Message = "Id or password incorrect.";
                }
            }

            Helper.Log.Info("GetToken called complete");

            return Json(result);
        }
        
        public ActionResult LoginToken(string token,string redirect = "0")
        {
            Helper.Log.Info("LoginToken called, token: "+token);
            Request.Cookies.Remove("Asp.Net_SessionId");
            try
            {
                if (String.IsNullOrWhiteSpace(token))
                {
                    return JavaScript("//0:Token Required");
                }

                var filePath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/Temp"), token);
                //Helper.Log.Info("Geting token file from:" + filePath);

                if ((AppContext.Current.User != null && AppContext.Current.User.tokenId != token))  //!System.IO.File.Exists(filePath) || 
                {
                    return JavaScript("//0:Token not found");
                }

                try
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        var loginResultJson = System.IO.File.ReadAllText(filePath);

                        var loginResult = JsonConvert.DeserializeObject<UserLoginResultDto>(loginResultJson);
                        //PermissionIds取值逻辑: string.format("/{0}/{1}",T_OBJECTS.OBJECT_REFERENCE,T_OPERATIVES.OPERATIVES_ACTION)
                        //Helper.Log.Info(string.Format("Current user retrieved, User:{0}, PermissionIds:{1}", loginResult.UserName, loginResult.PermissionIds.Length));

                        SetSession(loginResult, true);
                        //Helper.Log.Info(string.Format("Current user login successed, User:{0}, PermissionIds:{1}", loginResult.UserName, loginResult.PermissionIds.Length));

                        System.IO.File.Delete(filePath);
                    }
                }
                catch (Exception)
                {
#if DEBUG
                    throw;
#endif
                }
                Helper.Log.Info("LoginToken called complete");

            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw;
            }
            if(redirect=="1")
            {
                return Redirect(ConfigurationManager.AppSettings["OTC"] );
            }
            else
            {
                return JavaScript("");
            }
        }

        public ActionResult Logout()
        {
            if(AppContext.Current.User == null)
            {
                return Redirect(ConfigurationManager.AppSettings["Xccelerator"] + "/User/Logout");
            }
            var currUser = AppContext.Current.User.EID;

            // Clear session in OTC
            Session.Abandon();

            Helper.Log.Info(string.Format("Logout method called, User: [{0}] quit.", currUser));

            // Clear session in Xccelerator
            return Redirect(ConfigurationManager.AppSettings["Xccelerator"] + "/User/Logout");
        }

        /// <summary>
        /// 设置Session数据
        /// </summary>
        /// <param name="loginResult">数据源</param>
        private void SetSession(UserLoginResultDto loginResult, bool isTokenLogin = false)
        {
            AssertUtils.ArgumentNotNull(loginResult, "loginResult");
            AssertUtils.IsTrue(loginResult.Id > 0, "loginResult.Id");
            AssertUtils.ArgumentHasText(loginResult.UserName, "loginResult.UserName");
            AssertUtils.ArgumentHasText(loginResult.UserCode, "loginResult.UserCode");

            // 1, Create SysUser(without permission information) from UserLoginResultDto
            if (loginResult.UserCode.ToLower() != "arrow_tl") {
                loginResult.Permissions = loginResult.Permissions.Replace("alldataforsupervisor", "");
            }
            SysUser user = new SysUser()
            {
                // user Id in OTC system are int32.
                Id = Convert.ToInt32(loginResult.Id),
                PermissionIds = loginResult.PermissionIds,
                ActionPermissions = loginResult.Permissions,
                Name = loginResult.UserName,
                EID = loginResult.UserCode,
                tokenId = loginResult.tokenId
            };

            try
            {
                // 2, update user's orgnization from Xccelerator table
                XcceleratorService xService = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");
    
                T_USER_EMPLOYEE emp = xService.GetUserOrganization(user.EID);
                user.Deal = emp.T_ORG_DEAL?.DEAL_NAME;
                user.DealId = emp.DEAL_ID?.ToString();
                user.Region = emp.T_ORG_REGION?.REGION_NAME;
                user.RegionId = emp.REGION_ID?.ToString();
                user.Center = emp.T_ORG_CENTER?.CENTER_NAME;
                user.CenterId = emp.CENTER_ID?.ToString();
                user.Group = emp.T_ORG_GROUP?.GROUP_NAME;
                user.GroupId = emp.GROUP_ID?.ToString();
                user.Team = emp.T_ORG_TEAM?.TEAM_NAME;
                user.TeamId = emp.TEAM_ID?.ToString();
                user.Email = emp.USER_MAIL;
                user.TimeZone = Helper.GetRegionTimeSheft(user.Deal);
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Retrieve user's orgnization failed for userId: " + user.EID, ex);
                throw;
            }

            Helper.Log.Info("SessionId:" + HttpContext.Session.SessionID + " created for user:" + user.EID);
            Helper.Log.Info("Region:{0}, Center:{1}, Group:{2}, Deal:{3}, Team:{4}" + user.Region, user.Center, user.Group, user.Deal, user.Team);
            Helper.Log.Info("User owned permissions(controller/actionset): " + user.ActionPermissions);

            Helper.Log.Info("************* Set AppContext.Current.User");
            // 3, put user into session
            AppContext.Current.User = user;

            // 4, call Permission service
            IPermissionService service = SpringFactory.GetObjectImpl<IPermissionService>("PermissionService");
            List<UserPermission> permissions = service.GetUserPermissionByUser(user);
            //user.Permissons = permissions;

        }
    }
}