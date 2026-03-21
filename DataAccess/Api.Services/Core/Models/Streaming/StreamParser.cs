using System.Runtime.CompilerServices;

namespace Api.Services.Core.Models.Streaming;

// Api.Services.Core/Models/Streaming/StreamParser.cs
public static class StreamParser
{
    public static async IAsyncEnumerable<string> ParseSSEStream(
        Stream stream,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("data: "))
            {
                var data = line.Substring(6).Trim();

                if (data == "[DONE]")
                    yield break;

                yield return data;
            }
        }
    }
}