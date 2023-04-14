using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using Intelligent.OTC.Common.Exceptions;
using System.Data.Entity;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using System.Text.RegularExpressions;

namespace Intelligent.OTC.Business
{
    public class XcceleratorService
    {
        public XcceleratorRepository XRep { private get; set; }

        public OTCRepository CommonRep { private get; set; }

        public T_USER_EMPLOYEE GetUserOrganization(string userId)
        {
            var empId = (from u in XRep.GetDbSet<T_USERS>()
                         where u.USER_CODE == userId
                         select u.USER_EMPLOYEE_ID).FirstOrDefault();

            if (empId != null)
            {
                var userEmp = XRep.GetDbSet<T_USER_EMPLOYEE>()
                    .Include<T_USER_EMPLOYEE, T_ORG_CENTER>(e => e.T_ORG_CENTER)
                    .Include<T_USER_EMPLOYEE, T_ORG_DEAL>(e => e.T_ORG_DEAL)
                    .Include<T_USER_EMPLOYEE, T_ORG_GROUP>(e => e.T_ORG_GROUP)
                    .Include<T_USER_EMPLOYEE, T_ORG_REGION>(e => e.T_ORG_REGION)
                    .Include<T_USER_EMPLOYEE, T_ORG_TEAM>(e => e.T_ORG_TEAM)
                    .FirstOrDefault(ue => ue.ID == empId);
                if (userEmp != null)
                {
                    return userEmp;
                }
                else
                {
                    throw new OTCServiceException(string.Format("Xccelerator: User employee is not exist for user code: [{0}] employee: [{1}]", userId, empId));
                }
            }
            else
            {
                throw new OTCServiceException(string.Format("Xccelerator: User not exist or user employee is not setup for user code: [{0}]", userId));
            }
        }

        public List<SysUser> GetUsers(int regionId, int centerId, int groupId, int dealId, int teamId,string dealName)
        {
            var orgDeal = XRep.GetDbSet<T_ORG_DEAL>().Where(e =>
                e.DEAL_NAME == dealName).FirstOrDefault();
            return XRep.GetDbSet<T_USERS>().Where(e =>
                e.T_USER_EMPLOYEE.DEAL_ID == orgDeal.ID).Select(u => new SysUser()
                {
                    EID = u.USER_CODE,
                    Name = u.T_USER_EMPLOYEE.USER_NAME,
                    Email = u.T_USER_EMPLOYEE.USER_MAIL,
                    Deal = u.DEAL,
                    DealId = u.T_USER_EMPLOYEE.DEAL_ID.ToString(),
                    GroupId = u.T_USER_EMPLOYEE.GROUP_ID.ToString(),
                    RegionId = u.T_USER_EMPLOYEE.REGION_ID.ToString(),
                    CenterId = u.T_USER_EMPLOYEE.CENTER_ID.ToString(),
                    TeamId = u.T_USER_EMPLOYEE.TEAM_ID.ToString()
                }).ToList();
        }

        public List<SysUser> GetUsers()
        {
            var orgDeal = XRep.GetDbSet<T_ORG_DEAL>().Where(e =>
                e.DEAL_NAME == AppContext.Current.User.Deal).FirstOrDefault();
            return XRep.GetDbSet<T_USERS>().Where(e =>
                e.T_USER_EMPLOYEE.DEAL_ID == orgDeal.ID).Select(u => new SysUser()
                {
                    EID = u.USER_CODE,
                    Name = u.T_USER_EMPLOYEE.USER_NAME,
                    Email = u.T_USER_EMPLOYEE.USER_MAIL,
                    Deal = u.DEAL,
                    DealId = u.T_USER_EMPLOYEE.DEAL_ID.ToString(),
                    GroupId = u.T_USER_EMPLOYEE.GROUP_ID.ToString(),
                    RegionId = u.T_USER_EMPLOYEE.REGION_ID.ToString(),
                    CenterId = u.T_USER_EMPLOYEE.CENTER_ID.ToString(),
                    TeamId = u.T_USER_EMPLOYEE.TEAM_ID.ToString()
                }).ToList();
        }

