using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using AWSHelpers.S3.Interfaces;
using Core.Cache.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AWSHelpers.S3.Implementation
{
    public class S3Helper : IS3Helper
    {
        private const string CACHE_KEY = "S3GovData";
        private readonly IAmazonS3 _amazonS3;

        private readonly ICacheProvider _cache;

        public S3Helper(IAmazonS3 amazonS3, ICacheProvider cache)
        {
            _amazonS3 = amazonS3;
            _cache = cache;
        }

        public async Task<PutBucketResponse> CreateABucket(string bucketName)
        {
            var bucketCache = await _cache.GetOrCreateAsync(CACHE_KEY);

            if (string.IsNullOrEmpty(bucketCache))
            {
                var buckets = await ListingBuckets();
                if (buckets.HttpStatusCode == HttpStatusCode.OK &&
                    !buckets.Buckets.Any(x => x.BucketName == bucketName))
                {
                    PutBucketRequest request = new()
                    {
                        BucketName = bucketName
                    };

                    var result = await _amazonS3.PutBucketAsync(request);
                    if (result.HttpStatusCode == HttpStatusCode.OK)
                    {
                        await _cache.GetOrCreateAsync(CACHE_KEY, bucketName);
                        return result;
                    }
                }
            }
            return null;
        }

        public async Task<bool> UploadFileAsync(string bucketName, Stream fileStream, string fileName, string directory = null)
        {
            try
            {
                var bucketPath = !string.IsNullOrWhiteSpace(directory)
                    ? bucketName + @"/" + directory
                    : bucketName;

                var fileTransferUtility = new TransferUtility(_amazonS3);

                var fileUploadRequest = new TransferUtilityUploadRequest()
                {
                    CannedACL = S3CannedACL.PublicRead,
                    BucketName = bucketPath,
                    Key = fileName,
                    InputStream = fileStream
                };
                fileUploadRequest.UploadProgressEvent += (sender, args) =>
                    Console.WriteLine($"{args.FilePath} upload complete : {args.PercentDone}%");
                await fileTransferUtility.UploadAsync(fileUploadRequest);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// uploads file to s3 bucket from specified file path
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        public async Task<bool> UploadFileAsync(string bucketName, string filePath, string directory = null)

        {
            try
            {
                var fileTransferUtility = new TransferUtility(_amazonS3);
                var bucketPath = !string.IsNullOrWhiteSpace(directory)
                    ? bucketName + @"/" + directory
                    : bucketName;
                // 1. Upload a file, file name is used as the object key name.
                var fileUploadRequest = new TransferUtilityUploadRequest()
                {
                    CannedACL = S3CannedACL.PublicRead,
                    BucketName = bucketPath,
                    FilePath = filePath,
                };
                fileUploadRequest.UploadProgressEvent += (sender, args) =>
                    Console.WriteLine($"{args.FilePath} upload complete : {args.PercentDone}%");
                await fileTransferUtility.UploadAsync(fileUploadRequest);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return false;
            }
        }

        /// <summary>
        /// writes file to s3 bucket using specified contents, content type
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="contentType"></param>
        /// <param name="fileName"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        public async Task<bool> UploadFileAsync(string bucketName, string contents, string contentType, string fileName, string directory = null)
        {
            try
            {
                var bucketPath = !string.IsNullOrWhiteSpace(directory)
                    ? bucketName + @"/" + directory
                    : bucketName;
                //1. put object
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketPath,
                    Key = fileName,
                    ContentBody = contents,
                    ContentType = contentType,
                    CannedACL = S3CannedACL.PublicRead
                };
                var response = await _amazonS3.PutObjectAsync(putRequest);
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// returns file stream from s3 bucket
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        public async Task<(Stream FileStream, string ContentType)> ReadFileAsync(string bucketName, string fileName, string directory = null)
        {
            try
            {
                var fileTransferUtility = new TransferUtility(_amazonS3);
                var bucketPath = !string.IsNullOrWhiteSpace(directory)
                    ? bucketName + @"/" + directory
                    : bucketName;
                var request = new GetObjectRequest()
                {
                    BucketName = bucketPath,
                    Key = fileName
                };
                // 1. read files
                var objectResponse = await fileTransferUtility.S3Client.GetObjectAsync(request);
                return (objectResponse.ResponseStream, objectResponse.Headers.ContentType);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return (null, null);
            }
        }

        /// <summary>
        /// returns files from s3 folder
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public async Task<List<(Stream FileStream, string fileName, string ContentType)>> ReadDirectoryAsync(string bucketName, string directory)
        {
            var objectCollection = new List<(Stream, string, string)>();
            try
            {
                var fileTransferUtility = new TransferUtility(_amazonS3);
                var request = new ListObjectsRequest()
                {
                    BucketName = bucketName,
                    Prefix = directory
                };
                // 1. read files
                var objectResponse = await fileTransferUtility.S3Client.ListObjectsAsync(request);
                foreach (var entry in objectResponse.S3Objects)
                {
                    var fileName = entry.Key.Split('/').Last();
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        var response = await ReadFileAsync(fileName, directory);
                        objectCollection.Add((response.FileStream, fileName, response.ContentType));
                    }
                }

                return objectCollection;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return objectCollection;
            }
        }

        /// <summary>
        /// moves file (object) between bucket folders
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sourceDirectory"></param>
        /// <param name="destDirectory"></param>
        /// <returns></returns>
        public async Task<bool> MoveFileAsync(string bucketName, string fileName, string sourceDirectory, string destDirectory)
        {
            try
            {
                var copyRequest = new CopyObjectRequest
                {
                    SourceBucket = bucketName + @"/" + sourceDirectory,
                    SourceKey = fileName,
                    DestinationBucket = bucketName + @"/" + destDirectory,
                    DestinationKey = fileName
                };
                var response = await _amazonS3.CopyObjectAsync(copyRequest);
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    var deleteRequest = new DeleteObjectRequest
                    {
                        BucketName = bucketName + @"/" + sourceDirectory,
                        Key = fileName
                    };
                    await _amazonS3.DeleteObjectAsync(deleteRequest);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// removes file from s3 bucket
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        public async Task<bool> RemoveFileAsync(string buckName, string fileName, string directory = null)
        {
            try
            {
                var fileTransferUtility = new TransferUtility(_amazonS3);
                var bucketPath = !string.IsNullOrWhiteSpace(directory)
                    ? buckName + @"/" + directory
                    : buckName;

                // 1. deletes files
                await fileTransferUtility.S3Client.DeleteObjectAsync(new DeleteObjectRequest()
                {
                    BucketName = bucketPath,
                    Key = fileName
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<ListBucketsResponse> ListingBuckets()
        {
            return await _amazonS3.ListBucketsAsync();
        }
    }
}