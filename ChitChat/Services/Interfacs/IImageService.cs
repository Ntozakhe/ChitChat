namespace ChitChat.Services.Interfacs
{
    public interface IImageService
    {
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);
        //this is taking a file from the http upload request
        public string ConvertByteArrayToFile(byte[] fileData, string extension);
        //this returns the uploaded file back to be displayed together with its extemnsion.
    }
}
