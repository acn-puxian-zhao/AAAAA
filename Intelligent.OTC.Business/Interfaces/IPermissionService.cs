using System;
using System.Collections.Generic;
using Intelligent.OTC.Domain.DataModel;
using System.Linq;
using Intelligent.OTC.Domain.Repositories;
using Intelligent.OTC.Common.UnitOfWork;
using Intelligent.OTC.Domain.Proxy.Permission;
using Intelligent.OTC.Common;
namespace Intelligent.OTC.Business
{
    public interface IPermissionService
    {
        List<UserPermission> GetUserPermissionByUser(SysUser user);

        SysUser GetSysUserByEID(string collectorEID);

        List<Collector> GetCollectionTeamMember();

        List<SysUser> GetTeamUserByEID(string eid);
    }
}
