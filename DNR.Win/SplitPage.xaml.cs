using Windows.UI.Popups;
using Windows.UI.Xaml.Controls.Primitives;
using DNR.Portable;
using DNR.Portable.Models;
using DNR.Portable.Services;
using DNR.Win.Common;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
// The Split Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234234
using Microsoft.WindowsAzure.MobileServices;

namespace DNR.Win
{
  /// <summary>
  /// A page that displays a group title, a list of items within the group, and details for
  /// the currently selected item.
  /// </summary>
  public sealed partial class SplitPage : Page
  {

    private PodcastsViewModel viewModel;

    public PodcastsViewModel ViewModel
    {
      get { return viewModel ?? (viewModel = new PodcastsViewModel()); }
    }

    private NavigationHelper navigationHelper;
    /// <summary>
    /// NavigationHelper is used on each page to aid in navigation and 
    /// process lifetime management
    /// </summary>
    public NavigationHelper NavigationHelper
    {
      get { return this.navigationHelper; }
    }

    readonly DispatcherTimer timer;
    bool isPressed;
    double newValue;
    bool initialized;
    int timeToSet;


    public SplitPage()
    {
      this.InitializeComponent();

      // Setup the navigation helper
      this.navigationHelper = new NavigationHelper(this);
      this.navigationHelper.LoadState += navigationHelper_LoadState;
      this.navigationHelper.SaveState += navigationHelper_SaveState;

      // Setup the logical page navigation components that allow
      // the page to only show one pane at a time.
      this.navigationHelper.GoBackCommand = new DNR.Win.Common.RelayCommand(() => this.GoBack(), () => this.CanGoBack());
      this.itemListView.SelectionChanged += itemListView_SelectionChanged;

      // Start listening for Window size changes 
      // to change from showing two panes to showing a single pane
      Window.Current.SizeChanged += Window_SizeChanged;
      this.InvalidateVisualState();

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
      if (player.NaturalDuration.TimeSpan.TotalSeconds <= 0 || ViewModel.SelectedEpisode == null)
        return;

      var sliderVal = (player.Position.TotalSeconds / player.NaturalDuration.TimeSpan.TotalSeconds) * 100;
      if (sliderVal > 100)
        sliderVal = 100;
      else if (sliderVal < 0)
        sliderVal = 0;
      if (!isPressed)
        podcastSlider.Value = sliderVal;

      timeStamp.Text = viewModel.SelectedEpisode.GetTimeDisplay(player.Position, player.NaturalDuration.TimeSpan);
      
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

    async void itemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this.UsingLogicalPageNavigation())
      {
        this.navigationHelper.GoBackCommand.RaiseCanExecuteChanged();
      }

      var selectedItem = itemListView.SelectedItem as PodcastEpisode;

      if (selectedItem != null)
      {
        ViewModel.SelectedEpisode = selectedItem;
        itemDetailTitlePanel.Visibility = Visibility.Visible;

        initialized = false;
        player.Source = new Uri(ViewModel.SelectedEpisode.AudioUrl);
        player.MediaOpened += (s, args) =>
        {
          initialized = true;
          player.Position = new TimeSpan(0, 0, timeToSet);
          UpdateTimeDisplay(null, null);
        };

        podcastSlider.Value = 0;//reset slider
        timeToSet = 0;

        var updated = await ViewModel.SelectedEpisode.GetTimeAsync();
        if (updated == null || updated.ShowNumber != ViewModel.SelectedEpisode.ShowNumber)
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
      await ViewModel.SelectedEpisode.SaveTimeAsync((int)player.Position.TotalSeconds);
    }


    private void PodcastSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      if (!isPressed)
        return;

      newValue = e.NewValue;
    }

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



    /// <summary>
    /// Populates the page with content passed during navigation.  Any saved state is also
    /// provided when recreating a page from a prior session.
    /// </summary>
    /// <param name="sender">
    /// The source of the event; typically <see cref="NavigationHelper"/>
    /// </param>
    /// <param name="e">Event data that provides both the navigation parameter passed to
    /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
    /// a dictionary of state preserved by this page during an earlier
    /// session.  The state will be null the first time a page is visited.</param>
    private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
    {
      // TODO: Assign a bindable group to Me.DefaultViewModel("Group")
      // TODO: Assign a collection of bindable items to Me.DefaultViewModel("Items")

      if (DataContext == null)
        DataContext = ViewModel;

      if (e.PageState == null)
      {
        // When this is a new page, select the first item automatically unless logical page
        // navigation is being used (see the logical page navigation #region below.)
        if (!this.UsingLogicalPageNavigation() && ViewModel.Podcasts.Count == 0)
        {
          // Comment this out if you do not want Twitter Authentication
          // You will need to set your Azure permissions to any client with API Key
          await Authenticate();

          await ViewModel.ExecuteGetPodcastsCommand();

          if(itemListView.Items != null && itemListView.Items.Count > 0)
            itemListView.SelectedItem = itemListView.Items[0];
        }
      }
      else
      {
        // Restore the previously saved state associated with this page
        if (e.PageState.ContainsKey("SelectedItem"))
        {
          itemListView.SelectedItem = e.PageState["SelectedItem"];
        }
      }
    }

