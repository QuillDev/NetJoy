using System.Dynamic;

namespace NetJoy.Core.Config
{
    public class DefaultConfig
    {
        public bool isServer { get; set; } = false;
        public Client server{ get; set;  } = new Client();
    }

    public class Client
    {
        public string address { get; set; } = "127.0.0.1";
        public int port { get; set; } = 6069;
    }
}