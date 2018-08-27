using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathController : MonoBehaviour
{
    public Text scoreText, bestScoreText;

    public Image soundButtonImage;
    public Sprite soundOnSprite, soundOffSprite;

    void Start()
    {
        // Update the best score if necessary
        if (GameController.score > GamePersistence.gameData.bestScore)
            GamePersistence.gameData.bestScore = GameController.score;

        // Update played games
        GamePersistence.gameData.gamesPlayed++;
        // Reset elements data
        GamePersistence.gameData.elementsData = null;
        // Save
        GamePersistence.Save();
        // Are we signed in to GPGS ?
        if (GPGSController.IsSignedIn())
        {
            //  Upload score
            GPGSController.PostScore(GamePersistence.gameData.bestScore);
            // Unlock achievements
            GPGSController.UnlockAchievement(GPGSResources.achievement_the_beginning);
            if(GameController.score >= 100)
                GPGSController.UnlockAchievement(GPGSResources.achievement_just_warming_up);
            if (GameController.score >= 1000)
                GPGSController.UnlockAchievement(GPGSResources.achievement_getting_used);
            if (GameController.score >= 20000)
                GPGSController.UnlockAchievement(GPGSResources.achievement_impressive);
            if (GameController.score >= 50000)
                GPGSController.UnlockAchievement(GPGSResources.achievement_50k);
            if (GameController.score >= 500000)
                GPGSController.UnlockAchievement(GPGSResources.achievement_500k);
            if (GameController.score >= 1000000)
                GPGSController.UnlockAchievement(GPGSResources.achievement_millions);
            if (GameController.score >= 50000000)
                GPGSController.UnlockAchievement(GPGSResources.achievement_freakish);
            if (GameController.score >= 1000000000)
                GPGSController.UnlockAchievement(GPGSResources.achievement_just_quit);
        }
        // Set the score text
        scoreText.text = "" + GameController.score;
        // Update best score text
        bestScoreText.text = "" + GamePersistence.gameData.bestScore;
        // Show video ad
        AdsController.ShowRewardedVideoAd();
        // Change the sound button image accordingly
        soundButtonImage.sprite = GamePersistence.gameData.muted ? soundOffSprite : soundOnSprite;
    }

    void Update()
    {

    }

    public void OnRestartButton()
    {
        // Set to the game scene
        SceneManager.LoadScene("Game");
    }

    public void OnLeaderboardsButton()
    {
        // Display leaderboards screen
        GPGSController.ShowLeaderboards();
    }

    public void OnAchievementsButton()
    {
        // Display achievements screen
        GPGSController.ShowAchievements();
    }

    public void OnRateButton()
    {
        // Open Play Store
        Application.OpenURL("market://details?id=com.ormisiclapps.tffe");
    }

    public void OnSoundButton()
    {
        // Change the sound muted flag
        GamePersistence.gameData.muted = !GamePersistence.gameData.muted;
        // Change the sound button image accordingly
        soundButtonImage.sprite = GamePersistence.gameData.muted ? soundOffSprite : soundOnSprite;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // Notify application focus on ads
        if(hasFocus)
            AdsController.OnResume();
    }
}
