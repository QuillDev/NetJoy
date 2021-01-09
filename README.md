# NetJoy - Wireless Controller Node
NetJoy is a bundled client/server application that can send joystick/controller input
from one computer to another remotely over the network.


# Example Config
````toml
isServer = false

[server]
address = "127.0.0.1"
port = 6069
````

# Usage Instructions
> Both Clients & Servers
* Download NetJoy from the [Releases Page](https://github.com/QuillDev/NetJoy/releases).
* Un-zip NetJoy.7z into whatever folder you want.
> EXTRA STEP FOR SERVERS
* Open config.toml and set isServer to true.
* Change port to your desired port number.
> EXTRA STEP FOR CLIENTS
* Install vJoy from [here](http://vjoystick.sourceforge.net/site/index.php/download-a-install/download)
* Complete install & restart
> Both Clients & Servers
* Run NetJoy.exe & follow instructions.
