---
title: Server
nav_order: 3
---

# GameCastServer

The GameCastServer manages the creation and management of rooms, the relaying of messages between the host and controllers, and the caching of the current state in a room.

Multiple GameCastServers may be behind a load balancer to ensure a high number of concurrent players.  
A client can only join a room on the correct server, therefore, a GameCastServer provides a REST API to query and manage rooms.
