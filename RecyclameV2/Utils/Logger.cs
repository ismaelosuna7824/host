using RecyclameV2.Formularios;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecyclameV2.Utils
{
    /// <summary>
	/// Summary description for Logger.
	/// </summary>
	public class Logger
    {
        /// <summary>
        /// Used in synchronization.  Loggers should synchronize on this object when they must guarantee
        /// thread safety at all costs
        /// </summary>
        protected static readonly object monitor = new object();


        public Logger()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static void addLogEntry(string strLogFile, string strEntry)
        {
            //Monitor.Enter(monitor);

            try
            {
                using (TextWriter streamWriter = File.AppendText(strLogFile))
                {
                    streamWriter.Write(DateTime.Now.ToString() + " " + DateTime.Now.ToLocalTime() + ": " + strEntry);
                }
            }
            catch (Exception)
            {
            }

            //Monitor.Exit(monitor);
        }

        public static void addLogEntry(string strEntry)
        {
            try
            {
                using (TextWriter streamWriter = File.AppendText(FrmBascula.LOG_FILE_PATH))
                {
                    streamWriter.Write(DateTime.Now.ToString() + " " + DateTime.Now.ToLocalTime() + ": " + strEntry);
                }
            }
            catch (Exception)
            {
            }

            //Monitor.Exit(monitor);
        }
    }
}
