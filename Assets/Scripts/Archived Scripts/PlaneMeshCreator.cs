using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneMeshCreator : MonoBehaviour {

	private GameObject go;
	private MeshRenderer mr;
	private MeshFilter mf;
	private Mesh m;

	public GameObject CreatePlane(Vector3 bottomRight, Vector3 bottomLeft, Vector3 topRight, Vector3 topLeft, bool bAddCollider, Material mat)
	{
		go = new GameObject("Plane");
		mf = go.AddComponent(typeof(MeshFilter)) as MeshFilter;
		mr = go.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

		m = new Mesh();
		m.vertices = new Vector3[]
		{
			bottomRight,
			bottomLeft,
			topRight,
			topLeft
		};

		m.uv = new Vector2[]
		{
			new Vector2(0,0),
			new Vector2(0,1),
			new Vector2(1,1),
			new Vector2(1,0)
		};

		m.triangles = new int[]
		{
			0,1,2,1,3,2
		};

		mf.mesh = m;

		if (bAddCollider)
		{
			(go.AddComponent(typeof(MeshCollider)) as MeshCollider).sharedMesh = m;
		}

		mr.material = mat;

		m.RecalculateBounds();
		m.RecalculateNormals();

		return go;

	}
}
