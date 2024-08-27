---
title: Home
layout: home
nav_order: 1
description: ""
permalink: /
---

# GameCast Documentation

This documentation covers the GameCastServer as well as the GameCastClient.

## Principle

The principle behind GameCast is as follows:

The server does not process any data; it only relays messages between the host and the controllers.  
Because it contains no game logic, it is therefore usable for any number of games simultaneously.

The host is the only client that can manipulate any state.  
It may also function as a controller.

The controller cannot manipulate any state.  
It can only send messages to the host, and these messages can carry any meaning.  
For example: Select Answer, Pickup Item, Do Action, Send Chat Message.
