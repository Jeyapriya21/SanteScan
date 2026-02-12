namespace santeScan.Server.Services.Interfaces;

public interface IOllamaService
{
    Task<string> AnalyserAnalyseSanguine(string texteBrut);
}