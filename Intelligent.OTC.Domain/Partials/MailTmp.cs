using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;
using System.Web;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class MailTmp : IAggregateRoot
    {
        public List<CustomerKey> GetRelatedCustomers()
        {
            List<CustomerKey> cus = new List<CustomerKey>();
            if (this.CustomerMails != null)
            {
                cus.AddRange(this.CustomerMails.Select(cm => new CustomerKey() { CustomerNum = cm.CustomerNum, SiteUseId = cm.SiteUseId }));
            }
            return cus;
        }
            
    }

    public class CustomerKey
    {
        public string CustomerNum { get; set; }
        public string SiteUseId { get; set; }
        //public string LegalEntity { get; set; }
    }
}