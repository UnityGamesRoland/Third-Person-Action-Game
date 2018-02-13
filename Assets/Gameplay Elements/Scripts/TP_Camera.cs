using UnityEngine;
using System.Collections;

public class TP_Camera : MonoBehaviour
{
	public Camera playerCamera;
	public Vector3 offset;
	[Range(1, 179)] public float defaultFOV = 55f;
	[Range(1, 179)] public float actionFOV = 65f;

	private Vector3 positionChangeVelocity;

	private void Update()
	{
		//Update the camera's position and FOV smoothly.
		playerCamera.transform.position = Vector3.SmoothDamp(playerCamera.transform.position, transform.position + offset, ref positionChangeVelocity, 0.07f);
		playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, PlayerInformation.Instance.combatMode ? actionFOV : defaultFOV, Time.deltaTime * 4);
	}

}
