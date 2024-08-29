---
title: API
parent: Server
nav_order: 3
---

# REST API

## Room Creation

`POST /api/rooms`

A room reservation can be created by sending a `POST` request to `/api/rooms`.  
The body should contain the following data: `appId`, `appTag`, `minPlayers`, `maxPlayers`, and `password`.  
A reservation is turned into an active room once the host joins.  
A controller may not join a room while it is only reserved.  
A reservation has a short lifetime and will be destroyed if no host joins.

## Room Info

`GET /api/rooms/XXXX`

You can get room info by performing a `GET` request to `/api/rooms/XXXX`.  
A client has to look up a room before joining to ensure connection to the correct server.  
This is necessary because multiple servers may be behind a load balancer to accommodate any number of players.

## Join Room

`WS /api/rooms/XXXX/play?role=[host|player]&name=[]&user-id=[]`

You can join a room by connecting via WebSocket to `/api/rooms/XXXX/play`.  
You may pass query parameters specifying the `role`, `user-id`, and `name` of the client.
