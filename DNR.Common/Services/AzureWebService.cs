//#define LOCALDATA

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;

namespace DNR.Portable.Services
{
  /// <summary>
  /// Azure web service class to add/update time stamples
  /// </summary>
  public class AzureWebService
  {

    public AzureWebService()
    {
      podcastClient = new MobileServiceClient(
        "https://" + "PUT-SITE-HERE" + ".azure-mobile.net/",
        "PUT-YOUR-API-KEY-HERE");

      podcastTable = podcastClient.GetTable<PodcastEpisodeSecure>();
    }
    private MobileServiceClient podcastClient;
    private MobileServiceCollection<PodcastEpisodeSecure, PodcastEpisodeSecure> podcasts;
    private readonly IMobileServiceTable<PodcastEpisodeSecure> podcastTable;
    static readonly AzureWebService instance = new AzureWebService();

    public MobileServiceClient Client
    {
      get { return podcastClient; }
    }

    public async Task<PodcastEpisodeSecure> GetTimeAsync(int shownum)
    {
      if (podcastClient.CurrentUser == null)
        return null;



      try
      {
        //retrieve postcast based off show number
        podcasts = await podcastTable.Where(p=>p.ShowNumber == shownum).ToCollectionAsync();
        return podcasts.Count > 0 ? podcasts[0] : null;
      }
      catch (Exception ex)
      {
      }

      return new PodcastEpisodeSecure { ShowNumber = shownum, CurrentTime = 0 };
    }
    
    public async Task<PodcastEpisodeSecure> SaveTimeAsync(PodcastEpisodeSecure ep)
    {

      if (podcastClient.CurrentUser == null)
        return null;

      try
      {
        //Check to see if the podcast already exists, if Id is set.
        //then update it, else insert.
        if (ep.Id > 0)
        {
          await podcastTable.UpdateAsync(ep);
        }
        else
        {
          await podcastTable.InsertAsync(ep);
        }

        //return the episode back with the updated Id
        return ep;
      }
      catch (Exception ex)
      {
      }
      return null;
    }

    /// <summary>
    /// Gets the instance of the Azure Web Service
    /// </summary>
    public static AzureWebService Instance
    {
      get
      {
        return instance;
      }
    }

  }
}
