using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Hosting;
using System.Threading;
using UnityEngine.TestTools;

namespace Unity.Connect.Share.Editor.Tests
{
    class ShareTest
    {
        ShareWindow shareWindow;
        string outputFolder;

        [SetUp]
        public void SetUp()
        {
            outputFolder = Path.Combine(UnityEngine.Application.temporaryCachePath, "TempBuild/");
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }
            shareWindow = ShareWindow.OpenShareWindow();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(outputFolder))
            {
                Directory.Delete(outputFolder, true);
            }
            shareWindow.Close();
        }

        [TestCase("A Game With Spaces", "A Game With Spaces", TestName = "Normal title")]
        [TestCase("           ", ShareUtils.DefaultGameName, TestName = "All spaces")]
        [TestCase("", ShareUtils.DefaultGameName, TestName = "Empty title")]
        public void GetFilteredGameTitle_HandlesAllCases_Success(string originalTitle, string expectedResult)
        {
            string filteredTitle = ShareUtils.GetFilteredGameTitle(originalTitle);
            Assert.AreEqual(expectedResult, filteredTitle);
        }

        [UnityTest]
        public IEnumerator EventSystem_OnError_ShowsErrorTab()
        {
            string previousTab = shareWindow.currentTab;
            shareWindow.Store.Dispatch(new OnErrorAction { errorMsg = "Please build project first!" });

            yield return null;

            Assert.AreNotEqual(previousTab, shareWindow.currentTab);
            Assert.AreEqual(ShareWindow.TAB_ERROR, shareWindow.currentTab);
        }

        const ulong KB = 1024ul;

        [TestCase(5ul, "5 B", TestName = "5 B")]
        [TestCase(5 * KB, "5.00 KB", TestName = "5 KB")]
        [TestCase(15 * KB * KB, "15.00 MB", TestName = "15 MB")]
        [TestCase(999 * KB * KB * KB, "999.00 GB", TestName = "999 GB")]
        public void FormatBytes_HandlesAllCases_Success(ulong bytes, string expectedResult)
        {
            Assert.AreEqual(expectedResult, ShareUtils.FormatBytes(bytes));
        }

        [Test]
        public void GetUnityVersionOfBuild_ValidBuild_Success()
        {
            List<string> lines = new List<string>();
            lines.Add("m_EditorVersion: 2019.3.4f1");
            lines.Add("m_EditorVersionWithRevision: 2019.3.4f1(4f139db2fdbd)");
            File.WriteAllLines(Path.Combine(outputFolder, "ProjectVersion.txt"), lines);
            Assert.AreEqual("2019.3", ShareUtils.GetUnityVersionOfBuild(outputFolder));
        }

        [Test]
        public void GetUnityVersionOfBuild_InvalidVersionFile_Fails()
        {
            List<string> lines = new List<string>();
            lines.Add("m_EditorVersion: broken data");
            lines.Add("m_EditorVersionWithRevision: broken data");

            File.WriteAllLines(Path.Combine(outputFolder, "ProjectVersion.txt"), lines);
            Assert.AreEqual(string.Empty, ShareUtils.GetUnityVersionOfBuild(outputFolder));
        }
    }
}
