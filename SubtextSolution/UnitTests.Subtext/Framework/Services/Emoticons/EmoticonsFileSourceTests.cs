﻿using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using MbUnit.Framework;
using Subtext.Framework.Emoticons;

namespace UnitTests.Subtext.Framework.Emoticons
{
    [TestFixture]
    public class EmoticonsFileSourceTests
    {
        [Test]
        public void GetEmoticons_WithFileSource_LoadsEmoticonsFromStreamReader()
        {
            //arrange
            var memoryStream = new MemoryStream();
            StreamWriter writer = new StreamWriter(memoryStream);
            writer.WriteLine("[:)]");
            writer.WriteLine("<img src=\"{0}\" title=\"happy\" />");
            writer.WriteLine("[:(]");
            writer.WriteLine("<img src=\"{0}\" title=\"sad\" />");
            writer.Flush();
            memoryStream.Position = 0;
            var emoticonsSource = new EmoticonsFileSource(new StreamReader(memoryStream));

            //act
            var emoticons = emoticonsSource.GetEmoticons();

            //assert
            Assert.AreEqual(2, emoticons.Count());
            Assert.AreEqual("[:)]", emoticons.First().EmoticonText);
            Assert.AreEqual("<img src=\"{0}\" title=\"happy\" />", emoticons.First().ImageTag);
            Assert.AreEqual("[:(]", emoticons.ElementAt(1).EmoticonText);
            Assert.AreEqual("<img src=\"{0}\" title=\"sad\" />", emoticons.ElementAt(1).ImageTag);
        }

        [Test]
        [Category("Integration")]
        public void GetEmoticons_WithFileSource_LoadsEmoticonsFromFile()
        {
            //arrange
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "emoticons.txt");
            UnitTestHelper.UnpackEmbeddedResource("Web.emoticons.txt", path);
            var emoticonsSource = new EmoticonsFileSource(path);

            //act
            var emoticons = emoticonsSource.GetEmoticons();

            //assert
            Assert.AreEqual(24, emoticons.Count());
        }
    }
}
