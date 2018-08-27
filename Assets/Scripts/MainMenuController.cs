using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private bool hasJustShowenAd;

    public Image soundButtonImage;
    public Sprite soundOnSprite, soundOffSprite;
    public Button gpgsLoginButton;

	void Start ()
    {
        // Load data
        GamePersistence.Load();
        // Initialize GPGS
        GPGSController.Initialize();
        // Initialize ads
        AdsController.Initialize();
        // Display the banner ad
        AdsController.ShowBannerAd();
        // Change the sound button image accordingly
        soundButtonImage.sprite = GamePersistence.gameData.muted ? soundOffSprite : soundOnSprite;
        // Disable GPGS login is necessary
        if (GamePersistence.gameData.gpgsLogin)
            gpgsLoginButton.gameObject.SetActive(false);
    }
	
	void Update ()
    {
	    // Disable gpgs login if we're signed in
        if(GPGSController.IsSignedIn() && gpgsLoginButton.gameObject.activeSelf)
            gpgsLoginButton.gameObject.SetActive(false);
    }

    public void OnPlayButton()
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

    public void OnGPGSButton()
    {
        // Are we already signed ?
        if (GPGSController.IsSignedIn())
            return;

        // Set login flag
        GamePersistence.gameData.gpgsLogin = true;
        // Sign in
        GPGSController.SignIn();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            // Notify application focus on ads
            AdsController.OnResume();
    }
}
