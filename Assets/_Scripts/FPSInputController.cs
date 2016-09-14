using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Require a character controller to be attached to the same game object
[RequireComponent(typeof(CharacterMotor))]
[AddComponentMenu("Character/FPS Input Controller")]

public class FPSInputController : MonoBehaviour
{
	private CharacterMotor motor;
	public bool checkAutoWalk = false;

	private bool MFI_Connected = false;
	IEnumerator CheckForControllers()
	{
		while (true)
		{
			string[] controllers = Input.GetJoystickNames();

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
		motor = GetComponent<CharacterMotor>();
		StartCoroutine(CheckForControllers());

	}
	
	// Update is called once per frame
	void Update()
	{
		// Get the input vector from keyboard or analog stick
		Vector3 directionVector;
		if (!checkAutoWalk) { 
			directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		} else { 
			directionVector = new Vector3(0, 0, 1);
		}

		if (directionVector != Vector3.zero)
		{
			// Get the length of the directon vector and then normalize it
			// Dividing by the length is cheaper than normalizing when we already have the length anyway
			float directionLength = directionVector.magnitude;
			directionVector = directionVector / directionLength;
			
			// Make sure the length is no bigger than 1
			directionLength = Mathf.Min(1.0f, directionLength);
			
			// Make the input vector more sensitive towards the extremes and less sensitive in the middle
			// This makes it easier to control slow speeds when using analog sticks
			directionLength = directionLength * directionLength;
			
			// Multiply the normalized direction vector by the modified length
			directionVector = directionVector * directionLength;
		}
		
		// Apply the direction to the CharacterMotor
		motor.inputMoveDirection = transform.rotation * directionVector;
		motor.inputJump = Input.GetButton("Jump");

		if (Input.GetButton("Fire1")) {
			Debug.Log ("PRESSED!!!");
		};
	}
}