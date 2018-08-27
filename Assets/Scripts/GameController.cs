using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    // GameObjects
    public GameObject element;
    public Text scoreText, timeText;
    public Image pauseFade;
    public Text pauseText, pauseHintText;
    public Button restartButton, hintButton;
    public AudioClip elementSelectionAudio, combinationAudio;

    // Controllers
    private InputController inputController;
    private AudioSource audioSource;

    // Components
    private SpriteRenderer elementSpriteRenderer;
    private LineRenderer currentLineRenderer;
    private RectTransform restartButtonRect, hintButtonRect;

    // Arrays
    private ArrayList elements, combinedElements, lines;

    private long lastNumber, connectedElementNumber, combinedSum;
    private int highestPower, currentPower;
    private GameObject lastTouchedElement, currentLine, connectedElement, firstHintElement, secondHintElement;
    public static long score;
    private float timeSinceStart;
    private bool gameOver;
    private float gameOverTimer;
    private bool playingHint;
    private bool paused, canResume;

    private int combinationsSinceLastAd;
    private bool hasJustShowenbAd;

    public static readonly int VERTICAL_ELEMENTS = 7;
    public static readonly int HORIZONTAL_ELEMENTS = 5;

    private static readonly float VERTICAL_START = -2.9f;
    private static readonly float HORIZONTAL_START = 2.7f;

    private static readonly float DISTANCE_BETWEEN_ELEMENTS = 0.35f;

    private static readonly float LINE_WIDTH = 0.09f;

    private static readonly float GAME_OVER_TIME = 2f;

    private static readonly int AD_COMBINATIONS_NUMBER = 50;

    void Start ()
    {
        // Create arrays
        elements = new ArrayList();
        combinedElements = new ArrayList();
        lines = new ArrayList();

        // Get the element sprite renderer
        elementSpriteRenderer = element.GetComponent<SpriteRenderer>();
        // Get audio source
        audioSource = GetComponent<AudioSource>();
        // Get rect transforms
        restartButtonRect = restartButton.GetComponent<RectTransform>();
        hintButtonRect = hintButton.GetComponent<RectTransform>();
        // Mute the audio source if necessary
        audioSource.mute = GamePersistence.gameData.muted;
        // Get the input controller
        inputController = GetComponent<InputController>();
        // Disable pause
        TogglePause(false);
        // reset values
        combinationsSinceLastAd = 0;
        hasJustShowenbAd = false;
        // Start a new game
        NewGame(false);
    }
	
	void Update ()
    {
        // Update time since start
        timeSinceStart += Time.deltaTime;
        // Is game over ?
        if (gameOver)
        {
            // Update game over timer
            gameOverTimer += Time.deltaTime;
            // Did we reach the game over time
            if (gameOverTimer >= GAME_OVER_TIME)
                // Change to the death screen
                SceneManager.LoadScene("Death");
        }
        // Are we paused ?
        else if(paused)
        {
            // Set the can resume flag
            if (!canResume && !inputController.IsTouched() && !inputController.IsDragging())
                canResume = true;

            // Check resume
            if (canResume && inputController.IsTouched())
            {
                // Ignore restart button touch
                Vector3 touchPosition = Camera.main.WorldToScreenPoint(inputController.GetTouchingPosition());
                Vector3 buttonPosition = restartButton.transform.position;
                Vector3 buttonSize = new Vector3(restartButtonRect.rect.width, restartButtonRect.rect.height);
                if(!GameMath.IsInRange(touchPosition, buttonPosition, buttonSize, 0.5f))
                    // Resume
                    TogglePause(false);
            }
        }
        else
        {
            // Update input 
            UpdateInput();
            // Update UI
            UpdateUI();
        }

    }

    /* 
     * This will regulary update the UI
     * 1 - Update the time's text
    */
    private void UpdateUI()
    {
        // Get time
        int hours = (int)(timeSinceStart / 3600);
        int minutes = (int)(timeSinceStart % 3600 / 60);
        int seconds = (int)(timeSinceStart % 60);

        string hoursStr = hours < 10 ? "0" + hours : "" + hours;
        string minutesStr = minutes < 10 ? "0" + minutes : "" + minutes;
        string secondsStr = seconds < 10 ? "0" + seconds : "" + seconds;
        // Update time text
        timeText.text = "TIME\n" + hoursStr + ":" + minutesStr + ":" + secondsStr;
    }

    /* 
     * This will regulary retrieve and handle the user's input
     * 1 - If we're not already combining elements, this will detect the first element touched
     * 2 - If we are combining elements, this will handle the dragging and combination of elements
    */
    private void UpdateInput()
    {
        // Did we first touch the screen
        if(inputController.IsTouched())
        {
            // Ignore hint button touch
            Vector3 touchScreenPosition = Camera.main.WorldToScreenPoint(inputController.GetTouchingPosition());
            Vector3 buttonPosition = hintButton.transform.position;
            Vector3 buttonSize = new Vector3(hintButtonRect.rect.width, hintButtonRect.rect.height);
            if (GameMath.IsInRange(touchScreenPosition, buttonPosition, buttonSize, 0.5f))
                return;

            // Get the touch position
            Vector2 touchingPosition = inputController.GetTouchingPosition();
            Vector3 touchPosition = new Vector3(touchingPosition.x, touchingPosition.y, 0f);
            // Loop through the objects
            Vector3 position = new Vector3();
            Vector3 bounds = elementSpriteRenderer.bounds.size;
            foreach (GameObject currentElement in elements)
            {
                // Set the element's position
                position = currentElement.transform.position;              
                // Did we touch it ?
                if (GameMath.IsInRange(touchPosition, position, bounds, 0.8f))
                {
                    // Stop playing hint animations if available
                    if(playingHint)
                    {
                        // Stop hint animations on hint elements
                        firstHintElement.GetComponent<Animation>().Stop("ElementHinted");
                        secondHintElement.GetComponent<Animation>().Stop("ElementHinted");
                        // Restore to original size
                        firstHintElement.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
                        secondHintElement.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
                        // Reset playing hint flag
                        playingHint = false;
                    }
                    // Set it as the first touched element
                    combinedElements.Add(currentElement);
                    // Set the last touched element
                    lastTouchedElement = currentElement;
                    // Get the current element's color
                    Color color = currentElement.GetComponent<SpriteRenderer>().color;
                    // Create a new line
                    CreateLine(color);
                    // Play the element touched animation
                    currentElement.GetComponent<Animation>().Play("ElementTouched", PlayMode.StopAll);
                    // Get the element's number
                    Element elementScript = currentElement.GetComponent<Element>();
                    long number = elementScript.GetNumber();
                    currentPower = elementScript.GetPower() + 1;
                    combinedSum += number;
                    lastNumber = number;
                    // Play touched audio
                    audioSource.PlayOneShot(elementSelectionAudio);
                    break;
                }
            }
        }
        // Are we dragging ?
        else if(inputController.IsDragging())
        {
            // Ensure we have a first element
            if (combinedElements.Count == 0)
                return;

            // Get the touch position
            Vector2 touchingPosition = inputController.GetTouchingPosition();
            Vector3 touchPosition = new Vector3(touchingPosition.x, touchingPosition.y, 0f);
            // Change the line's position
            currentLineRenderer.SetPosition(0, lastTouchedElement.transform.position);
            currentLineRenderer.SetPosition(1, touchPosition);
            // Loop through the objects
            Vector3 position = new Vector3();
            Vector3 bounds = elementSpriteRenderer.bounds.size;
            bounds.x /= 2f;
            bounds.y /= 2f;
            foreach (GameObject currentElement in elements)
            {
                // Is it one of the surrounding elements ?
                if (GameMath.GetDistanceBetweenVectors(currentElement.transform.position,
                    lastTouchedElement.transform.position) > elementSpriteRenderer.bounds.size.x + DISTANCE_BETWEEN_ELEMENTS * 3f)
                    continue;

                // Skip already touched elements
                if (combinedElements.Contains(currentElement) && currentElement != connectedElement)
                {
                    // If its the last touched element then remove it from the list
                    continue;
                }
                // Set the element's position
                position = currentElement.transform.position;
                // Did we touch it ?
                if (GameMath.IsInRange(touchPosition, position, bounds, 1.15f))
                {
                    // Is it the connected element ?
                    if (currentElement == connectedElement)
                    {
                        // Remove the current line from the lines array
                        lines.Remove(currentLine);
                        // Destroy the current line
                        Destroy(currentLine);
                        // Set the current line to the one projected from the connected element
                        currentLine = (GameObject)lines[lines.Count - 1];
                        currentLineRenderer = currentLine.GetComponent<LineRenderer>();
                        // Remove the current element from the combined elements list
                        combinedElements.Remove(lastTouchedElement);
                        // Point the last touched element to this one element
                        lastTouchedElement = connectedElement;
                        // Get element script
                        Element elementScript = lastTouchedElement.GetComponent<Element>();
                        // Restore the power
                        currentPower = elementScript.GetPower() + 1;
                        // Remove its points
                        long number = elementScript.GetNumber();
                        combinedSum -= number;
                        lastNumber = number;
                        // Set the connected element
                        if (combinedElements.Count > 1)
                        {
                            connectedElement = (GameObject)combinedElements[combinedElements.Count - 2];
                            connectedElementNumber = connectedElement.GetComponent<Element>().GetNumber();
                        }
                        else
                        {
                            connectedElement = null;
                            connectedElementNumber = 0;
                        }
                    }
                    else
                    {
                        // Can we combine it ?
                        long number = currentElement.GetComponent<Element>().GetNumber();
                        if (number != lastNumber && number != connectedElementNumber + lastNumber)
                            continue;

                        // Increase power if necessary
                        if (number != lastNumber)
                            currentPower++;

                        // Change the line's position to hit the center of this element
                        currentLineRenderer.SetPosition(0, lastTouchedElement.transform.position);
                        currentLineRenderer.SetPosition(1, currentElement.transform.position);
                        // Set it as the first touched element
                        combinedElements.Add(currentElement);
                        // Set the connected element
                        connectedElement = lastTouchedElement;
                        // Set the connected element's number
                        connectedElementNumber = lastNumber;
                        // Update the last number
                        combinedSum += number;
                        lastNumber = number;
                        // Set the last touched element
                        lastTouchedElement = currentElement;
                        // Get the current element's color
                        Color color = currentElement.GetComponent<SpriteRenderer>().color;
                        // Create a new line
                        CreateLine(color);
                        // Play the element touched animation
                        currentElement.GetComponent<Animation>().Play("ElementTouched", PlayMode.StopAll);
                        // Play touched audio
                        audioSource.PlayOneShot(elementSelectionAudio);
                    }
                    break;
                }
            }
        }
        // Clear all elements after dragging
        else if(combinedElements.Count != 0)
        {
            // If its a single element then clear the combined elements only
            if(combinedElements.Count == 1)
            {
                combinedElements.Clear();
                // Destroy all lines
                foreach (GameObject line in lines)
                    // Destroy line
                    Destroy(line);

                // Clear lines
                lines.Clear();
                // Reset values
                lastNumber = 0;
                combinedSum = 0;
                connectedElementNumber = 0;
                currentPower = 0;
                lastTouchedElement = null;
                currentLine = null;
                currentLineRenderer = null;
                connectedElement = null;
                return;
            }
            // Loop through the combined elements
            foreach(GameObject currentElement in combinedElements)
            {
                // If its the last one then update its number
                if(currentElement == lastTouchedElement)
                {
                    // Update its number
                    currentElement.GetComponent<Element>().UpdateNumber(currentPower);
                }
                else
                {
                    // Translate it to the last touched element's position
                    currentElement.GetComponent<Element>().TranslateToObjectAndDestroy(lastTouchedElement);
                    // Remove it from the elements array
                    elements.Remove(currentElement);
                }
            }
            // Get combined elements number
            int combinedElementsCount = combinedElements.Count;
            // Clear combined elements
            combinedElements.Clear();
            // Destroy all lines
            foreach (GameObject line in lines)
                // Destroy line
                Destroy(line);

            // Clear lines
            lines.Clear();
            // Update score
            score += combinedSum;
            // Update the score UI
            UpdateScoreUI();
            // Increase highest power if necessary
            if (currentPower > highestPower + 1)
                highestPower = currentPower - 1;

            // Calculate the number
            int numberScored = (int)Mathf.Pow(2, currentPower);
            // Reset values
            lastNumber = 0;
            combinedSum = 0;
            connectedElementNumber = 0;
            currentPower = 0;
            lastTouchedElement = null;
            currentLine = null;
            currentLineRenderer = null;
            connectedElement = null;
            // Play combination audio
            audioSource.PlayOneShot(combinationAudio);
            // Re arrange elements
            ReArrangeElements();
            // Look for hints
            LookForHints();
            // If the game is not over yet then save the elements data
            if(!gameOver)
            {
                GamePersistence.SetElementsData(elements);
                GamePersistence.gameData.lastScore = score;
                GamePersistence.gameData.lastTimeSinceStart = timeSinceStart;
                GamePersistence.gameData.lastHighestPower = highestPower;
                GamePersistence.Save();
                // Are we signed in to GPGS ?
                if (GPGSController.IsSignedIn())
                {
                    // Unlock achievements
                    GPGSController.UnlockAchievement(GPGSResources.achievement_combined);
                    if (combinedElementsCount >= 5)
                        GPGSController.UnlockAchievement(GPGSResources.achievement_tiled);
                    if (combinedElementsCount >= 10)
                        GPGSController.UnlockAchievement(GPGSResources.achievement_really_tiled);
                    if (numberScored >= 128)
                        GPGSController.UnlockAchievement(GPGSResources.achievement_hundreads);
                    if (numberScored >= 1000)
                        GPGSController.UnlockAchievement(GPGSResources.achievement_thousands);
                    if (numberScored >= 4000)
                        GPGSController.UnlockAchievement(GPGSResources.achievement_4k);
                    if (numberScored >= 16000)
                        GPGSController.UnlockAchievement(GPGSResources.achievement_tens_of_thousands);
                    if (numberScored >= 128000)
                        GPGSController.UnlockAchievement(GPGSResources.achievement_hundreads_of_thousands);
                    if (numberScored >= 1000000)
                        GPGSController.UnlockAchievement(GPGSResources.achievement_millionaire);
                }
                // Do we need to display an ad ?
                combinationsSinceLastAd++;
                if (combinationsSinceLastAd >= AD_COMBINATIONS_NUMBER)
                {
                    // Show interstitial
                    AdsController.ShowInterstitialAd();
                    hasJustShowenbAd = true;
                    // Reset combinations since last ad
                    combinationsSinceLastAd = 0;
                }
            }
        }
    }

    /* 
     * This will start a new game:
     * 1 - Initialize a full screen of objects
    */
    private void NewGame(bool forceNewGame)
    {
        // Reset values
        lastNumber = 0;
        combinedSum = 0;
        connectedElementNumber = 0;
        gameOverTimer = 0f;
        gameOver = false;
        playingHint = false;

        lastTouchedElement = null;
        currentLine = null;
        currentLineRenderer = null;
        connectedElement = null;
        firstHintElement = secondHintElement = null;
        // Do we have a saved state ?
        if (!forceNewGame && GamePersistence.gameData.elementsData != null)
        {
            // Loop through the saved elements
            Vector3 position = new Vector3();
            foreach(GamePersistence.ElementData elementData in GamePersistence.gameData.elementsData)
            {
                // Create a new element
                GameObject newElement = Instantiate(element, position, Quaternion.identity);
                // Setup the element from the data
                newElement.GetComponent<Element>().SetupFromData(elementData);
                // Add it to the elements array
                elements.Add(newElement);
            }
            // Restore game stats
            score = GamePersistence.gameData.lastScore;
            timeSinceStart = GamePersistence.gameData.lastTimeSinceStart;
            highestPower = GamePersistence.gameData.lastHighestPower;
        }
        else
        {
            // Reset values
            score = 0;
            timeSinceStart = 0f;
            highestPower = 3;
            // Create a full screen of elements (VERTICAL_ELEMENTS * HORIZONTAL_ELEMENTS)
            Vector3 position = new Vector3();
            for (int i = 0; i < VERTICAL_ELEMENTS; i++)
            {
                for (int j = 0; j < HORIZONTAL_ELEMENTS; j++)
                {
                    // Calculate its position
                    position.x = VERTICAL_START + (elementSpriteRenderer.bounds.size.x + DISTANCE_BETWEEN_ELEMENTS) * (j + 1);
                    position.y = HORIZONTAL_START - (elementSpriteRenderer.bounds.size.y + DISTANCE_BETWEEN_ELEMENTS) * i;
                    // Create a new element
                    GameObject newElement = Instantiate(element, position, Quaternion.identity);
                    // Setup the element
                    newElement.GetComponent<Element>().Setup(highestPower);
                    // Add it to the elements array
                    elements.Add(newElement);
                }
            }
        }
        // Look for a hint
        LookForHints();
        // Update score UI
        UpdateScoreUI();
    }

    /* 
    * This will re arrange the grid elements after finishing combinations:
    * 1 - Drop down elements
    * 2 - Generate new elements
    */
    private void ReArrangeElements()
    {
        // Loop through every grid
        Vector3 position = new Vector3();
        Vector3 startingPosition = new Vector3();
        for (int i = 0; i < HORIZONTAL_ELEMENTS; i++)
        {
            int holesInColumn = 0;
            for (int j = VERTICAL_ELEMENTS - 1; j >= 0; j--)
            {
                // Calculate its position
                position.x = VERTICAL_START + (elementSpriteRenderer.bounds.size.x + DISTANCE_BETWEEN_ELEMENTS) * (i + 1);
                position.y = HORIZONTAL_START - (elementSpriteRenderer.bounds.size.y + DISTANCE_BETWEEN_ELEMENTS) * j;
                
                // Find an element in the position
                GameObject positionElement = FindElementAtPosition(position);
                // If no element found then increase the number of holes
                if (positionElement == null)
                    holesInColumn++;
                else if (holesInColumn != 0)                  
                    // Drop the element
                    positionElement.GetComponent<Element>().TranslateTo(new Vector3(position.x, position.y -
                        (elementSpriteRenderer.bounds.size.y + DISTANCE_BETWEEN_ELEMENTS) * holesInColumn, 1.0f));
            }
            // Set the starting position
            startingPosition.Set(position.x, 2.4f + elementSpriteRenderer.bounds.size.y + DISTANCE_BETWEEN_ELEMENTS, 1.0f);
            // At the end create new elements based on the number of holes
            while(holesInColumn > 0)
            {
                // Calculate its position
                position.x = VERTICAL_START + (elementSpriteRenderer.bounds.size.x + DISTANCE_BETWEEN_ELEMENTS) * (i + 1);
                position.y = HORIZONTAL_START - (elementSpriteRenderer.bounds.size.y + DISTANCE_BETWEEN_ELEMENTS) * (holesInColumn - 1);
                // Create a new element
                GameObject newElement = Instantiate(element, startingPosition, Quaternion.identity);
                // Setup the element
                newElement.GetComponent<Element>().Setup(highestPower);
                // Transit to the position
                newElement.GetComponent<Element>().TranslateTo(position);
                // Add it to the elements array
                elements.Add(newElement);
                // Decrease number of holes
                holesInColumn--;
            }
        }
    }

    /* 
        * This will create and setup a new line and point it as the last created line:
        * 1 - Create a new line game object
        * 2 - Set its size and color
        * 3 - Set its elements count to 2 (starting and ending points)
        * 4 - Add it to the lines array
    */
    private void CreateLine(Color color)
    {
        // Create a new line
        GameObject newLine = new GameObject("Line");
        // Add the line renderer and setup it
        currentLineRenderer = newLine.AddComponent<LineRenderer>();
        currentLineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        currentLineRenderer.startWidth = LINE_WIDTH;
        currentLineRenderer.endWidth = LINE_WIDTH;
        currentLineRenderer.startColor = color;
        currentLineRenderer.endColor = color;
        currentLineRenderer.positionCount = 2;
        // Add it to the lines array
        lines.Add(newLine);
        // Set it as the current line
        currentLine = newLine;
    }

    /* 
    * This will look for and save a new hint, if none are found then the game is over:
    * 1 - Look for a hint (next move)
    * 2 - End the game if none are found
    */
    private void LookForHints()
    {
        // Reset hint elements
        firstHintElement = secondHintElement = null;
        // Loop through all the elements
        foreach (GameObject currentElement in elements)
        {
            // Get element script
            Element elementScript = currentElement.GetComponent<Element>();
            // Get number
            long number = elementScript.GetNumber();
            // Loop through the elements again
            foreach (GameObject newElement in elements)
            {
                // Ignore the same element
                if (currentElement == newElement)
                    continue;

                // Get new element script
                Element newElementScript = newElement.GetComponent<Element>();
                // Is it one of the surrounding elements ?
                if (GameMath.GetDistanceBetweenVectors(elementScript.GetPositionAfterTranslation(),
                    newElementScript.GetPositionAfterTranslation()) > elementSpriteRenderer.bounds.size.x + DISTANCE_BETWEEN_ELEMENTS * 2f)
                    continue;

                // Are they the same number ?
                if(number == newElementScript.GetNumber())
                {
                    // Save hint elements
                    firstHintElement = currentElement;
                    secondHintElement = newElement;
                    break;
                }
            }

            // Did we find a hint ?
            if (firstHintElement != null)
                break;
        }

        // If we have no hints then end the game
        if (firstHintElement == null)
        {
            // End the game
            EndGame();
        }
    }

    private GameObject FindElementAtPosition(Vector3 position)
    {
        // Loop through the elements
        foreach(GameObject currentElement in elements)
        {
            // Compare positions
            if (GameMath.IsInRange(position, currentElement.transform.position, elementSpriteRenderer.bounds.size, 0.5f))
                return currentElement;
        }
        return null;
    }

    private void UpdateScoreUI()
    {
        // Set the score string
        string scoreString = score.ToString();
        string formattedScore = "";
        int i = scoreString.Length;
        // Loop until we reach the end
        while(i > 0)
        {
            // Compute the starting index
            int startIndex = i - 3 < 0 ? 0 : i - 3;
            // Get the last 3 digits
            string digits = scoreString.Substring(startIndex, i - 3 < 0 ? i : 3);
            // Insert the comma if necessary
            if (i != 0)
                formattedScore = " " + formattedScore;

            // Insert the digits
            formattedScore = digits + formattedScore;
            // Decrease iterator
            i -= 3;
        }
        // Set the score's UI text
        scoreText.text = "Score\n" + formattedScore;
    }

    private void EndGame()
    {
        // Loop through all the elements
        foreach(GameObject currentElement in elements)
        {
            // Play the element touched animation
            currentElement.GetComponent<Animation>().Play("ElementTouched", PlayMode.StopAll);
        }
        // Set the game over flag
        gameOver = true;
        // Reset game over timer
        gameOverTimer = 0;
    }

    public void OnHintButton()
    {
        // Make sure we're not dying
        if (playingHint || gameOver || firstHintElement == null || secondHintElement == null)
            return;

        // Show interstitial
        AdsController.ShowInterstitialAd();
        hasJustShowenbAd = true;
        // Play hint animation on hint elements
        firstHintElement.GetComponent<Animation>().Play("ElementHinted", PlayMode.StopAll);
        secondHintElement.GetComponent<Animation>().Play("ElementHinted", PlayMode.StopAll);
        // Set playing hint flag
        playingHint = true;
    }

    public void OnPauseButton()
    {
        // Ensure we're eligible to pause
        if (gameOver || paused)
            return;

        // Toggle the pause
        TogglePause(true);
    }

    public void OnRestartButton()
    {
        // Reset elements data
        GamePersistence.gameData.elementsData = null;
        GamePersistence.Save();
        // Destroy all elements
        foreach (GameObject currentElement in elements)
            Destroy(currentElement);

        elements.Clear();
        // Start a new game
        NewGame(true);
        // Toggle pause off
        TogglePause(false);
    }

    private void TogglePause(bool toggle)
    {
        // Set paused flag
        paused = toggle;
        // Toggle UI elements
        pauseFade.enabled = toggle;
        pauseText.enabled = toggle;
        pauseHintText.enabled = toggle;
        restartButton.gameObject.SetActive(toggle);
        // Set the pause elements
        if (paused)
        { 
            // Reset can resume flag
            canResume = false;
            // Bring pause UI to front
            pauseFade.transform.SetAsLastSibling();
            pauseText.transform.SetAsLastSibling();
            pauseHintText.transform.SetAsLastSibling();
            restartButton.transform.SetAsLastSibling();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            // Ignore if we've just showen an ad
            if (hasJustShowenbAd)
                hasJustShowenbAd = false;
            else
            {
                // Notify application focus on ads
                AdsController.OnResume();
                // Show interstitial
                AdsController.ShowInterstitialAd();
                // Set showen ad flag
                hasJustShowenbAd = true;
            }
        }
    }
}
