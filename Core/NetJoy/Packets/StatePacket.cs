using System;
using SharpDX.DirectInput;

namespace NetJoy.Core.NetJoy.Packets
{
    public class StatePacket
    {
        public StatePacket(JoystickUpdate state)
        {
            
            //get the offset of the state
            Offset = state.Offset.ToString();
            
            //get the value of the state
            Value = Convert.ToUInt16(state.Value);
        }

        public string Offset { get; set; }

        public ushort Value { get; set; }
    }
}