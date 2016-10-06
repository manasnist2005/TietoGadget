#region descrption
/*
 * Description  : Helper class to have common methods for custom logging
 * Author       : Mukesh Kumar
 * Date         : 01-August-2014
 */
#endregion
#region Namespace
using System;
using log4net;
using log4net.Config;
#endregion

namespace TietoGadget.Helpers
{
    public static class Logger
    {
        #region Members
        private static readonly ILog logger = LogManager.GetLogger(typeof(Logger));
        #endregion

        #region Constructors
        static Logger()
        {
            XmlConfigurator.Configure();
        }
        #endregion

        #region Methods

        public static void WriteLog(LogLevel logType, string log)
        {
            switch (logType)
            {
                case LogLevel.DEBUG:
                    logger.Debug(log);
                    break;
                case LogLevel.ERROR:
                    logger.Error(log);
                    break;
                case LogLevel.FATAL:
                    logger.Fatal(log);
                    break;
                case LogLevel.INFO:
                    logger.Info(log);
                    break;
                case LogLevel.WARN:
                    logger.Warn(log);
                    break;
                default:
                    break;
            }
        }

        public static void WriteException(LogLevel logType, string log, Exception ex)
        {
            if (logType.Equals(LogLevel.ERROR))
            {
                logger.Error(log, ex);
            }
        }

        public enum LogLevel
        {
            DEBUG = 0,
            ERROR,
            FATAL,
            INFO,
            WARN
        }

        #endregion
    }
}