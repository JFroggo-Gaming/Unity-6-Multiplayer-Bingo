# Unity 6 Multiplayer Bingo

A local multiplayer Bingo game built with Unity 6 and Mirror Networking. Players can host or join games on the same local network and compete to complete Bingo patterns.

> **Note:** This is a 10-hour fun project created as a prototype to experiment with Unity 6 and Mirror Networking. The code is simple and well-documented, but there are many areas that could be improved. Feel free to use it as a learning resource or starting point for your own multiplayer projects!

## About

This project implements a networked Bingo game using Unity 6 and the Mirror networking framework. The game features a lobby system, customizable Bingo patterns, and real-time multiplayer gameplay with up to 6 players.

## Technical Stack

- **Unity Version**: 6000.0.59f2
- **Networking**: Mirror Networking
- **Platform**: Multi-platform support
- **Network Mode**: Local network (localhost)

## Features

- Local multiplayer support (up to 6 players)
- Lobby system with player avatars
- Customizable Bingo patterns via ScriptableObjects
- Real-time number drawing synchronized across all clients
- Host-controlled game flow (start game, reset game)
- Automatic win condition checking
- Audio feedback and visual effects

## Getting Started

### Prerequisites

- Unity 6000.0.59f2 or later
- Basic understanding of Unity and networking concepts
- All players must be on the same local network

### Installation

1. Clone this repository:
   ```bash
   git clone https://github.com/yourusername/Unity-6-Multiplayer-Bingo.git
   ```

2. Open the project in Unity Hub

3. Open the `SampleScene` (lobby scene) and press Play to start

### How to Play

1. **Hosting a Game:**
   - One player clicks "Host" to start a server
   - The host will automatically enter the lobby
   - The host selects the number of players (2-6)
   - Once all players have joined, the host clicks "Start Game"

2. **Joining a Game:**
   - Other players click "Client"
   - Enter `localhost` as the server address
   - Click "Connect" to join the lobby

3. **In the Lobby:**
   - All connected players are visible in the lobby
   - Each player is assigned a random avatar automatically

4. **During the Game:**
   - Numbers are drawn automatically at regular intervals
   - Click on your Bingo card numbers as they're called
   - The pattern to complete is shown on the right side
   - Click "BINGO!" when you complete the pattern
   - Only the host can press **ESC** to reset the game

## Project Structure

### Main Folders

- `/Assets/BingoGame` - All game-specific assets and scripts
  - `/Art` - UI sprites and graphics
  - `/Audio` - Sound effects and music
  - `/Prefabs` - Player prefabs and UI elements
  - `/Scripts` - All C# scripts
  - `/SO` - ScriptableObjects (Bingo patterns)
- `/Packages` - Unity packages and dependencies
- `/ProjectSettings` - Unity project configuration
- `/BingoBuilds` - Compiled game builds

### Script Architecture

#### Network Scripts (`Scripts/Network/`)

- **`BingoNetworkManager.cs`** - Extends Mirror's NetworkManager, handles scene transitions and player spawning
- **`BingoPlayer.cs`** - Lobby player representation, handles player avatars and ready states

#### Manager Scripts (`Scripts/Managers/`)

- **`BingoManager.cs`** - Server-side game manager that handles:
  - Number drawing system
  - Pattern selection and validation
  - Win condition checking
  - Game reset functionality
  - Synchronized timer across all clients

- **`AudioManager.cs`** - Manages background music and sound effects

- **`SceneTransitionManager.cs`** - Handles smooth scene transitions between lobby and game

- **`SpawnPlayersHandler.cs`** - Manages player spawn positions in the game scene

#### Player Scripts (`Scripts/Player/`)

- **`GamePlayer.cs`** - In-game player controller:
  - Manages player's Bingo card
  - Handles win/lose UI panels
  - Communicates with BingoManager for validation
  - Host-only game reset (ESC key)

- **`CameraMovement.cs`** - Camera controller for the game scene

#### UI Scripts (`Scripts/UI/`)

- **`MainMenu.cs`** - Main menu UI handler
- **`LobbyUI.cs`** - Lobby UI management, player list updates
- **`BingoCard.cs`** - Bingo card generation and interaction
- **`PatternDisplay.cs`** - Displays the current winning pattern
- **`DrawnNumbersList.cs`** - Shows all drawn numbers
- **`CountdownTimer.cs`** - Displays time until next number draw
- **`PlayerCountSelection.cs`** - Lobby player count selection
- **`HelpButton.cs`** - In-game help panel
- **`MainMenuFadeEffect.cs`** - Visual fade effects

#### Game Logic (`Scripts/Game/`)

- **`BingoPattern.cs`** - ScriptableObject that defines winning patterns:
  - 24-cell grid (6x4 layout)
  - Pattern validation logic
  - Helper methods for creating common patterns (lines, corners, full card)

#### Editor Tools (`Scripts/Editor/`)

- **`CreateVisualVariants.cs`** - Editor utility for creating visual variants

## Customizing Bingo Patterns

Bingo patterns are defined as ScriptableObjects, making them easy to create and modify:

1. **Creating a New Pattern:**
   - Right-click in the Project window
   - Select `Create > Bingo > Pattern`
   - Name your pattern

2. **Editing a Pattern:**
   - Select the pattern asset in the Project window
   - In the Inspector, check the boxes for cells that must be marked to win
   - The pattern uses a 24-cell grid (6 columns × 4 rows)
   - Index 0 = top-left, Index 23 = bottom-right

3. **Using the Pattern:**
   - Add your pattern to the `availablePatterns` array in the `BingoManager` component
   - The game will randomly select one pattern at the start of each game

### Common Pattern Examples

The `BingoPattern` class includes helper methods for creating:
- **Horizontal Lines** - Complete any row
- **Vertical Lines** - Complete any column
- **Four Corners** - Mark all four corner cells
- **Full Card** - Mark all 24 cells

## Player System

The game uses two different player prefabs for different scenes:

1. **Lobby Player (`Player.prefab`):**
   - Used in the lobby scene
   - Displays player avatar selection
   - Manages ready states
   - Synchronizes player data across clients

2. **Game Player (`PlayerGameScene.prefab`):**
   - Used in the actual game scene
   - Has its own Bingo card
   - Independent camera and UI
   - Can claim Bingo and view results

This separation allows for different functionality and UI in each scene while maintaining network synchronization.

## Network Architecture

- **Server Authority**: The host acts as both server and client
- **Client-Server Model**: All game logic runs on the server, clients receive updates
- **Local Network Only**: Players must use `localhost` to connect
- **Synchronization**:
  - Number draws are synchronized via Mirror's `SyncList`
  - Timer is synchronized using RPC calls
  - Player states use Mirror's `SyncVar` for automatic updates

## Mirror Networking

This project uses [Mirror Networking](https://mirror-networking.com/) for multiplayer functionality. Mirror is a high-level networking library for Unity that provides:

- Easy-to-use networking components
- Automatic state synchronization
- Server-client architecture
- Built-in lobby system support

## Building

To create a build:

1. Go to `File > Build Settings`
2. Select your target platform
3. Click `Build` or `Build and Run`

Builds will be output to the `BingoBuilds/` directory (ignored by git).

## Git LFS

This project uses Git LFS for large binary files:
- Large cubemap files (`.cubemap`) are tracked by LFS
- Ensure you have Git LFS installed: `git lfs install`

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues.
