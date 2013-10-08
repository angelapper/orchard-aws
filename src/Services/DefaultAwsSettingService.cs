using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apper.Aws.Models;
using Orchard.Caching;
using Orchard.Data;

namespace Apper.Aws.Services
{
    public class DefaultAwsSettingService : IAwsSettingService
    {
        private readonly IRepository<AwsParaRecord> _repository;
//         private readonly ICacheManager _cacheManager;
//         private readonly ISignals _signals;
//         private const string SignalName = "Apper.Aws.ClientSettings.Bucket";
        //ICacheManager cacheManager, ISignals signals,

        public DefaultAwsSettingService(IRepository<AwsParaRecord> repository)
        {
//             _cacheManager = cacheManager;
//             _signals = signals;
            _repository = repository;
        }

        public AwsParaRecord GetAwsSettings()
        {
            var settings = _repository.Table.SingleOrDefault();
            if (settings == null) {
                _repository.Create(settings = new AwsParaRecord());
            }
            //TriggerSignal();
            return settings;
        }

//         private void MonitorSignal(AcquireContext<string> ctx)
//         {
//             ctx.Monitor(_signals.When(SignalName));
//         }
// 
//         private void TriggerSignal()
//         {
//             _signals.Trigger(SignalName);
//         }
// 

        public string GetBucketName(string bucketType)
        {
            /*return _cacheManager.Get(bucketType, ctx =>
            {
                MonitorSignal(ctx);
            });*/
            return GetBucketNameInner(bucketType);
        }

        private string GetBucketNameInner(string bucketType)
        {
            if (string.IsNullOrEmpty(bucketType))
            {
                return string.Empty;
            }
            var settings = _repository.Table.SingleOrDefault();

            if (settings ==null)
            {
                return string.Empty;
            }

            switch (bucketType)
            {
                case Constants.MediaStorageStorageConnectionStringSettingName:
                {
                    return settings.FileBucket;
                }
                case Constants.LoggingStorageStorageConnectionStringSettingName:
                {
                    return settings.LoggingBucket;
                }
                default: { return string.Empty; }
            }
        }

    }
}
