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
    public class CustAndBankCustService : ICustAndBankCustService
    {

        public OTCRepository CommonRep { get; set; }



        public CACustomerMappingDtoPage getCustomerMapping(int page, int pageSize, string legalEntity, string customerNum, string bankCustomerName)
        {
            CACustomerMappingDtoPage result = new CACustomerMappingDtoPage();

            if (string.IsNullOrEmpty(legalEntity) || legalEntity == "undefined")
            {
                legalEntity = "";
            }
            if (string.IsNullOrEmpty(customerNum) || customerNum == "undefined")
            {
                customerNum = "";
            }
            if (string.IsNullOrEmpty(bankCustomerName) || bankCustomerName == "undefined")
            {
                bankCustomerName = "";
            }

            string sql = string.Format(@"SELECT *  FROM(
	                                                            SELECT
		                                                            ROW_NUMBER () OVER ( ORDER BY CREATE_DATE ASC ) AS Row,
		                                                            id AS Id,
		                                                            CUSTOMER_NUM AS CustomerNum,
		                                                            BankCustomerName AS BankCustomerName,
		                                                            Status,
		                                                            CUSTOMER_NAME AS customerName,
		                                                            LOCALIZE_CUSTOMER_NAME AS localizeCustomerName,
		                                                            LegalEntity AS legalEntity 
	                                                            FROM
		                                                            (
		                                                            SELECT DISTINCT
			                                                            ROW_NUMBER () OVER ( partition BY id ORDER BY CREATE_Date DESC ) AS RowNumber,
			                                                            cm.*,
			                                                            vc.CUSTOMER_NAME,
			                                                            vc.LOCALIZE_CUSTOMER_NAME 
		                                                            FROM
			                                                            T_CA_CustomerMapping cm with (nolock)
			                                                            INNER JOIN V_CA_Customer vc with (nolock) ON cm.CUSTOMER_NUM = vc.CUSTOMER_NUM 
		                                                            WHERE
			                                                            Status = 1 
			                                                            AND (( cm.LegalEntity LIKE '%{2}%' ) OR '' = '{2}' ) 
			                                                            AND (( cm.CUSTOMER_NUM LIKE '%{3}%' ) OR '' = '{3}' ) 
			                                                            AND (( cm.BankCustomerName LIKE '%{4}%' ) OR '' = '{4}' ) 
		                                                            ) AS a 
	                                                            WHERE
		                                                            RowNumber = 1 
	                                                            ) t 
                                                            WHERE
	                                                            t.Row BETWEEN {0} AND {1}", page == 1 ? 0:pageSize*(page-1)+1, pageSize*page,legalEntity,customerNum,bankCustomerName);


            List<CACustomerMappingDto> dto = CommonRep.ExecuteSqlQuery<CACustomerMappingDto>(sql).ToList();

            string sql1 = string.Format(@"select count(1) as count from T_CA_CustomerMapping where Status = 1 AND ((LegalEntity like '%{0}%') OR '' = '{0}')
                                                AND((CUSTOMER_NUM like '%{1}%') OR '' = '{1}')
                                                AND((BankCustomerName like '%{2}%') OR '' = '{2}')", legalEntity, customerNum, bankCustomerName);


            result.list = dto;
            result.listCount = SqlHelper.ExcuteScalar<int>(sql1);

            return result;
        }

        public CACustomerMappingDto getCustomerName(string customerNum,string legalEntity)
        {
            string sql = string.Format(@"select top 1
                                       CUSTOMER_NAME          as CustomerName,
                                       LOCALIZE_CUSTOMER_NAME as LocalizeCustomerName
                                    from V_CA_Customer with (nolock)
                                    where CUSTOMER_NUM = '{0}' and LegalEntity = '{1}'", customerNum, legalEntity);


            List<CACustomerMappingDto> list = SqlHelper.GetList<CACustomerMappingDto>(SqlHelper.ExcuteTable(sql, System.Data.CommandType.Text, null));
            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
      
        }


        public int AddOrUpdate(CACustomerMappingDto model)
        {
            try
            {
                string sql = string.Format(@"select *
                                    from T_CA_CustomerMapping
                                    where id = '{0}'", model.Id);


            List<CACustomerMappingDto> list = SqlHelper.GetList<CACustomerMappingDto>(SqlHelper.ExcuteTable(sql, System.Data.CommandType.Text, null));

                string updateSql = "";
                if (list != null && list.Count > 0)
                {
                     updateSql = string.Format(@"update T_CA_CustomerMapping set  LegalEntity = N'{0}' ,CUSTOMER_NUM = N'{1}' ,BankCustomerName = N'{2}',modify_user= N'{3}',modify_date=getdate() where ID = '{4}'",
                        model.LegalEntity, model.CustomerNum, model.BankCustomerName, AppContext.Current.User.EID, model.Id);
                }
                else {
                    updateSql = string.Format(@"insert into T_CA_CustomerMapping(id, LegalEntity, customer_num, bankcustomername, status, create_user, create_date,
                                              modify_user, modify_date)
                                              values (newid(), N'{0}', N'{1}', N'{2}', 1, N'{3}', getdate(), N'{4}', getdate())",
                        model.LegalEntity, model.CustomerNum, model.BankCustomerName, AppContext.Current.User.EID, AppContext.Current.User.EID);
                }

                SqlHelper.ExcuteSql(updateSql);

                return 1;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                return -1;
            }
        }

        public void Remove(string id)
        {
            try
            {
                string sql = string.Format(@"delete from T_CA_CustomerMapping
                                           where id = '{0}'", id);

                SqlHelper.ExcuteSql(sql);
      
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
               
            }
        }

        public HttpResponseMessage exporAll()
        {
            string templateFile = "";
            string fileName = "";
            string tmpFile = "";

            try
            {
                //模板文件  
                templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ExportCustomerMappingTemplate"].ToString());
                fileName = "CustomerMapping_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
                tmpFile = Path.Combine(Path.GetTempPath(), fileName);

                List<CACustomerMappingDto> customerList = GetCustomerMappingAll();
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


        private void SetData(string templateFileName, string tmpFile, List<CACustomerMappingDto> customerList)
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
                    helper.SetData(rowNo, 0, lst.LegalEntity);
                    helper.SetData(rowNo, 1, lst.CustomerNum);
                    helper.SetData(rowNo, 2, lst.CustomerName);
                    helper.SetData(rowNo, 3, lst.LocalizeCustomerName);
                    helper.SetData(rowNo, 4, lst.BankCustomerName);

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

        private List<CACustomerMappingDto>  GetCustomerMappingAll()
        {           
            string sql = @"select cm.id               as Id,
                                  cm.LegalEntity       as legalEntity,
                                  cm.CUSTOMER_NUM     as CustomerNum,
                                  cm.BankCustomerName as BankCustomerName,
                                  cm.Status,
                                  vc.CUSTOMER_NAME    as customerName,
                                  vc.LOCALIZE_CUSTOMER_NAME as localizeCustomerName
                           from T_CA_CustomerMapping cm with (nolock)
                           left join V_CA_Customer vc with (nolock) on cm.CUSTOMER_NUM = vc.CUSTOMER_NUM
                           where Status = 1";


            return CommonRep.ExecuteSqlQuery<CACustomerMappingDto>(sql).ToList();
        }
    }
}
