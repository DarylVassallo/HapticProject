# HapticProject

_**Connecting  Players During Gameplay**_

**MainMenu**

In the main menu both players will be shown two buttons 'Play Together', and 'Play Alone'. 'Play Together' Allows the client to connect to the host, while “Play Alone” Allows only one player to play, without needing another player _(for testing only)_
If both players are required, it is **important** that the VR Player presses the **'Play Together'** interactive button first, before the PC Player presses the **'Play Together'** UI button. This is since in this game the VR Player is the host, while the PC Player is the client.

**Level**

In the level, if the scene is not loaded from the main menu, then the players will need to select either the ‘Host’, or ‘Client’ UI buttons on the **top right corner** of the screen. The ‘**Host**’ button, will connect the **VR Player**, while the ‘**Client**’ UI button will connect the **PC Player**. It is **important** that the VR Player presses the host button **first**, before the PC Player presses the client button.



_**Creating a build of the project**_

1. Open the project using Unity.
2. Select File -> Build Profiles
3. Ensure that the scene list within the build profile is set to MainMenuScene -> PlayLevelScene.
4. Select Build _(it should be visible on the bottom right corner of the Build Profiles menu)_.



_**Play Game In Play Mode**_

1. Play 'MainMenuScene' scene to play the main menu, which is leads to the introduction of the game.
2. Play 'PlayLevelScene' scene to play the game without using the main menu.
3. Upon starting, select ‘Host’ first to play as the host/VRPlayer, and select ‘Client’ to play as the client/PCPlayer _(These buttons are in the top right corner of the screen)_.



_**To Debug PlayLevelScene**_

Within the 'PlayLevelScene' scene, find the object ‘Manager’, which should have the ‘DebugManager’ component. This has several options that could be toggled to test various aspects of the gameplay, these include:

1. **Is Using Play Mode** -> Avoids attempting to connect to another player, ideal when testing players using only one computer
2. **Is Using Only VR Player** -> Used when only the VR Player is required
3. **Is Using Only PC Player** -> Used when only the PC Player is required
4. **Skip Tutorial** -> Used to skip several tutorial events, and begins gameplay immediately _(This also spawns the PC Player at the object 'PCDebugSpawn', allowing the PC Player to start anywhere)_
5. **Start With Player Icon** -> Enables the PC Player Icon used in the map immediately
6. **Start With Activated Spin Wheel** -> Begins the game with the interactive wheel _(this controls the bridge rotations)_ already active
7. **Start With Activated Health Ball** -> Begins the game with the interactive ball _(this gives the PC Player health)_ already active
8. **Start With Activated Defense Button** -> Begins the game with the defense button _(this spawns multiple enemies near the PC Player)_ already active
9. **Disable Enemies** -> Prevents all enemies from spawning
10. **Start With Instant Teleport** -> Begins the game with all teleport pads allowed to instantly teleport the PC Player without requiring any input
11. **Disable Teleport Change** -> Prevents the teleport pads from changing their destination
12. **Require Only One Collectable** -> Changes the game to only require one collectable before progressing to the next section of the game

_**ConnectUI Script**_
This is the script that controls the networking between the host and client. This can be found in both 'MainMenu' _(in the MainMenuCanvas object)_, and 'PlayLevelScene' _(in the TestCanvas object)_ scenes. 

_**EventsManager Script**_
This is a script with contains all the static events used to communicate between scripts. This is important to use when creating new events, or to identify which scripts use specific events.