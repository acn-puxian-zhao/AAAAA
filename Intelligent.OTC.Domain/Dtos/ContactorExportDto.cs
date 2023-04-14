namespace Intelligent.OTC.Domain.Dtos
{
    public class ContactorExportDto
    {
        public int Id { get; set; }

        public string EbName { get; set; }

        public string Collector { get; set; }

        public string CreditTerm { get; set; }

        public string Region { get; set; }

        public string Legal { get; set; }

        public string CustomerName { get; set; }

        public string CustomerNum { get; set; }

        public string SiteUseId { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public string EmailAddress { get; set; }
    }

    public class ContactorExportItem
    {
        public int Row { get; set; }

        public string Customer { get; set; }
        public string CustomerEmail { get; set; }
        public string Cs { get; set; }

        public string Sales { get; set; }

        public string CsEmail { get; set; }

        public string SalesEmail { get; set; }

        public string BranchManagerEmail { get; set; }

        public string CsManagerEmail { get; set; }

        public string SalesManagerEmail { get; set; }

        public string FinancialControllersEmail { get; set; }

        public string FinancialManagersEmail { get; set; }

        public string CreditOfficersEmail { get; set; }

        public string LocalFinanceEmail { get; set; }

        public string FinanceLeaderEmail { get; set; }

        public string CreditManagerEmail { get; set; }
    }
}
