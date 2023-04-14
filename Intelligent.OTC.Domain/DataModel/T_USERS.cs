//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Intelligent.OTC.Domain.DataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class T_USERS
    {
        public long ID { get; set; }
        public string USER_CODE { get; set; }
        public string USER_NAME { get; set; }
        public string USER_PASSWORD { get; set; }
        public Nullable<int> ACCOUNT_FLG { get; set; }
        public Nullable<int> USER_FLG { get; set; }
        public string USER_MAIL { get; set; }
        public string COUNTRY { get; set; }
        public string LOCATION { get; set; }
        public string DEAL { get; set; }
        public string VERTICAL { get; set; }
        public string TOWER { get; set; }
        public string PROCESS { get; set; }
        public string SUB_PROCESS { get; set; }
        public Nullable<int> DELETE_FLG { get; set; }
        public string INDUSTRY { get; set; }
        public string PRODUCTION { get; set; }
        public Nullable<long> USER_EMPLOYEE_ID { get; set; }
        public string EMPLOYEE_NAME { get; set; }
    
        public virtual T_USER_EMPLOYEE T_USER_EMPLOYEE { get; set; }
        public virtual T_USER_EMPLOYEE T_USER_EMPLOYEE1 { get; set; }
        public virtual T_USER_EMPLOYEE T_USER_EMPLOYEE2 { get; set; }
    }
}