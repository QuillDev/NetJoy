using System;
using System.IO;
using NetJoy.Core.Utils;
using Nett;

namespace NetJoy.Core.Config
{
    public static class ConfigService
    {
        
        //the filename for the toml file
        private const string FileName = "config.toml";

        public static Configuration ReadConfig()
        {
            try
            {
                
                //If the file exists, just read it
                if (File.Exists(FileName))
                {
                    return Toml.ReadFile<Configuration>(FileName);
                }
                
                //write the default config file
                Logger.Log("No config file detected, making one with default settings");
                Toml.WriteFile(new DefaultConfig(), FileName);

                return Toml.ReadFile<Configuration>(FileName);
            }
            catch(Exception e)
            {
                Logger.LogError(e.Message);
                Logger.LogError("Error reading config file, maybe it's corrupted?");
            }
            
            return new Configuration();
        }
    }
}