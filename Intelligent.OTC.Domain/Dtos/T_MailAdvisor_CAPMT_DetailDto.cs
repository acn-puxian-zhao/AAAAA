using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class T_MailAdvisor_CAPMT_DetailDto
    {
        public string Id { get; set; }
        public string CAPMTID { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
    }
}
