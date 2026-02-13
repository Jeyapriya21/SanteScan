namespace santeScan.Server.Exceptions;

public class OcrException : Exception
{
    public OcrException(string message) : base(message) { }
    
    public OcrException(string message, Exception innerException) 
        : base(message, innerException) { }
}