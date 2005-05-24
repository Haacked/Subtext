using System;
using Subtext.Extensibility.Providers;
using Subtext.Framework.Configuration;

namespace Subtext.Framework.Providers
{
	/// <summary>
	/// Summary description for EmailProvider.
	/// </summary>
	public sealed class EmailProvider
	{
		private EmailProvider(){}

		static EmailProvider()
		{
			EmailProviderConfiguration emailProvider = Config.Settings.BlogProviders.EmailProvider;
			imail = (IMailProvider)emailProvider.Instance();
			imail.AdminEmail = emailProvider.AdminEmail;
			imail.SmtpServer = emailProvider.SmtpServer;
			imail.Password = emailProvider.Password;
			imail.UserName = emailProvider.UserName;
			
		}

		private static IMailProvider imail = null;
		public static IMailProvider Instance()
		{
			return imail;
		}
	}
}
