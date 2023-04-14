using AutoMapper;
using Intelligent.OTC.Business.Collection;
using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.DomainModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Intelligent.OTC.Business
{
    public class CustomerAssessmentHistoryService : ICustomerAssessmentHistoryService
    {

        public OTCRepository CommonRep { get; set; }

        public string CurrentDeal
        {
            get
            {
                return AppContext.Current.User.Deal.ToString();
            }
        }

        public CustomerAssessmentModel getCustomerAssessmentHistory(VCustomerAssessmentDto vCustomerAssessmentDto)
        {
            CustomerAssessmentModel r = new CustomerAssessmentModel();
            var result = CommonRep.GetQueryable<V_CustomerAssessmentHistory>().Where(o => o.DEAL == CurrentDeal);

            if (!String.IsNullOrEmpty(vCustomerAssessmentDto.CustomerNum))
            {
                result = result.Where(p => p.CUSTOMER_NUM == vCustomerAssessmentDto.CustomerNum);
            }

            if (!String.IsNullOrEmpty(vCustomerAssessmentDto.CustomerName))
            {
                result = result.Where(p => p.CUSTOMER_NAME == vCustomerAssessmentDto.CustomerName);
            }

            if (!String.IsNullOrEmpty(vCustomerAssessmentDto.AssessmentType))
            {
                int assessmentType = Convert.ToInt32(vCustomerAssessmentDto.AssessmentType);
                result = result.Where(p => p.AssessmentType == assessmentType);
            }

            if (!String.IsNullOrEmpty(vCustomerAssessmentDto.SiteUseId))
            {
                result = result.Where(p => p.SiteUseId == vCustomerAssessmentDto.SiteUseId);
            }

            if (!String.IsNullOrEmpty(vCustomerAssessmentDto.LegalEntity))
            {
                result = result.Where(p => p.LegalEntity == vCustomerAssessmentDto.LegalEntity);
            }

            if (!String.IsNullOrEmpty(vCustomerAssessmentDto.AssessmentDate))
            {
                result = result.Where(p => SqlFunctions.DatePart("year", p.LastModificationTime).ToString()
                                        + "-" + SqlFunctions.DatePart("month", p.LastModificationTime).ToString()
                                        + "-" + SqlFunctions.DatePart("day", p.LastModificationTime).ToString()
                                        == vCustomerAssessmentDto.AssessmentDate);
            }
            var total = result.ToList();
            r.TotalItems = total.Count;
            if (!String.IsNullOrEmpty(vCustomerAssessmentDto.Index) && !String.IsNullOrEmpty(vCustomerAssessmentDto.ItemCount))
            {
                int index = Convert.ToInt32(vCustomerAssessmentDto.Index);
                int itemCount = Convert.ToInt32(vCustomerAssessmentDto.ItemCount);
                var query = total.Skip(itemCount * (index - 1)).Take(itemCount).ToList();
                r.List = Mapper.Map<List<CustomerAssessmentItem>>(query);
            }
            else
            {
                var query = result.Take(20).ToList();
                r.List = Mapper.Map<List<CustomerAssessmentItem>>(query);
            }

            return r;
        }
        public string exportCustomerAssessmentHistory(VCustomerAssessmentDto vCustomerAssessmentDto)
        {
            var result = CommonRep.GetQueryable<V_CustomerAssessmentHistory>().Where(o => o.DEAL == CurrentDeal);

            if (!String.IsNullOrEmpty(vCustomerAssessmentDto.CustomerNum))
            {
                result = result.Where(p => p.CUSTOMER_NUM == vCustomerAssessmentDto.CustomerNum);
            }

            if (!String.IsNullOrEmpty(vCustomerAssessmentDto.CustomerName))
            {
                result = result.Where(p => p.CUSTOMER_NAME == vCustomerAssessmentDto.CustomerName);
            }

            if (!String.IsNullOrEmpty(vCustomerAssessmentDto.AssessmentType))
            {
                int assessmentType = Convert.ToInt32(vCustomerAssessmentDto.AssessmentType);
                result = result.Where(p => p.AssessmentType == assessmentType);
            }

            if (!String.IsNullOrEmpty(vCustomerAssessmentDto.SiteUseId))
            {
                result = result.Where(p => p.SiteUseId == vCustomerAssessmentDto.SiteUseId);
            }

            if (!String.IsNullOrEmpty(vCustomerAssessmentDto.LegalEntity))
            {
                result = result.Where(p => p.LegalEntity == vCustomerAssessmentDto.LegalEntity);
            }

            if (!String.IsNullOrEmpty(vCustomerAssessmentDto.AssessmentDate))
            {
                result = result.Where(p => SqlFunctions.DatePart("year", p.LastModificationTime).ToString()
                                        + "-" + SqlFunctions.DatePart("month", p.LastModificationTime).ToString()
                                        + "-" + SqlFunctions.DatePart("day", p.LastModificationTime).ToString()
                                        == vCustomerAssessmentDto.AssessmentDate);
            }
            string templateName = "ARMasterTemplate";
            string outputPath = "ARMasterPath";
            var tplName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[templateName].ToString());
            var fileName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString());
            var pathName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString() + "ARMasterDataExport." + AppContext.Current.User.EID + ".xlsx");

            if (Directory.Exists(fileName) == false)
            {
                Directory.CreateDirectory(fileName);
            }

            WriteToExcel(tplName, pathName, result);

            HttpRequest request = HttpContext.Current.Request;
            StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
            appUriBuilder.Append(Uri.SchemeDelimiter);
            appUriBuilder.Append(request.Url.Authority);
            if (String.Compare(request.ApplicationPath, @"/") != 0)
            {
                appUriBuilder.Append(request.ApplicationPath);
            }
            var virPatnName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[outputPath].ToString().Trim('~') + "ARMasterDataExport." + AppContext.Current.User.EID + ".xlsx";
            return virPatnName;
        }
        public int getCustomerAssessmentHistoryCount()
        {
            return CommonRep.GetQueryable<V_CustomerAssessment>().Where(o => o.DEAL == CurrentDeal).Count();
        }
        private void WriteToExcel(string temp, string path, IQueryable<V_CustomerAssessmentHistory> list)
        {
            try
            {
                ExportService export = new ExportService(temp);
                export.Save(path, true);
                export = new ExportService(path);
                var sheetName = export.Sheets[0];
                export.ActiveSheetName = sheetName;
                export.ExportDataList(list.ToList());
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }
    }
}
