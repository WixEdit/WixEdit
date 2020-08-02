using System.ComponentModel;
using System.Diagnostics;

namespace WixEdit.Helpers
{
    internal static class FileHelper
    {
        internal static void OpenTarget(string target)
        {
            // Navigate to it.
            try
            {
                Process.Start(target);
            }
            catch (Win32Exception)
            {
                // Workaround for:
                // "Win32Exception: The requested lookup key was not found in any active activation context"   
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe"; // Win2K+
                process.StartInfo.Arguments = "/c start " + target;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
            }
        }
    }
}
