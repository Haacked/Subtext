#region Disclaimer/Info
///////////////////////////////////////////////////////////////////////////////////////////////////
// Subtext WebLog
// 
// Subtext is an open source weblog system that is a fork of the .TEXT
// weblog system.
//
// For updated news and information please visit http://subtextproject.com/
// Subtext is hosted at SourceForge at http://sourceforge.net/projects/subtext
// The development mailing list is at subtext-devs@lists.sourceforge.net 
//
// This project is licensed under the BSD license.  See the License.txt file for more information.
///////////////////////////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using Subtext.Extensibility.Plugins;
using Subtext.Framework;

namespace Subtext.Web.Admin.Pages
{
	public partial class PluginListPage : AdminOptionsPage
	{
		private const string VSKEY_KEYWORDID = "LinkID";

		private int _resultsPageNumber = 0;
		private bool _isListHidden = false;

		public string PluginID
		{
			get
			{
				if (ViewState[VSKEY_KEYWORDID] != null)
					return (string)ViewState[VSKEY_KEYWORDID];
				else
					return String.Empty;
			}
			set { ViewState[VSKEY_KEYWORDID] = value; }
		}

		protected override void Page_Load(object sender, EventArgs e)
		{
			base.Page_Load(sender, e);
			if (!IsPostBack)
			{
				if (null != Request.QueryString[Keys.QRYSTR_PAGEINDEX])
					_resultsPageNumber = Convert.ToInt32(Request.QueryString[Keys.QRYSTR_PAGEINDEX]);

				this.resultsPager.PageSize = Preferences.ListingItemCount;
				this.resultsPager.PageIndex = _resultsPageNumber;
				Results.Collapsible = false;

				BindList();
			}
		}

		private void BindList()
		{
			View.Visible = false;
			Dictionary<string, IPlugin>.ValueCollection pluginList = STApplication.Current.Plugins.Values;
			pluginListRpt.DataSource = pluginList;
			pluginListRpt.DataBind();
		}

		protected void pluginListRpt_ItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				Label pluginId = (Label)e.Item.FindControl("pluginId");
				Label pluginName = (Label)e.Item.FindControl("pluginName");
				Label pluginDescription = (Label)e.Item.FindControl("pluginDescription");

				LinkButton lnkView = (LinkButton)e.Item.FindControl("lnkView");
				LinkButton lnkEnable = (LinkButton)e.Item.FindControl("lnkEnable");
				LinkButton lnkPluginSettings = (LinkButton)e.Item.FindControl("lnkPluginSettings");
				LinkButton lnkDisable = (LinkButton)e.Item.FindControl("lnkDisable");

				IPlugin currentPlugin = (IPlugin)e.Item.DataItem;

				pluginId.Text = currentPlugin.Id.Guid.ToString();
				pluginName.Text = currentPlugin.Info.Name;
				pluginDescription.Text = currentPlugin.Info.Description;

				lnkView.CommandArgument = lnkEnable.CommandArgument = currentPlugin.Id.Guid.ToString();
				lnkPluginSettings.CommandArgument = lnkDisable.CommandArgument = currentPlugin.Id.Guid.ToString();

				if (IsPluginEnabled(currentPlugin))
				{
					lnkView.Visible = false;
					lnkPluginSettings.Visible = true;

					lnkEnable.Visible = false;
					lnkDisable.Visible = true;
				}
				else
				{
					lnkView.Visible = true;
					lnkPluginSettings.Visible = false;

					lnkEnable.Visible = true;
					lnkDisable.Visible = false;
				}
			}
		}

		private bool IsPluginEnabled(IPlugin plugin)
		{
			if (plugin.Info.Name.IndexOf("2")!=-1)
				return true;
			else
				return false;
		}

		protected void pluginListRpt_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
		{
			switch (e.CommandName.ToLower(System.Globalization.CultureInfo.InvariantCulture))
			{
				case "view":
					PluginID = e.CommandArgument.ToString();
					BindViewLink();
					break;
				case "settings":
					//Load the plugin info, and display the custom user control to edit global plugin settings
					break;
				case "enable":
					//Enable the plugin for the current blog
					break;
				case "disable":
					//Disable the plugin for the current blog
					break;
				default:
					break;
			}
		}

		private void BindViewLink()
		{
			IPlugin currentPlugin = STApplication.Current.GetPluginByGuid(PluginID);
			if (currentPlugin != null)
			{
				Results.Collapsed = true;
				Results.Collapsible = true;
				Edit.Visible = false;
				View.Visible = true;
			}
			else
				Messages.ShowMessage("Unable to find plugin with id" + PluginID);
		}

		// REFACTOR
		public string CheckHiddenStyle()
		{
			if (_isListHidden)
				return Constants.CSSSTYLE_HIDDEN;
			else
				return String.Empty;
		}
	}
}
