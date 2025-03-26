using Api.Domain;
using Api.Extensions;
using System.Reflection;

namespace Api.Services
{
    public class FileUploadService
    {
        string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "upload");

        public async Task Upload(Stream sourceStream, string fileName)
        {
            using Stream fileStream = new FileStream(Path.Combine(filePath, fileName), FileMode.Create);
            await sourceStream.CopyToAsync(fileStream);
        }

        public byte[] DownloadPlain(string fileName)
        {
            var path = Path.Combine(filePath, fileName);
            if (!File.Exists(path))
                throw new ApiException("File was not found", System.Net.HttpStatusCode.NotFound);

            return File.ReadAllBytes(path);
        }

        public byte[] DownloadAsZip(string fileName)
        {
            var path = Path.Combine(filePath, fileName);
            if (!File.Exists(path))
                throw new ApiException("File was not found", System.Net.HttpStatusCode.NotFound);

            var dictionary = new Dictionary<string, byte[]>
            {
                { fileName, File.ReadAllBytes(path) }
            };
            return FileExtensions.ZipFile(dictionary);
        }
    }
}
