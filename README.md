# VR LRC (title tentative)

This is an ongoing project, part of XR @ Berkeley. This readme contains larger-picture documentation on how the game functions.

### Physics Demo

![Physics Demo](https://media.giphy.com/media/SwlrRFxQQYo4GTLNP0/giphy.gif)

The physics demo functions as it does thanks in part to the interactions between several C# scripts (found in Assets/Scripts/):
* **PlayerSync** synchronizes the position of the avatar's head and hands with that of VR eyes and controllers provided by the SteamVR API. This lets the player fundamentally interact with the world.
* **Player_Controller** is a script attached to the **Left Player Control** and **Right Player Control** global objects and receives input directly from the controllers to pass on to a **Controller_State** script attached to player hands, which maintains the player's current controller state, including what objects the player hand is touching/interacting with and the positions of said objects. The **Controller_State** script not only renders the tethers between hand and objects but also does the work of applying pull forces onto interactee objects, with the details of applying the force kept in the **PlayerForce** script, which is also attached player hands.
* **PlayerLocomotion**, as its name suggests, handles player locomotion, computing movement of the player's pivot (foot) based on controller input and enabling basic walking, jumping, and synchronization of the pivot with the head. This script is currently attached to the **Left Player Control**.
* **PlayerTeleportation** largely handles player respawning, in which upon touching water, the player's screen fades to blue and back before the player is respawned without abrupt changes in visual input. This script is attached to the player pivot.
* **ObjectState** keeps an internal state within each throwable object, tracking which hands are picking a certain object up and telling game to update the object's color accordingly. In addition, object respawning after falling into the water is handled by this script.
* **PlatformMovement** contains the code controlling the logic of moving platforms, which currently move between two assignment points in an intuitive manner. Each platform can be in one of four states: "on", "off", "move to position 1", and "move to position 2".
* **Teleporter** contains the code controlling the logic of teleporters, which allow teleportation of players and objects between linked teleporters. Each teleporter can be in an "on" state or an "off" state.
* **PressurePlateState** contains the code controlling the logic of pressure plates, which respond to the weight of throwable objects and players touching the plate. Each pressure plate has a value between 0 and 1 stored within its state that indicates how activated the pressure plate is, with 1 signifying a complete press, and 0 signifying a complete unpress.
* **LeverState** contains the code controlling the logic of levers, whose handles can be dragged around by the player's hand. Similar to pressure plates, levers have an activation value, but the value in this case indicates how far along the arc the lever handle is at. 0 and 1 represent ends of the lever handle arc, while 0.5 represents the exact middle.
* **PuzzleManager** is a script that runs in a global object that manages the interactions between different puzzle elements. Specifically, it lets levers and pressure plates control the states of moving platforms and teleporters by conditions that depend on activation values and are declared inside the editor. For instance, the condition `if PressurePlate1 > 0.9 then activate MovingPlatform1:moveToPos1 Teleporter1:on` would cause the puzzle manager to set the moving platform `MovingPlatform1` to the `moveToPos1` state and the teleporter `Teleporter1` to the `on` state.

### Testing

Running the project requires having [SteamVR](https://store.steampowered.com/app/250820/SteamVR/) and [Unity 2018.4.9f1](https://unity3d.com/unity/whats-new/2018.4.9) installed.

Considering how VR hardware isn't particularly accessible or portable, the physics demo level can be run off-site via the **PhysicsDemoOffsite** scene, found in Assets/. In this scene, you are able to control the player's movements and hands via keyboard and mouse controls. The control scheme for the offsite demo is as follows:
* **Movement** - WASD keys
* **Object Pickup** - Trigger key (check the `SDK_InputSimulator` component of the `[VRSimulator_CameraRig]` child object of the `VRTK SDK Setup - VR Simulator` global game object key bindings).
* **Jumping** - Button 1 key (check the `SDK_InputSimulator` component of the `[VRSimulator_CameraRig]` child object of the `VRTK SDK Setup - VR Simulator` global game object key bindings).

It might take a while to understand how the controls work with the VR simulator, and to assist, a helpful list of control hints is displayed as you run the demo.

### Git Practices

Before you commit changes to the project in Git, follow the below steps:
* First, make sure you are in the master branch. You can check this by typing in `git status` if using a bash/terminal.
  * Also make sure you have access to the VRLRC remote, which you can set by typing in `git remote add origin `
* Create a new branch, which is done in bash/terminal via the `git checkout -n <branch name goes here, but without these brackets>`
* Add **only** the changes to the Assets folder, which can be done on terminal with `git add Assets/*`
* Commit the changes on your newly made branch with `git commit -m "choose a descriptive commit message"`
* Push your branch to this repository `git push origin <branch name>`
* Make a pull request in this repository, which the website should naturally lead you to if you successfully pushed to the new branch.