        /// <summary>
        /// Get all users from collection team.
        /// This method only used by global mail retrival timer.
        /// </summary>
        /// <returns>All collection teams users in all Deal</returns>
        public List<SysUser> GetCollectionTeamMember()
        {
            return XRep.GetDbSet<T_USERS>().Where(e => e.T_USER_EMPLOYEE.T_ORG_TEAM.TEAM_NAME == "Collection" && e.DEAL == "Avery")
                .Select(u => new SysUser()
                {
                    EID = u.USER_CODE,
                    Name = u.T_USER_EMPLOYEE.USER_NAME,
                    Email = u.T_USER_EMPLOYEE.USER_MAIL,
                    Deal = u.DEAL,
                    DealId = u.T_USER_EMPLOYEE.DEAL_ID.ToString()
                }).ToList();
        }
        /// <summary>
        /// 获取当前登录人及其下属
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<SysUser> GetUserTeamList(string userId)
        {
            T_USERS user = XRep.GetDbSet<T_USERS>().FirstOrDefault(s => s.USER_CODE == userId);
            if (user == null)
                return new List<SysUser>();
            List<SysUser> result = new List<SysUser>();
            getmemberList(result, user.USER_EMPLOYEE_ID);
            if (!result.Any(s => s.EID == user.USER_CODE))
            {
                result.Add(new SysUser
                {
                    EID = user.USER_CODE,
                    Name = user.T_USER_EMPLOYEE == null ? "" : user.T_USER_EMPLOYEE.USER_NAME,
                    Email = user.T_USER_EMPLOYEE == null ? "" : user.T_USER_EMPLOYEE.USER_MAIL,
                    Deal = user.DEAL,
                    DealId = user.T_USER_EMPLOYEE == null ? "" : user.T_USER_EMPLOYEE.DEAL_ID.ToString()
                });
            }

            var agentEids = CommonRep.GetQueryable<T_PermissionAgent>().Where(o => o.Agent == userId).Select(o => o.EId).DefaultIfEmpty().ToList();
            if (agentEids != null) { 
                foreach (var eid in agentEids)
                {
                    if (string.IsNullOrEmpty(eid)) { continue; }
                    if (!result.Any(s => s.EID == eid))
                    {
                        T_USERS agentUser = XRep.GetDbSet<T_USERS>().FirstOrDefault(s => s.USER_CODE == eid);
                        result.Add(new SysUser
                        {
                            EID = agentUser.USER_CODE,
                            Name = agentUser.T_USER_EMPLOYEE == null ? "" : agentUser.T_USER_EMPLOYEE.USER_NAME,
                            Email = agentUser.T_USER_EMPLOYEE == null ? "" : agentUser.T_USER_EMPLOYEE.USER_MAIL,
                            Deal = agentUser.DEAL,
                            DealId = agentUser.T_USER_EMPLOYEE == null ? "" : agentUser.T_USER_EMPLOYEE.DEAL_ID.ToString()
                        });
                        List<SysUser> subUsers = GetUserTeamList(eid);
                        foreach (SysUser u in subUsers)
                        {
                            if (!result.Any(s => s.EID == u.EID))
                            {
                                T_USERS agentUsersub = XRep.GetDbSet<T_USERS>().FirstOrDefault(s => s.USER_CODE == u.EID);
                                result.Add(new SysUser
                                {
                                    EID = agentUsersub.USER_CODE,
                                    Name = agentUsersub.T_USER_EMPLOYEE == null ? "" : agentUsersub.T_USER_EMPLOYEE.USER_NAME,
                                    Email = agentUsersub.T_USER_EMPLOYEE == null ? "" : agentUsersub.T_USER_EMPLOYEE.USER_MAIL,
                                    Deal = agentUsersub.DEAL,
                                    DealId = agentUsersub.T_USER_EMPLOYEE == null ? "" : agentUsersub.T_USER_EMPLOYEE.DEAL_ID.ToString()
                                });
                            }
                        }
                    }
                }
            }

            return result;
        }

