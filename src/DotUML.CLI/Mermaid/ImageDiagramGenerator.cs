using System.Formats.Asn1;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

using DotUML.CLI.Diagram;
using DotUML.CLI.Text;

using Microsoft.Extensions.Logging;

namespace DotUML.CLI.Mermaid;

public class ImageDiagramGenerator : IGenerateMermaidDiagram
{
    private readonly ILogger<ImageDiagramGenerator> _logger;

    public ImageDiagramGenerator(ILogger<ImageDiagramGenerator> logger)
    {
        _logger = logger;
    }

    public OutputType OutputType => OutputType.Image;

    public string GenerateDiagram(Namespaces namespaces)
    {
        var diagram = $"classDiagram{Environment.NewLine}";
        diagram += namespaces.GetUMLDiagram();
        var compressedBytes = Deflate(Encoding.UTF8.GetBytes(diagram));
        var encodedOutput = Convert.ToBase64String(compressedBytes).Replace('+', '-').Replace('/', '_');
        var imageUrl = $"https://kroki.io/mermaid/png/{encodedOutput}";
        return imageUrl;
    }

    private static byte[] Deflate(byte[] data, CompressionLevel? level = null)
    {
        byte[] newData;
        using (var memStream = new MemoryStream())
        {
            // write header:
            memStream.WriteByte(0x78);
            switch (level)
            {
                case CompressionLevel.NoCompression:
                case CompressionLevel.Fastest:
                    memStream.WriteByte(0x01);
                    break;
                case CompressionLevel.Optimal:
                    memStream.WriteByte(0xDA);
                    break;
                default:
                    memStream.WriteByte(0x9C);
                    break;
            }

            // write compressed data (with Deflate headers):
            using (var dflStream = level.HasValue
                       ? new DeflateStream(memStream, level.Value)
                       : new DeflateStream(memStream, CompressionMode.Compress
                       )) dflStream.Write(data, 0, data.Length);
            //
            newData = memStream.ToArray();
        }

        // compute Adler-32:
        uint a1 = 1, a2 = 0;
        foreach (byte b in data)
        {
            a1 = (a1 + b) % 65521;
            a2 = (a2 + a1) % 65521;
        }

        // append the checksum-trailer:
        var adlerPos = newData.Length;
        Array.Resize(ref newData, adlerPos + 4);
        newData[adlerPos] = (byte)(a2 >> 8);
        newData[adlerPos + 1] = (byte)a2;
        newData[adlerPos + 2] = (byte)(a1 >> 8);
        newData[adlerPos + 3] = (byte)a1;
        return newData;
    }

    public async Task WriteToFile(string outputPath, string content)
    {
        try
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(content).Result;
                response.EnsureSuccessStatusCode();
                var imageBytes = response.Content.ReadAsByteArrayAsync().Result;
                await File.WriteAllBytesAsync(outputPath, imageBytes);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while writing the diagram to file.");
            throw;
        }
    }
}
