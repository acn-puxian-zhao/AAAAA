using System;
using System.Collections.Generic;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.Repositories;
namespace Intelligent.OTC.Business
{
    public interface IMailTemplateResolver
    {
        MailTemplateType Type { get; set; }

        void Resolve(MailTemplate template);
    }
}
