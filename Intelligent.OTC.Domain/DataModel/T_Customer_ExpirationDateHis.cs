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
    
    public partial class T_Customer_ExpirationDateHis
    {
        public int ID { get; set; }
        public System.DateTime ChangeDate { get; set; }
        public string CustomerNum { get; set; }
        public string SiteUseId { get; set; }
        public Nullable<System.DateTime> OldCommentExpirationDate { get; set; }
        public Nullable<System.DateTime> NewCommentExpirationDate { get; set; }
        public string UserId { get; set; }
        public string Comment { get; set; }
    }
}