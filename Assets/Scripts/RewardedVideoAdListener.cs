using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppodealAds.Unity.Api;
using AppodealAds.Unity.Common;
using System;

public class RewardedVideoAdListener : IRewardedVideoAdListener
{
    public bool rewardedVideoFinished;

    public void onRewardedVideoClosed(bool finished)
    {
        rewardedVideoFinished = finished;
    }

    public void onRewardedVideoFailedToLoad()
    {
        
    }

    public void onRewardedVideoFinished(int amount, string name)
    {
        rewardedVideoFinished = true;
    }

    public void onRewardedVideoLoaded()
    {
        
    }

    public void onRewardedVideoShown()
    {
        
    }
}
