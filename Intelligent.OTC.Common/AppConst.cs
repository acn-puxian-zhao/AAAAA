using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Common
{
   public class AppConst
    {
        /// <summary>
        /// 计算因子
        /// </summary>
        public enum EnumAssessmentFactor
        {
            /// <summary>
            /// Late pay 金额/总交易金额
            /// </summary>
            LatePayAMTPercent = 1,
            /// <summary>
            /// 3个月内Late payInvoice张数/总Invoice张数
            /// </summary>
            LatePayInvPercent = 2,
            /// <summary>
            /// 3个月内发生late pay的Invoice张数
            /// </summary>
            LatePayInvTotal = 3
        }
        /// <summary>
        /// 客户类别
        /// </summary>
        public enum EnumAssessmentType
        {
            /// <summary>
            /// AMS优质客户
            /// </summary>
            AMSExcellent=1,
            /// <summary>
            /// AMS优良客户
            /// </summary>
            AMSGood = 2,
            /// <summary>
            /// AMS问题客户
            /// </summary>
            AMSIssue = 3,
            /// <summary>
            /// 非AMS优质客户
            /// </summary>
            NonAMSExcellent = 4,
            /// <summary>
            /// 非AMS优良客户
            /// </summary>
            NonAMSGood = 5,
            /// <summary>
            /// 非AMS问题客户
            /// </summary>
            NonAMSIssue = 6,
            /// <summary>
            /// 预付款客户
            /// </summary>
            Prepaid = 7,
            /// <summary>
            /// 新客户
            /// </summary>
            NewCustomer=8,
            /// <summary>
            /// 特殊优
            /// </summary>
            SpecialExcellent=9,
            /// <summary>
            /// 特殊差
            /// </summary>
            SpecialIssue = 10
        }
        /// <summary>
        /// 催收方式
        /// </summary>
        public enum EnumCommunicationMethod
        {
            /// <summary>
            /// 邮件
            /// </summary>
            Mail=1,
            /// <summary>
            /// 电话
            /// </summary>
            Phone=2,
            /// <summary>
            /// 邮件+电话
            /// </summary>
            MailPhone=3
        }
    }
}
