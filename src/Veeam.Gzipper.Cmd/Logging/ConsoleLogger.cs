using System;
using System.Linq;
using Veeam.Gzipper.Core.Logging.Abstraction;

namespace Veeam.Gzipper.Cmd.Logging
{
    /// <inheritdoc cref="ILogger"/>
    public class ConsoleLogger : ILogger
    {
        public void Info(string message)
        {
            LogTime();
            LogWithColor(message + "\n", ConsoleColor.Green);
        }

        public void Warning(string message)
        {
            LogTime();
            LogWithColor(message + "\n", ConsoleColor.DarkYellow);
        }

        public void Error(string message)
        {
            LogTime();
            LogWithColor(message + "\n", ConsoleColor.DarkRed);
        }

        public void InfoStatic(string message)
        {
            var length = Console.CursorLeft;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(Enumerable.Repeat(' ', length).ToArray()));
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(message);
        }

        private static void LogTime()
        {
            LogWithColor(DateTime.UtcNow.ToString("T") + " : ", ConsoleColor.DarkGray);
        }

        private static void LogWithColor(string message, ConsoleColor color)
        {
            var c = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ForegroundColor = c;
        }
    }
}
