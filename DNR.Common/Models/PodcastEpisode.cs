using System;
using System.Threading.Tasks;
using DNR.Portable.Services;
using Newtonsoft.Json;

namespace DNR.Portable.Models
{
  public class PodcastEpisode
  {

    /// <summary>
    /// Constructor for podcast episode.
    /// </summary>
    public PodcastEpisode()
    {
    }

    //Public properties
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string AudioUrl { get; set; }

    [JsonProperty(PropertyName = "shownum")]
    public int ShowNumber { get; set; }
    public bool IsFavorite { get; set; }
    public string Image { get; set; }

    [JsonProperty(PropertyName = "time")]
    public int CurrentTime { get; set; }

    [JsonProperty(PropertyName = "userId")]
    public string UserId { get; set; }

    /// <summary>
    /// Saves the current time out.
    /// </summary>
    /// <param name="time"></param>
    public async Task<PodcastEpisode> SaveTimeAsync(int time = -1)
    {
      if (time != -1)
          CurrentTime = time;

      var updated = await AzureWebService.Instance.SaveTimeAsync(this);
      if (updated != null)
          this.Id = updated.Id;

      return updated;
    }

    /// <summary>
    /// Saves the current time out.
    /// </summary>
    /// <param name="time"></param>
    public async Task<PodcastEpisode> GetTimeAsync(int time = -1)
    {
      var updated = await AzureWebService.Instance.GetTimeAsync(ShowNumber);
      if (updated != null)
          this.Id = updated.Id;

      return updated;
    }

    public string GetTimeDisplay(TimeSpan current, TimeSpan duration)
    {
      CurrentTime = (int)current.TotalSeconds;
      var currentText = string.Format("{0:hh\\:mm\\:ss}", current);
      var durationText = string.Format("{0:hh\\:mm\\:ss}", duration);
      return currentText + " / " + durationText;
    }


  }
}
