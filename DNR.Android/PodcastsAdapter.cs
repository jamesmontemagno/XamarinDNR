using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Views;
using Android.Widget;
using DNR.Droid;
using DNR.Portable;
using DNR.Portable.Models;

namespace DRN.Droid
{
  public class PodcastsAdapter : BaseAdapter
  {
    //Wrapper class for adapter for cell re-use
    private class PodcastsAdapterHelper : Java.Lang.Object
    {
      public TextView Title { get; set; }
      public TextView Description { get; set; }
      public ImageView Image { get; set; }
    }


    ObservableCollection<PodcastEpisode> podcasts = new ObservableCollection<PodcastEpisode>();
    readonly Activity activity;

    public override View GetView(int position, View convertView, ViewGroup parent)
    {
      //Create wrapper for speed improvements
      PodcastsAdapterHelper helper = null;
      if (convertView == null)
      {
        convertView = activity.LayoutInflater.Inflate(Resource.Layout.PodcastItem, null);
        helper = new PodcastsAdapterHelper();
        helper.Title = convertView.FindViewById<TextView>(Resource.Id.textView1);
        helper.Description = convertView.FindViewById<TextView>(Resource.Id.textView2);
        helper.Image = convertView.FindViewById<ImageView>(Resource.Id.podcastImage);
        convertView.Tag = helper;
      }
      else
      {
        helper = convertView.Tag as PodcastsAdapterHelper;
      }


      helper.Title.Text = podcasts[position].Name;
      helper.Description.Text = podcasts[position].Description;
      helper.Image.SetImageResource(Resource.Drawable.dnr_logo);
      return convertView;
    }

    /// <summary>
    /// Gets or sets the podcasts
    /// </summary>
    public ObservableCollection<PodcastEpisode> Podcasts
    {
      get { return podcasts; }
      set { podcasts = value; }
    }

    public PodcastsAdapter(Activity activity)
    {
      this.activity = activity;
    }

    public override Java.Lang.Object GetItem(int position)
    {
      return position;
    }

    public override long GetItemId(int position)
    {
      return podcasts[position].ShowNumber;
    }

    public override int Count
    {
      get
      {
        return podcasts.Count;
      }
    }
  }
}

