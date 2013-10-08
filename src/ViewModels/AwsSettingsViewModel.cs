using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Apper.Aws.ViewModels
{
    public class AwsSettingsViewModel
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string FileBucket { get; set; }
        public string LoggingBucket { get; set; }
    }
}