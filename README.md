# Oculus Dashboard Switcher
A tool to quickly swap between Oculus dashboards for those using [Oculus Killer](https://github.com/BnuuySolutions/OculusKiller) by [Bnuuy Solutions](https://github.com/BnuuySolutions) on Windows.

## Usage
1. Enable "File name extensions" in File Explorer. Tutorials: [Windows 10](https://www.youtube.com/watch?v=PoTah9YBG2Y) | [Windows 11](https://www.youtube.com/watch?v=z5FBLAagPIc)
2. Download [Oculus Killer](https://github.com/BnuuySolutions/OculusKiller) and place the executable in `C:\Program Files\Oculus\Support\oculus-dash\dash\bin` with the name `OculusDash.exe.bak`. **DO NOT replace the original `OculusDash.exe` file.** (see below if you've changed the Oculus install location)
    * If you've already downloaded Oculus Killer but replaced the original dashboard executable file entirely, you will likely need to reinstall Oculus. If you've backed the original up, ensure it has the name `OculusDash.exe.bak`
3. Open Oculus Dashboard Switcher and you will be shown a UAC (User Account Control) prompt. Click Yes.
    * ⚠️ **This tool requires elevated permissions to handle files in protected locations**, such as the Oculus install location.
4. Oculus Dashboard Switcher will tell you what dashboard is currently active (based on file size) before switching.
    * **NOTE:** Oculus Dashboard Switcher will refer to Oculus Killer as the "bypasser" dashboard.
5. Click Yes and Oculus Dashboard Switcher will swap the files `OculusDash.exe` and `OculusDash.exe.bak` with each other.

## Custom Oculus Install Location
By default, the tool will search for the necessary files in the following directory:<br>
`C:\Program Files\Oculus\Support\oculus-dash\dash\bin`

If you've changed the installation location of Oculus, you can specify the path where `OculusDash.exe` is located by passing it as a command line argument. The files will be located in:<br>
`(Oculus Install Location)\Support\oculus-dash\dash\bin`

**NOTE:** If your installation path contains spaces, **do not wrap it in quotation marks**. Ensure you're pointing to the folder containing `OculusDash.exe`, not the executable itself.

The files `OculusDash.exe` and `OculusDash.exe.bak` must be present in the target folder.