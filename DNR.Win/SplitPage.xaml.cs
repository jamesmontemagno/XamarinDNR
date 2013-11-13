using Windows.UI.Popups;
using DNR.Portable;
using DNR.Portable.Services;
using DNR.Win.Data;
using System;
using System.Collections.Generic;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Microsoft.WindowsAzure.MobileServices;

namespace DNR.Win
{
  public sealed partial class SplitPage : DNR.Win.Common.LayoutAwarePage
  {
    PodcastEpisode podcast;
    readonly DispatcherTimer timer;
    bool isPressed;
    double newValue;
    bool initialized;
    int timeToSet;


    public SplitPage()
    {
      this.InitializeComponent();
      itemDetailTitlePanel.Visibility = Visibility.Collapsed;
      timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
      timer.Tick += UpdateTimeDisplay;

      //detect if the user is touching the slider or not.
      Window.Current.CoreWindow.PointerPressed += (e, a) => { isPressed = true; newValue = -1; };
      Window.Current.CoreWindow.PointerReleased += (e, a) =>
      {
        isPressed = false;

        if (!initialized)
          return;
        if (newValue >= 0)
          player.Position = new TimeSpan(0, 0, (int)((newValue / 100) * player.NaturalDuration.TimeSpan.TotalSeconds));
      };
    }

    /// <summary>
    /// Update the user interface while playing
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void UpdateTimeDisplay(object sender, object e)
    {
      if (player.NaturalDuration.TimeSpan.TotalSeconds <= 0)
        return;

      var sliderVal = (player.Position.TotalSeconds / player.NaturalDuration.TimeSpan.TotalSeconds) * 100;
      if (sliderVal > 100)
        sliderVal = 100;
      else if (sliderVal < 0)
        sliderVal = 0;
      if (!isPressed)
        podcastSlider.Value = sliderVal;

      timeStamp.Text = string.Format("{0:hh\\:mm\\:ss}", player.Position) + " / " + string.Format("{0:hh\\:mm\\:ss}", player.NaturalDuration.TimeSpan);
    }

    void Play_Click(object sender, RoutedEventArgs e)
    {
      player.Play();
      timer.Start();
    }

    void Pause_Click(object sender, RoutedEventArgs e)
    {
      player.Pause();
      timer.Stop();
    }

    void Stop_Click(object sender, RoutedEventArgs e)
    {
      player.Stop();
      timer.Stop();
    }

    async void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this.UsingLogicalPageNavigation()) this.InvalidateVisualState();

      var selectedItem = (SampleDataItem)this.itemsViewSource.View.CurrentItem;

