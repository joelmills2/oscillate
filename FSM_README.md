# Oscillate FSM

####
Ethan Goldberg (20348076) and Joel Mills (20347322)

## FSM Diagram

<img src="FSM.jpg" alt="FSM Diagram" style="width:100%; max-width:1000px; display:block; margin:auto;" />

## States and Transitions

### States

#### Idle

The starting state of the enemy. Occurs when the enemy is not aware of the player's location and the timer to search has not yet elapsed (2 seconds). The enemy is aware of the location of the player when it is within 50 units of distance and has a clear line of sight to the player using a raycast, inside a 120-degree field of view. The enemy's colour changes to green while idle.


#### Search

The enemy is searching for the player. This state is entered when the enemy does not have sight of the player, or the timer to search from idle has elapsed. In this state, the enemy moves to random points within a certain radius around its current position (15 units). These points are chosen using a function that picks a random direction and distance, then finds the nearest valid position on the NavMesh. This allows the enemy to search the area intelligently rather than wandering aimlessly. Each search lasts up to 4 seconds before a new point is chosen or the enemy returns to idle. The enemy's colour changes to light blue while searching.


#### Chase

The enemy has detected the player and is actively pursuing them. This state is entered when the enemy sees the player or remembers their location for a short time after losing sight (up to 1 second). The enemy's colour changes to bright orange while chasing.

#### Attack

The enemy is in range to attack the player (20 units). This state is entered when the enemy is close enough to the player to perform a ranged attack. The enemy will continue attacking as long as the player is visible and within range, attempting to hold a distance of about 8 units. It fires projectiles every 0.75 seconds. The enemy's colour changes to red while attacking.

### Transitions

#### Idle -> Search

Occurs when the player is out of range of the enemy, and the enemy does not see the player from the idle state. The enemy will start searching for the player after a set amount of time has elapsed in the idle state (2 seconds).

#### Idle -> Chase

Occurs when the player is in range of the enemy, and the enemy sees the player from the idle state (within 50 units and with a clear line of sight).

#### Search -> Idle

Occurs when the player is out of range of the enemy, and the enemy does not see the player from the search state. The enemy will return to idle after a set amount of time has elapsed in the search state (4 seconds).

#### Search -> Chase

Occurs when the player is in range of the enemy, and the enemy sees the player from the search state.

#### Chase -> Search

Occurs when the enemy no longer sees the player from the chase state. The enemy will start searching for the player after briefly remembering the player’s last seen position (1 second).

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
https://youtu.be/JSsA-CsP0lA

