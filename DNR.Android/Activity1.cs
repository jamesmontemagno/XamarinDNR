using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using DNR.Droid;
using DNR.Portable;
using DNR.Portable.Services;
using Microsoft.WindowsAzure.MobileServices;

namespace DRN.Droid
{
  [Activity(Label = ".NET Rocks!", Theme = "@style/Theme", Icon = "@drawable/Icon", MainLauncher = true)]
  public class Activity1 : Activity
  {
    private PodcastsAdapter podcastAdapter;
    protected async override void OnCreate(Bundle bundle)
    {
      base.OnCreate(bundle);

      SetContentView(Resource.Layout.Main);
      SetupAzure();

      var podcastsView = FindViewById<ListView>(Resource.Id.podcastsListView);
      podcastAdapter = new PodcastsAdapter(this);
      podcastsView.Adapter = podcastAdapter;

      podcastsView.ItemClick += (sender, e) =>
      {

        var episode = ((PodcastsAdapter)podcastsView.Adapter).Podcasts[e.Position];

        PodcastDetailActivity.CurrentPodcastEpisode = episode;

        var intent = new Intent();
        intent.SetClass(this, typeof(PodcastDetailActivity));
        StartActivity(intent);
      };

      await Authenticate();

      var fetcher = new PodcastFetcher();
      var podcasts = await fetcher.GetPodcastsAsync();
      podcastAdapter.Podcasts = new List<PodcastEpisodeSecure>(podcasts);
      podcastAdapter.NotifyDataSetChanged();
    }

    private async System.Threading.Tasks.Task Authenticate()
    {
      while (AzureWebService.Instance.Client.CurrentUser == null)
      {
        string message;
        try
        {
          AzureWebService.Instance.Client.CurrentUser = await AzureWebService.Instance.Client
            .LoginAsync(this, MobileServiceAuthenticationProvider.Twitter);
          message =
            string.Format("You are now logged in - {0}", AzureWebService.Instance.Client.CurrentUser.UserId);
        }
        catch (InvalidOperationException ex)
        {
          message = "You must log in. Login Required";
        }

        Toast.MakeText(this, message, ToastLength.Long).Show();
      }
    }

    private void SetupAzure()
    {
      CurrentPlatform.Init();
    }
  }
}


