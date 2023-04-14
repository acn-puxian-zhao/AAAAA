using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Proxy.Permission;
using Intelligent.OTC.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Xccelerator.Sitemap.Domain;

namespace Intelligent.OTC.Business
{
    public class PermissionService : IPermissionService
    {
        public PermissionService()
        {
        }

        public OTCRepository CommonRep { get; set; }
        public ICacheService CacheSvr { get; set; }
        public XcceleratorService XccService { get; set; }

        public List<UserPermission> GetAllUserPermission()
        {
            List<UserPermission> allPermission = CacheSvr.GetOrSet<List<UserPermission>>("Cache_UserPermission", () =>
            {
                //TODO:
                // get full site map from Xccelerator
                Sitemap sitemap = Sitemap.Initial();

                // convert to user permission structure
                return convertToPermissions(sitemap);
            });

            return allPermission;
        }

        private static List<UserPermission> convertToPermissions(Sitemap siteMap)
        {
            List<UserPermission> res = new List<UserPermission>();
            siteMap.Maps.ForEach(m =>
                res.Add(getPermission(m))
            );

            return res;
        }

        private static UserPermission getPermission(Sitemap map)
        {
            UserPermission p = new UserPermission()
            {
                //FuncId 
                FuncId = Convert.ToString(map.OperativeId),
                Parent = map.MenuId,
                FuncName = map.Title,
                FuncPage = map.Url == null ? "" : map.Url,
                FuncLevel = Convert.ToString(map.Level),
                 Style = map.ClsClass,
                  Icon = map.Icon,
                  Title =map.Title
            };

            if (map.Propertys.Count > 0)
            {
                map.Propertys.ForEach(m =>
                    {
                        if (m.Key == "color")
                        {
                            p.Color = m.Value;
                        }
                        if (m.Key == "onclick")
                        {
                            p.OnClick = m.Value;
                        }
                    });
            }

            if (map.Maps.Count > 0)
            {
                map.Maps.ForEach(m =>
                {
                    var per = getPermission(m);
                    if (per != null)
                    {
                        if (p.SubFuncs == null)
                        {
                            p.SubFuncs = new List<UserPermission>();
                        }
                        p.SubFuncs.Add(per);
                    }
                });
            }
            return p;
        }

        public List<UserPermission> GetUserPermissionByUser(SysUser user)
        {
            List<UserPermission> res = new List<UserPermission>();
            List<UserPermission> clones = new List<UserPermission>();

            var allPermission = GetAllUserPermission();
            allPermission.ForEach(p => 
                {
                    var per = p.Clone() as UserPermission;
                    per.EID = user.EID;
                    per.UserName = user.Name;
                    clones.Add(per);
                });

            bool isDev = false;
            if (Boolean.TryParse(ConfigurationManager.AppSettings["IsDev"] as string, out isDev)
                && isDev)
            {
                return clones;
            }

            // do filter by permission
            long[] ids = user.PermissionIds;
            Helper.Log.Info(ids);

            clones.ForEach(p => 
            {
                res.Add(filterPermission(p, ids, true));
            });

            return res;
        }

        private static UserPermission filterPermission(UserPermission p, long[] ids, bool firstLevel = false)
        {
            bool grant = false;
            long id = 0;
            if (long.TryParse(p.FuncId, out id))
            {
                if (ids.Contains(id) || firstLevel)
                {
                    grant = true;
                }
            }

            if (p.SubFuncs != null && p.SubFuncs.Count > 0)
            {
                List<UserPermission> subs = new List<UserPermission>();
                p.SubFuncs.ForEach(sp =>
                {
                    var sub = filterPermission(sp, ids);
                    if (sub != null)
                    {
                        grant = true;
                        subs.Add(sub);
                    }
                });

                p.SubFuncs = subs;
            }

            if (grant)
            {
                return p;
            }
            else
            {
                return null;
            }
        }

        public SysUser GetSysUserByEID(string collectorEID)
        {
            Exception ex = new NotImplementedException();
            Helper.Log.Error(ex.Message, ex);
            throw ex;
        }

        public List<Collector> GetCollectionTeamMember()
        {
            Exception ex = new NotImplementedException();
            Helper.Log.Error(ex.Message, ex);
            throw ex;
        }

        public List<SysUser> GetTeamUserByEID(string eid)
        {
            Exception ex = new NotImplementedException();
            Helper.Log.Error(ex.Message, ex);
            throw ex;
        }

        public IQueryable<CollectorTeam> GetAllCollectors()
        {
            return CommonRep.GetDbSet<CollectorTeam>().Where(o => o.Deal == AppContext.Current.User.Deal && o.Collector != "exemption");
        }

        public List<SysUserDto> GetTeamUsers()
        {
            return XccService.GetUsers().Select(o => new SysUserDto() { Id = o.Id, EID = o.EID, Name = o.Name }).ToList();
        }

        public List<T_PermissionAgent> GetPermissionAgents()
        {
            string strUserId = AppContext.Current.User.EID;

            return CommonRep.GetDbSet<T_PermissionAgent>().Where(o => o.EId.Equals(strUserId) || o.Agent.Equals(strUserId)).ToList();
        }

        public int AddOrUpdate(T_PermissionAgent permissionAgent)
        {
            try
            {
                var entity = CommonRep.GetQueryable<T_PermissionAgent>().FirstOrDefault(o => o.Id == permissionAgent.Id);
                if (entity == null)
                {
                    permissionAgent.LastUpdateUser = AppContext.Current.User.EID;
                    permissionAgent.LastUpdateTime = AppContext.Current.User.Now;
                    CommonRep.Add(permissionAgent);
                }
                else
                {
                    entity.EId = permissionAgent.EId;
                    entity.Agent = permissionAgent.Agent;
                    permissionAgent.LastUpdateUser = AppContext.Current.User.EID;
                    permissionAgent.LastUpdateTime = AppContext.Current.User.Now;
                }

                CommonRep.Commit();
                return 1;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                return -1;
            }
        }

        public int Remove(int id)
        {
            var entity = CommonRep.GetQueryable<T_PermissionAgent>().FirstOrDefault(o => o.Id == id);
            if (entity != null)
            {
                CommonRep.Remove(entity);
                CommonRep.Commit();
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
