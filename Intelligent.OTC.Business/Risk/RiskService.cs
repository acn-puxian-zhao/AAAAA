using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.Repositories;
using System.Collections;
using Excel = Microsoft.Office.Interop.Excel;
using MathNet.Numerics.Statistics;
using Intelligent.OTC.Common.UnitOfWork;
using Intelligent.OTC.Domain;
using System.Threading.Tasks;
using System.Threading;
using System.Transactions;
using EntityFramework.MappingAPI.Mappings;
using System.Reflection;
using EntityFramework.MappingAPI.Exceptions;
using EntityFramework.BulkInsert.Extensions;
using System.Data.SqlClient;


namespace Intelligent.OTC.Business
{
    public class RiskService
    {
        public OTCRepository CommonRep { get; set; }
        private static string sysConByWeight = "004";     //sysConfig TABLE ByMultiRate 
        private static string sysConByMultiRate = "005"; //sysConfig TABLE  ByMultiRate
        private static int inPeriodId = 0;

        //NO USE METHOD
        public void GetRiskValue(RiskCalculatorContext context)
        {
            Helper.Log.Info("Start GetRiskValue method");
            SysConfig liSysCon = new SysConfig();
            PeriodControl period = new PeriodControl();
            try
            {
                if (context.CurrentAccountLevelAging.Count > 0 && context.CurrentInvoiceLevelAging.Count > 0)
                {
                    // 1. Data perparetion.
                    // Get calculate type by current deal, Then get calculater from factory
                    RiskRule calculateType = CommonRep.GetQueryable<RiskRule>().Where(type => type.Deal == AppContext.Current.User.Deal).FirstOrDefault();
                    if (calculateType == null)
                    {
                        throw new OTCServiceException("Not find the Deal in Table riskRule!");
                    }

                    CalculationType ct = Helper.CodeToEnum<CalculationType>(calculateType.CalculateType.ToString());

                    // Get risk rule indexs by ruleId
                    List<RiskRuleIndex> indexs = new List<RiskRuleIndex>();
                    indexs = (from index in CommonRep.GetQueryable<RiskRuleIndex>()
                              where index.RuleId == calculateType.Id
                              select index).ToList<RiskRuleIndex>();

                    if (indexs == null)
                    {
                        Exception ex = new OTCServiceException("Not find the index in Table riskRuleIndex!");
                        Helper.Log.Error(ex.Message, ex);
                        throw ex;
                    }

                    // Get risk exceptions list
                    List<CustomerPrioritizationExceptionList> riskli = (from ri in CommonRep.GetQueryable<CustomerPrioritizationExceptionList>()
                                                                        where ri.Deal == AppContext.Current.User.Deal && ri.ExType == "2"//2:risk,1:value
                                                                        && ri.ExpiryDate >= AppContext.Current.User.Now && ri.EffectDate <= AppContext.Current.User.Now
                                                                        select ri).ToList<CustomerPrioritizationExceptionList>();

                    // Get the currentPeriod
                    PeroidService service = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
                    period = service.getcurrentPeroid();
                    if (period != null)
                    {
                        inPeriodId = period.Id;
                    }

                    // Get  compare value from table Sys_Config
                    if ((calculateType.CalculateType).ToString() == Helper.EnumToCode<CalculationType>(CalculationType.ByWeight))
                    {
                        liSysCon = (from risk in CommonRep.GetQueryable<SysConfig>()
                                    where risk.CfgCode == sysConByWeight //ByWeight [004]
                                    select risk).FirstOrDefault();
                    }
                    else if ((calculateType.CalculateType).ToString() == Helper.EnumToCode<CalculationType>(CalculationType.MultiRate))
                    {
                        liSysCon = (from risk in CommonRep.GetQueryable<SysConfig>()
                                    where risk.CfgCode == sysConByMultiRate//MultiRate[005]
                                    select risk).FirstOrDefault();
                    }
                    if (liSysCon == null)
                    {
                        Exception ex = new OTCServiceException("risk Calculate Value is not set in Table SYS_CONFIG(CFG_CODE:004,005)");
                        Helper.Log.Error(ex.Message, ex);
                        throw ex;
                    }

                    double riskCompValue = double.Parse(liSysCon.CfgValue);

                    Helper.Log.Info("Start get the each index value for risk calculate");
                    // 2. Get the index value for each customer
                    CustomerRiskCollection custColl = new CustomerRiskCollection();
                    foreach (Customer cust in context.AllCustomers)
                    {
                        CustomerWithRisk vr = new CustomerWithRisk()
                        {
                            IndexRisks = new Dictionary<string, IndexRisk>(),
                            CustNum = cust.CustomerNum
                        };

                        custColl.Add(vr);

                        List<CustomerAging> accountsAging = new List<CustomerAging>();
                        List<InvoiceAging> invoiceAging = new List<InvoiceAging>();

                        //if same deal and same customernum ,billgorupcode is same
                        var agingRecord = (from aging in context.CurrentAccountLevelAging
                                           where aging.Deal == cust.Deal && aging.CustomerNum == cust.CustomerNum
                                           select aging).FirstOrDefault();

                        if (agingRecord != null)
                        {
                            if (agingRecord.BillGroupCode != null)
                            {
                                string billGroupCDstr = agingRecord.BillGroupCode.ToString();
                                accountsAging = (from ag in context.CurrentAccountLevelAging
                                                 where ag.BillGroupCode == billGroupCDstr && ag.Deal == cust.Deal
                                                 select ag).ToList<CustomerAging>();

                                invoiceAging = (from invo in context.CurrentInvoiceLevelAging
                                                where invo.BillGroupCode == billGroupCDstr && invo.Deal == cust.Deal
                                                select invo).ToList<InvoiceAging>();

                            }
                            else
                            {
                                accountsAging = (from ag in context.CurrentAccountLevelAging
                                                 where ag.Deal == cust.Deal && ag.CustomerNum == cust.CustomerNum
                                                 select ag).ToList<CustomerAging>();

                                invoiceAging = (from invo in context.CurrentInvoiceLevelAging
                                                where invo.Deal == cust.Deal && invo.CustomerNum == cust.CustomerNum
                                                select invo).ToList<InvoiceAging>();
                            }

                            // get the each index value
                            indexs.ForEach(inx =>
                            {
                                // calculate I1V
                                var tmpVal = inx.GetValue(accountsAging, invoiceAging);
                                IndexRisk tmp = new IndexRisk() { Value = tmpVal, IndexWeight = inx.IndexWeighted };
                                vr.IndexRisks.Add(inx.IndexName, tmp);
                            });
                        }
                        else
                        {
                            indexs.ForEach(inx =>
                            {
                                IndexRisk tmp = new IndexRisk() { Value = 0, IndexWeight = inx.IndexWeighted };
                                vr.IndexRisks.Add(inx.IndexName, tmp);
                            });
                        }

                    }
                    Helper.Log.Info("End get the each index value for risk calculate");

                    // 3, Apply devident method to all values from customer for each index
                    indexs.ForEach(index => index.ApplyDevidedMethodTo(custColl));

                    List<CustomerChangeHis> cuschanHisLis = new List<CustomerChangeHis>();
                    foreach (Customer cust in context.AllCustomers)
                    {
                        // 4, calculate the risk total score By Weight or By MultiRate
                        ICalculatorStrategy calculator = RiskCalactorFactory.GetCalculator(ct);
                        double score = calculator.Calcurate(custColl[cust.CustomerNum].IndexRisks.Values.ToList());

                        // 5, add customer change history item
                        CustomerChangeHis cuschanHis = new CustomerChangeHis()
                        {
                            Value = Convert.ToDecimal(score),
                            ValueLevel = score < riskCompValue ? "LR" : "HR",
                            CustomerNum = cust.CustomerNum,
                            Deal = cust.Deal,
                            VrType = "2",//2:risk,1:value
                            CreateDate = AppContext.Current.User.Now,
                            PeriodId = inPeriodId
                        };

                        string vrDesc = "Risk is:" + cuschanHis.ValueLevel + ";Risk Score is:" + score + ";Detail:";
                        foreach (var ri in custColl[cust.CustomerNum].IndexRisks)
                        {
                            vrDesc += ri.Key + ":" + ri.Value.Risk + ",";
                        }

                        vrDesc = vrDesc.Remove(vrDesc.Length - 1, 1);
                        cuschanHis.VrDesc = vrDesc;

                        // apply risk exception from ExceptionList
                        var riskEx = riskli.Find(riex => riex.CustomerNum == cust.CustomerNum);
                        if (riskEx != null)
                        {
                            cuschanHis.ValueLevel = riskEx.ExValue;
                        }

                        cuschanHisLis.Add(cuschanHis);
                    }

                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                    {
                        CommonRep.Commit();

                        (CommonRep.GetDBContext() as OTCEntities).BulkInsert(cuschanHisLis);

                        scope.Complete();
                    }

                    Helper.Log.Info("End Commit customer and history changes");
                }
            }
            catch (OTCServiceException ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public void GetRiskValueNoPa()
        {
            Helper.Log.Info("Start GetRiskValue method");
            SysConfig liSysCon = new SysConfig();
            PeriodControl period = new PeriodControl();
            double tmpVal = 0;
            try
            {

                //Get calculateType from DB
                RiskRule calculateType = CommonRep.GetQueryable<RiskRule>().Where(type => type.Deal == AppContext.Current.User.Deal).FirstOrDefault();
                if (calculateType == null)
                {
                    Exception ex = new OTCServiceException("Not find record of current Deal  in Table [riskRule]!");
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }

                //Get risk rule indexs by ruleId from DB
                List<RiskRuleIndex> indexs = new List<RiskRuleIndex>();
                indexs = (from index in CommonRep.GetQueryable<RiskRuleIndex>()
                          where index.RuleId == calculateType.Id
                          select index).ToList<RiskRuleIndex>();

                if (indexs == null)
                {
                    Exception ex = new OTCServiceException("Not find the index in Table [riskRuleIndex]!");
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }

                //Get The Amt,age,percent From DB VIEW
                //********************Get The Amt,age,percent From DB PROCEDURE*********************************************************************//
                var dateParam1 = new SqlParameter
                {
                    ParameterName = "Param1",
                    Value = AppContext.Current.User.Now
                };
                var dateParam2 = new SqlParameter
                {
                    ParameterName = "Param2",
                    Value = AppContext.Current.User.Deal
                };

                List<RiskC> liRiskCal = CommonRep.GetDBContext().Database.SqlQuery<RiskC>
                ("P_RISK_CALCULATE @Param1,@Param2", dateParam1, dateParam2).ToList<RiskC>();

                if (liRiskCal.Count == 0)
                {
                    throw new OTCServiceException("Risk Calculate Not Get the Data!");
                }
                //********************Get procedure*********************************************************************//
                CustomerRiskCollection custColl = new CustomerRiskCollection();
                Helper.Log.Info("Start LOOP CUSTOMER TO SET VALUE ");
                //LOOP CUSTOMER FROM DB AND SET THE VALUE TO custColl
                foreach (RiskC riskCalculate in liRiskCal)
                {
                    CustomerWithRisk vr = new CustomerWithRisk()
                    {
                        IndexRisks = new Dictionary<string, IndexRisk>(),
                        CustNum = riskCalculate.CustomerNum
                    };

                    // get the each index value
                    indexs.ForEach(inx =>
                    {
                            // calculate I1V
                        if (inx.IndexName == "Amount")
                        {
                            tmpVal = Convert.ToDouble(riskCalculate.Amt);
                        }
                        else if (inx.IndexName == "Age")
                        {
                            tmpVal = Convert.ToDouble(riskCalculate.Age);
                        }
                        else if (inx.IndexName == "OverDuePercentage")
                        {
                            tmpVal = Convert.ToDouble(riskCalculate.OverduePercent);
                        }
                        else
                        {
                            tmpVal = 0;
                       }
                        IndexRisk tmp = new IndexRisk() { Value = tmpVal, IndexWeight = inx.IndexWeighted };
                        vr.IndexRisks.Add(inx.IndexName, tmp);
                    });
                    custColl.Add(vr);
                }
                Helper.Log.Info("End LOOP CUSTOMER TO SET VALUE ");
                Helper.Log.Info("Start DevidedMethod ");
                //Calculate Rate value by DevidedMethod
                indexs.ForEach(index => index.ApplyDevidedMethodTo(custColl));
                Helper.Log.Info("End DevidedMethod ");

                Helper.Log.Info("Start GET RISKExceptionList ");
                // Get risk exceptions list
                List<CustomerPrioritizationExceptionList> riskli = (from ri in CommonRep.GetQueryable<CustomerPrioritizationExceptionList>()
                                                                    where ri.Deal == AppContext.Current.User.Deal && ri.ExType == "2"//2:risk,1:value
                                                                    && ri.ExpiryDate >= AppContext.Current.User.Now && ri.EffectDate <= AppContext.Current.User.Now
                                                                    select ri).ToList<CustomerPrioritizationExceptionList>();
                Helper.Log.Info("END GET RISKExceptionList ");
                // Get  compare value from table Sys_Config
                if ((calculateType.CalculateType).ToString() == Helper.EnumToCode<CalculationType>(CalculationType.ByWeight))
                {
                    liSysCon = (from risk in CommonRep.GetQueryable<SysConfig>()
                                where risk.CfgCode == sysConByWeight //ByWeight [004]
                                select risk).FirstOrDefault();
                }
                else if ((calculateType.CalculateType).ToString() == Helper.EnumToCode<CalculationType>(CalculationType.MultiRate))
                {
                    liSysCon = (from risk in CommonRep.GetQueryable<SysConfig>()
                                where risk.CfgCode == sysConByMultiRate//MultiRate[005]
                                select risk).FirstOrDefault();
                }
                if (liSysCon == null)
                {
                    Exception ex = new OTCServiceException("risk Calculate Value is not set in Table SYS_CONFIG(CFG_CODE:004,005)");
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }
                double riskCompValue = double.Parse(liSysCon.CfgValue);

                // Get the currentPeriod
                PeroidService service = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
                period = service.getcurrentPeroid();
                if (period != null)
                {
                    inPeriodId = period.Id;
                }

                CalculationType ct = Helper.CodeToEnum<CalculationType>(calculateType.CalculateType.ToString());

                List<CustomerChangeHis> cuschanHisLis = new List<CustomerChangeHis>();

                // By Weight or By MultiRate
                ICalculatorStrategy calculator = RiskCalactorFactory.GetCalculator(ct);

                Helper.Log.Info("START ADD TO HIS");
                foreach (RiskC cust in liRiskCal)
                {

                    //CALCULATE the risk total score
                    double score = calculator.Calcurate(custColl[cust.CustomerNum].IndexRisks.Values.ToList());

                    // add customer change history item
                    CustomerChangeHis cuschanHis = new CustomerChangeHis()
                    {
                        Value = Convert.ToDecimal(score),
                        ValueLevel = score < riskCompValue ? "LR" : "HR",
                        CustomerNum = cust.CustomerNum,
                        Deal = AppContext.Current.User.Deal,
                        VrType = "2",//2:risk,1:value
                        CreateDate = AppContext.Current.User.Now,
                        PeriodId = inPeriodId
                    };
                    string vrDesc = "Risk is:" + cuschanHis.ValueLevel + ";Risk Score is:" + score + ";Detail:";
                    foreach (var ri in custColl[cust.CustomerNum].IndexRisks)
                    {
                        vrDesc += ri.Key + ":" + ri.Value.Risk + ",";
                    }

                    vrDesc = vrDesc.Remove(vrDesc.Length - 1, 1);
                    cuschanHis.VrDesc = vrDesc;

                    // apply risk exception from ExceptionList
                    var riskEx = riskli.Find(riex => riex.CustomerNum == cust.CustomerNum);
                    if (riskEx != null)
                    {
                        cuschanHis.ValueLevel = riskEx.ExValue;
                    }
                    cuschanHisLis.Add(cuschanHis);
                }
                Helper.Log.Info("END ADD TO HIS");
                Helper.Log.Info("START COMMIT RISKCALCUATE");
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    CommonRep.Commit();

                    (CommonRep.GetDBContext() as OTCEntities).BulkInsert(cuschanHisLis);

                    scope.Complete();
                }
                Helper.Log.Info("END COMMIT RISKCALCUATE");
            }//try

            catch (OTCServiceException ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

        }
    }

    public class RiskCalactorFactory
    {
        public static ICalculatorStrategy GetCalculator(CalculationType type)
        {
            switch (type)
            {
                case CalculationType.ByWeight:
                    return new ByWeightCalculator();
                case CalculationType.MultiRate:
                    return new MultiRateCalculator();
                default:
                    throw new OTCServiceException("Not a supported risk calculator provided! Type:" + type.ToString());
            }
        }
    }

    public interface ICalculatorStrategy
    {
        double Calcurate(List<IndexRisk> indexs);
    }

    public enum CalculationType
    {
        [EnumCode("1")]
        ByWeight,

        [EnumCode("2")]
        MultiRate
    }
    public enum ScoreValueBase
    {
        [EnumCode("004")]
        ByWeight,

        [EnumCode("005")]
        MultiRate
    }
    public enum RateCalculationType
    {
        [EnumCode("1")]
        TenDevided,
        [EnumCode("2")]
        FourDevided,
    }

    public class ByWeightCalculator : ICalculatorStrategy
    {
        public double Calcurate(List<IndexRisk> indexs)
        {
            //AssertUtils.ArgumentHasElements(indexs, "IndexObj");

            // ByWeight, multiple index rate by index weight then add together.
            double finalIndex = 0;
            indexs.ForEach(index =>
            {
                if (index.IndexWeight == 0)
                {
                    Exception ex = new OTCServiceException("Weight is zero while calculation by weight. Please check with administrator!");
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }

                finalIndex += index.Risk * (index.IndexWeight / 100);
            });

            return finalIndex;
        }
    }

    public class MultiRateCalculator : ICalculatorStrategy
    {
        public double Calcurate(List<IndexRisk> indexs)
        {
            double finalIndex = 1;
            indexs.ForEach(index =>
            {
                finalIndex *= index.Risk;
            });

            return finalIndex;
        }
    }

    public static class RiskRuleIndexExtention
    {
        public static void ApplyDevidedMethodTo(this RiskRuleIndex index, CustomerRiskCollection custColl)
        {
            List<IndexRisk> indexs = custColl.GetIndexByName(index.IndexName);
            List<double> indexRates = new List<double>();
            List<double> indexValues = indexs.Select(i => i.Value).ToList();

            // input with valus, output with risk rate.
            // the devide logic should ensure the output order same with input order.
            switch (Helper.CodeToEnum<RateCalculationType>(index.IndexRateMethod.ToString()))
            {
                case RateCalculationType.TenDevided:
                    indexRates = getValueInTenDevided(indexValues, index.IndexRateValue);
                    break;
                case RateCalculationType.FourDevided:
                    indexRates = getValueInFourDevided(indexValues, index.IndexRateValue);
                    break;
                default:
                    throw new OTCServiceException("Not a supported rate calculation type provided! Type:" + index.IndexRateMethod.ToString());
            }

            custColl.SetRatesByName(index.IndexName, indexRates);
        }

        /// <summary>
        /// ten devided method
        /// </summary>
        /// <param name="indexValue"></param>
        /// <returns></returns>
        private static List<double> getValueInTenDevided(List<double> indexValue, double rateValue)
        {
            double ql = 0;
            double q2 = 0;
            double q3 = 0;
            double q4 = 0;
            double q5 = 0;
            double q6 = 0;
            double q7 = 0;
            double q8 = 0;
            double q9 = 0;

            List<double> rate = new List<double>();

            ql = MathNet.Numerics.Statistics.Statistics.QuantileCustom(indexValue, 0.10, QuantileDefinition.Excel);
            q2 = MathNet.Numerics.Statistics.Statistics.QuantileCustom(indexValue, 0.20, QuantileDefinition.Excel);
            q3 = MathNet.Numerics.Statistics.Statistics.QuantileCustom(indexValue, 0.30, QuantileDefinition.Excel);
            q4 = MathNet.Numerics.Statistics.Statistics.QuantileCustom(indexValue, 0.40, QuantileDefinition.Excel);
            q5 = MathNet.Numerics.Statistics.Statistics.QuantileCustom(indexValue, 0.50, QuantileDefinition.Excel);
            q6 = MathNet.Numerics.Statistics.Statistics.QuantileCustom(indexValue, 0.60, QuantileDefinition.Excel);
            q7 = MathNet.Numerics.Statistics.Statistics.QuantileCustom(indexValue, 0.70, QuantileDefinition.Excel);
            q8 = MathNet.Numerics.Statistics.Statistics.QuantileCustom(indexValue, 0.80, QuantileDefinition.Excel);
            q9 = MathNet.Numerics.Statistics.Statistics.QuantileCustom(indexValue, 0.90, QuantileDefinition.Excel);

            for (int i = 0; i < indexValue.Count; i++)
            {
                if (indexValue[i] <= ql)
                {
                    rate.Add(rateValue / 10 * 1);
                }
                else if (indexValue[i] > ql && indexValue[i] <= q2)
                {
                    rate.Add(rateValue / 10 * 2);
                }
                else if (indexValue[i] > q2 && indexValue[i] <= q3)
                {
                    rate.Add(rateValue / 10 * 3);
                }
                else if (indexValue[i] > q3 && indexValue[i] <= q4)
                {
                    rate.Add(rateValue / 10 * 4);
                }
                else if (indexValue[i] > q4 && indexValue[i] <= q5)
                {
                    rate.Add(rateValue / 10 * 5);
                }
                else if (indexValue[i] > q5 && indexValue[i] <= q6)
                {
                    rate.Add(rateValue / 10 * 6);
                }
                else if (indexValue[i] > q6 && indexValue[i] <= q7)
                {
                    rate.Add(rateValue / 10 * 7);
                }
                else if (indexValue[i] > q7 && indexValue[i] <= q8)
                {
                    rate.Add(rateValue / 10 * 8);
                }
                else if (indexValue[i] > q8 && indexValue[i] <= q9)
                {
                    rate.Add(rateValue / 10 * 9);
                }
                else if (indexValue[i] > q9)
                {
                    rate.Add(rateValue / 10 * 10);
                }
                else
                {
                    rate.Add(0);
                }
            }

            return rate;
        }

        private static List<double> getValueInFourDevided(List<double> indexValue, double rateValue)
        {
            double ql = 0;
            double q2 = 0;
            double q3 = 0;
            List<double> rate = new List<double>();

            ql = MathNet.Numerics.Statistics.Statistics.QuantileCustom(indexValue, 0.25, QuantileDefinition.Excel);
            q2 = MathNet.Numerics.Statistics.Statistics.QuantileCustom(indexValue, 0.5, QuantileDefinition.Excel);
            q3 = MathNet.Numerics.Statistics.Statistics.QuantileCustom(indexValue, 0.75, QuantileDefinition.Excel);

            for (int i = 0; i < indexValue.Count; i++)
            {
                if (indexValue[i] <= ql)
                {
                    rate.Add(rateValue / 4 * 1);
                }
                else if (indexValue[i] > ql && indexValue[i] <= q2)
                {
                    rate.Add(rateValue / 4 * 2);
                }
                else if (indexValue[i] > q2 && indexValue[i] <= q3)
                {
                    rate.Add(rateValue / 4 * 3);
                }
                else if (indexValue[i] > q3)
                {
                    rate.Add(rateValue / 4 * 4);
                }
                else
                {
                    rate.Add(0);
                }

            }

            return rate;
        }


        public static double GetValue(this RiskRuleIndex index, List<CustomerAging> accountsAging, List<InvoiceAging> invoiceAging)
        {
            int invoiceAge;
            double totalAge = 0;
            double totalAmount = 0;
            decimal totalInvoiceOver60 = 0;
            decimal totalInvoice = 0;
            try
            {
                switch (index.IndexName)
                {
                    case "Amount":
                        // get amount by customer group
                        if (accountsAging != null && accountsAging.Count > 0)
                        {
                            accountsAging.ForEach(a =>
                            {
                                totalAmount += Convert.ToDouble(a.TotalAmt);
                            }
                            );
                            return totalAmount;
                        }
                        else
                        {
                            return 0;
                        }
                        break;
                    case "Age":
                        // get avg age by customer group

                        if (invoiceAging != null && invoiceAging.Count > 0)
                        {
                            foreach (var invo in invoiceAging)
                            {
                                invoiceAge = (AppContext.Current.User.Now - Convert.ToDateTime(invo.DueDate)).Days;
                                if (invoiceAge < 0)
                                {
                                    invoiceAge = 0;
                                }
                                totalAge += invoiceAge;

                            }
                            return totalAge / invoiceAging.Count();
                        }
                        else
                        {
                            return 0;
                        }

                        break;
                    case "OverDuePercentage":
                        // get OverDuePercentage by customer group
                        if (invoiceAging != null && invoiceAging.Count > 0)
                        {
                            foreach (var invo in invoiceAging)
                            {
                                if ((AppContext.Current.User.Now - Convert.ToDateTime(invo.DueDate)).Days >= 60)
                                {
                                    totalInvoiceOver60 += (Convert.ToDecimal(invo.BalanceAmt));

                                }
                                totalInvoice += Convert.ToDecimal(invo.BalanceAmt);
                            }
                            if (totalInvoice != 0)
                            {
                                return Convert.ToDouble(totalInvoiceOver60 / totalInvoice);
                            }

                        }
                        else
                        {
                            return 0;
                        }

                        break;
                    default:
                        return 0;
                        break;
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
            return 0;
        }
    }

    public class RiskCalculatorContext
    {
        public List<Customer> AllCustomers { get; set; }

        public List<CustomerAging> CurrentAccountLevelAging { get; set; }

        public List<InvoiceAging> CurrentInvoiceLevelAging { get; set; }
    }

    public class CustomerWithRisk
    {
        public string CustNum { get; set; }
        public Dictionary<string, IndexRisk> IndexRisks { get; set; }
    }

    public class IndexRisk
    {
        public double IndexWeight { get; set; }

        public double Value { get; set; }

        public double Risk { get; set; }
    }

    public class CustomerRiskCollection : List<CustomerWithRisk>
    {
        public CustomerWithRisk this[string custNum]
        {
            get
            {
                return this.Find(custs => custs.CustNum == custNum);
            }
        }
        public List<IndexRisk> GetIndexByName(string name)
        {
            List<IndexRisk> res = new List<IndexRisk>();
            this.ForEach(vr =>
                {
                    if (vr.IndexRisks.ContainsKey(name))
                    {
                        res.Add(vr.IndexRisks[name]);
                    }
                });

            return res;
        }

        public void SetRatesByName(string name, List<double> rates)
        {
            int i = 0;
            this.ForEach(vr =>
            {
                if (vr.IndexRisks.ContainsKey(name))
                {
                    vr.IndexRisks[name].Risk = rates[i];
                    i++;
                }
            });
        }
    }

    public class RiskC
    {
        public Int64 Id { get; set; }
        public string CustomerNum { get; set; }
        public string BillGroupCode { get; set; }
        public decimal Amt { get; set; }
        public int Age { get; set; }
        public decimal OverduePercent { get; set; }
    }

}
