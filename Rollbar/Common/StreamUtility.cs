namespace Rollbar.Common
{
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Class StreamUtil.
    /// </summary>
    public static class StreamUtility
    {
        /// <summary>
        /// Captures a stream into a string.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>System.String.</returns>
        public static async Task<string?> CaptureAsStringAsync(Stream? stream)
        {
            if (stream == null || !stream.CanSeek || !stream.CanRead)
            {
                    return null;
            }

            var reader = new StreamReader(stream);
            stream.Seek(0, SeekOrigin.Begin);
            string content = await reader.ReadToEndAsync();
            stream.Seek(0, SeekOrigin.Begin);
            
            return content;
        }
    }
}
