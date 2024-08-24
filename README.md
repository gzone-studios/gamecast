# GameCast Solution

## Overview

Welcome to the **GameCast** repository! This solution is designed to facilitate the creation of async multiplayer games using a lightweight server-client architecture. The solution is built using .NET and C# and consists of three main projects:

### 1. GameCastCore

The **GameCastCore** project is the backbone of the solution, containing the shared logic and models used by both the server and client. It ensures consistency and reusability across the solution.

### 2. GameCastServer

The **GameCastServer** project is a simple yet powerful server designed for async multiplayer games. It manages the following:

- **Room Management**: Allows creating, joining, and querying game rooms via a REST API.
- **State Management**: Maintains the current state of the game and its players.
- **Message Relay**: Facilitates communication between the game host and connected players (controllers) via WebSockets.
- **Scalability**: The server architecture is designed to support multiple instances in the future, enabling it to handle a larger number of players.

### 3. GameCastClient

The **GameCastClient** project provides a simple client library that can be integrated into host or controller applications. It enables:

- **Connection Management**: Easily connect to the GameCastServer as a host or controller.
- **Message Handling**: Send and receive messages to/from the server.
- **State Synchronization**: Keeps the client and server states in sync.

## Contributing

Contributions are welcome! Please feel free to submit issues, fork the repository, and send pull requests.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.txt) file for details.