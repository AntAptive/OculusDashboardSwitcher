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
                    MessageBox.Show("Oculus Dashboard Switcher was not given administrative privileges.\nAdministrator is needed to handle files in protected locations.", "Oculus Dashboard Switcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                    return;
                }
                return;
            }

            string oculusDashPath = @"C:\Program Files\Oculus\Support\oculus-dash\dash\bin";

            if (args.Length != 0)
            {
                oculusDashPath = string.Join(" ", args);
            }

            string oculusDashExe = Path.Combine(oculusDashPath, "OculusDash.exe");
            string oculusDashBakExe = Path.Combine(oculusDashPath, "OculusDash.exe.bak");

            if (Directory.Exists(oculusDashPath))
            {
                // Check if the necessary executables exist
                if (!File.Exists(oculusDashExe))
                {
                    MessageBox.Show("OculusDash.exe does not exist at " + oculusDashPath, "Oculus Dashboard Switcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                    return;
                }
                else if (!File.Exists(oculusDashBakExe))
                {
                    MessageBox.Show("OculusDash.exe.bak does not exist at " + oculusDashPath, "Oculus Dashboard Switcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                    return;
                }

                const int OneMB = 1 * 1024 * 1024;

                // If the dash is >1 MB, it's the original.
                string activeDash = new FileInfo(oculusDashExe).Length > OneMB ? "Original" : "Bypasser";
                string activeDashInv = new FileInfo(oculusDashExe).Length > OneMB ? "Bypasser" : "Original";

                SystemSounds.Exclamation.Play();

                DialogResult result = MessageBox.Show(
                    $"Currently active dashboard: {activeDash}.\nSwitch dashboard to {activeDashInv}?",
                    "Oculus Dashboard Switcher",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.No)
                {
                    Environment.Exit(0);
                    return;
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

                    MessageBox.Show($"Successfully swapped Oculus dashboards.\nActive dashboard: {newActiveDash}", "Oculus Dashboard Switcher", MessageBoxButtons.OK, MessageBoxIcon.None);

                    Environment.Exit(0);
                } catch (Exception ex)
                {
                    MessageBox.Show("Failed to swap Oculus dashboards.\n" + ex.Message, "Oculus Dashboard Switcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                }               
            }
            else // If directory does not exist
            {
                MessageBox.Show(oculusDashPath + " is not a vaild directory.", "Oculus Dashboard Switcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
