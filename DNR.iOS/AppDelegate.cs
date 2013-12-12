using System.Net.Http;
using System.Threading.Tasks;
using DNR.Portable.Services;
using Microsoft.WindowsAzure.MobileServices;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace DNR
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		UIWindow window;
		UINavigationController navController;
		PodcastsController podcastsController;

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
      CurrentPlatform.Init();


			window = new UIWindow (UIScreen.MainScreen.Bounds);

			if (Application.IsiOS7) {
				window.TintColor = UIColor.Red;
			}

			podcastsController = new PodcastsController (new UICollectionViewFlowLayout {
				ItemSize = PodcastCell.PortraitSize,
				MinimumInteritemSpacing = 5.0f,
				MinimumLineSpacing = 5.0f
			});

			navController = new UINavigationController (podcastsController);
			window.RootViewController = navController;
			window.MakeKeyAndVisible ();

			return true;
		}
	}
}