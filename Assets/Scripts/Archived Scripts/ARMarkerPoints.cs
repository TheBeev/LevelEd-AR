using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using UnityEngine.UI;

public class ARMarkerPoints : MonoBehaviour {

	[SerializeField] private GameObject markerPrefab;
	[SerializeField] private GameObject guideMarkerPrefab;
	[SerializeField] private float createHeight;

	[SerializeField] private Text qualityText;

	private Color materialColour;
	private Color guideColour;
	private MaterialPropertyBlock props;
	private Vector3 spawnLocation;
	private GameObject guideMarkerSphere;
	private GameObject markerSphere;

	private Camera mainCamera;

	// Use this for initialization
	void Start () 
	{
        //
		guideColour = new Color (0.5f, 0.5f, 0.5f, 0.2f);
		mainCamera = Camera.main;
		props = new MaterialPropertyBlock ();
	}

	void CreateBall(Vector3 atPosition)
	{
		markerSphere = (GameObject)Instantiate (markerPrefab, atPosition, Quaternion.identity);
	
		props.SetColor("_InstanceColor", materialColour);

		MeshRenderer renderer = markerSphere.GetComponent<MeshRenderer>();

		renderer.SetPropertyBlock(props);

	}

	// Update is called once per frame
	void Update () 
	{

		var screenPosition = mainCamera.ScreenToViewportPoint(new Vector3 (Screen.width / 2, Screen.height / 2, mainCamera.transform.position.z));

		ARPoint point = new ARPoint 
		{
			x = screenPosition.x,
			y = screenPosition.y
		};

		List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (point, ARHitTestResultType.ARHitTestResultTypeFeaturePoint);

		if (hitResults.Count > 0)
		{
			foreach (var hitResult in hitResults)
			{
				spawnLocation = UnityARMatrixOps.GetPosition (hitResult.worldTransform);

				if (guideMarkerSphere == null)
				{
					guideMarkerSphere = Instantiate (markerPrefab, new Vector3 (0, 0, 0), Quaternion.identity);
					guideMarkerSphere.GetComponent<Renderer> ().material.color = guideColour;
				} else
				{
					guideMarkerSphere.SetActive (true);
					guideMarkerSphere.transform.position = spawnLocation;
				}

				break;
			}
		} else
		{
			guideMarkerSphere.SetActive (false);
		}

		if (Input.touchCount > 0 && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
		{
			var touch = Input.GetTouch(0);
			if (touch.phase == TouchPhase.Began)
			{
				CreateBall (new Vector3 (spawnLocation.x, spawnLocation.y, spawnLocation.z));
			}
		}
	}
		
	//Called from UI
	public void SetNewColour()
	{
		float r = Random.Range(0.0f, 1.0f);
		float g = Random.Range(0.0f, 1.0f);
		float b = Random.Range(0.0f, 1.0f);

		materialColour = new Color (r, g, b);
	}

	//called from UI
	public void UndoLastMarker()
	{
		if (markerSphere != null)
		{
			Destroy (markerSphere);
		}
	}
}
