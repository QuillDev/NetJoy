namespace NetJoy.Core.Config
{
    public class Configuration
    {
        public bool isServer { get; set; }
        public Client server{ get; set; }
        
        public class Client
        {
            public string address { get; set; }
            public int port { get; set; }
        }
    }
}