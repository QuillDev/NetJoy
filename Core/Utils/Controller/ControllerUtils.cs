using Nefarius.ViGEm.Client.Targets.Xbox360;
using NetJoy.Core.NetJoy.Packets;

namespace NetJoy.Core.Utils.Controller
{
    public static class ControllerUtils
    {
        /// <summary>
        /// Get the Xbox360Button for the given string
        /// </summary>
        /// <param name="packet">that we received from the server</param>
        /// <returns></returns>
        public static Xbox360Button StateToXbox360Button(StatePacket packet)
        {
            return packet.offset switch
            {
                "Buttons0" => Xbox360Button.A,
                "Buttons1" => Xbox360Button.B,
                "Buttons2" => Xbox360Button.X,
                "Buttons3" => Xbox360Button.Y,
                "Buttons4" => Xbox360Button.LeftShoulder,
                "Buttons5" => Xbox360Button.RightShoulder,
                "Buttons6" => Xbox360Button.Back,
                "Buttons7" => Xbox360Button.Start,
                "Buttons8" => Xbox360Button.Left,
                "Buttons9" => Xbox360Button.Right,
                _ => null
            };
        }
        
        /// <summary>
        /// Convert the given state to an Xbox360 controller axis
        /// </summary>
        /// <param name="packet">To convert to an axis</param>
        /// <returns></returns>
        public static Xbox360Axis StateToXbox360Axis(StatePacket packet)
        {
            return packet.offset switch
            {
                "X" => Xbox360Axis.LeftThumbX,
                "Y" => Xbox360Axis.LeftThumbY,
                "RotationX" => Xbox360Axis.RightThumbX,
                "RotationY" => Xbox360Axis.RightThumbY,
                _ => null
            };
        }
    }
}