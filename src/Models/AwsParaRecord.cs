using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Apper.Aws.Models
{
    public class AwsParaRecord
    {
        public virtual int Id { get; set; }
        public virtual string AccessKey { get; set; }
        public virtual string SecretKey { get; set; }
        public virtual string FileBucket { get; set; }
        public virtual string LoggingBucket { get; set; }
    }
}