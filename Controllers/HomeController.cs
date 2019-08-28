using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using MisakaDocs.JsonModels;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using MisakaDocs.Extensions;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MisakaDocs.Classes;

namespace MisakaDocs.Controllers
{
    public class HomeController : Controller
    {
        private readonly Config Options;
        private readonly ModuleContainer ModuleConfig;
        private JsonManager<SiteData> SiteDataManager;

        public HomeController(IOptions<Config> options, IOptions<ModuleContainer> moduleOptions, JsonManager<SiteData> siteDataManager)
        {
            Options = options.Value;
            ModuleConfig = moduleOptions.Value;
            SiteDataManager = siteDataManager;

            if (SiteDataManager.JsonObject.UsersLoggedIn == null)
                SiteDataManager.JsonObject.UsersLoggedIn = new List<string>();

            if (SiteDataManager.JsonObject.VisitorAddresses == null)
                SiteDataManager.JsonObject.VisitorAddresses = new List<string>();
        }

        private void SetFeedback(string title, string content)
        {
            TempData["HadFeedback"] = "true";
            TempData["FeedbackTitle"] = title;
            TempData["FeedbackMessage"] = content;
        }
        
        private void HideFeedback()
        {
            TempData["HadFeedback"] = "false";
        }

        private bool IsLoggedIn()
        {
            return (HttpContext.Session.Get<DiscordUser>("User") != null);
        }

        private void SetUserData()
        {
            DiscordUser currentUser = HttpContext.Session.Get<DiscordUser>("User");
            ViewData["Username"] = currentUser.Username;
            ViewData["Discriminator"] = currentUser.Discriminator;
            ViewData["AvatarUrl"] = $"https://cdn.discordapp.com/avatars/{currentUser.Id}/{currentUser.Avatar}.png?size=128";
        }

        public void SetSiteData()
        {
            ViewData["Visitors"] = SiteDataManager.JsonObject.SiteVisits;
            ViewData["Logins"] = SiteDataManager.JsonObject.UserLogins;
            ViewData["BugReports"] = Math.Max(SiteDataManager.JsonObject.BugsReported, 0);
            ViewData["FeatureSuggestions"] = SiteDataManager.JsonObject.FeaturesSuggested;
        }

        private async Task SendEmail(string title, string desc)
        {
            DiscordUser currentUser = HttpContext.Session.Get<DiscordUser>("User");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Misaka", Options.EmailAddress));
            message.To.Add(new MailboxAddress("Jeezy", Options.TargetEmailAddress));
            message.Subject = $"{currentUser.Username}#{currentUser.Discriminator} - {title}";
            message.Body = new TextPart("plain") { Text = desc };
            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync("smtp.gmail.com");
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(Options.EmailAddress, Options.EmailPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        public async Task<IActionResult> index(string code)
        {
            string userIP = Response.HttpContext.Connection.RemoteIpAddress.ToString();

            if (!SiteDataManager.JsonObject.VisitorAddresses.Contains(userIP))
            {
                SiteDataManager.JsonObject.SiteVisits++;
                SiteDataManager.JsonObject.VisitorAddresses.Add(userIP);
            }

            if (!string.IsNullOrEmpty(code) && !IsLoggedIn())
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("User-Agent", $"MisakaDocs ({Options.BaseUrl}, 0.0)");
                    var tokenParams = new Dictionary<string, string>() { { "grant_type", "authorization_code" }, { "code", code }, { "client_id", Options.DiscordClientId }, { "client_secret", Options.DiscordClientSecret }, { "redirect_uri", Options.BaseUrl } };
                    var res = await client.PostAsync("https://discordapp.com/api/oauth2/token", new FormUrlEncodedContent(tokenParams));
                    if(res.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        TokenResponse tokenResp = JsonConvert.DeserializeObject<TokenResponse>(await res.Content.ReadAsStringAsync());
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenResp.AccessToken}");
                        string getUserEndpoint = "https://discordapp.com/api/users/@me";
                        DiscordUser currentUser = JsonConvert.DeserializeObject<DiscordUser>(await client.GetStringAsync(getUserEndpoint));
                        SetFeedback($"Welcome {currentUser.Username}!", "You have successfully connected your Discord account to this website! You will be logged out after an hour of inactivity. If you'd like to logout before then, you may use the logout button on the navigation.");
                        HttpContext.Session.Set("User", currentUser);
                        string userQualifier = $"{currentUser.Username}#{currentUser.Discriminator}";

                        if (!SiteDataManager.JsonObject.UsersLoggedIn.Contains(userQualifier))
                        {
                            SiteDataManager.JsonObject.UsersLoggedIn.Add(userQualifier);
                            SiteDataManager.JsonObject.UserLogins++;
                        }      
                    }
                    else
                        SetFeedback("Connection Failed!", "Failed to connect your Discord account to the website, this shouldn't happen! Refer to the contact section.");

                    return Redirect("/");
                }
            }
            else if (IsLoggedIn())
                SetUserData();

