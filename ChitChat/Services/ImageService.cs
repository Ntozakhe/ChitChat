using ChitChat.Services.Interfacs;

namespace ChitChat.Services
{
    public class ImageService : IImageService
    {
        private readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };
        private readonly string defaultImage = "~/img/DefaultContactImage.png";
        //so if nothing is found, this will be displayed.

        public string ConvertByteArrayToFile(byte[] fileData, string extension)
        {
            //we'll convert the file array, to a file.
            if (fileData == null)
            {
                return defaultImage;
            }

            try
            {
                //convert to base 64 because the file is coming from storage.
                string imageBase64Data = Convert.ToBase64String(fileData);
                return string.Format($"data:{extension}; base64,{imageBase64Data}");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            //here the file is being uploaded.
            try
            {
                using MemoryStream memoryStream = new();
                await file.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();
                return byteFile;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
