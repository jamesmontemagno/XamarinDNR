using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using DNR.Droid;
using DNR.Portable;

namespace DRN.Droid
{
    public class PodcastsAdapter : BaseAdapter
    {
        List<PodcastEpisodeSecure> podcasts = new List<PodcastEpisodeSecure>();
      readonly Activity activity;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
          var view = convertView ?? activity.LayoutInflater.Inflate(Resource.Layout.PodcastItem, parent, false);

          var nameView = view.FindViewById<TextView>(Resource.Id.textView1);
          var descriptionView = view.FindViewById<TextView>(Resource.Id.textView2);
          var image = view.FindViewById<ImageView>(Resource.Id.podcastImage);
          image.SetImageResource(Resource.Drawable.dnr_logo);

          nameView.Text = podcasts[position].Name;
          descriptionView.Text = podcasts[position].Description;

          return view;
        }

        /// <summary>
        /// Gets or sets the podcasts
        /// </summary>
        public List<PodcastEpisodeSecure> Podcasts
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
            return null;
        }

        public override long GetItemId(int position)
        {
            return -1;
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

