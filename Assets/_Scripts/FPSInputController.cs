using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using System;


// Require a character controller to be attached to the same game object
[RequireComponent(typeof(CharacterMotor))]
[AddComponentMenu("Character/FPS Input Controller")]

public class FPSInputController : MonoBehaviour
{
	
	private CharacterMotor motor;
	public bool checkAutoWalk = false;
	private GameObject head;

	public Text timerText;
	public Text menu;
	public Text recordMenu;
	public float timeLeft;
	Scene currentScene;
	bool isGameActive = false;
	bool isGamePaused = false;
	bool levelFinished = false;
	bool gameOver = false;
	int playerLives;
	int MAXLIVES = 5;
	float START_TIME = 60.0f;

	int currentLevel;

	GameObject[] lives;

	private bool MFI_Connected = false;
	IEnumerator CheckForControllers()
	{
		while (true)
		{
			string[] controllers = Input.GetJoystickNames();
//			inputText.text = controllers [0];

			if (!MFI_Connected && controllers.Length > 0)
			{
				MFI_Connected = true;
				Debug.Log("Connected");
				GameObject.FindWithTag("CONTROL_PAD").SetActive(false);
				GameObject.FindWithTag("CONTROL_FIRE").SetActive(false);
			}
			else if (MFI_Connected && controllers.Length == 0)
			{
				MFI_Connected = false;
				Debug.Log("Disconnected");
				GameObject.FindWithTag("CONTROL_PAD").SetActive(true);
				GameObject.FindWithTag("CONTROL_FIRE").SetActive(true);

			}
			yield return new WaitForSeconds(1f);
		}
	}

	// Use this for initialization
	void Awake()
	{
		PersistentData.data.Load ();
		currentScene = SceneManager.GetActiveScene ();
		motor = GetComponent<CharacterMotor>();
		StartCoroutine(CheckForControllers());
		head = GameObject.FindWithTag ("GvrHead");

		lives = GameObject.FindGameObjectsWithTag ("Life").OrderBy( go => go.name ).ToArray();

		this.playerLives = PersistentData.data.playerLives;
		this.currentLevel = PersistentData.data.currentLevel;

		Debug.Log ("Life object count " + MAXLIVES);
		Debug.Log ("Player Lives: " + playerLives);
		Debug.Log ("Current Scene " + currentScene.name);
		Debug.Log ("Current Level: " + currentLevel);

	}

