# Oscillate FSM

####

Ethan Goldberg (20348076) and Joel Mills (20347322)

## Overview
This assignment builds upon our previous FSM by introducting smooth and dynamic pathfinding and movement using Unity's built-in NavMeshAgent. The NPC (a ghost enemy) now demonstrates decision-making and navigation, transitioning between Idle, Patrol, Chase, and Attack states based on player visibility and distance.

### FSM and Decision-Making
We slightly adapted our Finite State Machine from A2:

#### Idle
The NPC waits at its current patrol point for 1-4 seconds, scanning its surroundings and searching for the player.

#### Patrol
The NPC moves between predefined patrol points on the NavMesh, using smooth pathfinding.

#### Chase
Triggered when the player enters line of sight and detection range (30 units). The NPC uses the NavMesh pathfinding to pursue the player.

#### Attack
Activated when the player is within 12 units of the NPC. The NPC stops advancing, faces towards the player, and fires fireballs every 0.75 seconds while maintaining a ranged distance (8 units).

The ghost enemy is dynamically updated based on its state. It is a regular ghost material for Idle and Patrol, yellow and angry for chase, and red for attack, showing the changing FSM logic during gameplay.

### Pathfinding Implementation
The NPC uses Unity's internal NavMesh system for pathfinding.
* Navigation is handled directly through the NavMeshAgent, allowing the ghost to move smoothly across all traversable terrain defined by the NavMeshSurface (and selected by us).
* Our `SmartPathToNextPatrolPoint` function samples valid positions and then calculates the optimal path using NavMesh.CalculatePath().
* The `RequireNewPath` function ensures efficient repathing while going towards the next patrol point, checking if a new path is required based on time (get to the next patrol point as soon as possible), distance (go the shortest distance), and whether the target lies on a valid section of the NavMesh.
* Possible patrol destinations are chosen specifically by us to ensure the entire map can be patrolled by the NPC. The next destination is chosen at random to ensure non-repetitive patrol routes.

### Steering and Movement
The NPC shows thoughtful and believable ghost movement through:
* Adaptive speeds (different speeds for the various FSM states).
* Real-time rotation toward the movement diraction and oscillating look behviour while travelling, making a searching-like scanning motion.
* Arrival behaviour, the NPC has a set stopping distance to slow down, and smooths out turns to prevent too much sliding and abrupt stops at patrol points.
* The projectile firing appears realistic with forward spawning and acceleration.

### Integration
The FSM determines the logic and behaviour of the NPC, while the NavMesh handles navigation and pathfinding. Transitions in the FSM depend on visibility (raycasting within a certain field of view), proximity, and timers. When visibility is lose, the enemy remembers the player's last position for a short duration (1 second) and continues to head towards it before returning to patrol.

## Gameplay Video Link
https://youtu.be/0LXxUftJQnk

