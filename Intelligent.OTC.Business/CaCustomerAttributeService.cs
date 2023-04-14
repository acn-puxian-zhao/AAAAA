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
    public class CaCustomerAttributeService : ICaCustomerAttributeService
    {

        public OTCRepository CommonRep { get; set; }



        public CaCustomerAttributeDtoPage getCaCustomerAttribute(int page, int pageSize, string legalEntity, string customerNum)
        {
            CaCustomerAttributeDtoPage result = new CaCustomerAttributeDtoPage();

            if (string.IsNullOrEmpty(legalEntity) || legalEntity == "undefined")
            {
                legalEntity = "";
            }
            if (string.IsNullOrEmpty(customerNum) || customerNum == "undefined")
            {
                customerNum = "";
            }


            string sql = string.Format(@"select *
                                        from (
                                                select ROW_NUMBER() over (order by CREATE_Date desc) as RowNumber, * from  T_CA_CustomerAttribute  with(nolock) 
                                                WHERE  ((LegalEntity LIKE '%{2}%' ) OR '' = '{2}' ) 
			                                    AND (( CUSTOMER_NUM LIKE '%{3}%' ) OR '' = '{3}' ) 
                                             ) as a
                                        WHERE RowNumber BETWEEN {0} AND {1}", page == 1 ? 0:pageSize*(page-1)+1, pageSize*page, legalEntity, customerNum);


            List<CaCustomerAttributeDto> dto = CommonRep.ExecuteSqlQuery<CaCustomerAttributeDto>(sql).ToList();

            string sql1 = string.Format(@"select  count(1) from  T_CA_CustomerAttribute  with (nolock)
                                        WHERE  ((LegalEntity LIKE '%{0}%' ) OR '' = '{0}' )  AND ((CUSTOMER_NUM LIKE '%{1}%') OR '' = '{1}' ) ", legalEntity, customerNum);


            result.list = dto;
            result.listCount = SqlHelper.ExcuteScalar<int>(sql1);

            return result;
        }

        public CaCustomerAttributeDto getCaCustomerAttributeByCustomerNum(string customerNum,string legalEntity)
        {
            string sql = string.Format(@"select BankChargeTo, BankChargeFrom
                                            from T_CA_CustomerAttribute with (nolock) 
                                            where CUSTOMER_NUM = '{0}'
                                              and LegalEntity = '{1}'", customerNum,legalEntity);


            List<CaCustomerAttributeDto> list = SqlHelper.GetList<CaCustomerAttributeDto>(SqlHelper.ExcuteTable(sql, System.Data.CommandType.Text, null));
            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
      
        }


        public int AddOrUpdate(CaCustomerAttributeDto model)
        {
            try
            {
                if (model.BankChargeFrom ==null) {
                    model.BankChargeFrom = 0;
                }   
                if (model.BankChargeTo ==null) {
                    model.BankChargeTo = 0;
                }
                string sql = string.Format(@"select *
                                    from T_CA_CustomerAttribute with (nolock) 
                                    where id = '{0}'", model.ID);


                List<CaCustomerAttributeDto> list = SqlHelper.GetList<CaCustomerAttributeDto>(SqlHelper.ExcuteTable(sql, System.Data.CommandType.Text, null));

                string updateSql = "";
                if (list != null && list.Count > 0)
                {
                     updateSql = string.Format(@"update T_CA_CustomerAttribute
                                                set IsEntryAndWiteOff    = '{0}',
                                                    IsFixedBankCharge    =(case when lower('{1}') = 'true' then 1 else 0 end),
                                                    IsJumpBankStatement  =(case when lower('{2}') = 'true' then 1 else 0 end),
                                                    IsJumpSiteUseId=   (case when lower('{3}') = 'true' then 1 else 0 end),
                                                    IsMustPMTDetail      = '{4}',
                                                    IsMustSiteUseIdApply = (case when lower('{5}') = 'true' then 1 else 0 end),
                                                    IsNeedVat = (case when lower('{15}') = 'true' then 1 else 0 end),
                                                    IsFactoring = (case when lower('{16}') = 'true' then 1 else 0 end),
                                                    IsNeedRemittance     = '{6}',
                                                    LegalEntity          = '{7}',
                                                    Func_Currency        = '{8}',
                                                    CAOperator='{9}',
                                                    BankChargeFrom       ={10},
                                                    BankChargeTo         ={11},
                                                    MODIFY_User='{12}',
                                                    MODIFY_Date          = getdate(),
                                                    CUSTOMER_NUM = '{13}'
                                                    where ID = '{14}'",
                        model.IsEntryAndWiteOff, model.IsFixedBankCharge, model.IsJumpBankStatement,model.IsJumpSiteUseId,model.IsMustPMTDetail,model.IsMustSiteUseIdApply,
                        model.IsNeedRemittance,model.LegalEntity,model.Func_Currency, AppContext.Current.User.EID, model.BankChargeFrom, model.BankChargeTo, AppContext.Current.User.EID,model.CUSTOMER_NUM, model.ID,model.IsNeedVat,model.IsFactoring);
                }
                else {
                    updateSql = string.Format(@"insert into T_CA_CustomerAttribute(id, IsEntryAndWiteOff, IsFixedBankCharge, IsJumpBankStatement, IsJumpSiteUseId,
                                   IsMustPMTDetail,
                                   IsMustSiteUseIdApply, IsNeedRemittance, LegalEntity, Func_Currency, CAOperator,
                                   BankChargeFrom, BankChargeTo, create_user, create_date,
                                   modify_user, modify_date,CUSTOMER_NUM,IsNeedVat,IsFactoring)
                                   values (newid(), '{0}', (case when lower('{1}') = 'true' then 1 else 0 end),
                                  (case when lower('{2}') = 'true' then 1 else 0 end), (case when lower('{3}') = 'true' then 1 else 0 end), '{4}',
                                  (case when lower('{5}') = 'true' then 1 else 0 end), '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', getdate(),
                                  '{13}', getdate(),'{14}',(case when lower('{15}') = 'true' then 1 else 0 end),(case when lower('{16}') = 'true' then 1 else 0 end))",
                        model.IsEntryAndWiteOff, model.IsFixedBankCharge, model.IsJumpBankStatement, model.IsJumpSiteUseId, model.IsMustPMTDetail, model.IsMustSiteUseIdApply,
                        model.IsNeedRemittance, model.LegalEntity, model.Func_Currency, AppContext.Current.User.EID, model.BankChargeFrom, model.BankChargeTo, AppContext.Current.User.EID, AppContext.Current.User.EID,model.CUSTOMER_NUM,model.IsNeedVat,model.IsFactoring);
                }

                SqlHelper.ExcuteSql(updateSql);

                return 1;
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Add or Update failed, please contact the administrator.\r\n" + ex.Message, ex);
                return -1;
            }
        }

        public void Remove(string id)
        {
            try
            {
                string sql = string.Format(@"delete from T_CA_CustomerAttribute
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
                templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ExportCustomerAttributeTemplate"].ToString());
                fileName = "CustomerAttribute_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
                tmpFile = Path.Combine(Path.GetTempPath(), fileName);

                List<CaCustomerAttributeDto> customerList = GetCustomerAttributeAll();
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


        private void SetData(string templateFileName, string tmpFile, List<CaCustomerAttributeDto> customerList)
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

                    helper.SetData(rowNo, 0, lst.CUSTOMER_NUM);
                    helper.SetData(rowNo, 1, lst.IsEntryAndWiteOff);
                    helper.SetData(rowNo, 2, lst.IsFixedBankCharge);
                    helper.SetData(rowNo, 3, lst.IsJumpBankStatement);
                    helper.SetData(rowNo, 4, lst.IsJumpSiteUseId);
                    helper.SetData(rowNo, 5, lst.IsMustPMTDetail);
                    helper.SetData(rowNo, 6, lst.IsMustSiteUseIdApply);
                    helper.SetData(rowNo, 7, lst.IsNeedRemittance);
                    helper.SetData(rowNo, 8, lst.LegalEntity);
                    helper.SetData(rowNo, 9, lst.Func_Currency);
                    helper.SetData(rowNo, 10, lst.CAOperator);
                    helper.SetData(rowNo, 11, lst.BankChargeFrom);
                    helper.SetData(rowNo, 12, lst.BankChargeTo);
                    helper.SetData(rowNo, 13, lst.CREATE_User);
                    helper.SetData(rowNo, 14, lst.CREATE_Date);
                    helper.SetData(rowNo, 15, lst.MODIFY_User);
                    helper.SetData(rowNo, 16, lst.MODIFY_Date);

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

        private List<CaCustomerAttributeDto> GetCustomerAttributeAll()
        {           
            string sql = @"select  * from  T_CA_CustomerAttribute with (nolock) ";


            return CommonRep.ExecuteSqlQuery<CaCustomerAttributeDto>(sql).ToList();
        }
    }
}
