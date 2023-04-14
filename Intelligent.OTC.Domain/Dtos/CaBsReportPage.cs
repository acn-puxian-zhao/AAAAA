using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class CaBsReportDto {
        public string TRANSACTION_NUMBER { get; set; }
        public DateTime VALUE_DATE { get; set; }
        public string BSCURRENCY { get; set; }
        public string LegalEntity { get; set; }
        public string BSCustomerNum { get; set; }
        public int SortId { get; set; }
        public string TransactionINC { get; set; }
        public string BankID { get; set; }
        public string AccountNumber { get; set; }
        public string AccountID { get; set; }
        public string AccountName { get; set; }
        public string AccountOwnerID { get; set; }
        public string AccountCountry { get; set; }
        public string TransactionDate { get; set; }
        public string ValueDate { get; set; }
        public string Currency { get; set; }
        public string Amount { get; set; }
        public string ReferenceDRNM { get; set; }
        public string ReferenceBRCR { get; set; }
        public string ReferenceDESCR { get; set; }
        public string ReferenceENDTOEND { get; set; }
        public string Description { get; set; }
        public string ReferenceBRTN { get; set; }
        public string ReferenceDRBNK { get; set; }
        public string UserCode { get; set; }
        public string UserCodeDescription { get; set; }
        public string ItemType { get; set; }
        public string Owner { get; set; }
        public string Cheque { get; set; }
        public string needchecking { get; set; }
        public string Area { get; set; }
        public string Week { get; set; }
        public string Type { get; set; }
        public string UnknownType { get; set; }
        public string CustomerName { get; set; }
        public string Account { get; set; }
        public string SiteUseId { get; set; }
        public string EB_Name { get; set; }
        public decimal OperateAmount { get; set; }
        public string Term { get; set; }
        public string Comments { get; set; }
    }

    public class CaBsReportPage
    {
        public List<CaBsReportDto> dataRows;

        public int count;
    }
}
