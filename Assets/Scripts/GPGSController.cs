using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;

public static class GPGSController
{
    private static bool signedIn;
	
	public static void Initialize ()
    {
        // Reset flags
        signedIn = false;
        // If we've activated GPGS sign in by default then login
        if (GamePersistence.gameData.gpgsLogin)
            SignIn();
	}
	
    public static void SignIn()
    {
        // Don't sign in if we're already signed in
        if (IsSignedIn())
            return;

        // Activate the GPGS platform
        PlayGamesPlatform.Activate();
        // Sign in
        Social.localUser.Authenticate((bool success) => {
            // Set the sign in flag
            signedIn = success;
        });
    }

    public static void SignOut()
    {
        // We need to be signed in first
        if (!IsSignedIn())
            return;

        // Sign out
        /*Social.localUser.Authenticate((bool success) => {
            // Set the sign in flag
            signedIn = success;
        });*/
    }

    public static void PostScore(long score)
    {
        // We need to be signed in
        if (IsSignedIn())
            Social.ReportScore(score, GPGSResources.leaderboard_best_score, null);
    }

    public static void UnlockAchievement(string achievementId)
    {
        // We need to be signed in
        if (!IsSignedIn())
            return;

        // Unlock the specified achievement
        Social.ReportProgress(achievementId, 100.0f, null);
    }

    public static void ShowLeaderboards()
    {
        // Only if we're signed in
        if (IsSignedIn())
            Social.ShowLeaderboardUI();
        else
            SignIn();
    }

    public static void ShowAchievements()
    {
        // Only if we're signed in
        if (IsSignedIn())
            Social.ShowAchievementsUI();
        else
            SignIn();
    }

    public static bool IsSignedIn()
    {
        return signedIn;
    }
}
