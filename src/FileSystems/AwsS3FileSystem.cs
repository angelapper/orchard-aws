using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.FileSystems.Media;
using Amazon.S3;
using Amazon.S3.IO;
using Amazon.S3.Model;
using System.IO;
using Apper.Aws.Services;

namespace Apper.Aws.FileSystems
{
    public class AwsS3FileSystem
    {
        //https://s3.amazonaws.com/{BucketName}/{ContainerName}/{tenantRoot}/0dc116d.jpg
        //example:https://s3.amazonaws.com/genie0/media/daycare1/0dc116d.jpg
        //but the key should follow Path.DirectorySeparatorChar
        private readonly string BucketName;
        private S3DirectoryInfo s3FileRootDirectory=null;
        private readonly IAwsClientManager awss3Manager;
        private readonly IAwsSettingService _awsSettingsService;
        protected readonly string rootUrlPrefix;
        private readonly string basePath;
        private readonly string tenantRoot;
        protected readonly string absoluteUrlRoot;
        public const string FolderEntry = "$$$Cloudapper$$$.$$$";

        public string ContainerName { get; protected set; }

        public AwsS3FileSystem(string storageConnectionString, IAwsSettingService awsSettingsService,IAwsClientManager s3Manager, string moduleRootFolder, string tenantRootFolder)
        {
            _awsSettingsService = awsSettingsService;

            try
            {
                BucketName = _awsSettingsService.GetBucketName(storageConnectionString);
            }
            catch (System.Exception e)
            {
                BucketName = "";
            }

            awss3Manager = s3Manager;

            if (s3FileRootDirectory == null && awss3Manager.S3Client!=null)
            {
                s3FileRootDirectory = new S3DirectoryInfo(awss3Manager.S3Client, BucketName.ToLowerInvariant());
            }

            rootUrlPrefix = string.Format("https://{0}.s3.amazonaws.com/", BucketName.ToLowerInvariant());//s3FileRootDirectory.FullName;
            //string folderPublicurl = s3FileRootDirectory.ToString();
            ContainerName = moduleRootFolder.ToLowerInvariant();
            tenantRoot = tenantRootFolder.ToLowerInvariant();
            var tem = Fix(Combine(ContainerName, tenantRoot)).Trim('/');
            basePath = s3FileRootDirectory==null?"":Path.Combine(s3FileRootDirectory.FullName, tem) + Path.DirectorySeparatorChar;
            absoluteUrlRoot = Combine(Combine(rootUrlPrefix, ContainerName), tenantRoot);
        }

