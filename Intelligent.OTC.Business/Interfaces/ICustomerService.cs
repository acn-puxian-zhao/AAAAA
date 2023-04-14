using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using Intelligent.OTC.Common.UnitOfWork;
using Intelligent.OTC.Common;
using Intelligent.OTC.Domain;

namespace Intelligent.OTC.Business
{
    public interface ICustomerService
    {
        ICacheService CacheSvr { get; set; }
        OTCRepository CommonRep { get; set; }
        IQueryable<Customer> GetCustomer();
        void AddCustomer(Customer cust);
        IQueryable<CustomerAging> GetCustomerAging();
        IQueryable<CustomerAgingStaging> GetCustomerAgingStaging();
        void createAgingReport();
        void allFileImport();
        int SubmitInitialAging();
        void SubmitOneYearSales();
        IQueryable<CustomerAging> GetCustomerAgingByCollector(string eID);

        //get one customer by customernum and sitecode
        //add by pxc 20150722
        Customer GetOneCustomer(string num);
        Customer GetOneCustomer(string num, string siteUseId);
        List<Customer> GetCustomerByCustomerNum(string cusNum);
        void DeleteCustomerAging(List<int> custIds);
        void uploadOneYearSales();
        //get customermaster data
        IQueryable<CustomerMasterData> GetCustMasterData(string Contacter);
        IQueryable<CustomerMasterDto> GetCustMasterDataForAssign(string customers);
        List<CustomerGroupCfgStaging> GetGroupStaing();
        void allFileImportArrow(FileUploadHistory accFileName, FileUploadHistory invFileName, FileUploadHistory invDetailFileName, FileUploadHistory vatFileName = null);
        List<UploadLegal> getLegalHisByDate(string date);
        UploadLegalHisModel getFileHisByDate(int pageindex, int pagesize, string date);
        submitWaitInvDetModel getSubmitWaitInvDet(int pageindex, int pagesize);
        int SubmitInitialAging(string arrow);
        int SubmitInitialAgingNew(string deal);
        int SubmitInitialVAT(string deal);
        int SubmitInitialInvDet(string deal);
        int SubmitInitialSAPAging(string deal);
        int BuildContactor(string deal);
        int BuildInvoiceAgingStatus(string deal, string legalEntity, string customerNo, string siteUseId, string invoiceNo, string strOperator);
        FileUploadHistory getHisById(int id);
        string uploadAg(string acc, string inv, string invdet=null, string vat = null);
        string uploadVat(string vat);
        submitWaitVatModel getSubmitWaitVat(int pageindex, int pagesize);
        string GetLegalNewFile(string legal, int type);
        void Batch();
        void autoBuildContactor(string deal, string legalEntity);
        Dictionary<string, bool> getLegalByDash();
        string ImportCustomerLocalize();
        string ImportCreditHold();
        string ImportCustomerLocalize(FileUploadHistory fileUpHis);
        string ImportVatOnly();
        string ImportVarDataOnly();
        string ImportInvoiceDetailOnly();
        string ImportVatOnly(FileUploadHistory fileUpHis);
        string ImportInvoiceDetailOnly(FileUploadHistory fileUpHis);

        string ImportTWCurrencyAmount();
    }
}
