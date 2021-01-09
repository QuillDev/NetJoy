using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NetJoy.Core.NetJoy.Client.Handling;
using NetJoy.Core.NetJoy.Packets;
using NetJoy.Core.Utils.General;
using Newtonsoft.Json;
using Encoding = System.Text.Encoding;

namespace NetJoy.Core.NetJoy.Client
{
    public class NetJoyClient : IDisposable
    {
        private Socket _client; //the client socket
        private JoyHandler _joyHandler; // the joystick handler instance
        
        // ManualResetEvent instances signal completion.  
        private readonly ManualResetEvent _connectDone = new ManualResetEvent(false);

        /// <summary>
        /// Start the net joy client
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            
            //log that we want an address
            Logger.Log("Enter the IP address to connect to");
            
            var host = Console.ReadLine();
            
            //Enter the port to connect to
            Logger.Log("Enter the port to connect to: ");
            
            //create the boolean for whether the int is valid
            var port = GetValidInt();
            
            //create a new joystick handler using the given joystick port
            _joyHandler = new JoyHandler();
            
            //connect to the entered server
            await Connect(host, port).ConfigureAwait(false);
        }
        
        /// <summary>
        /// Get a valid int as input
        /// </summary>
        /// <returns>a valid integer</returns>
        private int GetValidInt()
        {
            
            bool valid;
            int port;
            
            //loop until the port is valid
            do
            {
                
                //check whether the port was an int or not
                valid = int.TryParse(Console.ReadLine(), out port);
                
                //if it was not valid, ask again
                if (!valid)
                {
                    Logger.LogError("Invalid number, Please try again: ");
                }
            } while (!valid);


            return port;
        }
        
        /// <summary>
        /// Connect to the given host and port
        /// </summary>
        /// <param name="host">to connect to</param>
        /// <param name="port">to connect to</param>
        /// <returns></returns>
        private async Task Connect(string host, int port)
        {
            // Connect to a remote device.  
            try
            {
                //Establish the endpoint for the socket to connect to
                var ipHostInfo = await Dns.GetHostEntryAsync(host);
                var ipAddress = ipHostInfo.AddressList[0];
                var remoteEp = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.  
                var client = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                
                //Log that we're trying to connect
                Logger.Log("Trying to connect to remote server...");
                
                // Connect to the remote endpoint.  
                client.BeginConnect(remoteEp,
                    ConnectCallback, client);

                //wait until we're connected
                _connectDone.WaitOne();
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
            
            //Run the tasks
            await Task.Run(() =>
            {
                for (;;)
                {
                    //if the client is null, continue
                    if (_client == null)
                    {
                        continue;
                    }
                    
                    //read from the socket
                    Read();
                }
            }).ConfigureAwait(false);
        }
        
        /// <summary>
        /// Try to read the data from the socket
        /// </summary>
        private void Read()
        {
            //if the client is null, return
            if (_client == null)
            {
                return;
            }
            
            //get how many bytes are available
            var available = _client.Available;

            //if there are no bytes available, return
            if (available <= 0)
            {
                return;
            }

            //read all available bytes
            var bytes = new byte[available];
            _client.Receive(bytes);

            //get the string from the given bytes
            var data = Encoding.ASCII.GetString(bytes).Split('\n');
            
            foreach(var datum in data)
            {
                HandleData(datum);
            }

        }
        
        /// <summary>
        /// Handle data we received from the web socket
        /// </summary>
        /// <param name="data"></param>
        private void HandleData(string data)
        {
            //deserialize the json into an object
            var json = JsonConvert.DeserializeObject<StatePacket>(data);
            
            //if the json is null, return

            if (json?.Offset == null)
            {
                return;
            }
            
            //if what we pressed was a button, handle it as a button
            if (json.Offset.Contains("Button"))
            {
                HandleButtonUpdate(json);
            }
            else
            {
                HandleAxisUpdate(json);
            }
        }
        
        /// <summary>
        /// Handle an axis update from the joystick
        /// </summary>
        /// <param name="packet"></param>
        private void HandleAxisUpdate(StatePacket packet)
        {
            //set the axis to the given value
            _joyHandler.SetAxis(packet);
        }
        /// <summary>
        /// Handle the update as if the packet were a button
        /// </summary>
        /// <param name="packet">the packet to use for the update</param>
        private void HandleButtonUpdate(StatePacket packet)
        {
            _joyHandler.SetButton(packet);
        }
        
        /// <summary>
        /// Callback for connecting to the server
        /// </summary>
        /// <param name="ar">response from the async method</param>
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                var client = (Socket) ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);
                
                //log that we connected
                Logger.Debug($"Socket connected to {client.RemoteEndPoint}");

                // Signal that the connection has been made.  
                _connectDone.Set();
                
                //set the client to the given client
                _client = client;
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
            _connectDone?.Dispose();
        }
    }
}