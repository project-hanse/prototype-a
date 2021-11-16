using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using FileService.Models.Dtos;
using FileService.Models.Requests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FileService.Services
{
	public class S3FilesService
	{
		private readonly ILogger<S3FilesService> _logger;
		private readonly IConfiguration _configuration;
		private readonly IAmazonS3 _s3Client;

		public S3FilesService(
			ILogger<S3FilesService> logger,
			IConfiguration configuration,
			IAmazonS3 s3Client)
		{
			_logger = logger;
			_configuration = configuration;
			_s3Client = s3Client;
		}

		private string GetBucketName(string userIdentifier)
		{
			return $"{_configuration.GetValue("S3Configuration:UserBucketPrefix", "users")}-{userIdentifier}".ToLower();
		}

		private async Task EnsureBucketExists(string bucketName)
		{
			_logger.LogDebug("Making sure bucket {BucketName} exists", bucketName);
			bool bucketExists;
			try
			{
				bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
			}
			catch (AmazonS3Exception e)
			{
				if (e.ErrorCode == "NotFound")
				{
					_logger.LogDebug(e, "Bucket {BucketName} not found", bucketName);
					bucketExists = false;
				}
				else
				{
					_logger.LogError(e, "Error checking if bucket {BucketName} exists", bucketName);
					throw;
				}
			}

			if (!bucketExists)
			{
				_logger.LogDebug("Bucket {BucketName} does not exist, creating", bucketName);

				var response = await _s3Client.PutBucketAsync(bucketName);

				if (response.HttpStatusCode == HttpStatusCode.OK)
				{
					_logger.LogInformation("Bucket {BucketName} created. {@BucketMetaData}", bucketName,
						response.ResponseMetadata);
				}
				else
				{
					_logger.LogWarning(
						"Unexpected status {HttpStatusCode} while creating new bucket {BucketName}, response {@PutBucketResponse}",
						response.HttpStatusCode, bucketName, response);
				}
			}
		}

		public async Task<FileInfoDto> UploadFile(UploadFileRequest request)
		{
			_logger.LogDebug("Uploading file to S3 store {@FileUploadRequest}", request);

			await EnsureBucketExists(GetBucketName(request.UserIdentifier));

			await _s3Client.UploadObjectFromStreamAsync(GetBucketName(request.UserIdentifier), request.FileName,
				request.File.OpenReadStream(), new Dictionary<string, object>());

			_logger.LogInformation("File {FileName} uploaded for user {UserIdentifier} to S3 store",
				request.FileName, request.UserIdentifier);

			return new FileInfoDto
			{
				FileName = request.FileName,
				LastModified = DateTime.UtcNow
			};
		}

		public async Task<IList<FileInfoDto>> GetFileInfosForUser(string userIdentifier)
		{
			_logger.LogDebug("Loading available file infos for user {UserIdentifier}", userIdentifier);

			await EnsureBucketExists(GetBucketName(userIdentifier));

			var bucketList = await _s3Client.ListObjectsAsync(new ListObjectsRequest
			{
				BucketName = GetBucketName(userIdentifier),
				MaxKeys = 1000
			});

			var fileInfos = bucketList.S3Objects
				.Select(o => new FileInfoDto
				{
					FileName = o.Key,
					LastModified = o.LastModified,
					Size = o.Size
				})
				.OrderByDescending(fi => fi.LastModified)
				.ToList();

			_logger.LogInformation("Loaded {FileCount} file info dtos for user {UserIdentifier}", fileInfos.Count,
				userIdentifier);

			return fileInfos;
		}
	}
}
