using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Element : MonoBehaviour
{
    public Canvas uiCanvas;
    public Text numberText;
    public Camera mainCamera;

    private Text elementNumberText;
    private long number;
    private int power;

    private bool translating, destroyAfterTranslation;
    private Vector3 translatingTo;
    private GameObject translatingToObject;

    // Elements colors
    private static readonly Color[] colors =
    {
        new Color(0.8274f, 0.3137f, 0.3450f, 1f), // 2
        new Color(0.9607f, 0.6901f, 0.2156f, 1f), // 4
        new Color(0.1921f, 0.6313f, 0.8117f, 1f), // 8
        new Color(0.5921f, 0.7019f, 0.2705f, 1f), // 16 
        new Color(0.4763f, 0.4666f, 0.3921f, 1f), // 32
        new Color(0.4588f, 0.7686f, 0.6862f, 1f), // 64
        new Color(0.4705f, 0.3921f, 0.6745f, 1f), // 128
        new Color(0.7764f, 0.1294f, 0.1529f, 1f), // 256
        new Color(0.9647f, 0.5568f, 0.1843f, 1f), // 512
        new Color(0.0784f, 0.3921f, 0.6627f, 1f), // 1K
    };

    // Elements numbers
    private static readonly string[] numbers =
    {
        "1",
        "2",
        "4",
        "8",
        "16",
        "32",
        "64",
        "128",
        "256",
        "512"
    };

    private static readonly float TRANSLATING_SPEED = 12f;

    void Start ()
    {

    }

    private void OnDestroy()
    {
        // Destroy the element text
        Destroy(elementNumberText);
    }

    public void Setup(long highestPower)
    {
        // Randomly get our number
        power = (int)Random.Range(1, highestPower + 1);   
        // Create ourselves a new text element
        elementNumberText = Instantiate(numberText, Camera.main.WorldToScreenPoint(transform.position), Quaternion.identity);
        elementNumberText.transform.SetParent(uiCanvas.transform);
        elementNumberText.rectTransform.position = Camera.main.WorldToScreenPoint(transform.position);
        // Update number
        UpdateNumber(power);
    }

    public void SetupFromData(GamePersistence.ElementData data)
    {
        // Set stats
        power = data.power;
        number = data.number;
        transform.position = new Vector3(data.x, data.y, 0f);
        // Create ourselves a new text element
        elementNumberText = Instantiate(numberText, Camera.main.WorldToScreenPoint(transform.position), Quaternion.identity);
        elementNumberText.transform.SetParent(uiCanvas.transform);
        elementNumberText.rectTransform.position = Camera.main.WorldToScreenPoint(transform.position);
        // Update number
        UpdateNumber(power);
    }

    void Update ()
    {
        // Are we translating ?
        if(translating)
        {
            // Get target
            Vector3 target = translatingToObject != null ? translatingToObject.transform.position : translatingTo;
            // Translate
            float step = TRANSLATING_SPEED * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, step);
            elementNumberText.rectTransform.position = Camera.main.WorldToScreenPoint(transform.position);
            // Did we finish ?
            if (transform.position.x == target.x && transform.position.y == target.y)
            {
                // Reset translating flag
                translating = false;
                // Reset translating to
                translatingTo = Vector3.zero;
                translatingToObject = null;
                // Should we destroy ?
                if (destroyAfterTranslation)
                {
                    Destroy(gameObject);
                    // Reset flags
                    destroyAfterTranslation = false;
                }
            }
        }
	}

    public void UpdateNumber(long power)
    {
        // Set the power
        this.power = (int)power;
        // Set the number
        number = (long)Mathf.Pow(2, power);
        // Convert numbers to the right symbol (K, M, B)
        string numberString = number.ToString();
        // Set the number
        if (number < 1000)
            elementNumberText.text = numberString;
        else
            elementNumberText.text = numbers[power % numbers.Length];

        if (numberString.Length > 3)
        {
            if (numberString.Length < 7)
                elementNumberText.text += "K";
            else if (numberString.Length < 10)
                elementNumberText.text += "M";
            else if (numberString.Length < 13)
                elementNumberText.text += "B";
            else if (numberString.Length < 16)
                elementNumberText.text += "Q";
        }

        // Set the color
        GetComponent<SpriteRenderer>().color = colors[(power - 1) % colors.Length];
    }

    public long GetNumber()
    {
        return number;
    }

    public int GetPower()
    {
        return power;
    }

    public Vector3 GetPositionAfterTranslation()
    {
        return translating ? (translatingToObject != null ? translatingToObject.transform.position : translatingTo) : 
            transform.position;
    }

    public void TranslateTo(Vector3 position)
    {
        // Set translating flag
        translating = true;
        // Set the translating to vector
        translatingTo = position;
        // Reset the translation target
        translatingToObject = null;
    }

    public void TranslateToAndDestroy(Vector3 position)
    {
        // Translate
        TranslateTo(position);
        // Set destroy flag
        destroyAfterTranslation = true;
    }

    public void TranslateToObject(GameObject target)
    {
        // Set translating flag
        translating = true;
        // Set the translation target
        translatingToObject = target;
        // Reset translation vector
        translatingTo = Vector3.zero;
    }

    public void TranslateToObjectAndDestroy(GameObject target)
    {
        // Translate
        TranslateToObject(target);
        // Set destroy flag
        destroyAfterTranslation = true;
    }
}
