using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.S3;
using Amazon.SimpleEmail;
using Apper.Aws.Models;
using Orchard.Caching;
using Orchard.Data;

namespace Apper.Aws.Services
{
    public class DefaultAwsClientManager : IAwsClientManager
    {
        AmazonS3 _s3Client;
        AmazonSimpleEmailService _sesClient;

        private readonly IAwsSettingService _awsSettingService;

        public DefaultAwsClientManager(IAwsSettingService awsSettingService)
        {
            _awsSettingService = awsSettingService;
        }
        #region IAwsS3Manager Members

        public AmazonS3 S3Client
        {
            get
            {
                if (_s3Client == null)
                {
                    var item = _awsSettingService.GetAwsSettings();
                    if (item == null)
                    {
                        return null;
                    }
                    string acckey = item.AccessKey;
                    string secKey = item.SecretKey;

                    _s3Client = Amazon.AWSClientFactory.CreateAmazonS3Client(acckey, secKey); //,RegionEndpoint.USWest2
                }
                return _s3Client;
            }
        }


        public AmazonSimpleEmailService SesClient
        {
            get
            {
                if (_sesClient == null)
                {
                    var item = _awsSettingService.GetAwsSettings();
                    if (item == null)
                    {
                        return null;
                    }

                    string acckey = item.AccessKey;
                    string secKey = item.SecretKey;

                    _sesClient = Amazon.AWSClientFactory.CreateAmazonSimpleEmailServiceClient(acckey, secKey); //,RegionEndpoint.USWest2
                }
                return _sesClient;

            }
        }

        #endregion     
    }
}