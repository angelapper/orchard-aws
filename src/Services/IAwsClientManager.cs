using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.S3;
using Amazon.SimpleEmail;
using Orchard;

namespace Apper.Aws.Services
{
    public interface IAwsClientManager : ISingletonDependency
    {
        AmazonS3 S3Client { get; }
        AmazonSimpleEmailService SesClient { get; }
    }
}
