namespace SimpleRSS;

internal static class FeedStreamUtility
{
    internal static async Task<MemoryStream> CopyLimitedAsync(
        Stream source,
        long maxBytes,
        CancellationToken cancellationToken)
    {
        var buffer = new MemoryStream();
        var totalBytes = 0L;
        var chunk = new byte[8192];

        while (true)
        {
            var read = await source.ReadAsync(chunk.AsMemory(0, chunk.Length), cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            totalBytes += read;
            if (totalBytes > maxBytes)
            {
                throw new FeedParseException($"Response exceeds the maximum allowed size of {maxBytes:N0} bytes.");
            }

            buffer.Write(chunk, 0, read);
        }

        buffer.Position = 0;
        return buffer;
    }
}
