using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class AppFile : IAggregateRoot
    {
        public Stream GetFileStream()
        {
            MemoryStream fileStream = new MemoryStream();
            if (File.Exists(this.PhysicalPath))
            {
                using (FileStream fs = File.OpenRead(this.PhysicalPath))
                {
                    fs.CopyTo(fileStream);
                }
            }
            else
            {
                throw new OTCServiceException("Get file failed because file not exist with physical path: " + this.PhysicalPath);
            }
            return fileStream;
        }
    }
}
