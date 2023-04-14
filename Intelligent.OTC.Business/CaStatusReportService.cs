using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using System.Collections.Generic;
using System;
using Intelligent.OTC.Domain.DomainModel;
using System.Linq;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.DataModel;
using System.Net.Http;
using System.Web;
using System.Configuration;
using System.IO;
using Intelligent.OTC.Common.Exceptions;
using System.Net.Http.Headers;
using System.Net;

namespace Intelligent.OTC.Business
{
    public class CaStatusReportService : ICaStatusReportService
    {

        public OTCRepository CommonRep { get; set; }



        public List<CaStatusReportDto> getStatusReport(string valueDateF, string valueDateT)
        {           
            
            string sql = string.Format(@"select count(1) as count, convert(char(10), VALUE_DATE, 120) as date, MATCH_STATUS as status,LegalEntity
                                        from T_CA_BankStatement
                                        where ISHISTORY = 0 
                                          and VALUE_DATE >= '{0} 00:00:00'
                                          and VALUE_DATE <='{1} 23:59:59'
                                        group by convert (char (10), VALUE_DATE, 120), MATCH_STATUS,LegalEntity
                                        union
                                        select count(1) as count, convert(char(10), VALUE_DATE, 120) as date, '999' as status,LegalEntity
                                        from T_CA_BankStatement
                                        where ISHISTORY = 0 
                                          and VALUE_DATE >= '{0} 00:00:00'
                                          and VALUE_DATE <= '{1} 23:59:59'
                                        group by convert(char(10), VALUE_DATE, 120),LegalEntity
                                        order by convert(char(10), VALUE_DATE, 120) desc, LegalEntity asc;", valueDateF, valueDateT);


            List<CaStatusReportProcessDto> dto = CommonRep.ExecuteSqlQuery<CaStatusReportProcessDto>(sql).ToList();

            return CalculationStatusReport(dto);
        }

        private List<CaStatusReportDto> CalculationStatusReport(List<CaStatusReportProcessDto> list)
        {
            List<CaStatusReportDto> result = new List<CaStatusReportDto>();
            Dictionary<string, CaStatusReportDto> dic = new Dictionary<string, CaStatusReportDto>();

            for (int i=0; i < list.Count; i++) {
                CaStatusReportProcessDto srp = list[i];
                CaStatusReportDto sr;
                string key = srp.date + "&" + srp.LegalEntity;
                if (dic.ContainsKey(key))
                {
                     sr = dic[key];

                }
                else {
                     sr = new CaStatusReportDto();
                    sr.CreateDate = srp.date;
                    sr.LegalEntity = srp.LegalEntity;

                    dic.Add(key ,sr);                    
                    result.Add(sr);
                }

                if (srp.status == "0")
                {
                    sr.UnknowCount = srp.count;
                }
                if (srp.status == "2")
                {
                    sr.UnMatchCount = srp.count;
                }
                if (srp.status == "4")
                {
                    sr.MatchCount = srp.count;
                }
                if (srp.status == "999")
                {
                    sr.Total = srp.count;
                }
            }

            foreach (CaStatusReportDto sr in result) {
                int total = sr.Total;
                int unknown = sr.UnknowCount;
                int unmacth = sr.UnMatchCount;
                int match = sr.MatchCount;

                double unknownpercent = (double)unknown / total;
                double unmacthpercent = (double)unmacth / total;
                double matchpercent = (double)match / total;

                sr.UnknowPercent = unknownpercent.ToString("0.00%");
                sr.UnMatchPercent = unmacthpercent.ToString("0.00%");
                sr.MatchPercent = matchpercent.ToString("0.00%");

            }


            return result;
        }

        public HttpResponseMessage exporAll(string valueDateF, string valueDateT)
        {
            string templateFile = "";
            string fileName = "";
            string tmpFile = "";

            try
            {
                //模板文件  
                templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ExportStatusReportTemplate"].ToString());
                fileName = "StatusReport_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
                tmpFile = Path.Combine(Path.GetTempPath(), fileName);

                List<CaStatusReportDto> customerList = getStatusReport(valueDateF,valueDateT);
                this.SetData(templateFile, tmpFile, customerList);

                HttpResponseMessage response = new HttpResponseMessage();
                response.StatusCode = HttpStatusCode.OK;
                MemoryStream fileStream = new MemoryStream();
                if (File.Exists(tmpFile))
                {
                    using (FileStream fs = File.OpenRead(tmpFile))
                    {
                        fs.CopyTo(fileStream);
                    }
                }
                else
                {
                    throw new OTCServiceException("Get file failed because file not exist with physical path: " + tmpFile);
                }
                Stream ms = fileStream;
                ms.Position = 0;
                response.Content = new StreamContent(ms);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentLength = ms.Length;

                return response;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }


        private void SetData(string templateFileName, string tmpFile, List<CaStatusReportDto> customerList)
        {
            int rowNo = 1;

            try
            {
                NpoiHelper helper = new NpoiHelper(templateFileName);
                helper.Save(tmpFile, true);
                helper = new NpoiHelper(tmpFile);
                string sheetName = "";

                foreach (string sheet in helper.Sheets)
                {
                    sheetName = sheet;
                    break;
                }

                //设置sheet
                helper.ActiveSheetName = sheetName;

                //设置Excel的内容信息
                foreach (var lst in customerList)
                {
                    helper.SetData(rowNo, 0, lst.CreateDate);
                    helper.SetData(rowNo, 1, lst.Total);
                    helper.SetData(rowNo, 2, lst.UnknowCount);
                    helper.SetData(rowNo, 3, lst.UnknowPercent);
                    helper.SetData(rowNo, 4, lst.UnMatchCount);
                    helper.SetData(rowNo, 5, lst.UnMatchPercent);
                    helper.SetData(rowNo, 6, lst.MatchCount);
                    helper.SetData(rowNo, 7, lst.MatchPercent);

                    rowNo++;
                }

                helper.Save(tmpFile, true);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

    }
}
