using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apper.Aws.Models;
using Orchard;

namespace Apper.Aws.Services
{
    public interface IAwsSettingService: IDependency 
    {
        AwsParaRecord GetAwsSettings();
        string GetBucketName(string bucketType);
    }
}
