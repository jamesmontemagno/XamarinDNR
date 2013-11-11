using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace DNR
{
  public partial class PodcastCell : UICollectionViewCell
  {
    public static readonly UINib Nib = UINib.FromName("PodcastCell", NSBundle.MainBundle);
    public static readonly NSString Key = new NSString("PodcastCell");

    internal static SizeF PortraitSize = new SizeF(310, 100);
    internal static SizeF LandscapeSize = new SizeF(230, 100);

    public PodcastCell(IntPtr handle)
      : base(handle)
    {
      SelectedBackgroundView = new UIView { BackgroundColor = UIColor.Blue };
    }

    public static PodcastCell Create()
    {
      return (PodcastCell)Nib.Instantiate(null, null)[0];
    }

    public string Name
    {
      set
      {
        PodcastTitle.Text = value;
      }
    }

    public string DetailText
    {
      set
      {
        PodcastDescription.Text = value;
      }
    }
  }
}

