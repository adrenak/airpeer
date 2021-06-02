# AirPeer
A WebRTC based networking plugin for Unity3D.

AirPeer allows Unity applications to communicate in a peer to peer manner using [WebRTC technology](https://webrtc.org/).

Built on top of [Christoph Kutza's](https://www.because-why-not.com/) WebRTC Network project with added features such as
- Differentiating peers into server and clients to form star networks with server at the center.
- For client to client communication (via server).
- Event based API.
- Message (de)serialization features.
  
## Installation
AirPeer is distributed as a UPM package. There are many ways to install AirPeer. Here are 5 of them:
<details>  
<summary>Click to see more.</summary>  
<br>

1. Packages>manifest.json file  
  * Easiest method for most people.  
  * Ensure you have NPMJS registry in manifest.json inside the `"scopedRegistries"` object and under the scopes `com.adrenak.airpeer` is added. Then add the package as a project dependency. Make sure `"dependencies"` array has this the package name. If done right your manifest.json should resemble this. Substitute `x.x.x` with the version of AirPeer that you want in your project :
```
        {
            "dependencies": {
                "com.adrenak.airpeer": "x.x.x"
                <<other dependencies of your project>>
            },
            "scopedRegistries": [
                {
                    "name": "npmjs",
                    "url": "https://registry.npmjs.com",
                    "scopes": [
                        "com.adrenak.airpeer",
                        <<other packages from npmjs registry>>
                    ]
                },
                <<Other scoped registries, if any>>
            ]
        }
```
  
2. Git clone method. Clone this repository. Then do either of these
    * Copy Assets/Adrenak.AirPeer into your projects Assets or Packages folder 
    * Checkout the upm branch in the cloned repository using `git checkout upm`. Go to Unity>Window>Package Manager. Click on the + button in the top left corner>Add package from disk... and select Assets/Adrenak.AirPeer/package.json inside the cloned repository.  

      This approach also allows you to change the version of AirPeer by doing a `git checkout` inside the cloned repository to a specific version and also lets you fetch the latest updates using `git pull origin upm`. 
    
      If  you know basic git and don't know much about upm, this is a good approach.

3. OpenUPM:  
    * AirPeer is available on [NPM](https://npmjs.com/package/com.adrenak.airpeer) and [OpenUPM](https://openupm.com/packages/com.adrenak.airpeer.html) registries. If you're using the OpenUPM CLI type any of these in the terminal:  
      * `openupm add com.adrenak.airpeer` which will install it using the OpenUPM registry  
      * `openupm add --registry https://registry.npmjs.com com.adrenak.airpeer` which will install it using the NPM registry.  

      If you know UPM and use OpenUPM CLI, this method is the best.

4. Via Git URL in Unity Package Manager:  
    * Press the + button in package manager and add the URL https://github.com/adrenak/airpeer.git#upm.

5. Github releases:
    * Go to https://github.com/adrenak/airpeer/tags and click on any `upm/x.x.x` where `x.x.x` is the version of AirPeer. You can download the zip and extract inside your projects Packages folder.

</details>  
  

## Documentation
Manual and tutorials is WIP. In the meantime, please refer to codedoc generated documentation [here](http://www.vatsalambastha.com/airpeer).
  
## Samples
AirPeers comes with a couple of handy samples that show how you can connect to a WebRTC network and exchange messages. To import samples, go to Window>Package Manager. Find `Adrenak.AirPeer` and install Samples from the options on the right hand side of the window.
  
## Signalling Server
A Signalling server implementation can be found in the [AirSignal](https://github.com/adrenak/airsignal) repository.
  
The samples in the AirPeer repository use an IP:Port that I am hosting, but it is neither garunteed to be online nor secure. 
  
It is HIGHLY advised that you run AirSignal (or your own WebRTC signalling server implementation) on your own server and don't rely on my server. AirSignal can also be run locally localhost for easier testing.
  
## Editor testing
Use [ParellSync](https://github.com/VeriorPies/ParrelSync) to use the samples within the Unity editor. 
  
## Connectivity issues
A major issue right now is that often two peers fail to connect. The problem is supposedly because of NAT failure.

The plugins on which AirPeer operates are old and their source code is not open for editir. Refactoring AirPeer to make APNetwork an abstraction is planned. 

This would then be used to make [Unity's WebRTC](https://github.com/Unity-Technologies/com.unity.webrtc), which is more robust, the underlying networking plugin. An abstract APNetwork would also mean that APNode can use different WebRTC implementations, such as the [MixedReality-WebRTC](https://github.com/microsoft/MixedReality-WebRTC) or custom ones.
  
## Contact
This repository is made by Vatsal Ambastha and is under the MIT license.  
  
Feel free to get in touch for helping support this project. As well as for commercial consultation/development in your own projects.

[@github](https://www.github.com/adrenak)  [@website](http://www.vatsalambastha.com)  [@twitter](https://www.twitter.com/vatsalambastha)  