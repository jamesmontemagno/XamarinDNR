using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DNR.Portable
{
  /// <summary>
  /// Retrieves a list of podcasts from a feed
  /// </summary>
  public class PodcastFetcher
  {
    const string PodcastUrl = "http://www.pwop.com/feed.aspx?show=dotnetrocks&filetype=master&tags=Mobile%2cMono";


    public async Task<IEnumerable<PodcastEpisode>> GetPodcastsAsync()
    {
      try
      {
        var client = new HttpClient();
        var stream = await client.GetStreamAsync(PodcastUrl);
        return ParseXml(stream);
      }
      catch (Exception ex)
      {
        
      }

      return new List<PodcastEpisode>();
    }

    private IEnumerable<PodcastEpisode> ParseXml(Stream stream)
    {
      var xdoc = XDocument.Load(stream);
      return (from item in xdoc.Descendants("item")
              select new PodcastEpisode
              {
                Name = (string)item.Element("title"),
                Description = (string)item.Element("description"),
                AudioUrl = (string)item.Element("enclosure").Attribute("url"),
                ShowNumber = ParseShowNumber((string)item.Element("link"))
              }).ToList();

    }


    static int ParseShowNumber(string link)
    {
      var i = link.IndexOf("ShowNum=", System.StringComparison.Ordinal) + 8;
      return Int32.Parse(link.Substring(i));
    }
  }
}
