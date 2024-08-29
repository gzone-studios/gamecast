---
title: Websockets
parent: Server
nav_order: 4
---


# WebSocket

After joining a room, you may send the following messages:

## SetRoomState(state)

Roles: `host`

Host -> Server -> Controller

This will cache and broadcast the room state to all controllers.

## SetUserState(userId, state)

Roles: `host`

Host -> Server -> Controller

This will cache and broadcast a user's state to all controllers.

## SendMessageToHost(message)

Roles: `host`, `player`

Controller -> Server -> Host

This will relay the message to the host.
