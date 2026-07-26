# Unity FPS Controller

A simple, easy-to-setup FPS controller for Unity 3D, built on Unity's Character Controller component.

## Requirements
- Input System package

## Features
- Character Controller-based movement
- Quick and easy setup
- Compatible with Domaing Reloading and Scene Reloading **being disabled**
- Customizable speeds for:
  - Walking
  - Sprinting
  - Crouching
- Two headbob styles (both equipped with event used for footsteps):
  - Simple - a basic sway effect driven by trigonometric functions
  - Realistic - velocity-driven sway with lateral movement and roll
- Full Input System integration - just drag and drop your actions
- Clean, easy-to-use API

 ## Installation
 - Install via UPM git URL `https://github.com/Zbajnek/urp-fps-controller.git?path=Packages/FPSController`

## How to setup your character
1. Install the package
2. Create the player with the following hierarchy
   ```
   Player
   └── Head
       └── MainCamera
   ```
   Alternatively, use the Player prefab included with the package.
3. Attach `PlayerController.cs` and `PlayerLook.cs` scripts to the Player object.
4. Add the required references, and you're good to go!

## License
The package is under the MIT license. See [LICENSE](https://github.com/Zbajnek/unity-fps-controller/blob/master/LICENSE) for details.
