using System;
using SharpDX.DirectInput;

namespace NetJoy.Core.NetJoy.Packets
{
    public class StatePacket
    {
        public string offset;
        public ushort value;
        
        public StatePacket(JoystickUpdate state)
        {
            //get the offset of the state
            offset = state.Offset.ToString();
            
            //get the value of the state
            value = Convert.ToUInt16(state.Value);
        }
    }
}