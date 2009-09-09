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
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Subtext.Framework;
using Subtext.Framework.Components;
using Subtext.Framework.Configuration;
using Subtext.Framework.Providers;
using Subtext.Framework.Tracking;
using Subtext.Web.UI.Controls;

namespace Subtext.Web
{
    /// <summary>
    /// This class writes out a consolidated rss feed for every blog in the system. 
    /// This is used by hosted solutions that contain an aggregate blog.
    /// </summary>
    public class RSSPage : AggregatePage
    {
        /// <summary>
        /// Returns the "Accept-Encoding" value from the HTTP Request header. 
        /// This is a list of encodings that may be sent to the browser.
        /// </summary>
        /// <remarks>
        /// Specifically we're looking for gzip.
        /// </remarks>
        /// <value></value>
        protected string AcceptEncoding
        {
            get { return Request.Headers["Accept-Encoding"]; }
        }

        #region Web Form Designer generated code

        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Load += new System.EventHandler(this.Page_Load);
        }

        #endregion

        private void Page_Load(object sender, EventArgs e)
        {
            int? groupId = GetGroupIdFromQueryString();

            Blog aggregateBlog = HostInfo.Instance.AggregateBlog;
            ICollection<Entry> entries = ObjectProvider.Instance().GetRecentEntries(
                HostInfo.Instance.AggregateBlog.Host, groupId, 25);

            //TODO: Use our other feed generation code.
            if(entries != null && entries.Count > 0)
            {
                string rssXml = GetRSS(entries, Request.ApplicationPath);
                Response.ContentEncoding = Encoding.UTF8;
                Response.ContentType = "text/xml";
                Response.Write(rssXml);
            }
        }

        private string GetRSS(IEnumerable<Entry> entries, string appPath)
        {
            if(!appPath.EndsWith("/"))
            {
                appPath += "/";
            }

            var sw = new StringWriter();
            var writer = new XmlTextWriter(sw);

            //RSS ROOT
            writer.WriteStartElement("rss");
            writer.WriteAttributeString("version", "2.0");
            writer.WriteAttributeString("xmlns:dc", "http://purl.org/dc/elements/1.1/");
            writer.WriteAttributeString("xmlns:trackback", "http://madskills.com/public/xml/rss/module/trackback/");
            writer.WriteAttributeString("xmlns:wfw", "http://wellformedweb.org/CommentAPI/");
            writer.WriteAttributeString("xmlns:slash", "http://purl.org/rss/1.0/modules/slash/");

            //Channel
            writer.WriteStartElement("channel");
            //Channel Description
            writer.WriteElementString("title", ConfigurationManager.AppSettings["AggregateTitle"]);
            writer.WriteElementString("link", Context.Request.Url.ToString());
            writer.WriteElementString("description", ConfigurationManager.AppSettings["AggregateDescription"]);
            writer.WriteElementString("generator", VersionInfo.VersionDisplayText);

            int serverTimeZone = Config.Settings.ServerTimeZone;
            string baseUrl = "http://{0}" + appPath + "{1}/";

            bool useAggBugs = Config.Settings.Tracking.EnableAggBugs;

            foreach(Entry entry in entries)
            {
                writer.WriteStartElement("item");
                writer.WriteElementString("title", entry.Title);

                string baselink = string.Format(baseUrl, entry.Blog.Host, entry.Blog.Subfolder);
                string link = string.Format(CultureInfo.InvariantCulture, baselink + "archive/{0:yyyy/MM/dd}/{1}.aspx",
                                            entry.DateCreated, entry.EntryName);
                writer.WriteElementString("link", link);

                DateTime entryTime = entry.DateCreated;

                writer.WriteElementString("pubDate", entry.Blog.TimeZone.ToUtc(entryTime).ToString("r"));
                writer.WriteStartElement("guid");
                writer.WriteAttributeString("isPermaLink", "true");
                writer.WriteString(link);
                writer.WriteEndElement();

                writer.WriteElementString("wfw:comment", string.Format(baselink + "comments/{0}.aspx", entry.Id));
                writer.WriteElementString("wfw:commentRss",
                                          string.Format(baselink + "comments/commentRss/{0}.aspx", entry.Id));
                writer.WriteElementString("comments", link + "#comment");
                writer.WriteElementString("slash:comments", entry.FeedBackCount.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("trackback:ping",
                                          string.Format(baselink + "services/trackbacks/{0}.aspx", entry.Id));


                writer.WriteStartElement("source");
                writer.WriteAttributeString("url", baselink + "rss.aspx");
                writer.WriteString(entry.Blog.Title);
                writer.WriteEndElement();

                string aggText = useAggBugs
                                     ? TrackingUrls.AggBugImage(string.Format(baselink + "aggbug/{0}.aspx", entry.Id))
                                     : string.Empty;

                writer.WriteElementString("description",
                                          string.Format(CultureInfo.InvariantCulture, "{0}{1}", entry.Description,
                                                        aggText));
                writer.WriteElementString("dc:creator", entry.Author);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
            sw.Close();
            return sw.ToString();
        }
    }
}