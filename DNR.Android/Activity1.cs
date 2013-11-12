using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using DNR.Droid;
using DNR.Portable;
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

      var fetcher = new PodcastFetcher();
      var podcasts = await fetcher.GetPodcastsAsync();
      podcastAdapter.Podcasts = new List<PodcastEpisode>(podcasts);
      podcastAdapter.NotifyDataSetChanged();
    }


    private void SetupAzure()
    {
      CurrentPlatform.Init();
    }
  }
}


