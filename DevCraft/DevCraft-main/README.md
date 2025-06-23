# DevCraft
A Minecraft clone written in C# using MonoGame Framework

### Features

* Simple terrain generation using FastNoiseLite https://github.com/Auburn/FastNoiseLite
* More than 20 types of blocks and more can be added easily
* Basic lighting system with daylight cycle
* World changes persisted in JSON files
* Customizable world generation seeds

### Controls

* WASD to move forward, left, backward, right
* Double tap W to sprint
* Space to jump
* Double tap space to switch between flying and walking
* Hold space to ascend
* Hold LShift to descend
* Left click to destroy a block
* Right click to place a block
* Esc to open menu
* E to open and close inventory
* F3 to open debug screen
* F2 to take screenshot

### Creating Worlds

When creating a new world, you can:
* Enter a custom world name
* Choose between "Default" and "Flat" world types
* Set a custom seed for world generation:
  - Leave the seed field empty for a random seed
  - Enter a number for a specific numeric seed
  - Enter text for a text-based seed (uses hash code)











































































C:\Users\jusss\Desktop\DevCraft\
└── DevCraft-main/                           ✅ Clean, single project folder
    ├── .vscode/                             ✅ VS Code configuration
    │   ├── launch.json                      ✅ Debug configuration
    │   ├── settings.json                    ✅ Project settings
    │   └── tasks.json                       ✅ Build/run tasks
    │
    ├── DevCraft/                            ✅ Main project folder
    │   ├── .config/                         ✅ MonoGame config
    │   ├── Assets/                          ✅ Game assets
    │   ├── bin/                             ✅ Build output
    │   ├── obj/                             ✅ Build intermediates
    │   ├── DataModel/                       ✅ Data structures
    │   ├── Effects/                         ✅ Shaders
    │   ├── GUI/                             ✅ User interface
    │   ├── MathUtilities/                   ✅ Math helpers
    │   ├── Persistence/                     ✅ Save/load system
    │   ├── Rendering/                       ✅ Graphics rendering
    │   ├── Saves/                           ✅ Save files
    │   ├── Screenshots/                     ✅ Screenshot storage
    │   ├── Utilities/                       ✅ Helper classes
    │   ├── World/                           ✅ Game world system
    │   ├── app.manifest                     ✅ Updated with "DevCraft"
    │   ├── DevCraft.csproj                  ✅ Project file
    │   ├── Icon.ico                         ✅ Application icon
    │   ├── MainGame.cs                      ✅ Main game class
    │   ├── Camera.cs                        ✅ Camera system
    │   ├── Physics.cs                       ✅ Physics system
    │   ├── Player.cs                        ✅ Player controller
    │   ├── Program.cs                       ✅ Entry point
    │   └── settings.json                    ✅ Game settings
    │
    ├── DevCraft.sln                         ✅ Solution file
    └── README.md                            ✅ Documentation