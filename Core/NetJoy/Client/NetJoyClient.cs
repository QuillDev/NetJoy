using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NetJoy.Core.Config;
using NetJoy.Core.NetJoy.Client.Handling;
using NetJoy.Core.NetJoy.Packets;
using Newtonsoft.Json;
using Encoding = System.Text.Encoding;

namespace NetJoy.Core.NetJoy.Client
{
    public class NetJoyClient
    {
        private readonly Configuration _configuration; // the config file
        
        private Socket _client; //the client socket
        private JoyHandler _joyHandler; // the joystick handler instance
        
        // ManualResetEvent instances signal completion.  
        private readonly ManualResetEvent _connectDone = new ManualResetEvent(false);
        
        public NetJoyClient(Configuration configuration)
        {
            _configuration = configuration;
        }
        
        /// <summary>
        /// Start the net joy client
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            
            //log that we want an address
            Console.WriteLine("Enter the IP address to connect to");
            
            var host = Console.ReadLine();
            
            //Enter the port to connect to
            Console.WriteLine("Enter the port to connect to: ");
            
            //create the boolean for whether the int is valid
            var port = GetValidInt();
            
            //Enter the joystick port
            Console.WriteLine("Enter The Joystick Port: ");
            
            //get a joystick port from the console
            var joystickPort = GetValidInt();
            
            //create a new joystick handler using the given joystick port
            _joyHandler = new JoyHandler(( uint ) joystickPort);
            
            //connect to the entered server
            await Connect(host, port);
        }

        public int GetValidInt()
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
                    Console.WriteLine("Invalid number, Please try again: ");
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
        public async Task Connect(string host, int port)
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
                Console.WriteLine("Trying to connect to remote server...");
                
                // Connect to the remote endpoint.  
                client.BeginConnect(remoteEp,
                    ConnectCallback, client);

                //wait until we're connected
                _connectDone.WaitOne();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
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
            });
        }
        
        /// <summary>
        /// Try to read the data from the socket
        /// </summary>
        public void Read()
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

            if (json?.offset == null)
            {
                return;
            }
            
            //if what we pressed was a button, handle it as a button
            if (json.offset.Contains("Button"))
            {
                HandleButtonUpdate(json);
            }
        }
        
        /// <summary>
        /// Handle the update as if the packet were a button
        /// </summary>
        /// <param name="packet">the packet to use for the update</param>
        private void HandleButtonUpdate(StatePacket packet)
        {
            //set index to an impossible number

            //try to parse the int
            var success = int.TryParse(packet.offset.Substring("Button".Length + 1), out var index);
            
            //if we failed to parse the int, return
            if (!success)
            {
                return;
            }
            
            index++; //add one to the index
            
            //if the index is an invalid number, return
            if (index <= 0)
            {
                return;
            }
            
            //get the button state, if the int is 128 it means the button is down (true)
            var buttonState = (packet.value == 128);
            
            //set the button to the given button state
            _joyHandler.setButton(index, buttonState);
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

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint);

                // Signal that the connection has been made.  
                _connectDone.Set();
                
                //set the client to the given client
                _client = client;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}