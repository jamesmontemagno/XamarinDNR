using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;
using DNR.Droid;
using System;
using DNR.Portable;

namespace DRN.Droid
{
  [Activity(Label = ".NET Rocks!", Theme = "@style/Theme", Icon = "@drawable/Icon")]
  public class PodcastDetailActivity : Activity
  {
    public static PodcastEpisodeSecure CurrentPodcastEpisode { get; set; }
    MediaPlayer player;
    TextView status;
    SeekBar seekBar;
    Handler updateHandler;
    int timeToSet = 0;
    bool initialized = false;

    protected async override void OnCreate(Bundle bundle)
    {
      base.OnCreate(bundle);

      SetContentView(Resource.Layout.PodcastDetail);


      var description = FindViewById<TextView>(Resource.Id.descriptionView);
      description.Text = CurrentPodcastEpisode.Description;

      var play = FindViewById<Button>(Resource.Id.playButton);
      var pause = FindViewById<Button>(Resource.Id.pauseButton);
      var stop = FindViewById<Button>(Resource.Id.stopButton);
      seekBar = FindViewById<SeekBar>(Resource.Id.seekBar1);
      status = FindViewById<TextView>(Resource.Id.statusText);
      updateHandler = new Handler();

      player = MediaPlayer.Create(this, Android.Net.Uri.Parse(CurrentPodcastEpisode.AudioUrl));

      player.Prepared += (sender, e) =>
          {
            initialized = true;
            player.SeekTo(timeToSet * 1000);
            UpdateStatus();
          };

      play.Click += (sender, e) =>
      {
        player.Start();
        updateHandler.PostDelayed(UpdateStatus, 1000);
      };

      pause.Click += (sender, e) => player.Pause();

      stop.Click += (sender, e) =>
      {
        player.Stop();
        player.Reset();
        player.SetDataSource(this, Android.Net.Uri.Parse(CurrentPodcastEpisode.AudioUrl));
        player.Prepare();
      };

      seekBar.ProgressChanged += (sender, e) =>
          {
            if (!e.FromUser)
              return;

            player.SeekTo((int)(player.Duration * ((float)seekBar.Progress / 100.0)));
          };

      var updated = await CurrentPodcastEpisode.GetTimeAsync();

      if (updated == null || updated.ShowNumber != CurrentPodcastEpisode.ShowNumber)
        return;

      if (initialized && player != null)
      {
        player.SeekTo(updated.CurrentTime * 1000);
        UpdateStatus();
      }
      else
      {
        timeToSet = updated.CurrentTime;
      }
    }

    private void UpdateStatus()
    {
      if (player == null || player.Duration == 0)
        return;

      RunOnUiThread(() =>
          {
            var current = player.CurrentPosition;
            var duration = player.Duration;
            seekBar.Progress = (int)(((float)current / (float)duration) * 100.0);
            var currentText = string.Format("{0:hh\\:mm\\:ss}", new TimeSpan(0, 0, 0, 0, current));
            var durationText = string.Format("{0:hh\\:mm\\:ss}", new TimeSpan(0, 0, 0, 0, duration));
            status.Text = currentText + " / " + durationText;
            if (player.IsPlaying)
              updateHandler.PostDelayed(UpdateStatus, 1000);
          });

    }

    protected override void OnResume()
    {
      base.OnResume();

      if (player == null)
      {
        player = MediaPlayer.Create(this, Android.Net.Uri.Parse(CurrentPodcastEpisode.AudioUrl));
      }
    }

    protected override void OnPause()
    {
      base.OnPause();
      player.Stop();
      player.Dispose();
      player = null;
    }

    ShareActionProvider actionProvider;
    int saveId = 0;
    public override bool OnCreateOptionsMenu(IMenu menu)
    {
      var item = menu.Add("Share");
      item.SetShowAsAction(ShowAsAction.Always);
      if (actionProvider == null)
        actionProvider = new ShareActionProvider(this);
      item.SetActionProvider(actionProvider);
      actionProvider.SetShareIntent(GetShareIntent());

      //If save:
      item = menu.Add("Save Progress");
      item.SetIcon(Resource.Drawable.ic_action_content_save);
      item.SetShowAsAction(ShowAsAction.Never);
      saveId = item.ItemId;


      return base.OnCreateOptionsMenu(menu);
    }

    public override bool OnOptionsItemSelected(IMenuItem item)
    {
      if (item.ItemId == saveId && CurrentPodcastEpisode != null)
      {
        CurrentPodcastEpisode.SaveTimeAsync(player.CurrentPosition / 1000);
        //CurrentPodcastEpisode.SaveTime(player.CurrentPosition / 1000);
        Toast.MakeText(this, "Progress Saved!", ToastLength.Short).Show();
      }
      return base.OnOptionsItemSelected(item);
    }

    private Intent GetShareIntent()
    {
      var intent = new Intent(Intent.ActionSend);
      intent.SetType("text/plain");
      intent.PutExtra(Intent.ExtraText, "Check out Dot Net Rocks!: " + CurrentPodcastEpisode.AudioUrl);
      return intent;
    }

  }
}

