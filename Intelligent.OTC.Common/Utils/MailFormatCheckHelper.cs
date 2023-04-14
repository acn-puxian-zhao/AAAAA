using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Intelligent.OTC.Common.Utils
{
    public static class MailFormatCheckHelper
    {
        public static bool checkMailFormat(string strMailAddress) {
            string emailStr = @"([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9_\-])+\.)+([a-zA-Z0-9_])+";
            Regex emailReg = new Regex(emailStr);
            if (emailReg.IsMatch(strMailAddress)) {
                return true;
            }
            return false;
        }
    }
}
