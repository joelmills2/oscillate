# Oscillate Networking

## Networking
We developed networking for our game using Unity's built-in Netcode. The code was written and tested in the `main/networking` branch on Github, and then merged over to `main`.

The networking allows two players (host and client) to play the game together in the same session using a local network connection. Each user can independently control their own character, and the two players can interact to defeat the enemy. The entire game state is updated smoothly and seamlessly between host and client, and cross-platform (mac and windows) works as well. 

Additional networked interactions include player-to-player in-game chat, swapping weapons, upgrading weapons, and fighting the enemy together.

## Set up
1. Clone the `main` or `networking` branch of the GitHub repository [link](https://github.com/joelmills2/oscillate).
2. Open the Unity Hub and click the "Add" button to add the project. Select the folder of the cloned GitHub repository.
3. Open the project in Unity.
4. From Unity go to `File -> Build Profiles` and click `Build`.
5. Open the saved game file.
6. Enjoy!

## Demo Video
