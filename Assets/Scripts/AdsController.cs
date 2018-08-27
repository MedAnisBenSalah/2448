using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppodealAds.Unity.Api;
using AppodealAds.Unity.Common;

public static class AdsController
{
    private static bool initialized = false, rewardedVideoAdStarted = false;
    private static RewardedVideoAdListener rewardedVideoAdListener;

    public static void Initialize()
    {
        // Only initialize once
        if (initialized)
            return;

        // Initialize Appodeal
        string appKey = "a2ebee9a87d28bc5611c20e328671b5acfc6687062e1ced0";
        Appodeal.disableLocationPermissionCheck();
        Appodeal.initialize(appKey, Appodeal.INTERSTITIAL | Appodeal.BANNER | Appodeal.REWARDED_VIDEO);

        // Set rewarded video ad listener
        rewardedVideoAdListener = new RewardedVideoAdListener();
        Appodeal.setRewardedVideoCallbacks(rewardedVideoAdListener);
        // Set the initialized flag
        initialized = true;
    }

    public static void OnResume()
    {
        Appodeal.onResume();
    }

    public static void ShowBannerAd()
    {
        // Show banner ad
        Appodeal.show(Appodeal.BANNER_BOTTOM);
    }

    public static void ShowInterstitialAd()
    {
        // Show interstitial ad
        if(Appodeal.isLoaded(Appodeal.INTERSTITIAL))
            Appodeal.show(Appodeal.INTERSTITIAL);
    }

    public static void ShowRewardedVideoAd()
    {
        // Show rewarded video ad
        if (Appodeal.isLoaded(Appodeal.REWARDED_VIDEO))
        {
            Appodeal.show(Appodeal.REWARDED_VIDEO);
            rewardedVideoAdStarted = true;
        }
    }

    public static bool IsRewardedVideoAdLoaded()
    {
        return Appodeal.isLoaded(Appodeal.REWARDED_VIDEO);
    }

    public static bool IsRewardedVideoAdFinished()
    {
        if(rewardedVideoAdStarted && rewardedVideoAdListener.rewardedVideoFinished)
        {
            // Reset flags
            rewardedVideoAdStarted = false;
            rewardedVideoAdListener.rewardedVideoFinished = false;
            return true;
        }
        return false;
    }
}
