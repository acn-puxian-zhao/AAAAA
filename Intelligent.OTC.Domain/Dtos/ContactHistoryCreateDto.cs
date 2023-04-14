namespace Intelligent.OTC.Domain.Dtos
{
    public class ContactHistoryCreateDto
    {
        public int[] InvoiceIds { get; set; }
        public string CustomerNum { get; set; }
        public string ContactType { get; set; }
        public string ContacterId { get; set; }
        public string LegalEntity { get; set; }
        public string SiteUseId { get; set; }
        public string Comments { get; set; }
        public bool? IsCostomerContact { get; set; }
    }
}
