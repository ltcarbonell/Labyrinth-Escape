using UnityEngine;
using System.Collections;

public class PersistentData : MonoBehaviour {

	public int currentLevel = 1;
	public int playerLives = 5;
	// Use this for initialization
	void Start () {
		DontDestroyOnLoad (this);
	}
}
