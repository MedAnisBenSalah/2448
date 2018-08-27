using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System;
using System.Text;

public static class GamePersistence
{
    public static readonly int ELEMENTS_COUNT = 35;

    [Serializable]
    public class ElementData
    {
        [XmlElement("Number")]
        public long number;

        [XmlElement("Power")]
        public int power;

        [XmlElement("X")]
        public float x;

        [XmlElement("Y")]
        public float y;

        public ElementData()
        {
        }

        public ElementData(Element element)
        {
            // Copy information from the element
            number = element.GetNumber();
            power = element.GetPower();
            x = element.GetPositionAfterTranslation().x;
            y = element.GetPositionAfterTranslation().y;
        }
    }

    [Serializable]
    public class GameData
    {
        [XmlElement("BestScore")]
        public long bestScore;

        [XmlElement("GamesPlayed")]
        public long gamesPlayed;

        [XmlElement("GPGSLogin")]
        public bool gpgsLogin;

        [XmlElement("Muted")]
        public bool muted;

        [XmlElement("FirstTime")]
        public bool firstTime;

        [XmlElement("ElementsData")]
        public ElementData[] elementsData;

        [XmlElement("LastScore")]
        public long lastScore;

        [XmlElement("LastTimeSinceStart")]
        public float lastTimeSinceStart;

        [XmlElement("LastHighestPower")]
        public int lastHighestPower;

        public GameData()
        {
            bestScore = 0;
            gamesPlayed = 0;
            gpgsLogin = false;
            muted = false;
            firstTime = true;
            elementsData = null;
            lastScore = 0;
            lastTimeSinceStart = 0f;
            lastHighestPower = 0;
        }
    }

    public static GameData gameData;

    // Keys
    private static readonly string SAVE_DATA_FILE = "2448.oap";

    public static void Load()
    {
        try
        {
            // Create an XML serialize instance
            XmlSerializer serializer = new XmlSerializer(typeof(GameData));
            // Open the save file
            using(FileStream stream = new FileStream(Application.persistentDataPath + SAVE_DATA_FILE, FileMode.Open))
            {
                // Get data
                byte[] encodedXmlData = new byte[stream.Length];
                stream.Read(encodedXmlData, 0, (int)stream.Length);
                // Decode data
                byte[] xmlData = System.Convert.FromBase64String(Encoding.UTF8.GetString(encodedXmlData));
                // Deserialize the data
                using (TextReader reader = new StringReader(Encoding.UTF8.GetString(xmlData)))
                {
                    gameData = serializer.Deserialize(reader) as GameData;
                }
            }
        }
        catch (Exception)
        {
            // Save an empty file
            Save();
        }
    }

    public static void Save()
    {
        // Create the game data if its empty
        if (gameData == null)
            gameData = new GameData();

        try
        {
            // Create an XML serialize instance
            XmlSerializer serializer = new XmlSerializer(typeof(GameData));
            // Open the save file
            using (FileStream stream = new FileStream(Application.persistentDataPath + SAVE_DATA_FILE, FileMode.Create))
            {
                byte[] xmlData;
                int length;
                using (StringWriter writer = new StringWriter())
                {
                    // Serialize the data
                    serializer.Serialize(writer, gameData);
                    // Get data
                    xmlData = Encoding.UTF8.GetBytes(writer.ToString());
                    // Get length
                    length = writer.ToString().Length;
                }
                // Base64 encode for the data
                String encodedData = System.Convert.ToBase64String(xmlData);
                // Persist data
                stream.Write(Encoding.UTF8.GetBytes(encodedData), 0, encodedData.Length);
            }
        }
        catch (Exception)
        {  
        }
    }

    public static void SetElementsData(ArrayList elements)
    {
        // Create the elements array
        if(gameData.elementsData == null)
            gameData.elementsData = new ElementData[ELEMENTS_COUNT];

        // Loop though the elements
        int i = 0;
        foreach(GameObject element in elements)
        {
            // Get the script
            Element script = element.GetComponent<Element>();
            // Create element data instance
            gameData.elementsData[i] = new ElementData(script);
            // Increase iterator
            i++;
        }
    }
}
