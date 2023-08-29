namespace Services.LoggerService.Interface
{
    public interface ILoggerManager
    {
        void LogError(string message);
        void LogWarn(string message);
        void LogInfo(string message);
        void LogDebug(string message);
    }
}
