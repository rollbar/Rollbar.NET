namespace Rollbar.NetPlatformExtensions
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.IO.Pipelines;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Class PipelineReaderUtil.
    /// </summary>
    public static  class PipelineReaderUtil
    {
        /// <summary>
        /// Gets the pipe content as string.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>System.String.</returns>
        public static async Task<string> GetPipeContentAsString(PipeReader reader)
        {
            var contentStrings = await PipelineReaderUtil.GetListOfStringsFromPipe(reader);
            
            StringBuilder stringBuilder = new StringBuilder();
            foreach(var str in contentStrings)
            {
                stringBuilder.AppendLine(str);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Gets the list of strings from pipe.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>List&lt;System.String&gt;.</returns>
        public static async Task<List<string>> GetListOfStringsFromPipe(PipeReader reader)
        {
            List<string> results = new List<string>();

            while(true)
            {
                ReadResult readResult = await reader.ReadAsync();
                var buffer = readResult.Buffer;

                SequencePosition? position = null;

                do
                {
                    // Look for a EOL in the buffer
                    position = buffer.PositionOf((byte) '\n');

                    if(position != null)
                    {
                        var readOnlySequence = buffer.Slice(0, position.Value);
                        AddStringToList(results, in readOnlySequence);

                        // Skip the line + the \n character (basically position)
                        buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
                    }
                }
                while(position != null);


                if(readResult.IsCompleted && buffer.Length > 0)
                {
                    AddStringToList(results, in buffer);
                }

                reader.AdvanceTo(buffer.Start, buffer.End);

                // At this point, buffer will be updated to point one byte after the last
                // \n character.
                if(readResult.IsCompleted)
                {
                    break;
                }
            }

            return results;
        }

        /// <summary>
        /// Adds the string to list.
        /// </summary>
        /// <param name="results">The results.</param>
        /// <param name="readOnlySequence">The read only sequence.</param>
        private static void AddStringToList(List<string> results, in ReadOnlySequence<byte> readOnlySequence)
        {
            // Separate method because Span/ReadOnlySpan cannot be used in async methods
            //ReadOnlySpan<byte> span = readOnlySequence.IsSingleSegment ? readOnlySequence.First.Span : readOnlySequence.ToArray().AsSpan();
            //results.Add(Encoding.UTF8.GetString(span));

            byte[] buffer = readOnlySequence.IsSingleSegment ? readOnlySequence.First.Span.ToArray() : readOnlySequence.ToArray();
            results.Add(Encoding.UTF8.GetString(buffer));
        }
    }
}
