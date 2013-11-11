// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace DNR
{
	[Register ("PodcastCell")]
	partial class PodcastCell
	{
		[Outlet]
		MonoTouch.UIKit.UILabel PodcastDescription { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel PodcastTitle { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (PodcastTitle != null) {
				PodcastTitle.Dispose ();
				PodcastTitle = null;
			}

			if (PodcastDescription != null) {
				PodcastDescription.Dispose ();
				PodcastDescription = null;
			}
		}
	}
}
