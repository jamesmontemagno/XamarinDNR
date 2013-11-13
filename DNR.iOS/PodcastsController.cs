using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DNR.Portable;
using DNR.Portable.Services;
using Microsoft.WindowsAzure.MobileServices;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;

namespace DNR
{
  public class PodcastsController : UICollectionViewController
  {
    List<PodcastEpisode> podcasts;
    List<PodcastEpisode> filteredPodcasts;
    PodcastDetailController podcastController;
    UISearchBar searchBar;
    PodcastFetcher fetcher;
    UIActivityIndicatorView activityView;

    public PodcastsController(UICollectionViewLayout layout)
      : base(layout)
    {
      podcasts = new List<PodcastEpisode>();
      filteredPodcasts = podcasts;

      searchBar = new UISearchBar
      {
        Placeholder = "Search for a podcast",
        AutocorrectionType = UITextAutocorrectionType.No,
        AutocapitalizationType = UITextAutocapitalizationType.None,
        AutoresizingMask = UIViewAutoresizing.All,
        Alpha = 0.4f
      };

      searchBar.SizeToFit();

      searchBar.SearchButtonClicked += (sender, e) =>
      {
        Search(searchBar.Text);
        searchBar.ResignFirstResponder();
      };

      searchBar.TextChanged += (sender, e) => Search(e.SearchText);
    }

    public override void ViewDidLoad()
    {
      base.ViewDidLoad();

      AddActivityIndicator();

      CollectionView.BackgroundColor = UIColor.LightGray;
      CollectionView.RegisterNibForCell(PodcastCell.Nib, PodcastCell.Key);
      NavigationController.NavigationBar.Add(searchBar);
      NavigationController.NavigationBar.BackgroundColor = UIColor.Black;
      GetEpisodes();
    }

    async void GetEpisodes()
    {
      await Authenticate();

      fetcher = new PodcastFetcher();
      activityView.StartAnimating();
      podcasts.AddRange(await fetcher.GetPodcastsAsync());
      filteredPodcasts = podcasts;
      CollectionView.ReloadData();
      activityView.StopAnimating();
    }

    public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
    {
      if (podcastController == null)
      {
        podcastController = new PodcastDetailController();
      }

      searchBar.Hidden = true;
      Title = ".NET Rocks";

      podcastController.CurrentPodcastEpisode = podcasts[indexPath.Row];
      NavigationController.PushViewController(podcastController, true);
    }



    public override void ViewWillAppear(bool animated)
    {
      base.ViewWillAppear(animated);

      Title = string.Empty;
      searchBar.Hidden = false;

      var orientation = UIDevice.CurrentDevice.Orientation;

      if (orientation == UIDeviceOrientation.LandscapeLeft || orientation == UIDeviceOrientation.LandscapeRight)
        SetLayout(false);
      else
        SetLayout(true);
    }

    public override void ViewWillDisappear(bool animated)
    {
      base.ViewWillDisappear(animated);

      searchBar.ResignFirstResponder();
    }

   

    public override int GetItemsCount(UICollectionView collectionView, int section)
    {
      return filteredPodcasts.Count;
    }

    public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
    {
      var podcastCell = (PodcastCell)collectionView.DequeueReusableCell(PodcastCell.Key, indexPath);
      podcastCell.Name = filteredPodcasts[indexPath.Row].Name;
      podcastCell.DetailText = filteredPodcasts[indexPath.Row].Description;

      return podcastCell;
    }

    void AddActivityIndicator()
    {
      activityView = new UIActivityIndicatorView
      {
        Frame = new RectangleF(0, 0, 50, 50),
        Center = View.Center,
        ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge
      };

      View.AddSubview(activityView);
    }

    public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
    {
      base.WillRotate(toInterfaceOrientation, duration);

      if (toInterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || toInterfaceOrientation == UIInterfaceOrientation.LandscapeRight)
        SetLayout(false);
      else
        SetLayout(true);
    }

    void SetLayout(bool isPortrait)
    {
      var layout = new UICollectionViewFlowLayout
      {
        MinimumInteritemSpacing = 5.0f,
        MinimumLineSpacing = 5.0f,
        SectionInset = new UIEdgeInsets(5, 5, 5, 5),
        ItemSize = isPortrait ? PodcastCell.PortraitSize : PodcastCell.LandscapeSize
      };

      CollectionView.SetCollectionViewLayout(layout, false);
    }

    void Search(string text)
    {
      filteredPodcasts = podcasts.Where(podcast =>
        podcast.Description.Contains(text) || podcast.Name.Contains(text)
      ).ToList();

      CollectionView.ReloadData();
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

        var alert = new UIAlertView("Login", message, null, "OK", null);
        alert.Show();
      }
    }
  }
}