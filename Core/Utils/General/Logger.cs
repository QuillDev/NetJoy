using System;

namespace NetJoy.Core.Utils.General
{
    public static class Logger
    {
        /// <summary>
        /// Log an error message
        /// </summary>
        /// <param name="message">to log</param>
        public static void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERROR]: " + message);
        }
        
        /// <summary>
        /// Clear the console
        /// </summary>
        public static void Clear()
        {
            Console.Clear();
        }
        
        /// <summary>
        /// Log the message with a white foreground
        /// </summary>
        /// <param name="message">as a string to log</param>
        public static void Log(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[LOG]: " + message);
        }
        
        /// <summary>
        /// Log a debug message to the console
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("[DEBUG]: " + message);
        }
    }
}