namespace WhisperByYashasVM.Services;

public sealed class VadService
{
    private const float SpeechThreshold = 0.02f;

    public bool IsSpeech(float rms)
    {
        return rms >= SpeechThreshold;
    }
}
