using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using AndroidHUD;
using DNR.Portable;
using DNR.Portable.Models;
using DNR.Portable.Services;
using Microsoft.WindowsAzure.MobileServices;
using Resource = DNR.Droid.Resource;

namespace DRN.Droid
{
  [Activity(Label = ".NET Rocks!", Theme = "@style/Theme", Icon = "@drawable/Icon", MainLauncher = true)]
  public class Activity1 : Activity
  {
    private static PodcastsViewModel viewModel;

    public static PodcastsViewModel ViewModel
    {
      get { return viewModel ?? (viewModel = new PodcastsViewModel()); }
    }

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
        
        var intent = new Intent();
        intent.SetClass(this, typeof(PodcastDetailActivity));
        intent.PutExtra("show_number", (int) e.Id);
        StartActivity(intent);
      };

      // Comment this out if you do not want Twitter Authentication
      // You will need to set your Azure permissions to any client with API Key
      await Authenticate();

      AndHUD.Shared.Show(this, Resources.GetString(Resource.String.loading));
      await ViewModel.ExecuteGetPodcastsCommand();
      podcastAdapter.Podcasts = ViewModel.Podcasts;
      podcastAdapter.NotifyDataSetChanged();
      AndHUD.Shared.Dismiss(this);
    }

    /// <summary>
    /// Initiate azure platform specific code, might be removed in future versions.
    /// </summary>
    private void SetupAzure()
    {
      CurrentPlatform.Init();
    }

    /// <summary>
    /// Authenticate the azure client with twitter authentication.
    /// </summary>
    /// <returns></returns>
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

        // display message to user.
        Toast.MakeText(this, message, ToastLength.Long).Show();
      }
    }

  }
}


