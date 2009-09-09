using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Subtext.Framework;
using Subtext.Framework.Configuration;
using Subtext.Framework.UI.Skinning;
using Subtext.Framework.Web;
using Subtext.Web.Admin.Pages;
using Subtext.Web.Properties;

namespace Subtext.Web.Admin
{
    public partial class Skins : AdminOptionsPage
    {
        private ICollection<SkinTemplate> mobileSkins;
        private ICollection<SkinTemplate> skins;

        protected ICollection<SkinTemplate> SkinTemplates
        {
            get
            {
                if(skins == null)
                {
                    var skinEngine = new SkinEngine();
                    skins = skinEngine.GetSkinTemplates(false /* mobile */).Values;
                    foreach(SkinTemplate template in skins)
                    {
                        if(template.MobileSupport == MobileSupport.Supported)
                        {
                            template.Name += Resources.Skins_MobileReady;
                        }
                    }
                }
                return skins;
            }
        }

        protected ICollection<SkinTemplate> MobileSkinTemplates
        {
            get
            {
                if(mobileSkins == null)
                {
                    var skinEngine = new SkinEngine();
                    var skins = new List<SkinTemplate>(skinEngine.GetSkinTemplates(true /* mobile */).Values);
                    skins.Insert(0, SkinTemplate.Empty);
                    mobileSkins = skins;
                }
                return mobileSkins;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            if(!IsPostBack)
            {
                BindLocalUI();
            }
            base.OnLoad(e);
        }

        protected override void BindLocalUI()
        {
            skinRepeater.DataSource = SkinTemplates;
            mobileSkinRepeater.DataSource = MobileSkinTemplates;
            DataBind();
        }

        protected SkinTemplate EvalSkin(object o)
        {
            return o as SkinTemplate;
        }

        protected string GetSkinClientId(object o)
        {
            return (o as SkinTemplate).SkinKey.Replace(".", "_");
        }

        protected string EvalChecked(object o)
        {
            if(IsSelectedSkin(o))
            {
                return "checked=\"checked\"";
            }
            else
            {
                return "";
            }
        }

        protected string EvalSelected(object o)
        {
            if(IsSelectedSkin(o))
            {
                return " selected";
            }
            else
            {
                return "";
            }
        }

        private bool IsSelectedSkin(object o)
        {
            string currentSkin = (o as SkinTemplate).SkinKey;
            string blogSkin = SubtextContext.Blog.Skin.SkinKey;
            return String.Equals(currentSkin, blogSkin, StringComparison.OrdinalIgnoreCase);
        }

        protected string GetSkinIconImage(object o)
        {
            var skin = o as SkinTemplate;

            var imageUrls = new[]
            {
                string.Format(CultureInfo.InvariantCulture, "~/skins/{0}/SkinIcon.png", skin.TemplateFolder),
                string.Format(CultureInfo.InvariantCulture, "~/skins/{0}/{1}-SkinIcon.png", skin.TemplateFolder,
                              skin.Name),
                "~/skins/_System/SkinIcon.png"
            };

            string imageUrl = imageUrls.First(path => File.Exists(Server.MapPath(path)));
            return HttpHelper.ExpandTildePath(imageUrl);
        }

        protected void OnSaveSkinClicked(object o, EventArgs args)
        {
            Blog blog = SubtextContext.Blog;
            var skinEngine = new SkinEngine();
            SkinTemplate skinTemplate =
                skinEngine.GetSkinTemplates(false /* mobile */).ItemOrNull(Request.Form["SkinKey"]);
            blog.Skin.TemplateFolder = skinTemplate.TemplateFolder;
            blog.Skin.SkinStyleSheet = skinTemplate.StyleSheet;
            Config.UpdateConfigData(blog);

            Messages.ShowMessage(Resources.Skins_SkinSaved);
            BindLocalUI();
        }
    }
}