	// Update is called once per frame
	void Update()
	{
//		string[] texts = Input.GetJoystickNames ();
//		inputText.text = texts [0];
		if (isGameActive && isGamePaused) {
			Vector3 directionVector;
			directionVector = new Vector3 (0, 0, 0);
			motor.inputMoveDirection = head.transform.rotation * directionVector;
			// Display pause menu
			int timeLeftSeconds = (int)timeLeft;
			menu.text = ("Press Start to Begin\n\n" +
			"Current Level:" + currentLevel.ToString () +
			"\nTime Remaining:" + timeLeftSeconds.ToString () +
			"\nLives:" + playerLives.ToString ());
			menu.gameObject.SetActive (true);
			if (Input.GetButtonDown ("Submit")) {
				PauseUnPauseGame ();
				Debug.Log (isGamePaused);
			}
		} else if (isGameActive && !isGamePaused) {
			menu.gameObject.SetActive (false);

			// Set forward direction toward camera
			//		transform.eulerAngles.y = head.transform.rotation.y; 		// Get the input vector from keyboard or analog stick
			//		Debug.Log(head.transform.localEulerAngles.y);
			Vector3 directionVector;
			directionVector = new Vector3 (Input.GetAxis ("Horizontal"), 0, Input.GetAxis ("Vertical"));

			if (directionVector != Vector3.zero) {
				// Get the length of the directon vector and then normalize it
				// Dividing by the length is cheaper than normalizing when we already have the length anyway
				float directionLength = directionVector.magnitude;
				directionVector = directionVector / directionLength;

				// Make sure the length is no bigger than 1
				directionLength = Mathf.Min (1.0f, directionLength);

				// Make the input vector more sensitive towards the extremes and less sensitive in the middle
				// This makes it easier to control slow speeds when using analog sticks
				directionLength = directionLength * directionLength;

				// Multiply the normalized direction vector by the modified length
				directionVector = directionVector * directionLength;
			}

			// Apply the direction to the CharacterMotor
			motor.inputMoveDirection = head.transform.rotation * directionVector;
			motor.inputJump = Input.GetButtonDown ("Jump");

//			inputText.text = 

			if (Input.GetButtonDown ("Fire1")) {
				//			Debug.Log ("PRESSED!!!");
//				Fire();
			}

			if (Input.GetButtonDown ("Submit")) {
				PauseUnPauseGame ();
				Debug.Log (isGamePaused);
			}

			RunTimer ();
			UpdateLifeSprites ();
		} else {
			if (levelFinished) {
				if (Input.GetButtonDown ("Submit")) {
					startNewLevel (PersistentData.data.currentLevel);
					levelFinished = false;
				}
			} 

			if (gameOver) {
				if (Input.GetButtonDown ("Submit")) {
					gameOver = false;
					levelFinished = false;
					PersistentData.data.currentLevel = 1;
					startNewLevel (PersistentData.data.currentLevel);
				}
			}

			else if (Input.GetButtonDown ("Submit")) {
				//				int timeLeftSeconds = (int)timeLeft;
				menu.text = ("Press Start to Begin\n\n" 
					//					+ "Current Level:" + currentLevel.ToString() + 
					//					"\nTime Remaining:" + timeLeftSeconds.ToString () +
					//					"\nLives:" + playerLives.ToString() 
				);
				menu.gameObject.SetActive (true);
				StartGame ();
			}
		}
		UpdateLifeSprites ();
			
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("TimePowerUp")){
			other.gameObject.SetActive(false);
			IncreaseTimer(5.0f);
		} else if (other.gameObject.CompareTag("LevelFinish")) {
			if (currentLevel < 3) {
				if (playerLives < MAXLIVES) {
					playerLives++;
					PersistentData.data.playerLives = this.playerLives;
				}
				PersistentData.data.currentLevel = ++currentLevel;
				LevelFinished ();
				other.gameObject.SetActive (false);
			}
			else {
				GameOverWithResult (true);
			}
		}

	}

	void GameOverWithResult(bool didWin)
	{
		isGameActive = false;
		isGamePaused = false;

		Vector3 directionVector;
		directionVector = new Vector3 (0, 0, 0);
		motor.inputMoveDirection = head.transform.rotation * directionVector;

		if (didWin) {
			PersistentData.data.currentMaxLevel = PersistentData.data.currentLevel;
		} else {
			PersistentData.data.currentMaxLevel = PersistentData.data.currentLevel - 1;
		}
		PersistentData.data.currentTimeTotal += (int)timeLeft;
		Debug.Log ("GameOver");
		// If new max level, or new min time or new max lives
		if (PersistentData.data.maxLevel <= PersistentData.data.currentMaxLevel) {
			Debug.Log("Persistant Data 1");
			if (PersistentData.data.maxLevel < PersistentData.data.currentLevel ||
				(PersistentData.data.maxLevel == PersistentData.data.currentMaxLevel && 
					PersistentData.data.maxLives <= PersistentData.data.playerLives)) {
				Debug.Log("Persistant Data 2");
				if ((PersistentData.data.maxLives < PersistentData.data.playerLives) ||
					(PersistentData.data.currentTimeTotal <= PersistentData.data.minTimeForMaxLevel && 
						PersistentData.data.maxLives == PersistentData.data.playerLives)) {
					// Broke record
					Debug.Log ("New Record");
					PersistentData.data.maxLevel = PersistentData.data.currentMaxLevel;
					PersistentData.data.minTimeForMaxLevel = PersistentData.data.currentTimeTotal;
					PersistentData.data.maxLives = PersistentData.data.playerLives;
					recordMenu.text = ("NEW RECORD\nMax Level: " + PersistentData.data.currentMaxLevel + "\nTotal Time: " + PersistentData.data.currentTimeTotal + "\nRemaining Lives: " + PersistentData.data.playerLives);
					recordMenu.gameObject.SetActive (true);




					PersistentData.data.currentLevel = 1;
					PersistentData.data.playerLives = 5;
					PersistentData.data.currentMaxLevel = 0;
					PersistentData.data.currentTimeTotal = 0;
					PersistentData.data.Save ();
				}
			}
		}



		if (didWin) {
			// You won
			menu.text = ("YOU WIN\n\nPress start to play again");
			menu.gameObject.SetActive (true);
		} else {
			// You lost
			menu.text = ("Game Over\n\nPress Start to play again");
			menu.gameObject.SetActive (true);
		}
		PersistentData.data.currentLevel = 1;
		this.currentLevel = PersistentData.data.currentLevel;
		levelFinished = false;
		gameOver = true;

		PersistentData.data.playerLives = 5;
		this.playerLives = PersistentData.data.playerLives;
		if (Input.GetButtonDown ("Submit")) {
			startNewLevel (PersistentData.data.currentLevel);
		}



	}

	void LevelFinished() {
		if (!gameOver) {
			isGameActive = false;
			isGamePaused = false;
			levelFinished = true;
			Vector3 directionVector;
			directionVector = new Vector3 (0, 0, 0);
			motor.inputMoveDirection = head.transform.rotation * directionVector;
			this.currentLevel = PersistentData.data.currentLevel;
			PersistentData.data.currentMaxLevel = PersistentData.data.currentLevel-1;
			PersistentData.data.currentTimeTotal += (int)(60 - timeLeft);
			menu.text = ("You finished level " + (currentLevel - 1).ToString () + " in " + (int)(60 - timeLeft) + " seconds\n\nPress start to move to next level");
			menu.gameObject.SetActive (true);
			if (Input.GetButtonDown ("Submit")) {
				startNewLevel (PersistentData.data.currentLevel);
			}
		}
	}

	public void StartGame()
	{
		timeLeft = START_TIME;
		isGameActive = true;
		isGamePaused = false;
	}

	public void PauseUnPauseGame()
	{
		isGamePaused = !isGamePaused;
		menu.gameObject.SetActive (isGamePaused);
	}

	void startNewLevel(int level) {
		Debug.Log ("Starting level " + level);
		LoadScene (level);
	}

	void SetTimerText()
	{
		int timeLeftSeconds = (int)timeLeft;
		timerText.text = timeLeftSeconds.ToString ();
		if (timeLeft > 30) {
			timerText.material.color = Color.green;
		} else if (timeLeft > 15) {
			timerText.material.color = Color.yellow;
		} else {
			timerText.material.color = Color.red;
		}
	}

	void RunTimer ()
	{
		if (isGameActive && !isGamePaused) {
			timeLeft -= Time.deltaTime;
			SetTimerText ();
			if (timeLeft < 0) {
				if (playerLives > 0) {
					playerLives--;
					Debug.Log ("Life removed.  Number left " + playerLives);
					PersistentData.data.playerLives = playerLives;
					UpdateLifeSprites ();
					RestartLevel ();
				} else {
					GameOverWithResult (false);
				}
			}
		}
	}
		
	void IncreaseTimer(float incrementBy) {
		timeLeft += incrementBy;
		SetTimerText ();

	}

	void UpdateLifeSprites() {
		for (int i = 0; i < lives.Length; i++) {
			if (i < playerLives) {
				lives [i].SetActive (true);
			} else {
				lives [i].SetActive (false);
			}
		}
	}

	public void LoadScene (int sceneNum) {
		SceneManager.LoadScene ("Level "+sceneNum);

		StartGame ();
	}

	void RestartLevel() {
		isGameActive = false;
		isGamePaused = false;
		LoadScene (currentLevel);
	}


//	void Fire()
//	{
//		// Create the Bullet from the Bullet Prefab
//		var bullet = (GameObject)Instantiate (
//			bulletPrefab,
//			bulletSpawn.position,
//			bulletSpawn.rotation);
//
//		// Add velocity to the bullet
//		bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 6;
//
//		// Destroy the bullet after 2 seconds
//		Destroy(bullet, 2.0f);
//	}

}
