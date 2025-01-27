# Puzzle Battle Royale (PBR)

Puzzle Battle Royale (PBR) is an innovative multiplayer puzzle game designed for Android and made in Unity. Players compete in real-time matches (2-10 players) to solve a challenging puzzle with a unique twist: abilities. These abilities let players sabotage others, peek at their progress, or gain advantages to secure victory.

The game is built with two main components:

- **Unity Game**: Frontend gameplay with core logic and a clean, modular structure coded in C#.

- **Flask API**: Lightweight backend for matchmaking and random, solvable puzzle generation. ([PBR-API](https://github.com/SamuelSoNChino/PBR-API))


## Features

- **Multiplayer Gameplay**: Engage in real-time matches with 2-10 players.


- **Unique Abilities**: Use powers to sabotage opponents, peek at their progress, or protect your own.

- **Dynamic Puzzles**: Randomly generated, solvable puzzles with unique geometric patterns.

- **Integrated Matchmaking**: Unity Relay combined with a Flask-based API ensures smooth and seamless multiplayer interactions.

- **Optional Single Player Mode**: Practice solving puzzles without competition.

- **Well-Organized Code**: Modular Unity scripts and a lightweight Python backend make the project easy to understand and expand.

## Gameplay Overview

The core gameplay loop of PBR revolves around solving puzzles as quickly as possible while navigating the chaos introduced by player abilities:

1. **Starting a Match**:

    - Players select a loadout of abilities before the match begins.

    - All players receive the same randomly generated puzzle to solve.

2. **Real-Time Strategy**:

    - As players solve pieces of the puzzle, a leaderboard dynamically updates to show progress.

    - Players can peek at the puzzle of a leading opponent, seeing their current progress.

    - Peeking introduces a vulnerability: if another player peeks at you during this time, they can rearrange your puzzle, setting you back.

3. **Abilities and Cooldowns**:

    - Powers are split into three categories:

        - **Passive**: Always active, granting constant benefits.

        - **Targetable**: Used when peeking to sabotage specific players.

        - **Active**: Can be triggered anytime to gain an advantage.

    - All abilities have cooldowns, requiring strategic timing.

4. **Winning Conditions**:

    - The first player to completely solve their puzzle wins the match.

    - Games are designed for fast-paced competition, keeping players engaged from start to finish.

    
## Core Structure

The core structure of PBR is built around the `Assets/Scripts` folder, which houses the majority of the game's logic. This folder is meticulously organized to separate various functionalities:

- `ConnectionTestManager`: Continuously tests the connection to the PBR-API. If the connection is lost, a popup alerts the player, ensuring the game can only proceed with an active connection.

- `LoadoutManager`: Manages the player's selected powers and saves the configuration to `PlayerPrefs` for seamless transitions between sessions.

- `MultiplayerManager`: Handles matchmaking by interacting with the PBR-API. It determines the host for each match, sets up the Unity Relay, and facilitates player connections.

- `PuzzleManager`: Handles tile movement, synchronization between the clients and progress management.

- `PuzzleGenerator`: Generates and initializes puzzles. On the host, this script requests puzzle images and grid backgrounds from the API, cuts them into tiles, and synchronizes them across clients.

- `PlayerManager` and Player Class: Stores all critical player-related data, such as tile positions, cooldowns, and progress. This ensures that the host can validate actions and manage the game's state.

- `LeaderboardManager`: Tracks and displays player progress during the match, dynamically updating as tiles are solved.

- `Power System`: Built around an abstract `Power` class, enabling easy extension and customization of new abilities. Derived classes handle specific power mechanics, cooldowns, and validations.

Each script is documented with detailed descriptions of its properties, fields, and methods, making it easier for developers to understand and extend the functionality. The design ensures all interactions are interconnected, supporting both client and host logic.

## Technical Architecture

PBR relies on a robust architecture that seamlessly integrates Unity (frontend) with Flask (backend):

### Unity (Frontend)

- The Unity game handles all user interactions, puzzle rendering, and game logic.

- It relies heavily on RPCs (Remote Procedure Calls) for communication between the host and clients.

- The host acts as both a client and a server, validating all actions, managing game state, and ensuring synchronization between players.

- Core processes, such as tile movements, power usage, and puzzle validation, are initiated by clients but must be approved by the host.

### Flask API (Backend)

- The Flask API supports matchmaking and puzzle generation.

- It ensures that every puzzle is unique, solvable, and appropriately challenging by using random seed generation.

- The API manages matchmaking queues, assigning hosts and distributing Unity Relay codes to connect players.

### Host-Client Interactions

- During matches, the host handles all critical validations:

    - Tile movements and snapping.

    - Puzzle and power usage validations.

    - Player progress tracking and leaderboard updates.

- Clients request actions (e.g., moving a tile, using a power) via RPCs. The host processes these requests, validates them, and sends the appropriate responses to maintain game state integrity.

### Security Considerations

- The current architecture places significant responsibility on the host, which introduces potential security risks (e.g., tampering by the host player).

- To address this, the design allows for a future transition to a dedicated server model. Unity Gaming Services could host a secure server build, mitigating risks while leveraging the existing host-based features.

- This future-proofing ensures scalability and security improvements without requiring major architectural overhauls.

- By balancing simplicity and flexibility, the technical architecture supports both immediate gameplay needs and long-term project goals.

## Running the Game

You can test and play PBR using the following options:

### Option 1: Test the Android Game

- Download the `.apk` file for PBR from [this link](https://drive.google.com/file/d/1JsJU47cUza6vGEE61SRQKkPHzrtXaI1F/view?usp=drive_link).

- Install it on your Android device and start playing.

- The game connects to the PBR-API at: https://samuelsonchino.eu.pythonanywhere.com.

### Option 2: Development Setup

- Clone the repository and open it in Unity.

- Ensure the Unity Editor is configured correctly to build for Android.

- For backend interaction, refer to the [PBR-API documentation](https://github.com/SamuelSoNChino/PBR-API) to set up and test the Flask API.

## Current State

PBR is in a functional but provisional state:

- **Visuals**: Temporary and unpolished, with AI-generated textures used for testing purposes.

- **Gameplay**: Fully functional and free of known bugs, providing a fun and competitive experience.

- **Powers**: The current powers are implemented but lack proper balancing and require further testing.

- **Missing Features**:

    - No audio or sound effects.

    - No progression system or social integrations.

    - Purely functional gameplay without additional polish.

Despite these limitations, the game is fully playable and demonstrates the core mechanics effectively.

## Future Plans

To enhance PBR and make it production-ready, the following improvements are planned:

1. **New Powers**:

    - Introduce more abilities and conduct thorough testing for balancing.

2. **Visual and Audio Enhancements**:

    - Replace provisional textures with polished visuals.

    - Add immersive sound effects and background music.

3. **Progression System**:

    - Implement accounts and an in-game economy.

    - Add features like player leveling, achievements, and cosmetics.

4. **Dedicated Server Build**:

    - Transition from the current host-based system to a dedicated server model.

    - Leverage Unity Gaming Services to improve security and prevent cheating.
