using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;
using System.Web;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class Mail : IAggregateRoot
    {

    }
    public partial class MailTmp : IAggregateRoot
    {
        public const char ATTACHMENT_SPLITER = ',';

        public string Raw { get; set; }
        public List<string> Attachments
        {
            get
            {
                if (this.Attachment != null)
                {
                    return this.Attachment.Split(ATTACHMENT_SPLITER).ToList();
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        public DateTime? MailTime
        {
            get
            {
                if (Type == "IN")
                {
                    if (InternalDatetime == null)
                    {
                        return null;
                    }
                    else
                    {
                        return InternalDatetime.Value;
                    }
                }
                else
                {
                    return CreateTime;
                }
            }
        }

        public string soaFlg { get; set; }
        public int[] invoiceIds { get; set; }
        public string DisplayName { get; set; }
        public string Comments { get; set; }
        //store Mail type ex: SOA:  001,SOA; Dunning: 002,Second Reminder Sent; 003,Final Reminder Sent;
        public string MailType { get; set; }
    }

    public class MailDto
    {
        public int Id { get; set; }
        public string Deal { get; set; }
        public string Bussiness_Reference { get; set; }
        public string Subject { get; set; }
        public string BodyFormat { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Attachment { get; set; }
        public string SavePath { get; set; }
        public string Operator { get; set; }
        public string CustomerAssignedFlg { get; set; }
        public string Category { get; set; }
        public System.DateTime CreateTime { get; set; }
        public Nullable<System.DateTime> UpdateTime { get; set; }
        public string Type { get; set; }
        public string Collector { get; set; }
        public string MessageId { get; set; }
        public Nullable<long> InternalTime { get; set; }
        public string MailBox { get; set; }
        public Nullable<System.DateTime> InternalDatetime { get; set; }
        public string FileId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNum { get; set; }
        public string SiteUseId { get; set; }
        public IEnumerable<CustomerMail> CustomerMails { get; set; }

        public DateTime? MailTime { get; set; }

        //TODO: Just for backward compatbility. 
        public string soaFlg { get; set; }
        public int[] invoiceIds { get; set; }
        public string DisplayName { get; set; }
        public string Comments { get; set; }
        //store Mail type ex: SOA:  001,SOA; Dunning: 002,Second Reminder Sent; 003,Final Reminder Sent;
        public string MailType { get; set; }

        public string Status { get; set; }
        public string Body { get; set; }
    }


    public class MailDtoPage
    {
        public List<MailDto> mailList;
        public int listCount;
    }
}