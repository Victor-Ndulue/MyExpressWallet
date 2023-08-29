using NLog;
using Services.LoggerService.Interface;

namespace Services.LoggerService.Implementation
{
    public class LoggerManager:ILoggerManager
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();

        public void LogError(string message) => logger.Error("Logged Error Detail: " + message); 
        public void LogWarn(string message) => logger.Warn("Logged Warning Detail: " + message); 
        public void LogInfo(string message) => logger.Info("Logged Information:" + message);
        public void LogDebug(string message) => logger.Debug("Logged debug Detail: " + message);
    }
}
