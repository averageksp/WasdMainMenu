# WasdMainMenu  
![License](https://img.shields.io/badge/License-MIT-green.svg)  ![Downloads](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2FKSP-CKAN%2FCKAN-meta%2Frefs%2Fheads%2Fmaster%2Fdownload_counts.json&query=WasdMainMenu&label=Downloads)  
![Imgur Image](https://imgur.com/vPivohr.png)  

## Overview  
WasdMainMenu is a mod for **Kerbal Space Program** that lets you move around the Main Menu.

## Installation Instructions  
1. **Download** the latest release from [SpaceDock](https://spacedock.info/mod/3846/WasdMainMenu) or the GitHub releases page.  
2. **Unzip** the downloaded file.  
3. **Place** the extracted folder into your `GameData` directory of your KSP installation.  
4. **Launch** KSP and enjoy the mod.

## Troubleshooting

### Keybinds Not Saving
**Q**: "My keybinds aren't saving!"  
**A**: Navigate to the `GameData/WasdMainMenu/Plugin` folder. If there isn't a file named `WasdKeybinds.txt`, create one manually, or the game will generate it for you upon launch.

### Skybox Appears Black
**Q**: "Why is the skybox black?"  
**A**: This happens. Simply navigate to **Settings** and return to the main menu (you can also do this by going to **Credits**). The skybox should refresh. Gameplay is not impacted, so you can continue as usual.

## Compiling from Source

1. Download the `.cs` source code file.  
2. Open the **Reference Manager** in your IDE and include the following references:
    ```bash
    KSPAssets.dll
    Assembly-CSharp.dll
    UnityEngine.CoreModule.dll
    UnityEngine.UI.dll
    UnityEngine.IMGUIModule.dll
    UnityEngine.dll
    UnityEngine.InputLegacyModule.dll
    KSPAssets.XmlSerializers.dll
    UnityEngine.PhysicsModule.dll
    UnityEngine.AnimationsModule.dll
    ```
3. Build the project and move the generated `.dll` file into your `GameData` folder.

## Changelog  

### **v1.0**  
- Initial release.

### **v1.1**  
- Fixed CKAN compatibility issues.  
- Moved mod files from `WasdMainMenu/GameData` to `WasdMainMenu/Plugin` for proper organization.

### **v1.2**  
- Added a **Control GUI** to adjust speed, zoom, and rotation settings.  
- Introduced a **Settings GUI** to customize keybinds.  
- Keybinds now save properly.  
- Changed background to black (to refresh the main menu, go to Settings and return, or simply continue playing).  
- Added new preset keybinds.  
- Introduced **Up** and **Down** keybinds (Spacebar for up, Ctrl for down).

### **v.1.3**
- Added Toolbar Icon

### **v.1.4**
- Fixed Camera Issues
- Fixed Toolbar Icon

## License  
This mod is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for more details.
