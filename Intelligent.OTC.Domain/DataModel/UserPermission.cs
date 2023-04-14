using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OTC.POC.Repository.DataModel.DTO
{
    public class UserPermission
    {
        public int Id { get; set; }
        public string EID { get; set; }
        public string FuncId { get; set; }
        public string FuncName { get; set; }
        public string FuncPage { get; set; }
        public string Parent { get; set; }
        public int Seq { get; set; }
        public string Style { get; set; }
        public string FuncLevel { get; set; }

        public string OnClick { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }


        public List<UserPermission> SubFuncs { get; set; }
    }
}
