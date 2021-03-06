﻿using System;
using System.Threading.Tasks;
using NetJoy.Core.NetJoy.Client;
using NetJoy.Core.NetJoy.Server;
using NetJoy.Core.Utils;

namespace NetJoy.Core.NetJoy
{
    public class NetJoyManager
    {
        private readonly NetJoyServer _server;
        private readonly NetJoyClient _client;

        public NetJoyManager(NetJoyServer server, NetJoyClient client)
        {
            _server = server;
            _client = client;
        }
        
        /// <summary>
        /// Start the NetJoy manager in an asynchronous context
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            //ask whether they want to start it as a server
            var server = Prompts.YesNoPrompt("Start as Server?");
            
            //start either the client or the server depending on the config file
            if (server)
            {
                await _server.Start();
            }
            else
            {
                await _client.Start();
            }
        }
    }
}