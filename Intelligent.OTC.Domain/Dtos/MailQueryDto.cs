using System;

namespace Intelligent.OTC.Domain.Dtos
{
    public class MailQueryDto: PaginationQueryDto
    {
        public string Category { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime Start { get; set; }
        public DateTime end { get; set; }
        public string SiteUseId { get; set; }
        public string CustomerNum { get; set; }
        public string CustomerName { get; set; }
        public string OrderBy { get; set; }
        public bool Desc { get; set; }
    }

    public class MailCountDto
    {
        public int? CustomerNew { get; set; }
        public int? Unknow { get; set; }
        public int? Draft { get; set; }
        public int? Sent { get; set; }
        public int? Processed { get; set; }
        public int? Pending { get; set; }
        public int? Total
        {
            get
            {
                return CustomerNew + Unknow + Draft + Sent + Processed + Pending;
            }
        }

        public int? CommonTotal { get; set; }
    }
}
