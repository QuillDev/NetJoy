using System.Threading;
using System.Threading.Tasks;
using NetJoy.Core.Config;
using NetJoy.Core.NetJoy.Client;
using NetJoy.Core.NetJoy.Server;

namespace NetJoy.Core.NetJoy
{
    public class NetJoyManager
    {

        private readonly Configuration _config;
        private readonly NetJoyServer _server;
        private readonly NetJoyClient _client;

        public NetJoyManager(Configuration config, NetJoyServer server, NetJoyClient client)
        {
            _config = config;
            _server = server;
            _client = client;
        }
        
        /// <summary>
        /// Start the NetJoy manager in an asynchronous context
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            //start either the client or the server depending on the config file
            if (_config.isServer)
            {
                await _server.Start();
            }
            else
            {
                _client.Start();
            }
        }
    }
}