            SetSiteData();

            await Task.CompletedTask;
            return View();
        }

        public IActionResult InviteToGuild()
        {
            return Redirect($"https://discordapp.com/api/oauth2/authorize?client_id={Options.MisakaClientId}&scope=bot&permissions={Options.Permissions.ToString()}");
        }

        public IActionResult commands()
        {
            if(IsLoggedIn())
                SetUserData();

            SetSiteData();
            
            ViewData["ModuleContainer"] = ModuleConfig;
            return View();
        }

        public IActionResult docs()
        {
            if (IsLoggedIn())
                SetUserData();

            SetSiteData();

            return View();
        }

        public IActionResult contact()
        {
            if (IsLoggedIn())
                SetUserData();

            SetSiteData();

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult LoginToDiscord()
        {
            if (IsLoggedIn())
            {
                SetFeedback("Already Logged In!", "You cannot connect your Discord account to this website as it's already connected.");
                return Redirect($"/");
            }

            string authUrl = "https://discordapp.com/api/oauth2/authorize?";
            string query = $"response_type=code&client_id={Options.DiscordClientId}&scope=identify&redirect_uri={Options.BaseUrl}";
            return Redirect($"{authUrl}{query}");
        }

        public IActionResult LogoutOfDiscord()
        {
            if (!IsLoggedIn())
            {
                SetFeedback("Not Logged In!", "You cannot disconnect your Discord account from this website as it's already not connected.");
                return Redirect($"/");
            }

            HttpContext.Session.Set<DiscordUser>("User", null);
            SetFeedback("Connection Ceased!", "You successfully logged out and disconnected your Discord account from this website.");
            return Redirect($"/");
        }

        public async Task<IActionResult> ReportBug(string title, string desc)
        {
            if (!IsLoggedIn())
            {
                SetFeedback("Not Logged In!", "In order to prevent spam and to contact users easily, you must connect your Discord account to this website before reporting a bug.");
                return Redirect("/");
            }
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(desc))
            {
                SetFeedback("Incomplete Fields!", "Either the title or the description doesn't have a value");
                return Redirect("/home/contact");
            }

            await SendEmail($"Bug Report: {title}", desc);
            SetFeedback("Sent Bug Report!", $"Successfully sent your bug report, you have been redirected back to home.");
            SiteDataManager.JsonObject.BugsReported++;
            await Task.CompletedTask;
            return Redirect($"/");
        }

        public async Task<IActionResult> SuggestFeature(string title, string desc)
        {
            if (!IsLoggedIn())
            {
                SetFeedback("Not Logged In!", "In order to prevent spam and to contact users easily, you must connect your Discord account to this website before requesting a feature.");
                return Redirect("/");
            }
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(desc))
            {
                SetFeedback("Incomplete Fields!", "Either the title or the description doesn't have a value");
                return Redirect("/home/contact");
            }

            await SendEmail($"Feature Request: {title}", desc);
            SetFeedback("Sent Feature Request!", $"Successfully sent your feature request, you have been redirected back to home.");
            SiteDataManager.JsonObject.FeaturesSuggested++;
            await Task.CompletedTask;
            return Redirect($"/");
        }
    }
}
