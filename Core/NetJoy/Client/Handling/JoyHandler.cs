using System;
using System.Threading;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using NetJoy.Core.NetJoy.Packets;
using NetJoy.Core.Utils.Controller;
using NetJoy.Core.Utils.General;
using static System.Int16;

namespace NetJoy.Core.NetJoy.Client.Handling
{
    
    // Don't forget to add this
    public class JoyHandler
    {
        private readonly IXbox360Controller _controller;
        public JoyHandler()
        {
            //Create a new ViGem Client
            var client = new ViGEmClient();
            
            //create the controller
            _controller = client.CreateXbox360Controller();
            
            //connect the controller?
            _controller.Connect();

            Thread.Sleep(500);
            _controller.SetButtonState(Xbox360Button.X, false);
            
        }
        
        /// <summary>
        /// Set a button from the given packet
        /// </summary>
        /// <param name="packet">data to use for setting button</param>
        public void SetButton(StatePacket packet)
        {
            //Get the button from the packet data
            var button = ControllerUtils.StateToXbox360Button(packet);
            
            //if we got an incompatible button, return
            if (button == null)
            {
                return;
            }
            
            //get whether the button was pressed
            var pressed = ( packet.Value == 128 );
            
            //set the given button
            _controller.SetButtonState(button, pressed);
        }

        /// <summary>
        /// Set the value of the axis to the given one
        /// </summary>
        /// <param name="state">The state to use for setting the controller</param>
        public void SetAxis(StatePacket state)
        {
            //convert the state to a controller axis
            var axis = ControllerUtils.StateToXbox360Axis(state);
            
            try
            {

                var src = state.Value - MaxValue;
                
                //convert the ushort value to a short
                var value = src > (ushort) MaxValue
                    ? MaxValue
                    : (short) src;
                
                //NOTE: 0 is the middle point, values go from (-32768 => 32768)
                //set the value of the axis from the controller
                _controller.SetAxisValue(axis, value);
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }

        }
    }
}