using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using System;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    public class UsersController : ApiController
    {
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("api/user/updatePwd")]
        public string updatePwd(string[] password)
        {
            string oldPwd = password[0];
            string newPwd = password[1];
            string confirmPwd = password[2];
            if (!newPwd.Equals(confirmPwd))
            {
                return "The new password is inconsistent with the confirm password";
            }
            if (oldPwd.Equals(newPwd))
            {
                return "The new password is consistent with the old password";
            }

            XcceleratorService service = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");

            //密码强度判断
            if (!service.TestPattern(newPwd)) {
                return "Invalid new Password.(8-32 bits, must contain letters, numbers, case characters, special characters)";
            }

            return service.updatePwd(oldPwd, newPwd);
        }
    }
}
