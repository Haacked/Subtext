﻿using System;
using System.Collections.Specialized;
using System.Web.UI;
using MbUnit.Framework;
using Moq;
using Subtext.Framework;
using Subtext.Framework.UI.Skinning;
using Subtext.Web.UI.Controls;
using Subtext.Web.UI.Pages;

namespace UnitTests.Subtext.SubtextWeb.UI.Pages
{
    [TestFixture]
    public class SubtextMasterPageTests
    {
        [Test]
        public void AddControlToBody_WithComments_AddsControlToUpdatePanel()
        {
            // arrange
            var updatePanel = new UpdatePanel();
            var control = new UserControl {Visible = false};
            var bodyControl = new UserControl();
            var page = new SubtextMasterPage();

            // act
            page.AddControlToBody("Comments", control, updatePanel, bodyControl);
            
            // assert
            Assert.AreEqual(control, updatePanel.ContentTemplateContainer.Controls[0]);
            Assert.IsTrue(control.Visible);
        }

        [Test]
        public void AddControlToBody_WithPostComment_AddsControlToUpdatePanelAndUpdatePanelToCenterBodyControl()
        {
            // arrange
            var updatePanel = new UpdatePanel();
            var postCommentControl = new PostComment();
            var bodyControl = new UserControl();
            var page = new SubtextMasterPage();

            // act
            page.AddControlToBody("PostComment", postCommentControl, updatePanel, bodyControl);

            // assert
            Assert.AreEqual(postCommentControl, updatePanel.ContentTemplateContainer.Controls[0]);
            Assert.AreEqual(updatePanel, bodyControl.Controls[0]);
            Assert.IsTrue(postCommentControl.Visible);
        }

        [Test]
        public void AddControlToBody_WithOtherControl_AddsControlToBodyControl()
        {
            // arrange
            var updatePanel = new UpdatePanel();
            var control = new UserControl();
            var bodyControl = new UserControl();
            var page = new SubtextMasterPage();

            // act
            page.AddControlToBody("Other", control, updatePanel, bodyControl);

            // assert
            Assert.AreEqual(control, bodyControl.Controls[0]);
        }

        [RowTest]
        [Row("javascript", "scripts/test.js", "", "", @"<script type=""javascript"" src=""scripts/test.js""></script>")]
        [Row("javascript", "scripts/test.js", "", "/Subtext.Web/MyBlog/",
            @"<script type=""javascript"" src=""/Subtext.Web/MyBlog/scripts/test.js""></script>")]
        [Row("javascript", "~/scripts/test.js", "Subtext.Web", "/Anything/",
            @"<script type=""javascript"" src=""/Subtext.Web/scripts/test.js""></script>")]
        [Row("javascript", "~/scripts/test.js", "", "/Anything/",
            @"<script type=""javascript"" src=""/scripts/test.js""></script>")]
        [Row("javascript", "/scripts/test.js", "Subtext.Web", "/Anything/",
            @"<script type=""javascript"" src=""/scripts/test.js""></script>")]
        public void RenderScriptElementRendersAppropriatePath(string type, string src, string virtualDir,
                                                              string skinPath, string expected)
        {
            UnitTestHelper.SetHttpContextWithBlogRequest("localhost", "Anything", virtualDir);

            var script = new Script {Type = type, Src = src};

            string scriptTag = ScriptElementCollectionRenderer.RenderScriptElement(skinPath, script);
            Assert.AreEqual(scriptTag, expected + Environment.NewLine,
                            "The rendered script tag was not what we expected.");
        }

