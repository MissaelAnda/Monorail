using System;
using System.Drawing;
using Console = Colorful.Console;

namespace Monorail.Debug
{
    public class Log
    {
        //static List<string> _logs = new List<string>();

        public static Log Core = new Log("Core");
        public static Log Client = new Log("App");

        public readonly string _name;

        public Log(string name) => _name = name;

        public void Info(string pattern, params object[] args) => LogMessage(pattern, Color.Cyan, args);

        public void Succes(string pattern, params object[] args) => LogMessage(pattern, Color.Green, args);

        public void Warn(string pattern, params object[] args) => LogMessage(pattern, Color.Orange, args);

        public void Error(string pattern, params object[] args) => LogMessage(pattern, Color.Red, args);

        void LogMessage(string pattern, Color color, params object[] args)
        {
            var now = DateTime.Now.ToString("dd-MM-yyyy HHH:mm:ss:fff");
            var format = string.Format(pattern, args);
            //var final = string.Format("[{0}] {1}: {2}", now, "Core", format);
            Console.WriteLine("[{0}] {1}: {2}", color, now, _name, format);
            //_logs.Add(final);
        }
    }
}
