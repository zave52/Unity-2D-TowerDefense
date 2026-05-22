# 2D Tower Defense Game in Unity

A 2D Tower Defense game developed in Unity 6, fully optimized for WebGL (browser-based) deployment. The project features a structured round cycle, dual roles (Defender and Attacker), dynamic wave compilation, and robust performance optimization via Object Pooling.

---

## Getting Started

Follow these instructions to clone, open, and run the project in the Unity Editor:

### Prerequisites
* Install Unity Hub.
* Install Unity 6 (version 6000.4.1f1 or compatible Unity 6 version) with the WebGL Build Support module.

### Cloning the Repository
Clone this repository to your local machine:
```bash
git clone https://github.com/your-username/Unity-2D-TowerDefense.git
```

### Opening the Project
1. Open Unity Hub.
2. Click "Add" -> "Add project from disk".
3. Select the root folder of the cloned repository ("Unity-2D-TowerDefense").
4. Open the project. During the first launch, Unity will import the assets and resolve the package dependencies (this may take a few minutes).

### Running the Game
When you open the project in the Unity Editor for the first time, you must manually select and open the main game scene:
1. In the Project window, navigate to: `Assets/Game/Scenes/`.
2. Double-click the Game.unity scene to open it.
3. In the Game view tab, ensure the resolution/aspect ratio is set to a standard 16:9 ratio (e.g., 1920x1080 Full HD).
4. Click the Play button in the Unity Editor toolbar to launch the game.

---

## Game Design and Mechanics

### Game Field and Grid
* The gameplay takes place on a 12x8 tile grid.
* A fixed movement path is defined using a sequence of waypoints. No dynamic pathfinding is required, which dramatically improves WebGL runtime efficiency.
* Towers can only be constructed on unoccupied cells outside of the predefined enemy path.

### Game Modes
1. **Player vs Computer (PvE)**: The default MVP mode where the Player acts as the Defender, and the Computer (AI Attacker) dynamically schedules enemy waves.
2. **Player vs Player (PvP Hot-Seat)**: An advanced hot-seat mode where two players alternate roles (Defender and Attacker) on a single PC.

### Player Roles and Economy
* **Defender**:
  * Starts with 300 gold.
  * Purchases and places towers during the Preparation phase.
  * Gains gold rewards for defeating enemies.
  * Defends the base, which starts with 20 base health (HP).
  * Objective: Survive all 10 rounds with base health above 0.
* **Attacker**:
  * Starts with an attack budget (200 points).
  * Spends attack points during the Preparation phase to purchase and queue enemies.
  * Objective: Reduce base health to 0 before the end of the 10th round.
  * In PvE mode, the AI Attacker uses a progressive algorithm to automatically generate challenging wave compositions within the round budget.

### Round Structure
Each game round transitions through a three-state machine:
1. **Preparation**: The Defender places/upgrades towers, while the Attacker compiles the upcoming wave.
2. **Battle**: Enemies spawn at fixed intervals (0.8s to 1.2s) and follow the waypoints. Towers target and fire at them automatically.
3. **RoundEnd**: The round results are processed, gold rewards are distributed, the attack budget is incremented, and game completion conditions are validated.

### Wave Spawning and WebGL Safety
To avoid performance degradation under WebGL constraints:
* Mixed enemy types are permitted in a single wave queue.
* The total cost of the compiled wave cannot exceed the current round's attack budget. Unused budget is discarded.
* A hard limit of 50 concurrent enemies per wave is enforced to prevent GPU/memory bottlenecks.

### Targeting and Shooting
Towers automatically scan their attack range using a "closest to base" targeting strategy. A tower will always target the enemy in its range that has achieved the greatest progress along the predefined path, rather than the closest geometric distance.

---

## Specifications

### Defensive Towers

| Name | Attack Speed | Attack Type | Range | Price |
| :--- | :--- | :--- | :--- | :--- |
| Archer | Medium | Single Target | Medium | 100 |
| Mage | Slow | Area of Effect (AoE) | Short | 150 |
| Freezer | Medium | Slow Effect (Slowing) | Medium | 120 |
| Cannon | Very Slow | Single Target (Heavy Damage) | Long | 200 |

### Enemy Units

| Name | Health | Movement Speed | Special Feature | Spawn Cost |
| :--- | :--- | :--- | :--- | :--- |
| Goblin | Low | Fast | Weak but swift | 10 |
| Orc | High | Slow | Durability tank | 25 |
| Ghost | Medium | Medium | Immune to Freezer slow | 20 |

### Win and Loss Conditions
* **Defender Victory**: The Defender survives all 10 rounds with base health greater than 0.
* **Attacker Victory**: The base health is reduced to 0.

---

## Technical Specifications and Optimizations

### WebGL-Specific Optimization
* **Object Pooling**: To prevent frequent memory allocations and garbage collection spikes under WebGL, the project implements a strict Object Pooling system for all enemies, projectiles, and visual impact effects. Frequent "Instantiate" and "Destroy" calls during active combat are entirely avoided.
* **Concurrency Limits**: The physics engine and rendering pipelines are optimized to support 20 towers and over 50 concurrent enemies on screen at a stable, lag-free frame rate.
* **Universal Render Pipeline (URP)**: The project uses URP 2D renderer for hardware-accelerated draw call batching and optimal rendering paths in standard web browsers.
