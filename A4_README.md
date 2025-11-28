# Oscillate Networking

## Networking
We developed networking for our game using Unity's built-in Netcode. The code was written and tested in the separate `main/networking` branch on GitHub, and then merged over to `main`.

The networking allows two players (host and client) to play the game together in the same session using a local network connection. Each user can independently control their own character, and the two players can interact to defeat the enemy. The entire game state is updated smoothly and seamlessly between the host and the client, and cross-platform (Mac and Windows) support works as well. 

Additional networked interactions include player-to-player in-game chat, swapping weapons, upgrading weapons, and fighting the enemy together.

## Set up
1. Clone the `main` or `networking` branch of the GitHub repository [link](https://github.com/joelmills2/oscillate).
2. Open the Unity Hub and click the "Add" button to add the project. Select the folder of the cloned GitHub repository.
3. Open the project in Unity.
4. From Unity, go to `File -> Build Profiles` and click `Build`.
5. Open the saved game file.
6. The host should start the game with the "Host Game" button.
7. The client can then join by inputting the host's IP and clicking the "Join Game" button.
8. Enjoy!

## Demo Video
https://youtu.be/oG-yAVZaFeA
