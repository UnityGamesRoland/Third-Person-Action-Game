using UnityEngine;
using System.Collections;

public class TP_Camera : MonoBehaviour
{
	public Transform cameraHolder;
	public Camera playerCamera;
	public Vector3 offset;
	[Range(1, 179)] public float defaultFOV = 55f;
	[Range(1, 179)] public float actionFOV = 65f;

	private Vector3 positionChangeVelocity;
	private bool isShaking;
	private int shakeLayer;
	private float shakePower;
	private float shakeAmountX;
	private float shakeAmountY;

	#region Singleton
	public static TP_Camera Instance {get; private set;}
	private void Awake()
	{
		if(Instance == null) Instance = this;
	}
	#endregion

	private void Update()
	{
		//Update the camera's position and FOV smoothly.
		cameraHolder.position = Vector3.SmoothDamp(cameraHolder.position, transform.position + offset, ref positionChangeVelocity, 0.07f);
		playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, PlayerInformation.Instance.combatMode ? actionFOV : defaultFOV, Time.deltaTime * 4);

		//Update the camera's shaking rotation.
		Quaternion targetAngle = Quaternion.Euler(isShaking ? playerCamera.transform.localEulerAngles + new Vector3(shakeAmountX, shakeAmountY, 0) : Vector3.zero);
		playerCamera.transform.localRotation = Quaternion.Lerp(playerCamera.transform.localRotation, targetAngle, Time.deltaTime / 0.1f);
	}

	public void Shake(float amount, float length, int layer)
	{
		//Set the shake variable and start shaking the camera.
		if(shakeLayer <= layer)
		{
			shakePower = amount;
			shakeLayer = layer;
			InvokeRepeating("BeginShake", 0, 0.1f);
			Invoke("StopShake", length);
		}
	}

	private void BeginShake()
	{
		//Check if the player is alive.
		if(PlayerInformation.Instance.isDead) return;

		//Get the random shake amount and add it to the position.
		shakeAmountX = Random.value * shakePower * 2 - shakePower;
		shakeAmountY = Random.value * shakePower * 2 - shakePower;

		//Update the shaking state.
		isShaking = true;
	}

	private void StopShake()
	{
		//Stop the shaking of the camera.
		CancelInvoke("BeginShake");
		shakeAmountX = 0f;
		shakeAmountY = 0f;
		shakeLayer = 0;
		isShaking = false;
	}
}
