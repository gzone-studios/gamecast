# GameCastServer

The GameCastServer manages the creation and management of rooms, the relaying of messages between the host and controllers, and the caching of the current state in a room.

Multiple GameCastServers may be behind a load balancer to ensure a high number of concurrent players.  
A client can only join a room on the correct server, therefore, a GameCastServer provides a REST API to query and manage rooms.

## REST API

### Room Creation

`POST /api/rooms`

A room reservation can be created by sending a `POST` request to `/api/rooms`.  
The body should contain the following data: `appId`, `appTag`, `minPlayers`, `maxPlayers`, and `password`.  
A reservation is turned into an active room once the host joins.  
A controller may not join a room while it is only reserved.  
A reservation has a short lifetime and will be destroyed if no host joins.

### Room Info

`GET /api/rooms/XXXX`

You can get room info by performing a `GET` request to `/api/rooms/XXXX`.  
A client has to look up a room before joining to ensure connection to the correct server.  
This is necessary because multiple servers may be behind a load balancer to accommodate any number of players.

### Join Room

`WS /api/rooms/XXXX/play?role=[host|player]&name=[]&user-id=[]`

You can join a room by connecting via WebSocket to `/api/rooms/XXXX/play`.  
You may pass query parameters specifying the `role`, `user-id`, and `name` of the client.

## WebSocket

After joining a room, you may send the following messages:

### SetRoomState(state)

Roles: `host`

Host -> Server -> Controller

This will cache and broadcast the room state to all controllers.

### SetUserState(userId, state)

Roles: `host`

Host -> Server -> Controller

This will cache and broadcast a user's state to all controllers.

### SendMessageToHost(message)

Roles: `host`, `player`

Controller -> Server -> Host

This will relay the message to the host.
