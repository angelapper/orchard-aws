using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Apper.Aws
{
    public class Constants
    {
        public const string ShellSettingsStorageConnectionStringSettingName = "Apper.Aws.Settings.StorageConnectionString";
        public const string ShellSettingsContainerName = "sites"; // Container names must be lower case.
        public const string ShellSettingsFileName = "Settings.txt";

        public const string MediaStorageFeatureName = "Apper.Aws";
        public const string MediaStorageStorageConnectionStringSettingName = "Apper.Aws.Media.StorageConnectionString";
        public const string MediaStorageContainerName = "media"; // Container names must be lower case.

        public const string LoggingStorageStorageConnectionStringSettingName = "Apper.Aws.Logging.StorageConnectionString";


//         public const string OutputCacheFeatureName = "Orchard.Azure.OutputCache";
//         public const string OutputCacheSettingNamePrefix = "Orchard.Azure.OutputCache.";
//         public const string DatabaseCacheFeatureName = "Orchard.Azure.DatabaseCache";
//         public const string DatabaseCacheSettingNamePrefix = "Orchard.Azure.DatabaseCache.";
// 
//         public const string CacheHostIdentifierSettingName = "HostIdentifier";
//         public const string CacheCacheNameSettingName = "CacheName";
//         public const string CacheAuthorizationTokenSettingName = "AuthorizationToken";

    }
}