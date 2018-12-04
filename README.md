## AirPeer
A WebRTC based networking plugin for Unity3D.

## Intro
 AirPeer allows Unity applications to communicate without an authoritative server using [WebRTC technology](https://webrtc.org/).

This project expands on [Christoph Kutza's WebRTC project](https://www.because-why-not.com/webrtc/) by adding features for events, byte streams and more controllable data exchange between the participants.

## Hosting  
A directory called `server` in the repository root contains the backend node. This can be run on a service, like `Heroku`

## API/Usage

### `Node` class
_Methods_

- `Node.New` Creates a new Node instance. The instance can be used as a client or a server
    - `Return Node` The created instance

- `Node.Init` Initializes the inner network
    - `Return bool` Whether the initialization was successful

- `Node.Deinit` Deinitializes the inner network
    - `Return bool` Whether the deinitialization can be done

- `Node.StartServer` Starts the server with a given server name on this node
    - `Arguments`
        - `strig name` The name of the server to be started
        - `Action<bool> callback` Callback for whether the server could start
    - `Return bool` If the server can be started

- `Node.StopServer` stops the server on the node
    - `Arguments`
        - `Action<bool> callback` a callback for when the server stops
    - `Return bool` If the server can be stopped

- `Node.Connect` Connects the inner client to a server identified by the name
    - `Arguments`
        - `string name` The name of the server to be connected to
        - `Action<bool> callback` Whether the connection was successful
    - `Return bool` If the inner client can connect to a server

- `Node.Disconnect` Disconnects the inner client from the server
    - `Return bool` If the inner client disconnected successfully

- `Node.Send` Sends a packet over the network
    - `Arguments`
        - `Packet packet` The Packet instance that has to be sent over the network
        - `bool reliable`Whether the transmission is reliable (slow) or unreliable (fast)
    - `Return bool` Whether message can be sent or not

- `Node.Send` Send a "raw" byte array over the network. 
    - `Arguments`
        - `byte[] bytes` The byte array that has to be sent over the network
        - `bool reliable`Whether the transmission is reliable (slow) or unreliable (fast)
    - `Return bool` Whether message can be sent or not

_Properties_
- `ConnectionId CId` the `ConnectionId` of this node
- `List<ConnectionId> ConnectionIds` the `ConnectionIds` that are currently connected to this `Node`
- `State NodeState` the current state of the node

_Events_
- `Action OnServerStopped` Fires on the client end when the server has been stopped
- `Action<ConnectionId> OnJoin` Fired when a new client has joined the network
- `Action<ConnectionId> OnLeave` Fired when a client has left the network
- `Action<ConnectionId, Packet, bool> OnGetMessage` Fired when a packet has been received by the node
- `Action<ConnectionId, byte[], bool> OnGetRawMessage` Fired when a raw message (byte array) has been received by the node

## Contact
[@github](https://www.github.com/adrenak)  
[@www](http://www.vatsalambastha.com)