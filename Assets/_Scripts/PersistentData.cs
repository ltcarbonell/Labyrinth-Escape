using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class PersistentData : MonoBehaviour {

	public static PersistentData data;

	public int currentLevel;
	public int currentMaxLevel;
	public int playerLives;
	public int currentTimeTotal;

	public int maxLevel;
	public int minTimeForMaxLevel;
	public int maxLives;
	// Use this for initialization
	void Awake () {
		if (data == null) {
			DontDestroyOnLoad (gameObject);
			data = this;
//			data.Load ();
		} else if (data != this) {
			Destroy (gameObject);
		}
	}

	public void Save() {
		Debug.Log ("Saving");
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (Application.persistentDataPath + "/playerInfo.dat");
		PlayerData newData = new PlayerData ();
		newData.playerLives = playerLives;
		newData.currentLevel = currentLevel;
		newData.currentMaxLevel = currentMaxLevel;
		newData.currentTimeTotal = currentTimeTotal;

		newData.maxLevel = maxLevel;
		newData.minTimeForMaxLevel = minTimeForMaxLevel;
		newData.maxLives = maxLives;

		Debug.Log ("Saved");

		bf.Serialize (file, newData);
		file.Close ();
	}

	public void Load() {
		Debug.Log ("Loading");
		if (File.Exists (Application.persistentDataPath + "/playerInfo.dat")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);
			PlayerData newData = (PlayerData)bf.Deserialize (file);
			file.Close ();

			currentMaxLevel = newData.currentMaxLevel;
			currentTimeTotal = newData.currentTimeTotal;

			maxLevel = newData.maxLevel;
			minTimeForMaxLevel = newData.minTimeForMaxLevel;
			maxLives = newData.maxLives;
			Debug.Log ("Loaded");
		}
	}
}

[Serializable]
class PlayerData  {
	public int currentLevel;
	public int currentMaxLevel;
	public int playerLives;
	public int currentTimeTotal;

	public int maxLevel;
	public int minTimeForMaxLevel;
	public int maxLives;
}