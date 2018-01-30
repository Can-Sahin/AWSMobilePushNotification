using System.IO;
using System.IO.Compression;

namespace AWSMobilePushNotificationService.Utility
{
    internal class StreamUtility
    {

        public static MemoryStream ToGzipMemoryStream(byte[] byteArray)
        {
            using (MemoryStream output = new MemoryStream())
            using (GZipStream zipStream = new GZipStream(output, CompressionMode.Compress, true))
            {
                zipStream.Write(byteArray, 0, byteArray.Length);
                return output;
            }
        }

        public static byte[] FromGzipMemoryStream(MemoryStream stream)
        {
            using (GZipStream zipStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                return ReadFully(zipStream);
            }
        }
        public static byte[] StreamToByteArray(Stream stream)
        {
            if (stream is MemoryStream)
            {
                return ((MemoryStream)stream).ToArray();
            }
            else
            {
                return ReadFully(stream);
            }
        }
        private static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
