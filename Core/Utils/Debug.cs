using System;
using NetJoy.Core.Config;

namespace NetJoy.Core.Utils
{
    public class Debug
    {
        private readonly Configuration _config;
        public Debug(Configuration config)
        {
            _config = config;
            
            Console.WriteLine("got to constructor");
        }
    }
}