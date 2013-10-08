using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Apper.Aws.Services;

namespace Apper.Aws.FileSystems.Media
{
    [OrchardFeature(Constants.MediaStorageFeatureName)]
    [OrchardSuppressDependency("Orchard.FileSystems.Media.FileSystemStorageProvider")]
    public class AwsS3FileSystemStorageProvider : AwsS3FileSystem, IStorageProvider
    {
        private readonly string _storagePath;
        private readonly string _publicPath;

        public AwsS3FileSystemStorageProvider(IAwsSettingService settingService,IAwsClientManager awsS3Manager, ShellSettings settings) :
            base(Constants.MediaStorageStorageConnectionStringSettingName, settingService,awsS3Manager, Constants.MediaStorageContainerName, settings.Name)
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool TrySaveStream(string path, Stream inputStream)
        {
            try
            {
                SaveStream(path, inputStream);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public void SaveStream(string path, Stream inputStream)
        {
            // Create the file.
            // The CreateFile method will map the still relative path
            var file = CreateFile(path);

            using (var outputStream = file.OpenWrite())
            {
                var buffer = new byte[8192];
                for (; ; )
                {
                    var length = inputStream.Read(buffer, 0, buffer.Length);
                    if (length <= 0)
                        break;
                    outputStream.Write(buffer, 0, length);
                }
            }
        }

        /// <summary>
        /// Retrieves the local path for a given URL within the storage provider.
        /// </summary>
        /// <param name="url">The public URL of the media.</param>
        /// <returns>The corresponding local path.</returns>
        public string GetStoragePath(string url)
        {
            if (url.StartsWith(absoluteUrlRoot))
            {
                return url.Substring(Combine(absoluteUrlRoot, "/").Length);
            }

            return null;
        }

        public string GetRelativePath(string path)
        {
            return GetPublicUrl(path);
        }
    }
}