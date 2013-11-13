using DNR.Portable;
using MonoTouch.AVFoundation;
using MonoTouch.CoreMedia;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Timers;

namespace DNR
{
  class PodcastDetailController : UIViewController
  {
    PodcastEpisode currentPodcastEpisode;
    AVPlayer player;
    UITextView descriptionView;
    UIToolbar playerBar;
    UIBarButtonItem play, pause, save;

    UIBarButtonItem soundBarsItem;
    UISlider podcastSlider;
    UILabel timeStamp;
    UIView soundBars;
    Timer timer;
    readonly List<UIView> rectangles = new List<UIView>();
    private int startTime;
    private bool playing;

    static readonly Random Rnd = new Random();

    public PodcastEpisode CurrentPodcastEpisode
    {
      get
      {
        return currentPodcastEpisode;
      }

      set
      {
        currentPodcastEpisode = value;
        Title = currentPodcastEpisode.Name;
        descriptionView.Text = CurrentPodcastEpisode.Description;
        startTime = 0;
      }
    }

    public PodcastDetailController()
    {
      View.BackgroundColor = UIColor.White;
      AddUI();

      if (Application.IsiOS7)
      {
        EdgesForExtendedLayout = UIRectEdge.None;
      }

      podcastSlider.ValueChanged += async (sender, e) =>
      {

        var newTime = CMTime.FromSeconds((double)(podcastSlider.Value * player.CurrentItem.Duration.Seconds), 1);
        await player.SeekAsync(newTime);
        if (playing)
          player.Play();
      };

      AddTransportControls();
    }

    private void AddUI()
    {
      timer = new Timer(1000);
      timer.Elapsed += timer_Elapsed;

      timeStamp = new UILabel(new RectangleF(10, 10, 300, 20))
      {
        AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
        TextColor = UIColor.Black,
        Text = "0:00 / 50:00"
      };

      View.Add(timeStamp);

      podcastSlider = new UISlider(new RectangleF(10, 40, 300, 20))
      {
        AutoresizingMask = UIViewAutoresizing.FlexibleWidth
      };

      View.Add(podcastSlider);

      descriptionView = new UITextView(new RectangleF(10, 70, 300, View.Bounds.Height - 130))
      {
        AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
        Editable = false
      };


      View.Add(descriptionView);

      playerBar = new UIToolbar
      {
        Frame = new RectangleF(0, View.Bounds.Height - 44, View.Bounds.Width, 44),
        AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin
      };
    }

    private void AddTransportControls()
    {

      play = new UIBarButtonItem(UIBarButtonSystemItem.Play);
      pause = new UIBarButtonItem(UIBarButtonSystemItem.Pause);

      play.Clicked += async (sender, e) =>
            {
              player.Play();

              AddBars();
              timer.Start();
              playing = true;
              if (startTime != 0)
              {
                await player.SeekAsync(CMTime.FromSeconds(startTime, 1));
                timer_Elapsed(null, null);//update UI
              }
            };

      pause.Clicked += (sender, e) =>
      {
        player.Pause();
        RemoveBars();
        timer.Stop();
        playing = false;
      };



      var flexSpace = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
      var space = new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) {Width = 40};


      soundBars = new UIView(new RectangleF(0, 0, 50, 50));
      soundBarsItem = new UIBarButtonItem(soundBars);

      #region save
      save = new UIBarButtonItem { Title = "Save" };
      save.Clicked += (sender, e) => currentPodcastEpisode.SaveTimeAsync((int)player.CurrentItem.CurrentTime.Seconds);
      #endregion

      playerBar.SetItems(new[] { play, space, pause, space, soundBarsItem, flexSpace, save }, false);

      View.Add(playerBar);
    }

    

    void timer_Elapsed(object sender, ElapsedEventArgs e)
    {
      var sliderVal = (player.CurrentItem.CurrentTime.Seconds / player.CurrentItem.Duration.Seconds);
      if (sliderVal > 1)
        sliderVal = 100;
      else if (sliderVal < 0)
        sliderVal = 0;

      InvokeOnMainThread(() =>
        {
          podcastSlider.Value = (float)sliderVal;

          timeStamp.Text = string.Format("{0:hh\\:mm\\:ss}", new TimeSpan(0, 0, (int)player.CurrentItem.CurrentTime.Seconds)) + " / " + string.Format("{0:hh\\:mm\\:ss}", new TimeSpan(0, 0, (int)player.CurrentItem.Duration.Seconds));
        });
    }

    public async override void ViewDidAppear(bool animated)
    {
      base.ViewDidAppear(animated);

      var url = new NSUrl(CurrentPodcastEpisode.AudioUrl);
      player = AVPlayer.FromUrl(url);

      var updated = await CurrentPodcastEpisode.GetTimeAsync();
      if (updated == null || updated.ShowNumber != CurrentPodcastEpisode.ShowNumber)
        return;

	  if (player == null || player.Status != AVPlayerStatus.ReadyToPlay)
      {
        startTime = updated.CurrentTime;
        return;
      }

      await player.SeekAsync(CMTime.FromSeconds(updated.CurrentTime, 1));
      timer_Elapsed(null, null);//update UI
    }

    public override void ViewDidDisappear(bool animated)
    {
      base.ViewDidDisappear(animated);

      base.ViewWillDisappear(animated);
      if (player != null)
      {
        player.Pause();
        if (playing)
          RemoveBars();
      }
    }

    void AddBars()
    {
	  if (!Application.IsiOS7)
		return;

      for (int i = 0; i < 5; i++)
      {
        var rectangle = new UIView(new RectangleF(i * 10 + 20, 45, 5, -40))
        {
          BackgroundColor = UIColor.LightGray
        };

        soundBars.AddSubview(rectangle);
        rectangles.Add(rectangle);
        AnimateViewWithKeyframes(rectangle, -40, -5);
      }
    }

    void RemoveBars()
    {
      foreach (var r in rectangles)
      {
        r.Layer.RemoveAllAnimations();
        r.RemoveFromSuperview();
      }
      rectangles.Clear();
    }

    void AnimateViewWithKeyframes(UIView rectangle, int min, int max)
    {
      var x = rectangle.Frame.X;

      var h1 = Rnd.Next(min, max);
      var h2 = Rnd.Next(min, max);
      var h3 = Rnd.Next(min, max);
      var h4 = Rnd.Next(min, max);

      UIView.AnimateKeyframes(0.75f, 0, UIViewKeyframeAnimationOptions.Autoreverse | UIViewKeyframeAnimationOptions.Repeat, () =>
      {
        UIView.AddKeyframeWithRelativeStartTime(0.0, 0.25, () =>
        {
          rectangle.Frame = new RectangleF(x, 45, 5, h1);
        });

        UIView.AddKeyframeWithRelativeStartTime(0.25, 0.25, () =>
        {
          rectangle.Frame = new RectangleF(x, 45, 5, h2);
        });

        UIView.AddKeyframeWithRelativeStartTime(0.5, 0.25, () =>
        {
          rectangle.Frame = new RectangleF(x, 45, 5, h3);
        });

        UIView.AddKeyframeWithRelativeStartTime(0.75, 0.25, () =>
        {
          rectangle.Frame = new RectangleF(x, 45, 5, h4);
        });

      }, _ => { });
    }
  }
}