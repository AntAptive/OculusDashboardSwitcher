using System;
using System.IO;
using System.Media;
using System.Security.Principal;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;

namespace OculusDashboardSwitcher
{
    internal class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static void Main(string[] args)
        {
            // Hide the window. 5 will show it.
            ShowWindow(GetConsoleWindow(), 0);

            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);

            // Run as administrator
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = Process.GetCurrentProcess().MainModule.FileName,
                    UseShellExecute = true,
                    Verb = "runas", // Trigger UAC prompt
                    Arguments = string.Join(" ", Environment.GetCommandLineArgs().Skip(1)) // Remove the first argument which is the executable path
                };

                try
                {
                    Process.Start(processInfo);
                }
                catch (Exception)
                {
                    Environment.Exit(1);
                }
                return;
            }

            string oculusPath = @"C:\Program Files\Oculus";
            string steamappsPath = @"C:\Program Files (x86)\Steam\steamapps";

            // Set custom paths if provided
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower() == "-oculuspath")
                {
                    oculusPath = ReadPath(args, ref i);
                }
                else if (args[i].ToLower() == "-steamappspath")
                {
                    steamappsPath = ReadPath(args, ref i);
                }
            }

            string oculusDashPath = Path.Combine(oculusPath, @"Support\oculus-dash\dash\bin");
            string oculusDashExe = Path.Combine(oculusDashPath, "OculusDash.exe");
            string oculusDashBakExe = Path.Combine(oculusDashPath, "OculusDash.exe.bak");

            // Dev note: Oculus has a 32-bit OpenXR manifest file, but it doesn't seem to be used anymore, so only the 64-bit manifests are used here.
            string oculusOpenXRManifest = Path.Combine(oculusPath, @"Support\oculus-runtime\oculus_openxr_64.json");
            string steamOpenXRManifest = Path.Combine(steamappsPath, @"common\SteamVR\steamxr_win64.json");

            // Checks if Oculus or SteamVR are open
            CheckIfVRProcessesAreOpen();

            // Check if Steam install location is valid
            if (!File.Exists(steamOpenXRManifest))
            {
                MessageBox.Show(
                    $"{steamappsPath} is not a valid Steam install location.\n\n" +
                    $"The following should be a valid file:\n" +
                    $"{steamOpenXRManifest}\n\n" +
                    $"Please restart Oculus Dashboard Switcher with the command-line argument \"-steamappsPath\" followed by a valid Steam install location.",
                    "Oculus Dashboard Switcher",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            // Check if Oculus install location is valid
            if (!File.Exists(oculusOpenXRManifest))
            {
                MessageBox.Show(
                    $"{oculusPath} is not a valid Oculus install location.\n\n" +
                    $"The following should be a valid file:\n" +
                    $"{oculusOpenXRManifest}\n\n" +
                    $"Please restart Oculus Dashboard Switcher with the command-line argument \"-oculusPath\" followed by a valid Oculus install location.",
                    "Oculus Dashboard Switcher",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            if (Directory.Exists(oculusDashPath))
            {
                // Check if the necessary executables exist
                if (!File.Exists(oculusDashExe))
                {
                    MessageBox.Show("OculusDash.exe does not exist at " + oculusDashPath, "Oculus Dashboard Switcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                }
                else if (!File.Exists(oculusDashBakExe))
                {
                    MessageBox.Show("OculusDash.exe.bak does not exist at " + oculusDashPath, "Oculus Dashboard Switcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                }

                const int OneMB = 1 * 1024 * 1024;

                // If the dash is >1 MB, it's the original.
                string activeDash = new FileInfo(oculusDashExe).Length > OneMB ? "Original" : "Bypasser";
                string activeDashInv = new FileInfo(oculusDashExe).Length > OneMB ? "Bypasser" : "Original";

                SystemSounds.Asterisk.Play();

                DialogResult result = MessageBox.Show(
                    $"Currently active dashboard: {activeDash}.\nSwitch dashboard to {activeDashInv}?",
                    "Oculus Dashboard Switcher",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.No)
                {
                    Environment.Exit(0);
                }
                
                try
                {
                    // Rename the original temporarily
                    File.Move(oculusDashExe, Path.Combine(oculusDashPath, "OculusDash.exe.bak2"));

                    // Rename the backup to replace the original
                    File.Move(oculusDashBakExe, Path.Combine(oculusDashPath, "OculusDash.exe"));

                    // Rename the temporary
                    File.Move(Path.Combine(oculusDashPath, "OculusDash.exe.bak2"), Path.Combine(oculusDashPath, "OculusDash.exe.bak"));

                    // Calculate this again just to make sure the dashboard changed correctly
                    string newActiveDash = new FileInfo(oculusDashExe).Length > OneMB ? "Original" : "Bypasser";

                    PlayTaDaSound();

                    RuntimeSwitcher runtimeSwitcher = new RuntimeSwitcher();

                    // Only ask to swap OpenXR runtime if it's not already set
                    if (
                        (newActiveDash == "Original" && runtimeSwitcher.GetCurrent64BitRuntime().ToLower().Contains("steam")) ||
                        (newActiveDash == "Bypasser" && runtimeSwitcher.GetCurrent64BitRuntime().ToLower().Contains("oculus"))
                       )
                    {
                        DialogResult successResult = MessageBox.Show(
                           $"Success!\n\n" +
                           $"Active dashboard is now: {newActiveDash}\n\n" +
                           $"Would you like to set {(newActiveDash == "Original" ? "Oculus" : "SteamVR")} to the default OpenXR runtime?\n" +
                           $"Current OpenXR Runtime: {(runtimeSwitcher.GetCurrent64BitRuntime().ToLower().Contains("steam") ? "SteamVR" : "Oculus")}",
                           "Oculus Dashboard Switcher",
                           MessageBoxButtons.YesNo,
                           MessageBoxIcon.None);
                        if (successResult == DialogResult.No)
                        {
                            Environment.Exit(0);
                        }
                        else if (successResult == DialogResult.Yes)
                        {
                            // If original, set Oculus as OpenXR Runtime
                            if (newActiveDash == "Original")
                                runtimeSwitcher.SetOpenXRRuntime(oculusOpenXRManifest);

                            // If bypasser, set SteamVR as OpenXR Runtime
                            else if (newActiveDash == "Bypasser")
                                runtimeSwitcher.SetOpenXRRuntime(steamOpenXRManifest);

                            // Show success message if we got this far
                            PlayTaDaSound();
                            MessageBox.Show(
                                "Success!\n\n" +
                                $"Current OpenXR Runtime: {(runtimeSwitcher.GetCurrent64BitRuntime().ToLower().Contains("steam") ? "SteamVR" : "Oculus")}",
                                "Oculus Dashboard Switcher",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.None);
                        }
                    }
                    else
                    {
                        MessageBox.Show(
                            "Success!\n\n" +
                            $"Active dashboard is now: {newActiveDash}\n" +
                            $"Current OpenXR Runtime: {(runtimeSwitcher.GetCurrent64BitRuntime().ToLower().Contains("steam") ? "SteamVR" : "Oculus")}",
                            "Oculus Dashboard Switcher",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.None);
                    }

                    Environment.Exit(0);
                } catch (Exception ex)
                {
                    MessageBox.Show("Failed to swap Oculus dashboards.\n" + ex.Message, "Oculus Dashboard Switcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                }               
            }
            else // If directory does not exist
            {
                MessageBox.Show($"{oculusPath} is not a valid Oculus install location.\n{oculusDashPath} should be a valid directory.", "Oculus Dashboard Switcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        static string ReadPath(string[] args, ref int index)
        {
            index++; // Move to the next argument
            if (index >= args.Length)
                return "";

            var pathParts = new System.Collections.Generic.List<string>();

            // Collect all parts of the path until another flag is found
            while (index < args.Length && !args[index].StartsWith("-"))
            {
                pathParts.Add(args[index]);
                index++;
            }

            // Adjust index since the loop overshoots
            index--;

            return string.Join(" ", pathParts);
        }

        static void PlayTaDaSound()
        {
            try
            {
                using (SoundPlayer player = new SoundPlayer(@"C:\Windows\Media\tada.wav"))
                {
                    player.Play();
                }
            }
            catch
            {
                // Silently catch errors. This isn't a critical feature.
            }
        }

        static bool VRProcessesAreOpen()
        {
            string[] vrProcesses = { "OculusClient", "vrmonitor", "vrserver" };

            // Returns true if any of the processes are running
            return vrProcesses.Any(processName =>
                Process.GetProcessesByName(processName).Any());
        }

        static void CheckIfVRProcessesAreOpen()
        {
            if (VRProcessesAreOpen())
            {
                SystemSounds.Asterisk.Play();
                DialogResult result = MessageBox.Show(
                    "Instances of Oculus or SteamVR are open.\n" +
                    "Swapping the Oculus dashboard or OpenXR runtimes while either are open can cause issues. " +
                    "It is recommended that you do not continue until both are closed.\n\n" +
                    "Click Abort to close Oculus Dashboard Switcher.\n" +
                    "Click Retry to check if the apps are now closed.\n" +
                    "Click Ignore if you'd like to continue while these instances are open. (not recommended)",
                    "Oculus Dashboard Switcher",
                    MessageBoxButtons.AbortRetryIgnore,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Abort)
                    Environment.Exit(1);

                else if (result == DialogResult.Retry)
                    CheckIfVRProcessesAreOpen();

                else if (result == DialogResult.Ignore)
                {
                    SystemSounds.Asterisk.Play();

                    DialogResult ignoreConfirm = MessageBox.Show(
                        "WARNING\n\n" +
                        "Proceeding with Oculus or SteamVR open is DANGEROUS and may cause CORRUPTION.\n" +
                        "In the event, Oculus or SteamVR will need to be reinstalled.\n" +
                        "It is highly recommended you do not continue without closing Oculus and SteamVR.\n" +
                        "If these apps are not open visibly, check your Task Manager to ensure they are not running in the background.\n\n" +
                        "By continuing, you accept all risks of corruption.\n\n" +
                        "Are you sure you want to continue?",
                        "Oculus Dashboard Switcher",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (ignoreConfirm == DialogResult.No)
                        Environment.Exit(1);
                }
            }
        }
    }
}
