using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OculusDashboardSwitcher
{
    public class RuntimeSwitcher
    {
        private const string OpenXR64BitRegistryKey = @"SOFTWARE\Khronos\OpenXR\1";
        private const string OpenXR32BitRegistryKey = @"SOFTWARE\WOW6432Node\Khronos\OpenXR\1";

        private const string ActiveRuntimeValue = "ActiveRuntime";

        public string GetCurrent64BitRuntime()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(OpenXR64BitRegistryKey))
            {
                return key?.GetValue(ActiveRuntimeValue) as string ?? string.Empty;
            }
        }

        // Currently unused, but left in just in case.
        public string GetCurrent32BitRuntime()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(OpenXR32BitRegistryKey))
            {
                return key?.GetValue(ActiveRuntimeValue) as string ?? string.Empty;
            }
        }

        public void SetOpenXRRuntime(string manifestPath)
        {
            try
            {
                // Set 64 bit key
                using (var key = Registry.LocalMachine.OpenSubKey(OpenXR64BitRegistryKey, true))
                {
                    key?.SetValue(ActiveRuntimeValue, manifestPath, RegistryValueKind.String);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to swap OpenXR runtimes.\n" + ex.Message, "Oculus Dashboard Switcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            
        }
    }
}
