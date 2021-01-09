using System;
using System.IO;
using Nett;

namespace NetJoy.Core.Config
{
    public class ConfigService
    {
        
        //the filename for the toml file
        private static readonly string fileName = "config.toml";
        
        public ConfigService(){}

        public static Configuration ReadConfig()
        {
            try
            {
                
                //If the file exists, just read it
                if (File.Exists(fileName))
                {
                    return Toml.ReadFile<Configuration>(fileName);
                }
                
                //write the default config file
                Console.WriteLine("No config file detected, making one with default settings");
                Toml.WriteFile(new DefaultConfig(), fileName);

                return Toml.ReadFile<Configuration>(fileName);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Error reading config file, maybe it's corrupted?");
            }
            
            return new Configuration();
        }
    }
}