        private void getmemberList(List<SysUser> list, long? userId)
        {
            var ue = XRep.GetDbSet<T_USER_EMPLOYEE>().Where(s => s.DIRECT_MANAGER_ID == userId);
            if (ue != null && ue.Count() > 0)
            {
                foreach (var u in ue)
                {
                    var user = XRep.GetDbSet<T_USERS>().FirstOrDefault(s => s.USER_EMPLOYEE_ID == u.ID);
                    if (user != null)
                        list.Add(new SysUser
                        {
                            EID = user.USER_CODE,
                            Name = user.T_USER_EMPLOYEE == null ? "" : user.T_USER_EMPLOYEE.USER_NAME,
                            Email = user.T_USER_EMPLOYEE == null ? "" : user.T_USER_EMPLOYEE.USER_MAIL,
                            Deal = user.DEAL,
                            DealId = user.T_USER_EMPLOYEE == null ? "" : user.T_USER_EMPLOYEE.DEAL_ID.ToString()
                        });
                    getmemberList(list, u.ID);
                }
            }
        }

        public string updatePwd(string oldPwd, string newPwd)
        {
            oldPwd = NSH.Core.Cryptogram.MD5(oldPwd);
            string eid = AppContext.Current.User.EID;
            T_USERS user = XRep.GetDbSet<T_USERS>().FirstOrDefault(s => s.USER_CODE == eid && s.USER_PASSWORD == oldPwd);
            if (user == null)
            {
                return "Old Password is wrong!";
            }
            newPwd = NSH.Core.Cryptogram.MD5(newPwd);
            T_USERS old = user;
            user.USER_PASSWORD = newPwd;
            Helper.Log.Info(user);
            ObjectHelper.CopyObjectWithUnNeed(user, old, new string[] { "Id" });
            XRep.Commit();
            return "Update Success!";
        }

        /// <summary>
        /// 包含小写字母
        /// </summary>
        private static readonly string REG_CONTAIN_LOWERCASE_ASSERTION =
            @"(?=.*[a-z])";

        /// <summary>
        /// 包含大写字母
        /// </summary>
        private static readonly string REG_CONTAIN_UPPERCASE_ASSERTION =
            @"(?=.*[A-Z])";

        /// <summary>
        /// 包含数字
        /// </summary>
        private static readonly string REG_CONTAIN_DIGIT_ASSERTION =
            @"(?=.*\d)";

        /// <summary>
        /// 包含特殊字符(https://www.owasp.org/index.php/Password_special_characters)
        /// </summary>
        private static readonly string REG_CONTAIN_SPECIAL_CHAR_ASSERTION =
            @"(?=.*[ !""#$%&'()*+,-./:;<=>?@\[\]\^_`{|}~])";

        public static readonly string PASSWORD_STRENGTH_1 =
            $"{REG_CONTAIN_LOWERCASE_ASSERTION}" +
            $"{REG_CONTAIN_UPPERCASE_ASSERTION}" +
            $"{REG_CONTAIN_DIGIT_ASSERTION}" +
            $"{REG_CONTAIN_SPECIAL_CHAR_ASSERTION}" +
            @"^.{8,32}$";

        /// <summary>
        /// PASSWORD_STRENGTH_1 的另一种写法
        /// </summary>
        public static readonly string PASSWORD_STRENGTH_2 = @"(?=(.*[a-z]))(?=(.*[A-Z]))(?=(.*\d))(?=(.*[ !""#$%&'()*+,-./:;<=>?@\[\]\^_`{|}~]))^.{8,32}$";

        public bool TestPattern(string input)
        {
            Regex regex = new Regex(PASSWORD_STRENGTH_1);
            return regex.IsMatch(input);
        }
    }
}

