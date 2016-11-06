using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GamePlayMenu : MonoBehaviour {

	public FPSInputController playerController;

	//	public bool isGameActive = false;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}

	// Reset the game and start again
	public void LoadScene () {
		Scene currScene = SceneManager.GetActiveScene ();
		//		Scene nextScene;
		//		if (currScene.name == "Level1") {
		//			nextScene = SceneManager.GetSceneByName("Level2");
		//			SceneManager.LoadScene (nextScene.name);
		//		}

		playerController.StartGame ();
		//		isGameActive = true;
	}

	public void StartScene() {

	}
}
