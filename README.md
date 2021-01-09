[![Codacy Badge](https://app.codacy.com/project/badge/Grade/85194601df794282adcf5332d0ddadcd)](https://www.codacy.com/gh/QuillDev/NetJoy/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=QuillDev/NetJoy&amp;utm_campaign=Badge_Grade)


# NetJoy - Wireless Controller Node
NetJoy is a bundled client/server application that can send joystick/controller input
from one computer to another remotely over the network.

## Support
Currently the only supported platform is Windows, Linux is planned in the future!

## Example Config
````toml
isServer = false

[server]
address = "127.0.0.1"
port = 6069
````

## Usage Instructions
> Both Clients & Servers
*   Download NetJoy from the [Releases Page](https://github.com/QuillDev/NetJoy/releases).
*   Un-zip NetJoy.7z into whatever folder you want.
> EXTRA STEP FOR SERVERS
*   Open config.toml and set isServer to true.
*   Change port to your desired port number.
*   configure ngrok using your ngrok token [FROM HERE](https://ngrok.com/) by running the below code in the NeyJoy Directory
```
cd PATH_TO_NETJOY_FOLDER
ngrok authtoken TOKEN_GOES_HERE
```

> EXTRA STEP FOR CLIENTS
*   Install ViGEm from [here](https://github.com/ViGEm/ViGEmBus)
*   Complete install & restart
> Both Clients & Servers
*   Run NetJoy.exe & follow instructions in the application for setting up your server/client.
