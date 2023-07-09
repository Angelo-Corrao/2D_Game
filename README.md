# Nacon-Test

I made this project in 2D and I implemented every point (1 to 5 plus extras). The procedural map is generated with tunnels and to
implement it I used the Wave Function Collapse algorithm. To implement the online PvP I used "Netcode for GameObjects", the unity's
first-party networking library. In the Online mode the turn is changed every time the player move or shoot, if the player shoot he will
be unable to move or shoot waiting for he's next turn. After the player shoot the turn is changed when the projectile is destoyed,
so as to check whether the player killed the enemy or not.

I have also implemented saves so that the player is able to continue a game, which he left, at any time.
The saves also work for the procedural map, in this way, if the player wants to continue a game started in this mode,
the same map, previously created procedurally, will be recreated.
