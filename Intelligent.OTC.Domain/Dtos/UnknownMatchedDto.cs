using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class UnknownMatchedDto
    {
        public string No { get; set; }
        public string TransactionNum { get; set; }
        public DateTime? Date { get; set; }
        public string Description { get; set; }
        public Decimal? amount { get; set; }
        public string group_id { get; set; }

        public CaUnknowARDto ar;
    }

    public class CaUnknowARDto 
    {
        public string accnt_number { get; set; }
        public decimal? amt_remaining { get; set; }

        public string customer_name { get; set; }

        public string site_use_id { get; set; }
        public string selling_location_code { get; set; }
        public string Class { get; set; }
        public string trx_num { get; set; }
        public DateTime? trx_date { get; set; }
        public DateTime? due_date { get; set; }
        public decimal? over_credit_lmt { get; set; }
        public string over_credit_lmt_acct { get; set; }
        public string func_curr_code { get; set; }
        public string inv_curr_code { get; set; }
        public int? due_days { get; set; }
        public decimal? amount_wo_vat { get; set; }
        public string aging_bucket { get; set; }
        public string payment_term_desc { get; set; }
        public string selling_location_code2 { get; set; }
        public string ebname { get; set; }
        public string customertype { get; set; }
        public string isr { get; set; }
        public string fsr { get; set; }
        public string org_id { get; set; }
        public string cmpinv { get; set; }
        public string sales_order { get; set; }
        public string cpo { get; set; }
        public string eb { get; set; }
        public decimal? amt_remaining_tran { get; set; }
        public int isValid { get; set; }
    }
}
