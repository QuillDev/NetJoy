using System;
using vJoyInterfaceWrap;

namespace NetJoy.Core.NetJoy.Client.Handling
{
    
    // Don't forget to add this
    public class JoyHandler
    {
        //the port for the controller
        private readonly uint _port;
        private readonly vJoy _joystick;
        
        public JoyHandler(uint port)
        {
            
            //set the joystick port
            _port = port;
            
            //create a new vJoy instance
            _joystick = new vJoy();
            
            //if vJoy is not enabled
            if (!_joystick.vJoyEnabled())
            {
                //if vJoy is not enabled throw an exception
                throw new Exception("vJoy is not enabled!");
            }
            
            //log information about the joystick driver
            LogJoystickInfo(_joystick);
            
            //check whether the dll version and the driver version match
            CheckVersions(_joystick);
            
            //check whether the device is available
            var available = IsDeviceAvailable(_joystick, _port);
            
            //if the joystick is not available we can't use it
            if (!available)
            {
                throw new Exception("Joystick is not available and therefore cannot be used! Free the joystick before restarting the program!");
            }
            
            //try to acquire the device
            var acquired = AcquireDevice(_joystick, port);
            
            //if we failed to acquire the device, throw an exception
            if (!acquired)
            {
                throw new Exception($"Failed to acquire device @ port {_port}");
            }
        }
        
        /// <summary>
        /// Try to set the button with the given id to the given sate
        /// </summary>
        /// <param name="id">of button to change state of</param>
        /// <param name="pressed">whether the button should be pressed</param>
        public void setButton(int id, bool pressed)
        {
            try
            {
                _joystick.SetBtn(pressed, _port, (uint) id);
            }
            catch
            {
                //ignored
            }
        }
        /// <summary>
        /// Convert a double percentage to an axis value
        /// </summary>
        /// <param name="percent"></param>
        /// <returns>The Joystick value from 0 - 65536</returns>
        private int PercentToAxisValue(double percent)
        {
            return (int) (short.MaxValue * percent);
        }
        
        /// <summary>
        /// Acquire the device to be fed from joystick input
        /// </summary>
        /// <param name="joy">driver to use</param>
        /// <param name="port">to acquire</param>
        /// <returns></returns>
        private bool AcquireDevice(vJoy joy, uint port)
        {
            //get the status of the joystick @ the given port
            var status = joy.GetVJDStatus(port);

            // Acquire the target
            if (status == VjdStat.VJD_STAT_OWN ||
                (status == VjdStat.VJD_STAT_FREE) && (!joy.AcquireVJD(port)))
            {
                Console.WriteLine($"Failed to acquire vJoy device number {port}.");
                return false;
            }
            
            Console.WriteLine($"Acquired: vJoy device number {port}.");
            return true;

        }
        
        /// <summary>
        /// Check whether the given device is available or not
        /// </summary>
        /// <param name="joy">to check</param>
        /// <param name="id">of the device in question</param>
        /// <returns></returns>
        private bool IsDeviceAvailable(vJoy joy, uint id)
        {
            // Get the state of the requested device
            var status = joy.GetVJDStatus(id);
            switch (status)
            {
                case VjdStat.VJD_STAT_FREE:
                    Console.WriteLine("vJoy Device {0} is free\n", id);
                    return true;
                case VjdStat.VJD_STAT_OWN:
                    Console.WriteLine("vJoy Device {0} is already owned by this feeder\n", id);
                    return true;
                case VjdStat.VJD_STAT_BUSY:
                    Console.WriteLine(
                        "vJoy Device {0} is already owned by another feeder\nCannot continue\n", id);
                    break;
                case VjdStat.VJD_STAT_MISS:
                    Console.WriteLine(
                        "vJoy Device {0} is not installed or disabled\nCannot continue\n", id);
                    break;
                case VjdStat.VJD_STAT_UNKN:
                    break;
                default:
                    Console.WriteLine("vJoy Device {0} general error\nCannot continue\n", id);
                    break;
            };

            return false;
        }
        
        /// <summary>
        /// Log information about the joystick driver
        /// </summary>
        /// <param name="joy"></param>
        private void LogJoystickInfo(vJoy joy)
        {
            //log vJoy information
            Console.WriteLine("vJoy Information:\nVendor: {0}\nProduct: {1}\nVersion Number: {2}\n",
                joy.GetvJoyManufacturerString(),
                joy.GetvJoyProductString(),
                joy.GetvJoySerialNumberString());
        }
        
        /// <summary>
        /// Check whether the versions of the driver and the dll used match
        /// </summary>
        /// <param name="joy">The vJoy instance to check</param>
        private void CheckVersions(vJoy joy)
        {
            //Check if dll and driver version match
            uint dllVer = 0, drvVer = 0;
            var match = joy.DriverMatch(ref dllVer, ref drvVer);
            
            //Log data depending on whether they matched or not
            if (match)
            {
                Console.WriteLine("Version of Driver Matches DLL Version ({0:X})\n", dllVer);
            }
            else
            {
                Console.WriteLine("Version of Driver ({0:X}) does NOT match DLL Version ({1:X})\nIf you experience errors please upgrade/downgrade accordingly!",
                    drvVer, dllVer);
            }
        }
    }
}