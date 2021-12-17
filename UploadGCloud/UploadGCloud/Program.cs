using CommandDotNet;
using Google.Apis.Upload;
using Google.Cloud.Storage.V1;
using HeyRed.Mime;

namespace UploadGcloud
{
    public sealed class Program
    {
        public static int Main(string[] args)
        {
            return new AppRunner<Program>().Run(args);
        }

        [Command("upload", Description = "Uploads a file to gcloud.")]
        public async Task Upload(
            [Operand(Description = "The path to the file.")] string path,
            [Operand(Description = "The name of the bucket.")] string bucket,
            [Option(Description = "The optional object name.")] string? objectName)
        {
            using var storageClient = await StorageClient.CreateAsync();

            var fileInfo = new FileInfo(path);
            var fileStream = fileInfo.OpenRead();

            if (string.IsNullOrEmpty(objectName))
            {
                objectName = fileInfo.Name;
            }

            var mimeType = MimeTypesMap.GetMimeType(fileInfo.Name);

            await storageClient.UploadObjectAsync(bucket, objectName, mimeType, fileStream, null, default, new ProgressReporter(fileInfo.Length));
        }

        class ProgressReporter : IProgress<IUploadProgress>
        {
            private readonly long fileSize;
            private long previous;
            private long position;

            public ProgressReporter(long fileSize)
            {
                position = Console.GetCursorPosition().Top;

                this.fileSize = fileSize;
            }

            public void Report(IUploadProgress value)
            {
                var progress = (long)(100 * ((double)value.BytesSent / fileSize));

                if (progress != previous)
                {
                    Console.SetCursorPosition(0, (int)position);
                    Console.WriteLine("Progress: {0:000}%", progress);

                    previous = progress;
                }
            }
        }
    }
}