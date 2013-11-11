using System.Threading.Tasks;
using DNR.Portable.Services;
using Newtonsoft.Json;

namespace DNR.Portable
{
  //[DataContract]
  public class PodcastEpisode
  {

    /// <summary>
    /// Constructor for podcast episode.
    /// </summary>
    public PodcastEpisode()
    {
    }


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



  }
}
