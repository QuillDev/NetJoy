using System;
using System.Diagnostics;
using System.Net;
using NetJoy.Core.Config;
using NetJoy.Core.Utils;
using Newtonsoft.Json.Linq;

namespace NetJoy.Core.NetJoy.Server
{
    public class NgrokUtils
    {

        private readonly int _port;

        public NgrokUtils(Configuration configuration)
        {
            _port = configuration.server.port;
        }
        public void Start()
        {
            
            
            //get the address string for ngrock
            var address = GetNgrok();
            
            //if we already have an instance skip the spawning process
            if (address != null)
            {
                Logger.Debug("Found existing ngrock instance @" + address);
                return;
            }
            
            //log that we couldn't find any instances
            Logger.Debug("No existing ngrock instances. Spawning new instance.");
            
            //spawn a new ngrok instance
            SpawnNgrock(_port);
            
            //log that we created a new ngrock instance
            Logger.Debug("Spawned ngrock instance @" + GetNgrok());
        }
        
        /// <summary>
        /// Get data about the Ngrok instance from the local webserver
        /// </summary>
        /// <returns></returns>
        public string GetNgrok()
        {
            var wr = WebRequest.Create("http://127.0.0.1:4040/api/tunnels");

            wr.Method = "GET";
            wr.Timeout = 1000;
            wr.ContentType = "application/json";


            try
            {
                using var s = wr.GetResponse().GetResponseStream();
                
                if (s == null)
                {
                    return null;
                }

                using var sr = new System.IO.StreamReader(s);
                var jsonResponse = sr.ReadToEnd();
                var json = JObject.Parse(jsonResponse);
                
                //get the base url by parsing the json data
                var baseurl = json["tunnels"]?[0]?["public_url"]?.ToString();
                
                //parse the string to get just the part we need from the url
                var prettyString = baseurl?.Substring(baseurl.IndexOf("//", StringComparison.Ordinal) + 2);
                
                return prettyString;
            }
            catch
            {
                return null;
            }
            
            
        }

        //process for killing ngrok instances
        private readonly Process _taskKill = new Process
        {
            StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = "taskkill.exe",
                RedirectStandardOutput = true,
                Arguments = "/f /im \"ngrok.exe\""
            }
        };
        
        //Process for creating new ngrok instances
        private void SpawnNgrock(int port)
        {
            new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = "ngrok.exe",
                    RedirectStandardOutput = true,
                    Arguments = $"tcp {_port}"
                }
            }.Start();
        }
    }
}