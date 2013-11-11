using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace DNR
{
  public class Application
  {
    // This is the main entry point of the application.
    static void Main(string[] args)
    {
      // if you want to use a different Application Delegate class from "AppDelegate"
      // you can specify it here.
      UIApplication.Main(args, null, "AppDelegate");
    }

    public static bool IsiOS7
    {
      get { return UIDevice.CurrentDevice.CheckSystemVersion(7, 0); }
    }
  }
}
