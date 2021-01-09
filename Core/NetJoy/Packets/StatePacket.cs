using SharpDX.DirectInput;

namespace NetJoy.Core.NetJoy.Packets
{
    public class StatePacket
    {
        public string offset;
        public int value;
        
        public StatePacket(JoystickUpdate state)
        {
            //get the offset of the state
            offset = state.Offset.ToString();
            
            //get the value of the state
            value = state.Value;
        }
    }
}