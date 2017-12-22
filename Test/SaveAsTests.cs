using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WixEdit.Xml;
using System.IO;

namespace Test
{
    [TestClass]
    public class SaveAsTests
    {
        [TestMethod]
        public void SaveAs_WithWixFileContainingNewLinesInAttributeValue_PreservesWhitespace()
        {
            // Arrange
            string expectedContents = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Wix xmlns=""{2}"">
  <Product Id=""{0}"" Name=""TestProduct"" Language=""1033"" Version=""0.0.0.1"" Manufacturer=""WixEdit"" UpgradeCode=""{1}"">
    <Package Description=""Test file in a Product"" Comments=""Simple test"" InstallerVersion=""200"" Compressed=""yes"" />
    <Media Id=""1"" Cabinet=""simple.cab"" EmbedCab=""yes"" />
    <Directory Id=""TARGETDIR"" Name=""SourceDir"">
      <Directory Id=""ProgramFilesFolder"" Name=""PFiles"" />
    </Directory>
    <Feature Id=""DefaultFeature"" Title=""Main Feature"" Level=""1"">
    </Feature>
    <UI />
    <CustomAction Id=""Test"" Property=""Test"" Value=""line 1
            line 2
            line 3
            line 4"" Execute=""immediate"" />
  </Product>
</Wix>",
            Guid.NewGuid().ToString().ToUpper(),
            Guid.NewGuid().ToString().ToUpper(),
            WixFiles.WixNamespaceUri);

            // create document
            WixFiles wixFiles = new WixFiles(expectedContents);

            string tempFilePath = null;

            try
            {
                tempFilePath = Path.GetTempFileName();

                // Act
                // save
                wixFiles.SaveAs(tempFilePath);

                // Assert
                string contents = File.ReadAllText(tempFilePath);

                Assert.AreEqual(expectedContents, contents);
            }
            finally
            {
                if ((!string.IsNullOrEmpty(tempFilePath)) && File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }
    }
}
