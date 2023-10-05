# 2D GAME

## What it is?
This is a 2D game made in unity based in a 20x20 grid where each cell is hidden by the fog of war.
The player is able to move, with "WASD", one cell at time and each time he moves to a new cell it will be
discovered from the fog of war, the player also has 5 projectile that he can shoots with "IJKL". Both the Player and
the projectile, if they move to a cell that contains a curve, will follow the curve until they move
to a cell without a curve. The goal is to find and kill a monster hidden in the map and each time
the player shoots without killing him he will be teleported in another cell of the map still in
the fog of war. The player will lose if he moves to a cell with the monster, if he kill himself 
(shooting a projectile that following a curve will return to the player cell) or if he goes out of ammo
without killing the monster. Besides the monster there are also other two elements in the map,
the wells, that will kill the player if he moves to a cell with one of them, and the teleports 
that will move the player to a random cell where is not present nether the monster, a well or a teleport.
As a hint for the player the game will show an interface for the monster, one for the well and on for 
the teleport if one of them is in a cell adjacent to one where the player currently is.

## Modes
In this game there are three modes:
- Standard (The map is always the same but the monster, the wells and the teleports are spawend in a random position every new game)
- Procedural (The map is generated procedurally every new game)
- Online PvP (Turn based)

## Implementation
To create the map procedurally I used the Wave Function Collapse algorithm. To implement the online 
PvP I used "Netcode for GameObjects", the unity's first-party networking library. In the Online mode the turn is changed every time the player
move or shoot, if the player shoot he will be unable to move or shoot waiting for he's next turn. After the player shoot the turn is changed 
when the projectile is destoyed, so as to check whether the player killed the enemy or not.

I have also implemented saves so the player is able to continue a game, which he left, at any time.
The saves also work for the procedural map, in this way, if the player wants to continue a game started in this mode,
the same map, previously created procedurally, will be recreated.
