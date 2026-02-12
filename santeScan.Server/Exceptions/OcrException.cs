namespace santeScan.Server.Exceptions;

public class OcrException : Exception
{
    public OcrException(string message) : base(message) { }
    
    public OcrException(string message, Exception innerException) 
        : base(message, innerException) { }
}

public class AiAnalysisException : Exception
{
    public AiAnalysisException(string message) : base(message) { }
    public AiAnalysisException(string message, Exception innerException) : base(message, innerException) { }
}

public class InvalidFileException : Exception
{
    public InvalidFileException(string message) : base(message) { }
}