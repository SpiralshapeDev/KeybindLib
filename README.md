# KeybindLib

[Apache License, Version 2.0]: https://github.com/SpiralShapeDev/KeybindLib/blob/main/LICENSE-APACHE
[BepInEx pack for Undermine]: https://www.nexusmods.com/undermine/mods/5
[Issues]: https://github.com/SpiralShapeDev/KeybindLib/issues
[Discord]: https://discord.gg/hpZAmAYMJh

Keybind Lib is an API for Undermine modders to easily add keybinds to their mods.

Report issues here on the GitHub [Issues] page, or if it's urgent, join [my Discord][Discord] and contact me directly.

## License
KeybindLib is released under the [Apache License, Version 2.0].

## Usage for players
### Requirements
- [BepInEx pack for Undermine]

### Installing
1. Install the [BepInEx pack for Undermine]
2. Download and extract the zip.
3. Copy the .dll file to `<Undermine-Steam-Files>/BepInEx/plugins` directory.
4. Run the game. Enjoy!

## Usage for developers
### Referencing library
- <b>Visual Studio:</b> right-click on `Dependencies` in your project, click on `Add Project Reference...`, then `Browse...`, select the dll, and click `OK`. The XML will be detected automatically if it's in the same location as the dll.
- <b>JetBrains Rider:</b> right-click on `Dependencies` in your project, click on `Reference`, then `Add From...`, select the dll, and click `Add`. The XML will be detected automatically if it's in the same location as the dll.

It is highly recommended that you also download [KeybindLib.xml](https://github.com/SpiralShapeDev/KeybindLib/blob/main/KeybindLib.xml) with the `KeybindLib.dll` and put them in the same directory. This will let you reference KeybindLib and view its documentation in your IDE with more in-depth information on how to use functions.

### Registering and listening to an Event
This is example will show you how to create a keybind listener. 
- In this case, the mod's ID is `"TestDeveloper.TestMod"` and the key's ID is `"test_key"`.
- It has the displayName value of `"Test Keybind"` which is displayed in the Custom Mod Keybinds input page.
- It has the defaultKey value of `KeyCode.F`, which will, if an existing key assignment for that id (`"test_key"` in this case) isn't found, will be set to this default value.
- It has the keyEnvironment value of `KeybindManager.KeyEnvironment.Any` so, the keybind will be active ingame, on the title screen, etc.
- It has the keyPressType value of `KeybindManager.KeyPressType.OnKeyDown` so, keybind will only run once instantly on key press.

```
using UnityEngine;
using HarmonyLib;

KeybindManager.KeyDetails testKeydetails = new KeybindManager.KeyDetails(
    KeyCode.F,
    KeybindManager.KeyEnvironment.Any,
    KeybindManager.KeyPressType.OnKeyDown,
    "Test Keybind", 
    "Test Category"
);
KeybindManager.RegisterEvent("TestDeveloper.TestMod", "test_key", testKeydetails).AddListener(() =>
{
    KeyCode currentKey = KeybindManager.GetKey("TestDeveloper.TestMod", "test_key"); 
    Debug.Log($"{currentKey} was pressed!");
});
```

## Building library
1. Clone [this GitHub repository](https://github.com/SpiralShapeDev/KeybindLib.git).


2. Copy files from `<Undermine-Steam-Files>/BepInEx/core` to `.libs`
   1. `0Harmony.dll`
   2. `BepInEx.dll`
   3. `BepInEx.Harmony.dll`


3. Copy files from `<Undermine-Steam-Files>/UnderMine_Data/Managed` to `.libs`
   1. `Rewired_Core.dll`
   2. `Undermine.dll`
   3. `Unity.TextMeshPro.dll`
   4. `UnityEngine.dll`
   5. `UnityEngine.CoreModule.dll`
   6. `UnityEngine.UI.dll`


4. Build solution


5. Built .dll and .xml should be located in `bin/Debug/KeybindLib.dll`
