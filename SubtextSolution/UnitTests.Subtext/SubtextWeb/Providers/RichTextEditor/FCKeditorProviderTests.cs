#region Disclaimer/Info
///////////////////////////////////////////////////////////////////////////////////////////////////
// Subtext WebLog
// 
// Subtext is an open source weblog system that is a fork of the .TEXT
// weblog system.
//
// For updated news and information please visit http://subtextproject.com/
// Subtext is hosted at Google Code at http://code.google.com/p/subtext/
// The development mailing list is at subtext-devs@lists.sourceforge.net 
//
// This project is licensed under the BSD license.  See the License.txt file for more information.
///////////////////////////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Security.Principal;
using System.Threading;
using System.Web.UI.WebControls;
using MbUnit.Framework;
using Moq;
using Subtext.Framework;
using Subtext.Providers.BlogEntryEditor.FCKeditor;

namespace UnitTests.Subtext.SubtextWeb.Providers.RichTextEditor
{
    /// <summary>
    /// Summary description for FCKeditorProviderTests.
    /// </summary>
    [TestFixture]
    public class FCKeditorProviderTests
    {
        string _hostName;
        FckBlogEntryEditorProvider frtep;

        [SetUp]
        public void SetUp()
        {
            _hostName = UnitTestHelper.GenerateUniqueHostname();

            IPrincipal principal = UnitTestHelper.MockPrincipalWithRoles("Admins");
            Thread.CurrentPrincipal = principal;
            frtep = new FckBlogEntryEditorProvider();
            UnitTestHelper.SetHttpContextWithBlogRequest(_hostName, "MyBlog", "Subtext.Web");
        }

        [TearDown]
        public void TearDown()
        {
            Thread.CurrentPrincipal = null;
        }

        [Test]
        public void SetControlID()
        {
            string test = "MyTestControlID";
            frtep.ControlId = test;
            Assert.AreEqual(test, frtep.ControlId);
        }

        [Test]
        public void SetText()
        {
            Blog blog = new Blog { Host = "localhost", Subfolder = "subfolder" };
            string test = "Lorem Ipsum";
            var subtextContext = new Mock<ISubtextContext>();
            subtextContext.Setup(c => c.Blog).Returns(blog);
            frtep.InitializeControl(subtextContext.Object);
            frtep.Text = test;
            Assert.AreEqual(test, frtep.Text);
            Assert.AreEqual(test, frtep.Xhtml);
        }

        [Test]
        public void SetWidth()
        {
            Unit test = 200;
            Blog blog = new Blog { Host = "localhost", Subfolder = "subfolder" };
            var subtextContext = new Mock<ISubtextContext>();
            subtextContext.Setup(c => c.Blog).Returns(blog);
            frtep.InitializeControl(subtextContext.Object);

            frtep.Width = test;
            Assert.AreEqual(test, frtep.Width);
        }

        [Test]
        public void SetHeight()
        {
            Unit test = 100;
            Blog blog = new Blog { Host = "localhost", Subfolder = "subfolder" };
            var subtextContext = new Mock<ISubtextContext>();
            subtextContext.Setup(c => c.Blog).Returns(blog);
            frtep.InitializeControl(subtextContext.Object);
            frtep.Height = test;
            Assert.AreEqual(test, frtep.Height);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestInitializationWithNullName()
        {
            frtep.Initialize(null, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestInitializationWithNullConfigValue()
        {
            frtep.Initialize("FCKProvider", null);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestInitializationWithEmptyWebFolder()
        {
            frtep.Initialize("FCKProvider", new System.Collections.Specialized.NameValueCollection());
        }
    }
}
