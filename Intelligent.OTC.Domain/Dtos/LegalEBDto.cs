using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class LegalEBDto
    {
        public int Id { get; set; }
        public string LEGAL_ENTITY { get; set; }
        public string EB { get; set; }
        public string CREDIT_TREM { get; set; }
        public string Collector { get; set; }
        public string Region { get; set; }
        public string CONTACT_LANGUAGE { get; set; }
        public string CONTACT_LANGUAGENAME { get; set; }
        public string CreditOfficer { get; set; }
        public string FinancialController { get; set; }
        public string CSManager { get; set; }
        public string FinancialManagers { get; set; }
        public string FinanceLeader { get; set; }
        public string LocalFinance { get; set; }
        public string BranchManager { get; set; }
        public string CollectorEmail { get; set; }
    }
    public class LegalEBDtoPage
    {
        public List<LegalEBDto> dataRows;

        public int count;
    }
}
