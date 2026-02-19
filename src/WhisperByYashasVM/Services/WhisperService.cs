using System.IO;
using System.Net.Http;
using Whisper.net;

namespace WhisperByYashasVM.Services;

public sealed class WhisperService
{
    private const string ModelFileName = "ggml-base.en.bin";
    private const string ModelUrl = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.en.bin";
    private readonly string _modelPath;

    public WhisperService()
    {
        var modelDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WhisperByYashasVM",
            "models");
        Directory.CreateDirectory(modelDir);
        _modelPath = Path.Combine(modelDir, ModelFileName);
    }

    public async Task EnsureModelAsync()
    {
        if (File.Exists(_modelPath))
        {
            return;
        }

        using var http = new HttpClient();
        await using var source = await http.GetStreamAsync(ModelUrl);
        await using var output = File.Create(_modelPath);
        await source.CopyToAsync(output);
    }

    public async Task<string> TranscribeAsync(byte[] wavData)
    {
        await EnsureModelAsync();
        var builder = WhisperFactory.FromPath(_modelPath).CreateBuilder();
        using var processor = builder.WithLanguage("en").Build();
        await using var audioStream = new MemoryStream(wavData);

        var segments = new List<string>();
        await foreach (var segment in processor.ProcessAsync(audioStream))
        {
            if (!string.IsNullOrWhiteSpace(segment.Text))
            {
                segments.Add(segment.Text.Trim());
            }
        }

        return string.Join(" ", segments).Trim();
    }
}
