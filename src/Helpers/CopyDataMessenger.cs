using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace WixEdit.Helpers
{
    /// <summary>
    /// Summary description for CopyDataMessenger.
    /// </summary>
    public class CopyDataMessenger
    {
        [DllImport("User32")]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, ref COPYDATASTRUCT lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        public const int WM_COPYDATA = 0x4A;

        public CopyDataMessenger()
        {
        }

        public static string DecodeMessage(ref Message message)
        {
            string messageText = string.Empty;

            COPYDATASTRUCT cds = new COPYDATASTRUCT();
            cds = (COPYDATASTRUCT)Marshal.PtrToStructure(message.LParam, typeof(COPYDATASTRUCT));
            if (cds.cbData > 0)
            {
                byte[] data = new byte[cds.cbData];
                Marshal.Copy(cds.lpData, data, 0, cds.cbData);
                MemoryStream stream = new MemoryStream(data);
                BinaryFormatter b = new BinaryFormatter();
                messageText = (string)b.Deserialize(stream);
            }

            return messageText;
        }

        public static void SendMessage(IntPtr hWnd, string message)
        {
            BinaryFormatter b = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            b.Serialize(stream, message);
            stream.Flush();

            // Now move the data into a pointer so we can send
            // it using WM_COPYDATA:
            // Get the length of the data:
            int dataSize = (int)stream.Length;
            if (dataSize > 0)
            {
                byte[] data = new byte[dataSize];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(data, 0, dataSize);
                IntPtr ptrData = Marshal.AllocCoTaskMem(dataSize);
                Marshal.Copy(data, 0, ptrData, dataSize);

                COPYDATASTRUCT cds = new COPYDATASTRUCT();
                cds.cbData = dataSize;
                cds.dwData = IntPtr.Zero;
                cds.lpData = ptrData;
                int res = SendMessage(hWnd, WM_COPYDATA, 0, ref cds);

                // Clear up the data:
                Marshal.FreeCoTaskMem(ptrData);
            }

            stream.Close();
        }
    }
}