        private static string EnsurePathIsRelative(string path)
        {
            path = path ?? String.Empty;

            var newPath = path.Replace(@"\", "/");

            if (newPath.StartsWith("/") || newPath.StartsWith("http://") || newPath.StartsWith("https://"))
            {
                throw new ArgumentException("Path must be relative");
            }

            return newPath;
        }

        private string MapStorageKey(string path)
        {
            string mappedPath = string.IsNullOrEmpty(path) ? absoluteUrlRoot : Combine(absoluteUrlRoot, path);
            return UrlValidatePath(absoluteUrlRoot, mappedPath);
        }

        /// <summary>
        /// Validate the url path which do not allow go beyond the tenant root
        /// </summary>
        /// <param name="baseUrlPath"></param>
        /// <param name="mappedPath"></param>
        /// <returns></returns>
        private string UrlValidatePath(string baseUrlPath, string mappedPath)
        {
            bool valid = false;
            string storageKey = String.Empty;
            try
            {
                // Check that we are indeed within the storage directory boundaries
                storageKey = new Uri(mappedPath).AbsoluteUri;
                valid = storageKey.StartsWith(baseUrlPath, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                // Make sure that if invalid for medium trust we give a proper exception
                valid = false;
            }

            if (!valid)
            {
                throw new ArgumentException("Invalid path");
            }

            if (valid)
            {
                return storageKey.Remove(0, rootUrlPrefix.Length).Trim('/').Replace('/', Path.DirectorySeparatorChar).ToLowerInvariant();
            }

            return String.Empty;
        }

        public string Combine(string path1, string path2)
        {
            if (path1 == null)
            {
                throw new ArgumentNullException("path1");
            }

            if (path2 == null)
            {
                throw new ArgumentNullException("path2");
            }

            if (String.IsNullOrEmpty(path2))
            {
                return path1;
            }

            if (String.IsNullOrEmpty(path1))
            {
                return path2;
            }

            if (path2.StartsWith("http://") || path2.StartsWith("https://"))
            {
                return path2;
            }

            var ch = path1[path1.Length - 1];

            if (ch != '/')
            {
                return (path1.TrimEnd('/') + '/' + path2.TrimStart('/'));
            }

            return (path1 + path2);
        }

        private string Fix(string path)
        {
            return string.IsNullOrEmpty(path)
                       ? ""
                       : Path.DirectorySeparatorChar != '/'
                             ? path.Replace('/', Path.DirectorySeparatorChar)
                             : path;
        }

        public IStorageFile GetFile(string path)
        {
            path=EnsurePathIsRelative(path);

            var file =
                new S3FileInfo(awss3Manager.S3Client, BucketName, CalculateObjKey(path));

            if (file.Exists)
            {
                return new AwsS3StorageFile(file, basePath);
            }
            throw new ArgumentException("File {0} does not exist", path);

            //return null;
        }

        private string CalculateObjKey(string relativePath)
        {
            //string relativeUri= Combine(Combine(ContainerName, tenantRoot), relativePath);
            string key = MapStorageKey(relativePath);
            return key;
        }

        public bool FileExists(string path)
        {
            path = EnsurePathIsRelative(path);

            var file =
                new S3FileInfo(awss3Manager.S3Client, BucketName, CalculateObjKey(path));
            return file.Exists;
        }

        public bool FolderExists(string path)
        {
            path = EnsurePathIsRelative(path);

            var folder =
                new S3DirectoryInfo(awss3Manager.S3Client, BucketName, CalculateObjKey(path));
            return folder.Exists;
        }

        public IEnumerable<IStorageFile> ListFiles(string path)
        {
            path = path ?? String.Empty;

            path = EnsurePathIsRelative(path);

            var dir =
                new S3DirectoryInfo(awss3Manager.S3Client, BucketName, CalculateObjKey(path));

            return dir.GetFiles().Select(c => new AwsS3StorageFile(c, basePath)).ToArray();
        }

        public IEnumerable<IStorageFolder> ListFolders(string path)
        {
            //path = EnsurePathIsRelative(path);

            var dir = new S3DirectoryInfo(awss3Manager.S3Client, BucketName, CalculateObjKey(path));

            return dir.GetDirectories().Select(d => new AwsS3StorageFolder(d, basePath)).ToArray();
        }

        public bool TryCreateFolder(string path)
        {
            try
            {
                var dir = new S3DirectoryInfo(awss3Manager.S3Client, BucketName,
                    CalculateObjKey(path));
                if (dir.Exists)
                {
                    return false;
                }

                CreateFolder(path);
            }
            catch
            {
                return false;
            }
            return true;
        }


        public void CreateFolder(string path)
        {
            var dir = new S3DirectoryInfo(awss3Manager.S3Client, BucketName,
                CalculateObjKey(path));
            if (!dir.Exists)
            {
                dir.Create();
            }
        }

        public void DeleteFolder(string path)
        {
            var dir = new S3DirectoryInfo(awss3Manager.S3Client, BucketName,
                CalculateObjKey(path));
            if (dir.Exists)
            {
                dir.Delete();
            }
        }

        public void RenameFolder(string oldPath, string newPath)
        {
            var dir = new S3DirectoryInfo(awss3Manager.S3Client, BucketName,
                CalculateObjKey(oldPath));
            if (dir.Exists)
            {
                var newFolder = new S3DirectoryInfo(awss3Manager.S3Client, BucketName, CalculateObjKey(newPath));
                newFolder.Create();
                dir.MoveTo(newFolder);
            }
        }

        public void DeleteFile(string path)
        {
            var file = new S3FileInfo(awss3Manager.S3Client, BucketName,
                CalculateObjKey(path));
            if (file.Exists)
            {
                file.Delete();
            }
        }

        public void RenameFile(string oldPath, string newPath)
        {
            var file = new S3FileInfo(awss3Manager.S3Client, BucketName,
                CalculateObjKey(oldPath));
            if (file.Exists)
            {
                file.MoveTo(new S3FileInfo(awss3Manager.S3Client, BucketName,
                CalculateObjKey(newPath)));
            }
        }

        public IStorageFile CreateFile(string path)
        {
            path = EnsurePathIsRelative(path);

            var file = new S3FileInfo(awss3Manager.S3Client, BucketName,
                CalculateObjKey(path));

            //             int lastIndex;
            //             var localPath = path;
            //             while ((lastIndex = localPath.LastIndexOf('/')) > 0)
            //             {
            //                 localPath = localPath.Substring(0, lastIndex);
            //                 var folder = new S3DirectoryInfo(awss3Manager.Client, BucketName,
            //                 CalculateObjKey(FolderEntry));
            //                 if (!folder.Exists)
            //                 {
            //                     folder.Create();
            //                 }
            //             }

            if (!file.Exists)
            {
                using (var writer = file.CreateText())
                {
                    writer.WriteLine("ddd");
                }
            }
            else
            {
                throw new ArgumentException("File " + path + " already exists");
            }

            return new AwsS3StorageFile(file, basePath);
        }

        public string GetPublicUrl(string path)
        {
            path = EnsurePathIsRelative(path);

            string publicUrl = String.Empty;

            var file = GetFile(path);

            if (!String.IsNullOrEmpty(file.GetPath()))
            {
                publicUrl = new Uri(Combine(rootUrlPrefix, CalculateObjKey(path))).AbsoluteUri;
            }

            return publicUrl;
        }

        private class AwsS3StorageFile : IStorageFile
        {
            private readonly S3FileInfo s3File;
            private readonly string _rootPath;

            public AwsS3StorageFile(S3FileInfo fileObj, string rootRelativePath)
            {
                s3File = fileObj;
                _rootPath = rootRelativePath;
            }
            public string GetPath()
            {
                return s3File.FullName.Remove(0, _rootPath.Length);
            }

            public string GetName()
            {
                return s3File.Name;
            }

            public long GetSize()
            {
                return s3File.Length;
            }

            public DateTime GetLastUpdated()
            {
                return s3File.LastWriteTimeUtc;
            }

            public string GetFileType()
            {
                return s3File.Extension;
            }

            public System.IO.Stream OpenRead()
            {
                return s3File.OpenRead();
            }

            public System.IO.Stream OpenWrite()
            {
                return s3File.OpenWrite();
            }

            public System.IO.Stream CreateFile()
            {
                return s3File.Create();
            }
        }

        private class AwsS3StorageFolder : IStorageFolder
        {
            private readonly S3DirectoryInfo dir;
            private readonly string _rootPath;

            public AwsS3StorageFolder(S3DirectoryInfo d, string rootRelativePath)
            {
                dir = d;
                _rootPath = rootRelativePath;
            }

            public string GetPath()
            {
                return dir.FullName.Remove(0, _rootPath.Length).Trim(Path.DirectorySeparatorChar);
            }

            public string GetName()
            {
                return dir.Name;
            }

            public long GetSize()
            {
                return GetDirectorySize(dir);
            }

            private long GetDirectorySize(S3DirectoryInfo dir)
            {
                long size = 0;

                foreach (var item in dir.GetFileSystemInfos())
                {
                    if (item is S3FileInfo)
                        size += (item as S3FileInfo).Length;

                    if (item is S3DirectoryInfo)
                        size += GetDirectorySize((S3DirectoryInfo)item);
                }

                return size;

            }

            public DateTime GetLastUpdated()
            {
                return dir.LastWriteTimeUtc;
            }

            public IStorageFolder GetParent()
            {
                return new AwsS3StorageFolder(dir.Parent, _rootPath);
            }
        }
    }
}