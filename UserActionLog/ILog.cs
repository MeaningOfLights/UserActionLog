using System;

namespace JeremyThompsonLabs.UserActionLog
{
    public interface ILog
    {
        void LogAction(string frmName, string ctrlName, string eventName, string value);
        void LogAction(DateTime timeStamp, string frmName, string ctrlName, string eventName, string value);
        string GetLogFileName();
        string[] GetTodaysLogFileNames();
        void WriteLogActionsToFile();
    }
}
