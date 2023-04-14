using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Proxy.Permission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;


namespace Intelligent.OTC.WebApi.Controllers
{
    //[ODataRoutePrefix("permission")]
    [UserAuthorizeFilter(actionSet: "common")]
    public class PermissionController : ApiController
    {
        //[ODataRoute]
        //[PagingQueryable]
        public IQueryable<UserPermission> Get()
        { 
            Helper.Log.Info("User permissions:" + AppContext.Current.User.PermissionIds);
            Helper.Log.Info("User Action permissions:" + AppContext.Current.User.ActionPermissions);
            
            PermissionService service = SpringFactory.GetObjectImpl<PermissionService>("PermissionService");

            var funcList = service.GetUserPermissionByUser(AppContext.Current.User);

            Helper.Log.Info("System permissions counts(second level) after filter by user permission:" + AppContext.Current.User.PermissionIds);
            foreach (var func in funcList)
            {
                if (func.SubFuncs != null)
                {
                    Helper.Log.Info("permission counts: " + func.SubFuncs.Count + " for " + func.FuncName);
                }
                else
                {
                    Helper.Log.Info("permission counts: " + 0 + " for " + func.FuncName);
                }
            }

            return funcList.AsQueryable();
        }

        [HttpGet]
        public SysUser GetCurrentUser(string dummy)
        {
            return AppContext.Current.User;
        }

        [HttpGet]
        public SysUser Getbyid(int id)
        {
            PermissionService service = SpringFactory.GetObjectImpl<PermissionService>("PermissionService");
            return service.CommonRep.FindBy<SysUser>(id);
        }

        [HttpGet]
        public IQueryable<SysUser> GetTeamUser(string eid, string dummy)
        {
            IPermissionService service = SpringFactory.GetObjectImpl<IPermissionService>("PermissionService");
            List < SysUser> collectorList = service.GetTeamUserByEID(eid);
            return collectorList.AsQueryable<SysUser>();
        }

        [HttpGet]
        public IQueryable<CollectorTeam> Get(string getCollectList)
        {
            PermissionService service = SpringFactory.GetObjectImpl<PermissionService>("PermissionService");
            return service.GetAllCollectors();
        }

        [HttpGet]
        [Route("api/permission/teamusers")]
        public List<SysUserDto> GetTeamUsers()
        {
            PermissionService service = SpringFactory.GetObjectImpl<PermissionService>("PermissionService");
            return service.GetTeamUsers();
        }


        [HttpGet]
        [Route("api/permissionagent")]
        public IEnumerable<T_PermissionAgent> GetList()
        {
            //strDeal = dear;
            PermissionService service = SpringFactory.GetObjectImpl<PermissionService>("PermissionService");
            return service.GetPermissionAgents();
        }

        [HttpPost]
        [Route("api/permissionagent")]
        public void Post(T_PermissionAgent model)
        {
            PermissionService service = SpringFactory.GetObjectImpl<PermissionService>("PermissionService");
            try
            {
                service.AddOrUpdate(model);
            }
            catch (OTCServiceException ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException(ex.Message);
            }
            catch
            {
                Exception ex = new OTCServiceException("Add Or Update Error!");
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

        }

        [HttpDelete]
        [Route("api/permissionagent")]
        public void Delete(int id)
        {
            PermissionService service = SpringFactory.GetObjectImpl<PermissionService>("PermissionService");
            service.Remove(id);
        }
    }
}