    /// <summary>
    /// Preserves state associated with this page in case the application is suspended or the
    /// page is discarded from the navigation cache.  Values must conform to the serialization
    /// requirements of <see cref="SuspensionManager.SessionState"/>.
    /// </summary>
    /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
    /// <param name="e">Event data that provides an empty dictionary to be populated with
    /// serializable state.</param>
    private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
    {
      if (itemListView.SelectedItem != null)
      {
        e.PageState["SelectedItem"] = ViewModel.SelectedEpisode;

      }
    }

    #region Logical page navigation

    // The split page isdesigned so that when the Window does have enough space to show
    // both the list and the dteails, only one pane will be shown at at time.
    //
    // This is all implemented with a single physical page that can represent two logical
    // pages.  The code below achieves this goal without making the user aware of the
    // distinction.

    private const int MinimumWidthForSupportingTwoPanes = 768;

    /// <summary>
    /// Invoked to determine whether the page should act as one logical page or two.
    /// </summary>
    /// <returns>True if the window should show act as one logical page, false
    /// otherwise.</returns>
    private bool UsingLogicalPageNavigation()
    {
      return Window.Current.Bounds.Width < MinimumWidthForSupportingTwoPanes;
    }

    /// <summary>
    /// Invoked with the Window changes size
    /// </summary>
    /// <param name="sender">The current Window</param>
    /// <param name="e">Event data that describes the new size of the Window</param>
    private void Window_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
    {
      this.InvalidateVisualState();
    }

    /// <summary>
    /// Invoked when an item within the list is selected.
    /// </summary>
    /// <param name="sender">The GridView displaying the selected item.</param>
    /// <param name="e">Event data that describes how the selection was changed.</param>
    private void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      // Invalidate the view state when logical page navigation is in effect, as a change
      // in selection may cause a corresponding change in the current logical page.  When
      // an item is selected this has the effect of changing from displaying the item list
      // to showing the selected item's details.  When the selection is cleared this has the
      // opposite effect.
      if (this.UsingLogicalPageNavigation()) this.InvalidateVisualState();
    }

    private bool CanGoBack()
    {
      if (this.UsingLogicalPageNavigation() && this.itemListView.SelectedItem != null)
      {
        return true;
      }
      else
      {
        return this.navigationHelper.CanGoBack();
      }
    }
    private void GoBack()
    {
      if (this.UsingLogicalPageNavigation() && this.itemListView.SelectedItem != null)
      {
        // When logical page navigation is in effect and there's a selected item that
        // item's details are currently displayed.  Clearing the selection will return to
        // the item list.  From the user's point of view this is a logical backward
        // navigation.
        this.itemListView.SelectedItem = null;
      }
      else
      {
        this.navigationHelper.GoBack();
      }
    }

    private void InvalidateVisualState()
    {
      var visualState = DetermineVisualState();
      VisualStateManager.GoToState(this, visualState, false);
      this.navigationHelper.GoBackCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Invoked to determine the name of the visual state that corresponds to an application
    /// view state.
    /// </summary>
    /// <returns>The name of the desired visual state.  This is the same as the name of the
    /// view state except when there is a selected item in portrait and snapped views where
    /// this additional logical page is represented by adding a suffix of _Detail.</returns>
    private string DetermineVisualState()
    {
      if (!UsingLogicalPageNavigation())
        return "PrimaryView";

      // Update the back button's enabled state when the view state changes
      var logicalPageBack = this.UsingLogicalPageNavigation() && this.itemListView.SelectedItem != null;

      return logicalPageBack ? "SinglePane_Detail" : "SinglePane";
    }

    #endregion

    #region NavigationHelper registration

    /// The methods provided in this section are simply used to allow
    /// NavigationHelper to respond to the page's navigation methods.
    /// 
    /// Page specific logic should be placed in event handlers for the  
    /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
    /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
    /// The navigation parameter is available in the LoadState method 
    /// in addition to page state preserved during an earlier session.

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      navigationHelper.OnNavigatedTo(e);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
      navigationHelper.OnNavigatedFrom(e);
    }

    #endregion
  }
}
