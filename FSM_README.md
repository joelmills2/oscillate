# Oscillate FSM

## FSM Diagram

![FSM Diagram](FSM_Diagram.png)

## States and Transitions

### States

#### Idle

The starting state of the enemy. Occurs when the enemy is not aware of the player's location and the timer to search has not yet elapsed. The enemy is aware of the location of the player when it is within 50 units of distance and has a clear line of sight to the player using a raycast. The enemy's color changes to green while idle.

#### Search

The enemy is searching for the player. This state is entered when the enemy does not have sight of the player or the timer to search from idle has elapsed. In this state, the enemy moves to random points within a certain radius around its current position. These points are chosen using a function that picks a random direction and distance, then finds the nearest valid position on the NavMesh. This allows the enemy to search the area intelligently rather than wandering aimlessly. The enemy's color changes to light blue while searching.

#### Chase

The enemy has detected the player and is actively pursuing them. This state is entered when the enemy sees the player or remembers their location for a short time after losing sight. The enemy's color changes to bright orange while chasing.

#### Attack

The enemy is in range to attack the player (20 units). This state is entered when the enemy is close enough to the player to perform a ranged attack. The enemy will continue attacking as long as the player is visible and within range. The enemy's color changes to red while attacking.

### Transitions

#### Idle -> Search

Occurs when the player is out of range of the enemy, and the enemy does not see the player from the idle state. The enemy will start searching for the player after a set amount of time has elapsed in the idle state.

#### Idle -> Chase

Occurs when the player is in range of the enemy, and the enemy sees the player from the idle state.

#### Search -> Idle

Occurs when the player is out of range of the enemy, and the enemy does not see the player from the search state. The enemy will return to idle after a set amount of time has elapsed in the search state.

#### Search -> Chase

Occurs when the player is in range of the enemy, and the enemy sees the player from the search state.

#### Chase -> Search

Occurs when the enemy no longer sees the player from the chase state. The enemy will start searching for the player.

#### Chase -> Attack

Occurs when the enemy is in range to attack the player from the chase state (20 units).

#### Attack -> Chase

Occurs when the enemy is no longer in range to attack the player from the attack state, but can still see the player.

#### Attack -> Search

Occurs when the enemy no longer sees the player from the attack state. The enemy will start searching for the player again.

### Unreachable State Transitions

Cannot directly transition from:

- Idle to Attack
- Search to Attack
- Chase to Idle
- Attack to Idle

## Gameplay Video Link