        [RowTest]
        [Row("style/test.css", "", "", @"style/test.css")]
        [Row("style/test.css", "", "/Subtext.Web/MyBlog/", @"/Subtext.Web/MyBlog/style/test.css")]
        [Row("~/style/test.css", "Subtext.Web", "/Anything/", @"/Subtext.Web/style/test.css")]
        [Row("~/style/test.css", "", "/Anything/", "/style/test.css")]
        [Row("/style/test.css", "Subtext.Web", "/Anything/", "/style/test.css")]
        [Row("http://haacked.com/style/test.css", "Subtext.Web", "/Anything/", "http://haacked.com/style/test.css")]
        public void GetStylesheetHrefPathRendersAppropriatePath(string src, string virtualDir, string skinPath,
                                                                string expected)
        {
            UnitTestHelper.SetHttpContextWithBlogRequest("localhost", "Anything", virtualDir);

            var style = new Style {Href = src};

            string stylePath = StyleSheetElementCollectionRenderer.GetStylesheetHrefPath(skinPath, style);
            Assert.AreEqual(stylePath, expected, "The rendered style path was not what we expected.");
        }
        
        [Test]
        public void InitializeControls_WithControlNames_AddsControlsToBody()
        {
            // arrange
            var page = new SubtextMasterPage();
            var context = new Mock<ISubtextContext>();
            page.SubtextContext = context.Object;
            context.Setup(c => c.HttpContext.Request.UrlReferrer).Returns((Uri)null);
            context.Setup(c => c.HttpContext.Request.IsLocal).Returns(false);
            page.SetControls(new[]{"Test"});
            var loader = new Mock<ISkinControlLoader>();
            loader.Setup(l => l.LoadControl("Test")).Returns(new UserControl());

            // act
            page.InitializeControls(loader.Object);

            // assert
            loader.Verify(l => l.LoadControl("Test"));
        }

        [Test]
        public void InitializeControls_WithReferrer_LoadsMoreResultsControl()
        {
            // arrange
            var page = new SubtextMasterPage();
            var context = new Mock<ISubtextContext>();
            page.SubtextContext = context.Object;
            context.Setup(c => c.HttpContext.Request.UrlReferrer).Returns(new Uri("http://bing.com/?q=test"));
            context.Setup(c => c.HttpContext.Request.IsLocal).Returns(false);
            context.Setup(c => c.HttpContext.Request.Url).Returns(new Uri("http://example.com/"));
            page.SetControls(new[] { "Test" });
            var loader = new Mock<ISkinControlLoader>();
            loader.Setup(l => l.LoadControl("MoreResults")).Returns(new UserControl());

            // act
            page.InitializeControls(loader.Object);

            // assert
            loader.Verify(l => l.LoadControl("Test"));
        }

        [Test]
        public void InitializeControls_WithReferrerButNoControls_LoadsMoreResultsControl()
        {
            // arrange
            var page = new SubtextMasterPage();
            var context = new Mock<ISubtextContext>();
            page.SubtextContext = context.Object;
            context.Setup(c => c.HttpContext.Request.UrlReferrer).Returns(new Uri("http://bing.com/?q=test"));
            context.Setup(c => c.HttpContext.Request.IsLocal).Returns(false);
            page.SetControls(null);
            var loader = new Mock<ISkinControlLoader>();
            loader.Setup(l => l.LoadControl("MoreResults")).Throws(new InvalidOperationException());

            // act, assert
            page.InitializeControls(loader.Object);
        }


        [Test]
        public void InitializeControls_WithLocalRequsetAndReferrerInQueryString_LoadsMoreResultsControl()
        {
            // arrange
            var page = new SubtextMasterPage();
            var context = new Mock<ISubtextContext>();
            page.SubtextContext = context.Object;
            context.Setup(c => c.HttpContext.Request.UrlReferrer).Returns((Uri)null);
            context.Setup(c => c.HttpContext.Request.IsLocal).Returns(true);
            context.Setup(c => c.HttpContext.Request.Url).Returns(new Uri("http://example.com/"));
            var queryString = new NameValueCollection { { "referrer", "http://bing.com/?q=test" } };
            context.Setup(c => c.HttpContext.Request.QueryString).Returns(queryString);
            page.SetControls(new[] { "Test" });
            var loader = new Mock<ISkinControlLoader>();
            loader.Setup(l => l.LoadControl("MoreResults")).Returns(new UserControl());

            // act
            page.InitializeControls(loader.Object);

            // assert
            loader.Verify(l => l.LoadControl("Test"));
        }
    }
}