using System.Threading.Tasks;

namespace NLightning.Utils.Extensions
{
    public static class StreamExtensions
    {
        public static async Task<byte[]> ReadExactly(this System.IO.Stream stream, int count)
        {
            byte[] buffer = new byte[count];
            int offset = 0;
            while (offset < count)
            {
                int read = await stream.ReadAsync(buffer, offset, count - offset);
                if (read == 0)
                {
                    throw new System.IO.EndOfStreamException();
                }

                offset += read;
            }
            
            System.Diagnostics.Debug.Assert(offset == count);
            return buffer;
        }
    }
}