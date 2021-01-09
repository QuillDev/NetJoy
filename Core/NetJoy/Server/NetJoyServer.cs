using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NetJoy.Core.Config;
using NetJoy.Core.NetJoy.Packets;
using Newtonsoft.Json;
using SharpDX.DirectInput;

using Encoding = System.Text.Encoding;

namespace NetJoy.Core.NetJoy.Server
{
    public class NetJoyServer
    {
        private Joystick _joystick; //joystick we're polling for input
        private Socket _clientSocket = null; //the connected client socket 
        private readonly Configuration _configuration; //the configuration of the client 
        private readonly ManualResetEvent _allDone = new ManualResetEvent(false); //thread signal for the server
        
        
        /// <summary>
        /// Create a NetJoy Server
        /// </summary>
        /// <param name="configuration">config to launch the server with</param>
        public NetJoyServer(Configuration configuration)
        {
            _configuration = configuration;
        }
        
        /// <summary>
        /// Start the controller and server listener
        /// </summary>
        /// <returns>the task when both are completed</returns>
        public async Task Start()
        {
            
            //try to get the joystick
            GetJoystick();

            await Task.Run(() =>
            {
                //Start the server and controller listener
                var serverListener = StartServerListener();
                var controllerListener = StartControllerListener();
                
                //wait for all tasks to complete 
                Task.WhenAll(serverListener, controllerListener);
            }).ConfigureAwait(false);
        }
        
        /// <summary>
        /// Start the server listener
        /// </summary>
        /// <returns>the server task</returns>
        private async Task StartServerListener()
        {

            await Task.Run(() =>
            {
                //get the ip we'll be using
                var ip = IPAddress.Any;
                var localEndPoint = new IPEndPoint(IPAddress.Any, _configuration.server.port);
                
                //log that we started the server
                Console.WriteLine($"Started Server @{ip}:{_configuration.server.port}");
                
                // Create a TCP/IP socket.
                var listener = new Socket(ip.AddressFamily,  
                    SocketType.Stream, ProtocolType.Tcp );  
  
                // Bind the socket to the local endpoint and listen for incoming connections.  
                try {  
                    listener.Bind(localEndPoint);  
                    listener.Listen(100);
                    
                    //loop forever waiting for a connection
                    for(;;) {  
                        // Set the event to non signaled state.  
                        _allDone.Reset();  
  
                        // Start an asynchronous socket to listen for connections.  
                        Console.WriteLine("Waiting for a connection...");  
                        listener.BeginAccept(
                            AcceptCallback,  
                            listener );  
  
                        // Wait until a connection is made before continuing.  
                        _allDone.WaitOne();  
                    }  
  
                } catch (Exception e) {  
                    Console.WriteLine(e.ToString());  
                }
            }).ConfigureAwait(false);
        }
        
        /// <summary>
        /// Callback after accepting a connection from the remote socket
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            _allDone.Set();  
  
            // Get the socket that handles the client request.  
            var listener = (Socket) ar.AsyncState;  
            var handler = listener.EndAccept(ar);

            //set the client socket
            _clientSocket = handler;
            
            Console.WriteLine($"Accepted connection from Client");
        }
        
        /// <summary>
        /// Start the controller listener
        /// </summary>
        /// <returns>the controller task</returns>
        private async Task StartControllerListener()
        {
            await Task.Run(() =>
            {
                for (;;)
                {
                    //if the joystick is null, continue
                    if (_joystick == null)
                    {
                        continue;
                    }
                
                    //Poll the joystick
                    _joystick.Poll();
                    
                    //get the data from the joystick
                    var data = _joystick.GetBufferedData();
                
                    //Send each instruction we got
                    foreach (var state in data)
                    {
                        //create a data string
                        var jsonData = JsonConvert.SerializeObject(new StatePacket(state), Formatting.None);
                        
                        //TODO Send this packet
                        Send(jsonData);
                    }
                }
                
            }).ConfigureAwait(false);
        }
        
        
        
        /// <summary>
        /// Send the given packet to the server
        /// </summary>
        /// <param name="message"> to send to the server</param>
        /// <returns></returns>
        private void Send(string message)
        {
            //if the client socket is null, return
            if (_clientSocket == null)
            {
                return;
            }
            
            // Convert the string data to byte data using ASCII encoding.  
            var byteData = Encoding.ASCII.GetBytes($"{message}\n");
            
            _clientSocket.Send(byteData);
        }

        /// <summary>
        /// Setup the joystick by getting the joystick in slot 0
        /// </summary>
        private void GetJoystick()
        {
            var directInput = new DirectInput();
            var joystickGuid = Guid.Empty;
            
            //try to get a gamepad
            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
            {
                joystickGuid = deviceInstance.InstanceGuid;
            }
            
            // If Gamepad not found, look for a Joystick
            if (joystickGuid == Guid.Empty)
            {
                foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                {
                    joystickGuid = deviceInstance.InstanceGuid;
                }
            }
            
            // If Joystick not found, throws an error
            if (joystickGuid == Guid.Empty)
            {
                Console.WriteLine("No joystick/Gamepad found.");
                Console.ReadKey();
                Environment.Exit(1);
            }
            
            //create a new joystick from the input and id we found
            _joystick= new Joystick(directInput, joystickGuid);
            
            //log that we found a joystick
            Console.WriteLine($"Found Joystick with GUID: {joystickGuid}");
            
            //query all available effects
            var allEffects = _joystick.GetEffects();
            
            //log all of the available effects
            foreach (var effect in allEffects)
            {
                Console.WriteLine($"Found Effect: {effect}");
            }
            
            //set the buffer size for the joystick
            _joystick.Properties.BufferSize = 128;
            
            //acquire the joystick
            _joystick.Acquire();
        }
    }
}