      if (selectedItem != null)
      {
        itemDetailTitlePanel.Visibility = Visibility.Visible;
        podcast = selectedItem.Podcast;
        initialized = false;
        player.Source = new Uri(podcast.AudioUrl);
        player.MediaOpened += (s, args) =>
        {
          initialized = true;
          player.Position = new TimeSpan(0, 0, timeToSet);
          UpdateTimeDisplay(null, null);
        };

        podcastSlider.Value = 0;//reset slider

        var updated = await podcast.GetTimeAsync();
        if (updated == null || updated.ShowNumber != podcast.ShowNumber)
          return;

        if (initialized)
        {
          player.Position = new TimeSpan(0, 0, updated.CurrentTime);
          UpdateTimeDisplay(null, null);
        }
        else
        {
          timeToSet = updated.CurrentTime;
        }
      }
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
      await podcast.SaveTimeAsync((int)player.Position.TotalSeconds);
    }


    private void PodcastSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      if (!isPressed)
        return;

      newValue = e.NewValue;
    }



    #region Page state management

    /// <summary>
    /// Populates the page with content passed during navigation.  Any saved state is also
    /// provided when recreating a page from a prior session.
    /// </summary>
    /// <param name="navigationParameter">The parameter value passed to
    /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
    /// </param>
    /// <param name="pageState">A dictionary of state preserved by this page during an earlier
    /// session.  This will be null the first time a page is visited.</param>
    protected async override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
    {

      if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
      {
        await Authenticate();
      }
      var group = SampleDataSource.GetGroup("Group-1");
      this.DefaultViewModel["Group"] = group;
      this.DefaultViewModel["Items"] = group.Items;

      if (pageState == null)
      {
        this.itemListView.SelectedItem = null;
        // When this is a new page, select the first item automatically unless logical page
        // navigation is being used (see the logical page navigation #region below.)
        if (!this.UsingLogicalPageNavigation() && this.itemsViewSource.View != null)
        {
          this.itemsViewSource.View.MoveCurrentToFirst();
        }
      }
      else
      {
        // Restore the previously saved state associated with this page
        if (pageState.ContainsKey("SelectedItem") && this.itemsViewSource.View != null)
        {
          var selectedItem = SampleDataSource.GetItem((String)pageState["SelectedItem"]);
          this.itemsViewSource.View.MoveCurrentTo(selectedItem);
        }
      }
    }

    /// <summary>
    /// Preserves state associated with this page in case the application is suspended or the
    /// page is discarded from the navigation cache.  Values must conform to the serialization
    /// requirements of <see cref="SuspensionManager.SessionState"/>.
    /// </summary>
    /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
    protected override void SaveState(Dictionary<String, Object> pageState)
    {
      if (this.itemsViewSource.View != null)
      {
        var selectedItem = (SampleDataItem)this.itemsViewSource.View.CurrentItem;
        if (selectedItem != null) pageState["SelectedItem"] = selectedItem.UniqueId;
      }
    }

    #endregion

    #region Logical page navigation

    // Visual state management typically reflects the four application view states directly
    // (full screen landscape and portrait plus snapped and filled views.)  The split page is
    // designed so that the snapped and portrait view states each have two distinct sub-states:
    // either the item list or the details are displayed, but not both at the same time.
    //
    // This is all implemented with a single physical page that can represent two logical
    // pages.  The code below achieves this goal without making the user aware of the
    // distinction.

    /// <summary>
    /// Invoked to determine whether the page should act as one logical page or two.
    /// </summary>
    /// <param name="viewState">The view state for which the question is being posed, or null
    /// for the current view state.  This parameter is optional with null as the default
    /// value.</param>
    /// <returns>True when the view state in question is portrait or snapped, false
    /// otherwise.</returns>
    private bool UsingLogicalPageNavigation(ApplicationViewState? viewState = null)
    {
      if (viewState == null) viewState = ApplicationView.Value;
      return viewState == ApplicationViewState.FullScreenPortrait ||
          viewState == ApplicationViewState.Snapped;
    }

    /// <summary>
    /// Invoked when the page's back button is pressed.
    /// </summary>
    /// <param name="sender">The back button instance.</param>
    /// <param name="e">Event data that describes how the back button was clicked.</param>
    protected override void GoBack(object sender, RoutedEventArgs e)
    {
      if (this.UsingLogicalPageNavigation() && itemListView.SelectedItem != null)
      {
        // When logical page navigatio is in effect and there's a selected item that
        // item's details are currently displayed.  Clearing the selection will return
        // to the item list.  From the user's point of view this is a logical backward
        // navigation.
        this.itemListView.SelectedItem = null;
      }
      else
      {
        // When logical page navigation is not in effect, or when there is no selected
        // item, use the default back button behavior.
        base.GoBack(sender, e);
      }
    }

    /// <summary>
    /// Invoked to determine the name of the visual state that corresponds to an application
    /// view state.
    /// </summary>
    /// <param name="viewState">The view state for which the question is being posed.</param>
    /// <returns>The name of the desired visual state.  This is the same as the name of the
    /// view state except when there is a selected item in portrait and snapped views where
    /// this additional logical page is represented by adding a suffix of _Detail.</returns>
    protected override string DetermineVisualState(ApplicationViewState viewState)
    {
      // Update the back button's enabled state when the view state changes
      var logicalPageBack = this.UsingLogicalPageNavigation(viewState) && this.itemListView.SelectedItem != null;
      var physicalPageBack = this.Frame != null && this.Frame.CanGoBack;
      this.DefaultViewModel["CanGoBack"] = logicalPageBack || physicalPageBack;

      // Determine visual states for landscape layouts based not on the view state, but
      // on the width of the window.  This page has one layout that is appropriate for
      // 1366 virtual pixels or wider, and another for narrower displays or when a snapped
      // application reduces the horizontal space available to less than 1366.
      if (viewState == ApplicationViewState.Filled ||
          viewState == ApplicationViewState.FullScreenLandscape)
      {
        var windowWidth = Window.Current.Bounds.Width;
        if (windowWidth >= 1366) return "FullScreenLandscapeOrWide";
        return "FilledOrNarrow";
      }

      // When in portrait or snapped start with the default visual state name, then add a
      // suffix when viewing details instead of the list
      var defaultStateName = base.DetermineVisualState(viewState);
      return logicalPageBack ? defaultStateName + "_Detail" : defaultStateName;
    }

    #endregion

  
    private void pageTitle_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
      this.Frame.Navigate(typeof(GetStarted));
    }



    private async System.Threading.Tasks.Task Authenticate()
    {
      while (AzureWebService.Instance.Client.CurrentUser == null)
      {
        string message;
        try
        {
          AzureWebService.Instance.Client.CurrentUser = await AzureWebService.Instance.Client
            .LoginAsync(MobileServiceAuthenticationProvider.Twitter);
          message =
            string.Format("You are now logged in - {0}", AzureWebService.Instance.Client.CurrentUser.UserId);
        }
        catch (InvalidOperationException ex)
        {
          message = "You must log in. Login Required";
        }


        var dialog = new MessageDialog(message);
        dialog.Commands.Add(new UICommand("OK"));
        await dialog.ShowAsync();
      }
    }

  }
}
