using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using DNR.Portable.Helpers;
using DNR.Portable.Models;

namespace DNR.Portable
{
  /// <summary>
  /// Retrieves a list of podcasts from a feed
  /// </summary>
  public class PodcastsViewModel : ViewModelBase
  {
    //data from server
    const string PodcastUrl = "http://www.pwop.com/feed.aspx?show=dotnetrocks&filetype=master&tags=Mobile%2cMono";


    private ObservableCollection<PodcastEpisode> podcasts = new ObservableCollection<PodcastEpisode>();

    public ObservableCollection<PodcastEpisode> Podcasts
    {
      get { return podcasts; }
      set { podcasts = value; OnPropertyChanged("Podcasts"); }
    }

    private ObservableCollection<PodcastEpisode> filteredPodcasts = new ObservableCollection<PodcastEpisode>();

    public ObservableCollection<PodcastEpisode> FilteredPodcasts
    {
      get { return filteredPodcasts; }
      set { filteredPodcasts = value; OnPropertyChanged("FilteredPodcasts"); }
    }

    private PodcastEpisode selectedEpisode;

    public PodcastEpisode SelectedEpisode
    {
      get { return selectedEpisode; }
      set { selectedEpisode = value; OnPropertyChanged("SelectedEpisode"); }
    }

    private RelayCommand getPodcastsCommand;

    public ICommand GetPodcastsCommand
    {
      get 
      { 
        return getPodcastsCommand ??
        (getPodcastsCommand =
        new RelayCommand(async () => await ExecuteGetPodcastsCommand()));
      }
    }

    public async Task ExecuteGetPodcastsCommand()
    {
      if (IsBusy)
        return;

      try
      {
        IsBusy = true;
        var client = new HttpClient();
        // Request from server podcast xml
        var podcastString = await client.GetStringAsync(PodcastUrl);

        // Parse Xml into data model and load into list
        var casts = ParseXml(podcastString);

        foreach (var cast in casts)
        {
          Podcasts.Add(cast);
          FilteredPodcasts.Add(cast);
        }

      }
      catch (Exception ex)
      {
        Debug.WriteLine("Unable to get podcasts: " + ex);
      }
      finally
      {
        IsBusy = false;
      }
    }

    private IEnumerable<PodcastEpisode> ParseXml(string podcastString)
    {
      var xdoc = XDocument.Parse(podcastString, LoadOptions.None);
      return (from item in xdoc.Descendants("item")
              select new PodcastEpisode
              {
                Name = (string)item.Element("title"),
                Description = (string)item.Element("description"),
                Image = "Assets/dnr_logo.jpg",
                AudioUrl = (string)item.Element("enclosure").Attribute("url"),
                ShowNumber = ParseShowNumber((string)item.Element("link"))
              }).ToList();

    }


    static int ParseShowNumber(string link)
    {
      var i = link.IndexOf("ShowNum=", System.StringComparison.Ordinal) + 8;
      return Int32.Parse(link.Substring(i));
    }

    public PodcastEpisode GetPodcast(int showNumber)
    {
      return Podcasts.FirstOrDefault(s => s.ShowNumber == showNumber);
    }

    private RelayCommand<string> filterPodcastCommand;

    public ICommand FilterPodcastCommand
    {
      get 
      {
        return filterPodcastCommand ??
        (filterPodcastCommand =
        new RelayCommand<string>(ExecuteFilterPodcastCommand));
      }
    }

    public void ExecuteFilterPodcastCommand(string text)
    {
      text = text.ToLower();
      FilteredPodcasts.Clear();
      var filtered = Podcasts.Where(podcast =>
        podcast.Description.ToLower().Contains(text) || podcast.Name.ToLower().Contains(text));
      foreach (var cast in filtered)
      {
        FilteredPodcasts.Add(cast);
      }
      OnPropertyChanged("FilteredPodcasts");
    }


  }
}
