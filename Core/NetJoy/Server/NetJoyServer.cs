using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NetJoy.Core.Config;
using NetJoy.Core.NetJoy.Packets;
using NetJoy.Core.Utils;
using Newtonsoft.Json;
using SharpDX.DirectInput;

using Encoding = System.Text.Encoding;

namespace NetJoy.Core.NetJoy.Server
{
    public sealed class NetJoyServer
    {
        private Joystick _joystick; //joystick we're polling for input
        private Socket _clientSocket = null; //the connected client socket 
        private readonly Configuration _configuration; //the configuration of the client 
        private readonly ManualResetEvent _allDone = new ManualResetEvent(false); //thread signal for the server
        private readonly NgrokUtils _ngrok;
        
        
        /// <summary>
        /// Create a NetJoy Server
        /// </summary>
        /// <param name="configuration">config to launch the server with</param>
        public NetJoyServer(Configuration configuration, NgrokUtils ngrok)
        {
            _configuration = configuration;
            _ngrok = ngrok;
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
                Logger.Debug($"Started Server @{ip}:{_configuration.server.port}");
                
                //start an ngrok server for connecting to externally
                _ngrok.Start();

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
                        Logger.Debug("Waiting for a connection...");  
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

            //get the joystick guid & select a stick
            var joystickGuid = SelectStick(directInput);

            // If Joystick not found, throws an error
            if (joystickGuid == Guid.Empty)
            {
                Logger.LogError("No joystick/Gamepad found.");
                Console.ReadKey();
                Environment.Exit(1);
            }
            
            //create a new joystick from the input and id we found
            _joystick= new Joystick(directInput, joystickGuid);
            
            //log that we found a joystick
            Logger.Debug($"Found Joystick with GUID: {joystickGuid}");
            
            //query all available effects
            var allEffects = _joystick.GetEffects();
            
            //log all of the available effects
            foreach (var effect in allEffects)
            {
                Logger.Log($"Found Effect: {effect}");
            }
            
            //set the buffer size for the joystick
            _joystick.Properties.BufferSize = 128;
            
            //acquire the joystick
            _joystick.Acquire();
        }
        
        /// <summary>
        /// Draw menu for selecting a stick
        /// </summary>
        /// <param name="directInput"> stick to select</param>
        /// <returns></returns>
        private Guid SelectStick(DirectInput directInput)
        {
            do
            {
                //the joysticks to select from
                var joysticks = directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices);

                //add all detected gamepads to the joystick list
                foreach (var pad in directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
                {
                    joysticks.Add(pad);
                }

                //Log the joysticks
                for (var index = 0; index < joysticks.Count; index++)
                {
                    var stick = joysticks[index];

                    Logger.Log($"{index + 1}) {stick.ProductName}");
                }
                
                //Log message depending on joysticks
                Logger.Log(!joysticks.Any()
                    ? "Plug in a joystick & press enter to rescan!"
                    : "Enter the ID of the stick you wish to use: ");
                
                //check if the input from the console is valid
                var validInput = int.TryParse(Console.ReadLine(), out var choice);
                
                //if the input was invalid continue
                if (validInput && choice >= 1 && choice <= joysticks.Count)
                {
                    //clear the console
                    Console.Clear();
                    
                    //select the stick and log it
                    var stick = joysticks[choice - 1];
                    Logger.Debug($"Selected Joystick: {stick.ProductName}");
                    return stick.InstanceGuid;
                }
                
                //clear the logger
                Logger.Clear();
                
                //log an error
                Logger.LogError("Invalid Input, Please select from the sticks below!");
            } while (true);
        }
    }
}