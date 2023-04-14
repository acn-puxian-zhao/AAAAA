using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public partial class CaBankStatementDto
    {
        DateTime? _PTPDATE = null;
        public string ID { get; set; }
        public string LegalEntity { get; set; }
        public string REGION { get; set; }
        public int? SortId { get; set; }
        public string BSTYPE { get; set; }
        public string BSTYPENAME { get; set; }
        public string TRANSACTION_NUMBER { get; set; }
        public decimal? TRANSACTION_AMOUNT { get; set; }
        public DateTime? TRANSACTION_DATE { get; set; }
        public DateTime? VALUE_DATE { get; set; }
        public string CURRENCY { get; set; }
        public decimal? CURRENT_AMOUNT { get; set; }
        public string Description { get; set; }
        public string REFERENCE1 { get; set; }
        public string REFERENCE2 { get; set; }
        public string REFERENCE3 { get; set; }
        public string FORWARD_NUM { get; set; }
        public string FORWARD_NAME { get; set; }
        public string TYPE { get; set; }
        public string CUSTOMER_NUM { get; set; }
        public string CUSTOMER_NAME { get; set; }
        public bool? IsFixedBankCharge { get; set; }
        public decimal? BankChargeFrom { get; set; }
        public decimal? BankChargeTo { get; set; }
        public string ReceiptsMethod { get; set; }
        public string BankAccountNumber { get; set; }
        public DateTime? IDENTIFY_TIME { get; set; }
        public string MATCH_STATUS { get; set; }
        public string MATCH_STATUS_NAME { get; set; }
        public string APPLY_STATUS { get; set; }
        public DateTime? APPLY_TIME { get; set; }
        public string PMTNUMBER { get; set; }
        public DateTime? ADVISOR_TIME { get; set; }
        public DateTime? RECON_TIME { get; set; }
        public string CLEARING_STATUS { get; set; }
        public DateTime? CLEARING_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public DateTime? CREATE_DATE { get; set; }
        public DateTime? UPDATE_DATE { get; set; }
        public DateTime? MaturityDate { get; set; }
        public string checkNumber { get; set; }
        public string FACTREGION { get; set; }
        public string REF1 { get; set; }
        public string REF2 { get; set; }
        public string REF3 { get; set; }
        public string REF4 { get; set; }
        public string REF5 { get; set; }
        public string REF6 { get; set; }
        public string REF7 { get; set; }
        public string REF8 { get; set; }
        public string REF9 { get; set; }
        public string REF10 { get; set; }
        public string REF11 { get; set; }
        public string REF12 { get; set; }
        public bool? DEL_FLAG { get; set; }
        public string TASK_FILE_ID { get; set; }
        public string FED1 { get; set; }
        public string FED2 { get; set; }
        public string FED3 { get; set; }
        public string FED4 { get; set; }
        public string FED5 { get; set; }
        public string FED6 { get; set; }
        public string FED7 { get; set; }
        public string FED8 { get; set; }
        public string FED9 { get; set; }
        public string FED10 { get; set; }
        public string FED11 { get; set; }
        public string FED12 { get; set; }
        public string FED13 { get; set; }
        public string FED14 { get; set; }
        public string FED15 { get; set; }
        public string FED16 { get; set; }
        public string FED17 { get; set; }
        public string FED18 { get; set; }
        public string FED19 { get; set; }
        public string FED20 { get; set; }
        public string FED21 { get; set; }
        public string FED22 { get; set; }
        public string FED23 { get; set; }
        public string FED24 { get; set; }
        public string FED25 { get; set; }
        public string FED26 { get; set; }
        public string FED27 { get; set; }
        public string FED28 { get; set; }
        public string FED29 { get; set; }
        public string FED30 { get; set; }
        public string FED31 { get; set; }
        public string FED32 { get; set; }
        public string FED33 { get; set; }
        public string FED34 { get; set; }
        public string FED35 { get; set; }
        public string FED36 { get; set; }
        public string FED37 { get; set; }
        public string FED38 { get; set; }
        public string FED39 { get; set; }
        public string FED40 { get; set; }
        public string FED41 { get; set; }
        public string FED42 { get; set; }
        public string FED43 { get; set; }
        public string FED44 { get; set; }
        public string FED45 { get; set; }
        public string FED46 { get; set; }
        public string FED47 { get; set; }
        public string FED48 { get; set; }
        public string FED49 { get; set; }
        public string FED50 { get; set; }
        public string FED51 { get; set; }
        public string FED52 { get; set; }
        public string FED53 { get; set; }
        public string FED54 { get; set; }
        public string FED55 { get; set; }
        public string FED56 { get; set; }
        public string FED57 { get; set; }
        public string FED58 { get; set; }
        public string FED59 { get; set; }
        public string FED60 { get; set; }
        public string FED61 { get; set; }
        public string FED62 { get; set; }
        public string FED63 { get; set; }
        public string FED64 { get; set; }
        public string FED65 { get; set; }
        public string FED66 { get; set; }
        public string FED67 { get; set; }
        public string FED68 { get; set; }
        public string FED69 { get; set; }
        public string FED70 { get; set; }
        public string FED71 { get; set; }
        public string FED72 { get; set; }
        public string FED73 { get; set; }
        public string FED74 { get; set; }
        public string FED75 { get; set; }
        public string FED76 { get; set; }
        public string FED77 { get; set; }
        public string FED78 { get; set; }
        public string FED79 { get; set; }
        public string FED80 { get; set; }
        public string FED81 { get; set; }
        public string FED82 { get; set; }
        public string FED83 { get; set; }
        public string FED84 { get; set; }
        public string FED85 { get; set; }
        public string FED86 { get; set; }
        public string FED87 { get; set; }
        public string FED88 { get; set; }
        public string FED89 { get; set; }
        public string FED90 { get; set; }
        public string FED91 { get; set; }
        public string FED92 { get; set; }
        public string FED93 { get; set; }
        public string FED94 { get; set; }
        public string FED95 { get; set; }
        public string FED96 { get; set; }
        public string FED97 { get; set; }
        public string FED98 { get; set; }
        public string FED99 { get; set; }
        public string FED100 { get; set; }
        public decimal? AMOUNT { get; set; }
        public decimal? ReconBS_Amount { get; set; }
        public string DATA_STATUS { get; set; }
        public bool? ISLOCKED { get; set; }
        public int HASPMTDETAIL { get; set; }
        
        public bool? ISPMTDetailMail { get; set; }
        public bool? ISClearConfirMail { get; set; }
        public string statuscolor { get; set; }
        public int countIdentify { get; set; }
        public decimal? unClear_Amount { get; set; }

        public int? datasheetNum { get; set; }

        public int? dataRowNum { get; set; }

		public int? HASFILE { get; set; }

        public string Comments  { get; set; }

        public bool ISHISTORY { get; set; }

        public int countCustomer { get; set; }
        public string GroupNo { get; set; }
        public string PMTFileName { get; set; }
        public int postMailFlag { get; set; }
        public int clearMailFlag { get; set; }
        public string SiteUseId { get; set; }
        public string PMTDetailFileName { get; set; }
        public DateTime? PMTReceiveDate { get { return _PTPDATE; } set { _PTPDATE = value == new DateTime(1900, 1, 1) ? null : value; } }
        public string reconType { get; set; }
        public string reconId { get; set; }
    }

    public class CaBankStatementDtoPage
    {
        public List<CaBankStatementDto> dataRows;

        public int count;
    }
}
