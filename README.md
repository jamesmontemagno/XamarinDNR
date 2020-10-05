DotNetRocks
===========

Setup

* Create a new Azure Mobile Services Table Called "PodcastEpisode"
* Follow this guide to setup the Table for Authentication: https://www.windowsazure.com:80/develop/mobile/tutorials/get-started-with-users-dotnet/?WT.mc_id=xamarindnr-github-jamont
* Setup Azure Scripts for Insert, Read, Update: https://www.windowsazure.com:80/develop/mobile/tutorials/authorize-users-in-scripts-dotnet/?WT.mc_id=xamarindnr-github-jamont
* Setup Twitter App for Authentication: http://www.dotnetcurry.com/ShowArticle.aspx?ID=860
* Optionally you can setup Facebook, Microsoft, or Google, however the sample is setup for Twitter
* Open "AzureWebService.cs" in DNR.Portable
* Edit: podcastClient = new MobileServiceClient(
        "https://"+"PUT-SITE-HERE" +".azure-mobile.net/",
        "PUT-YOUR-API-KEY-HERE");
* This information can be found on Azure
        


Integrating Azure Mobile Services into your project and PCL:

Here is a guide with step by step instructions on: http://motzcod.es/post/70038987318/azure-mobile-services-xamarin-pcl
        


