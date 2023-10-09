namespace WixEdit.src.Exceptions
{
    internal class WixBinariesDirectoryFileNotFoundException : WixEditException
    {
        public WixBinariesDirectoryFileNotFoundException(string fileName)
            : base($"The executable \"{fileName}\" could not be found.\r\n\r\nPlease specify the correct path to the Wix binaries in the settings dialog.")
        {
        }
    }
}
