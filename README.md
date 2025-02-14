# Oculus Dashboard Switcher
A tool to quickly swap between Oculus dashboards and OpenXR runtimes for those using [Oculus Killer](https://github.com/BnuuySolutions/OculusKiller) by [Bnuuy Solutions](https://github.com/BnuuySolutions) on Windows.

## ⚠️FOR THOSE COMING FROM ANTAPTIVE'S VR OPTIMIZATION TUTORIAL⚠️
**The usage of Oculus Dashboard Switcher has changed. Please see below for proper usage information.**<br>
Here's an updated tutorial for how to set custom install locations: https://www.youtube.com/watch?v=Y1KVjFAje3c

## Usage
1. Enable "File name extensions" in File Explorer. Tutorials: [Windows 10](https://www.youtube.com/watch?v=PoTah9YBG2Y) | [Windows 11](https://www.youtube.com/watch?v=z5FBLAagPIc)
2. Download [Oculus Killer](https://github.com/BnuuySolutions/OculusKiller) and place the executable in `C:\Program Files\Oculus\Support\oculus-dash\dash\bin` with the name `OculusDash.exe.bak`. **DO NOT replace the original `OculusDash.exe` file.** (see below if you've changed the Oculus install location)
    * If you have already downloaded Oculus Killer but replaced the original dashboard executable file entirely, you will likely need to reinstall Oculus. If you have backed the original up, ensure it has the name `OculusDash.exe.bak`
3. Open Oculus Dashboard Switcher and you will be shown a UAC (User Account Control) prompt. Click Yes.
    * ⚠️ **This tool requires elevated permissions to handle files in protected locations**, such as the Oculus install location. **This tool also includes the ability to change the active OpenXR runtime, which involves modifying certain Windows registry keys.**
4. Oculus Dashboard Switcher will tell you what dashboard is currently active (based on file size) before switching.
    * **NOTE:** Oculus Dashboard Switcher will refer to Oculus Killer as the "bypasser" dashboard.
5. Click Yes and Oculus Dashboard Switcher will swap the files `OculusDash.exe` and `OculusDash.exe.bak` with each other.
6. If the switch is successful, you will be given the option to swap the active OpenXR runtime to the dashboard's corresponding runtime.

## Custom Oculus or Steam Install Location
### Confused? Here's a tutorial: https://www.youtube.com/watch?v=Y1KVjFAje3c

### Oculus
By default, the tool will look for the following directory:<br>
`C:\Program Files\Oculus`

If you have changed the installation location of Oculus, you can specify the path by passing it as a command-line argument like this:<br>
`-oculusPath D:\Oculus` *("D:\\Oculus" is only an example)*

### Steam
By default, the tool will look for the following directory:<br>
`C:\Program Files (x86)\Steam\steamapps`

If you have changed the install location of SteamVR, you can specify the path by passing the parent `steamapps` folder as a command-line argument like this:<br>
`-steamappsPath E:\My Steam Games\steamapps` *("E:\\My Steam Games\steamapps" is only an example)*

### Both
If you have changed the install locations of both Oculus and Steam and need to specify their paths, you can list them consecutively like this:<br>
`-oculusPath C:\My Path To Oculus -steamappsPath E:\My Path To Steam\steamapps`<br>
For clarity, you can enclose the paths in quotation marks.