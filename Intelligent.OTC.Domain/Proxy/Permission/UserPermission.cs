using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intelligent.OTC.Domain.Proxy.Permission
{
    public class UserPermission : ICloneable
    {
        public int Id { get; set; }
        public string EID { get; set; }
        public string UserName { get; set; }
        public string FuncId { get; set; }
        public string FuncName { get; set; }
        public string FuncPage { get; set; }
        public string Parent { get; set; }
        public int Seq { get; set; }
        public string Style { get; set; }
        public string FuncLevel { get; set; }
        public string Title { get; set; }

        public string OnClick { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }

        public List<UserPermission> SubFuncs { get; set; }

        public object Clone()
        {
            var newUserPermission = new UserPermission()
            {
                Id = this.Id,
                EID = this.EID,
                UserName = this.UserName,
                FuncId = this.FuncId,
                FuncName = this.FuncName,
                FuncPage = this.FuncPage,
                Parent = this.Parent,
                Seq = this.Seq,
                FuncLevel = this.FuncLevel,
                Title = this.Title,
                OnClick = this.OnClick,
                Color = this.Color,
                Icon = this.Icon,
                Style = this.Style
            };

            List<UserPermission> subfuncs = new List<UserPermission>();
            if (this.SubFuncs != null && this.SubFuncs.Count> 0)
            {
                this.SubFuncs.ForEach(sf => subfuncs.Add(sf.Clone() as UserPermission));
            }
            newUserPermission.SubFuncs = subfuncs;            

            return newUserPermission;
        }
    }
}
