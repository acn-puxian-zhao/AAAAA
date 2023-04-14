using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.DataModel
{
    public class SendMailDto
    {
        public MailTmp mailInstance { get; set; }
        public string invoiceNums { get; set; }
    }
}
