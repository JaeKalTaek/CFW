using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SC_SavedDataManager {

    [Serializable]
    public class SavedData {

        public Dictionary<string, string> decks;

        public SavedData () {

            decks = new Dictionary<string, string> ();

        }

    }

    public static SavedData savedData;

    public static void SaveDeck (string name, string content) {        

        savedData.decks[name] = content;

        FileStream file = File.Open (Application.persistentDataPath + "/MySaveData.sav", FileMode.Open);

        new BinaryFormatter ().Serialize (file, savedData);

        file.Close ();

    }    

    public static void LoadDecks () {

        if (File.Exists (Application.persistentDataPath + "/MySaveData.sav")) {            

            FileStream file = File.Open (Application.persistentDataPath + "/MySaveData.sav", FileMode.Open);

            savedData = (file.Length > 0) ? (SavedData) new BinaryFormatter ().Deserialize (file) : new SavedData ();

            file.Close ();

        } else {

            File.Create (Application.persistentDataPath + "/MySaveData.sav").Close ();

            savedData = new SavedData ();

        }

    }

}
