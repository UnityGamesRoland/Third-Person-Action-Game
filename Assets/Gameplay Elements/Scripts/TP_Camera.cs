using UnityEngine;
using System.Collections;

public class TP_Camera : MonoBehaviour
{
	public Camera playerCamera;
	public float height = 7f;
	public float distance = 8f;
	[Range(1, 179)] public float defaultFOV = 55f;
	[Range(1, 179)] public float actionFOV = 65f;

	private Vector3 positionChangeVelocity;
	private PlayerInformation info;

	private void Start()
	{
		//Initialization.
		info = GetComponent<PlayerInformation>();
	}

	private void Update()
	{
		//Update the camera's position and FOV smoothly.
		playerCamera.transform.position = Vector3.SmoothDamp(playerCamera.transform.position, transform.position + Vector3.forward * -distance + Vector3.up * height, ref positionChangeVelocity, 0.07f);
		playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, info.combatMode ? actionFOV : defaultFOV, Time.deltaTime * 4);
	}

}
