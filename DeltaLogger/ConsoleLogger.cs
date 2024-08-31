using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confuser.Core;
using Confuser.Renamer;
using System.Diagnostics;

namespace DeltaLogger
{
    public class ConsoleLogger : ILogger
    {
        public void Debug(string msg)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] " + msg);
        }
        public void DebugFormat(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] " + string.Format(format, args));
        }

        public void EndProgress()
        {
            
        }

        public void Error(string msg)
        {
            System.Diagnostics.Debug.WriteLine("[ERROR] " + msg);
        }

        public void ErrorException(string msg, Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(msg + " " + ex.ToString());
        }

        public void ErrorFormat(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine("[ERROR] " + string.Format(format, args));
        }

        public void Fatal(string msg)
        {
            System.Diagnostics.Debug.WriteLine("[FATAL] " + msg);
        }

        public void FatalFormat(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine("[FATAL] " + string.Format(format, args));
        }

        public void Finish(bool successful) 
        {
            
        }

        public void Info(string msg)
        {
            System.Diagnostics.Debug.WriteLine("[INFO] " + msg);
        }

        public void InfoFormat(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine("[INFO] " + string.Format(format, args));
        }

        public void Progress(int progress, int overall)
        {
           
        }

        public void Warn(string msg)
        {
            System.Diagnostics.Debug.WriteLine("[WARN] " + msg);
        }

        public void WarnException(string msg, Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[WARN] {msg} {ex}");
        }

        public void WarnFormat(string format, params object[] args)
        {
            Console.WriteLine("[WARN] " + string.Format(format, args));
        }
    }
}
