# Game Idea and Overview

Fighting/shooter style game with multiple ‘environments’. The player will move between environments with different constraints. When moving between environments, you can only take a certain number of items with you, and some items will change between environments. The boss/enemy will also change between environments.

*As an example for fire and water environments, a spear item would stay constant, but a water gun would become a flamethrower and vice versa. Moreover, the boss will not be able to walk over pits to reach the player (but they could swim if that pit becomes a lake in the water environment\!), thus updating the computed path and FSM.*

Players win by defeating the boss. The player damages the boss with items that it picks up from the environment. As you use an item, it will level up to do more damage. However, the boss will continuously come back stronger each time it is defeated. Players get a higher score the more powerful bosses they defeat.

# AI Plan

## Enemy FSM

Idle  
Searching for the player character when the position is unknown  
Chasing after the player character when the position is known  
Attacking the player character when in range

# Scripted Event

Every 60 seconds, the environment will transform into a new one with different hazards and items; some items may change their functionality, and the enemy will adapt to match the new environment, possibly altering its abilities.

# Multiplayer Plan

The game will be expanded to allow two players to play together as a team. They would both be working towards the same goal of defeating the enemy and would be able to use their own items.

# Environment and Assets

The environment would be designed as two different arenas of the same size, but with distinct themes, hazards, and items.   
Assets would include all the possible items the players can pick up and use to fight the enemy, such as weapons and abilities.
