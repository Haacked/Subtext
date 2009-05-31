﻿using System.Collections.Generic;
using System.Net;
using MbUnit.Framework;
using Moq;
using Subtext.Extensibility;
using Subtext.Framework;
using Subtext.Framework.Components;
using Subtext.Framework.Exceptions;
using Subtext.Framework.Services;

namespace UnitTests.Subtext.Framework.Services
{
    [TestFixture]
    public class CommentFilterTests
    {
        /// <summary>
        /// Make sure that comments and Track/Pingbacks generated 
        /// by the blog owner (logged in Administrator) don't get 
        /// filtered.
        /// </summary>
        [Test]
        public void FilterBeforePersistDoesNotAllowTooManyCommentsWithinCommentDelay()
        {
            //arrange
            var subtextContext = new Mock<ISubtextContext>();
            TestCache cache = new TestCache();
            cache["COMMENT FILTER:127.0.0.1"] = new FeedbackItem(FeedbackType.Comment);
            subtextContext.Setup(c => c.Cache).Returns(cache);
            subtextContext.Setup(c => c.User.IsInRole("Admins")).Returns(false); // change to true.
            subtextContext.Setup(c => c.Blog).Returns(new Blog { CommentDelayInMinutes = 100 });
            var commentSpamFilter = new Mock<ICommentSpamService>();
            var commentFilter = new CommentFilter(subtextContext.Object, commentSpamFilter.Object);

            //act, assert (no throw)
            UnitTestHelper.AssertThrows<CommentFrequencyException>(() =>
                commentFilter.FilterBeforePersist(new FeedbackItem(FeedbackType.Comment) { IpAddress = IPAddress.Parse("127.0.0.1") })
            );
        }

        /// <summary>
        /// Make sure that comments and Track/Pingbacks generated 
        /// by the blog owner (logged in Administrator) don't get 
        /// filtered.
        /// </summary>
        [Test]
        public void FilterBeforePersistIgnoresAdminRole()
        {
            //arrange
            var subtextContext = new Mock<ISubtextContext>();
            TestCache cache = new TestCache();
            cache["COMMENT FILTER:127.0.0.1"] = new FeedbackItem(FeedbackType.Comment);
            subtextContext.Setup(c => c.Cache).Returns(cache);
            subtextContext.Setup(c => c.User.IsInRole("Admins")).Returns(true);
            subtextContext.Setup(c => c.Blog).Returns(new Blog { CommentDelayInMinutes = 1 });
            var commentSpamFilter = new Mock<ICommentSpamService>();
            var commentFilter = new CommentFilter(subtextContext.Object, commentSpamFilter.Object);

            //act, assert (no throw)
            commentFilter.FilterBeforePersist(new FeedbackItem(FeedbackType.PingTrack) { IpAddress = IPAddress.Parse("127.0.0.1") });
        }

        [Test]
        public void FilterBeforePersistDoesNotAllowDuplicateComments()
        {
            //arrange
            Queue<string> recentCommentChecksums = new Queue<string>();
            recentCommentChecksums.Enqueue("TestChecksum");
            var subtextContext = new Mock<ISubtextContext>();
            TestCache cache = new TestCache();
            cache["COMMENT FILTER:.RECENT_COMMENTS"] = recentCommentChecksums;
            subtextContext.Setup(c => c.Cache).Returns(cache);
            subtextContext.Setup(c => c.User.IsInRole("Admins")).Returns(false);
            subtextContext.Setup(c => c.Blog).Returns(new Blog { CommentDelayInMinutes = 0, DuplicateCommentsEnabled = false });
            var commentSpamFilter = new Mock<ICommentSpamService>();
            var commentFilter = new CommentFilter(subtextContext.Object, commentSpamFilter.Object);

            //act, assert
            UnitTestHelper.AssertThrows<CommentDuplicateException>(() =>
                commentFilter.FilterBeforePersist(new FeedbackItem(FeedbackType.Comment) { ChecksumHash = "TestChecksum" })
            );
        }

        [Test]
        [RollBack]
        public void FilterAfterPersistWithCommentModerationDisabledCausesNewCommentsToBeActive()
        {
            //arrange
            var subtextContext = new Mock<ISubtextContext>();
            TestCache cache = new TestCache();
            subtextContext.Setup(c => c.Cache).Returns(cache);
            subtextContext.Setup(c => c.User.IsInRole("Admins")).Returns(false);
            subtextContext.Setup(c => c.Blog).Returns(new Blog { ModerationEnabled = false });
            FeedbackItem savedFeedback = null;
            subtextContext.Setup(c => c.Repository.Update(It.IsAny<FeedbackItem>())).Callback<FeedbackItem>(f => savedFeedback = f);

            var commentSpamFilter = new Mock<ICommentSpamService>();
            var commentFilter = new CommentFilter(subtextContext.Object, commentSpamFilter.Object);
            var feedback = new FeedbackItem(FeedbackType.Comment) { };
            Assert.IsFalse(feedback.Approved);

            //act
            commentFilter.FilterAfterPersist(feedback);

            //assert
            Assert.IsTrue(savedFeedback.Approved);
        }

        [Test]
        [RollBack]
        public void FilterAfterPersistWithCommentModerationEnabledCausesNewCommentsToNeedApproval()
        {
            //arrange
            var subtextContext = new Mock<ISubtextContext>();
            TestCache cache = new TestCache();
            subtextContext.Setup(c => c.Cache).Returns(cache);
            subtextContext.Setup(c => c.User.IsInRole("Admins")).Returns(false);
            subtextContext.Setup(c => c.Blog).Returns(new Blog { ModerationEnabled = true });
            FeedbackItem savedFeedback = null;
            subtextContext.Setup(c => c.Repository.Update(It.IsAny<FeedbackItem>())).Callback<FeedbackItem>(f => savedFeedback = f);

            var commentSpamFilter = new Mock<ICommentSpamService>();
            var commentFilter = new CommentFilter(subtextContext.Object, commentSpamFilter.Object);
            var feedback = new FeedbackItem(FeedbackType.Comment) { };
            Assert.IsFalse(feedback.NeedsModeratorApproval);

            //act
            commentFilter.FilterAfterPersist(feedback);

            //assert
            Assert.IsTrue(savedFeedback.NeedsModeratorApproval);
        }
    }
}
