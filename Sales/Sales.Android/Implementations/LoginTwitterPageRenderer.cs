﻿[assembly: Xamarin.Forms.ExportRenderer(
    typeof(Sales.Views.LoginTwitterPage),
    typeof(Sales.Droid.Implementations.LoginTwitterPageRenderer))]

namespace Sales.Droid.Implementations
{
    using System;
    using System.Threading.Tasks;
    using Android.App;
    using Common.Models;
    using Newtonsoft.Json;
    using Sales.Services;
    using Xamarin.Auth;
    using Xamarin.Forms;
    using Xamarin.Forms.Platform.Android;

    public class LoginTwitterPageRenderer : PageRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);
            var activity = this.Context as Activity;
            var TwitterKey = Xamarin.Forms.Application.Current.Resources["TwitterConsumerKey"].ToString();
            var TwitterSecret = Xamarin.Forms.Application.Current.Resources["TwitterConsumerSecret"].ToString();
            var TwitterRequestURL = Xamarin.Forms.Application.Current.Resources["TwitterRequestTokenURL"].ToString();
            var TwitterAuthURL = Xamarin.Forms.Application.Current.Resources["TwitterAuthorizeURL"].ToString();
            var TwitterCallbackURL = Xamarin.Forms.Application.Current.Resources["TwitterAccessTokenURL"].ToString();
            var TwitterURLAccess = Xamarin.Forms.Application.Current.Resources["Url"].ToString();

            var auth = new OAuth1Authenticator(
                consumerKey: TwitterKey,
                consumerSecret: TwitterSecret,
                requestTokenUrl: new Uri(TwitterRequestURL),
                authorizeUrl: new Uri(TwitterAuthURL),
                callbackUrl: new Uri(TwitterCallbackURL),
                accessTokenUrl: new Uri(TwitterURLAccess));

            auth.Completed += async (sender, eventArgs) =>
            {
                if (eventArgs.IsAuthenticated)
                {
                    var token = await GetTwitterProfileAsync(eventArgs.Account);
                    App.NavigateToProfile(token);
                }
                else
                {
                    App.HideLoginView();
                }
            };

            activity.StartActivity(auth.GetUI(activity));
        }

        public async Task<TokenResponse> GetTwitterProfileAsync(Account account)
        {
            var TwitterProfileInfoURL = Xamarin.Forms.Application.Current.Resources["TwitterProfileInfoURL"].ToString(); var requestUrl = new OAuth1Request(
                "GET",
                new Uri(TwitterProfileInfoURL), null,
                account);

            var response = await requestUrl.GetResponseAsync();
            var responseTwitter = JsonConvert.DeserializeObject<TwitterResponse>(response.GetResponseText());
            var url = Xamarin.Forms.Application.Current.Resources["UrlAPI"].ToString();
            var prefix = Xamarin.Forms.Application.Current.Resources["UrlPrefix"].ToString();
            var controller = Xamarin.Forms.Application.Current.Resources["UrlUsersController"].ToString();
            var apiService = new ApiService();
            var token = await apiService.LoginTwitter(
                url,
                prefix,
                $"{controller}/LoginTwitter",
                responseTwitter);
            return token;
        }
    }

}
