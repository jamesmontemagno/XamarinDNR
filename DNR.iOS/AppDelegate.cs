using System.Net.Http;
using System.Threading.Tasks;
using DNR.Portable;
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
	  private int azureGet = 0;

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
      CurrentPlatform.Init();
		  AzureWebService.AzureFactory.Get = () =>
		  {
			if (azureGet == 0) {
		      azureGet = 1;
          	  return "http://" + PodcastFetcher.localIP + "/timestampempty{0}.json";
		    }
        	return "http://" + PodcastFetcher.localIP + "/timestamp{0}.json";
		  };


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


    public class AzureHandler : DelegatingHandler
    {
      protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
      {
        return await base.SendAsync(request, cancellationToken);
      }
    }
	}
}