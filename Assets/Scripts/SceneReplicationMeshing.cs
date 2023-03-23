using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

//[ExecuteInEditMode]
public class SceneReplicationMeshing : MonoBehaviour {

	private FTPUtility ftpClient;

	public List<Vector3> planeMarkerPositions = new List<Vector3>();
	public List<Vector3> depthMarkerPositions = new List<Vector3>();
	public List<FreeformListV> freeformMarkerPosition = new List<FreeformListV>();
	public List<ContinuousListV> continuousMarkerPosition = new List<ContinuousListV>();

	[SerializeField] private GameObject markerPrefab;
	[SerializeField] private string fileName = "/P1_Task1Plane.dat";

	//plane
	private GameObject newMarkerRef;
	private GameObject go;
	private MeshRenderer mr;
	private MeshFilter mf;
	private Mesh m;
	private GameObject groupGO;

	//cube
	private GameObject newGameObject;
	private MeshRenderer meshRenderer;
	private MeshFilter meshFilter;
	private Mesh mesh;
	private MeshCollider meshCollider;

	public Mesh tempMesh;

	public Material planeMaterial;
    public Material nonTwoSidedMaterial;

	// Use this for initialization
	void Start () 
	{
		ftpClient = new FTPUtility(@"server name", "username", "password");
		groupGO = new GameObject ("GroupObject");
		groupGO.transform.position = Vector3.zero;
        DownloadFile();
	}

	public void DownloadFile()
	{

		//downloads the file from the FTP server and stores it at default location
		string location = Application.persistentDataPath + fileName;
		ftpClient.download(fileName, @location);

		LoadFile ();
	}

	//loads the data from the file.
	void LoadFile()
	{
		if (File.Exists (Application.persistentDataPath + fileName)) {

			BinaryFormatter bf = new BinaryFormatter ();
			FileStream saveFile = File.Open (Application.persistentDataPath + fileName, FileMode.Open);
			WorldSaveData worldData = new WorldSaveData ();
			worldData = (WorldSaveData)bf.Deserialize (saveFile);
			saveFile.Close ();

			//freeform data
			if (worldData.freeformPointLocationList.Count > 0)
			{
                GameObject newObject = null;

				for (int i = 0; i < worldData.freeformPointLocationList.Count; i++)
				{
                    //plane code (should be replaced with polygon code from LevelEd VR eventually
					FreeformListV tempVectorListToCreate = new FreeformListV();
                    FreeformListV tempVectorList = new FreeformListV();
                    freeformMarkerPosition.Add (tempVectorList);
					for (int j = 0; j < worldData.freeformPointLocationList[i].freeformList.Count; j++) 
					{
						PositionV pos = new PositionV (worldData.freeformPointLocationList[i].freeformList[j].x, worldData.freeformPointLocationList[i].freeformList[j].y, worldData.freeformPointLocationList[i].freeformList[j].z);
						freeformMarkerPosition[i].freeformList.Add (pos);
                        tempVectorListToCreate.freeformList.Add(pos);

                    }

                    newObject = new GameObject(worldData.freeformPointLocationList[i].objectTag);
                    newObject.transform.position = new Vector3(tempVectorListToCreate.freeformList[0].x, tempVectorListToCreate.freeformList[0].y, tempVectorListToCreate.freeformList[0].z);
                    GenerateFreeformPoints(tempVectorListToCreate, newObject);
                    tempVectorList.freeformList.Clear();

                    //polygon cap code
                    FreeformListV tempPolycapVectorList = new FreeformListV();
                    freeformMarkerPosition.Add(tempPolycapVectorList);
                    int startListPosition = worldData.freeformPointLocationList[i].freeformList.Count - (worldData.freeformPointLocationList[i].freeformList.Count / 2);
                    Vector2[] polygonTopVertices = new Vector2[startListPosition];
                    int polygonTopVerticesIndex = 0;

                    for (int j = startListPosition; j < worldData.freeformPointLocationList[i].freeformList.Count; j++)
                    {
                        polygonTopVertices[polygonTopVerticesIndex] = new Vector2(worldData.freeformPointLocationList[i].freeformList[j].x, worldData.freeformPointLocationList[i].freeformList[j].z);

                        polygonTopVerticesIndex++;
                    }

                    GeneratePolygonCaps(polygonTopVertices, new Vector3(worldData.freeformPointLocationList[i].freeformList[startListPosition].x, worldData.freeformPointLocationList[i].freeformList[startListPosition].y, worldData.freeformPointLocationList[i].freeformList[0].z), newObject);

                }
                   
            }

			//continuous data
			if (worldData.continuousPointLocationList.Count > 0)
			{
				for (int i = 0; i < worldData.continuousPointLocationList.Count; i++)
				{
                    ContinuousListV tempVectorListToCreate = new ContinuousListV();
                    ContinuousListV tempVectorList = new ContinuousListV();
					continuousMarkerPosition.Add (tempVectorList);
					for (int j = 0; j < worldData.continuousPointLocationList[i].continuousList.Count; j++) 
					{
						PositionV pos = new PositionV (worldData.continuousPointLocationList[i].continuousList[j].x, worldData.continuousPointLocationList[i].continuousList[j].y, worldData.continuousPointLocationList[i].continuousList[j].z);
						continuousMarkerPosition[i].continuousList.Add (pos);
                        tempVectorListToCreate.continuousList.Add(pos);

                    }
                    GameObject newObject = new GameObject(worldData.continuousPointLocationList[i].wallTag);
                    newObject.transform.position = new Vector3(tempVectorListToCreate.continuousList[0].x, tempVectorListToCreate.continuousList[0].y, tempVectorListToCreate.continuousList[0].z);
                    GenerateContinuousPoints(tempVectorListToCreate,newObject);
                    tempVectorListToCreate.continuousList.Clear();
                }

				
			}

		}
	}

    void GenerateFreeformPoints(FreeformListV freeformListVertices, GameObject parentObject)
	{
		int vertexCounter = 0;

			for (int j = 0; j < freeformListVertices.freeformList.Count; j++) 
			{
				vertexCounter++;
				newMarkerRef = (GameObject)Instantiate (markerPrefab, new Vector3 (freeformListVertices.freeformList[j].x, freeformListVertices.freeformList[j].y, freeformListVertices.freeformList[j].z), markerPrefab.transform.rotation);
				newMarkerRef.transform.SetParent (parentObject.transform);

				int halfwayPoint = freeformListVertices.freeformList.Count / 2;

                bool bShouldInvert = false;

                //checks to see if the mesh has become inverted
                float negativeXCheck = freeformListVertices.freeformList[0].x - freeformListVertices.freeformList[1].x;
                float negativeZCheck = freeformListVertices.freeformList[0].z - freeformListVertices.freeformList[1].z;

                if (Mathf.Sign(negativeXCheck) == 1)
                {
                    bShouldInvert = true;
                }

                else if (Mathf.Sign(negativeZCheck) == -1)
                {
                    bShouldInvert = true;
                }

				if (j < halfwayPoint - 1)
				{
                    //call create plane and pass it the last 4 vertices iterated upon
					CreatePlane (
						new Vector3 (freeformListVertices.freeformList [j].x, freeformListVertices.freeformList [j].y, freeformListVertices.freeformList [j].z), 
						new Vector3 (freeformListVertices.freeformList [j + 1].x, freeformListVertices.freeformList [j + 1].y, freeformListVertices.freeformList [j + 1].z), 
						new Vector3 (freeformListVertices.freeformList [halfwayPoint + j].x, freeformListVertices.freeformList [halfwayPoint + j].y, freeformListVertices.freeformList [halfwayPoint + j].z), 
						new Vector3 (freeformListVertices.freeformList [halfwayPoint + j + 1].x, freeformListVertices.freeformList [halfwayPoint + j + 1].y, freeformListVertices.freeformList [halfwayPoint + j + 1].z), 
						true, 
                        nonTwoSidedMaterial,
                        bShouldInvert,
                        false,
                        parentObject);
				} 
				else if(j == freeformListVertices.freeformList.Count / 2)
				{
					//this is used to create the final plane as it is based on the first markers and the last markers.
					int listCount = freeformListVertices.freeformList.Count - 1;

					CreatePlane (
						new Vector3 (freeformListVertices.freeformList [listCount - halfwayPoint].x, freeformListVertices.freeformList [listCount - halfwayPoint].y, freeformListVertices.freeformList [listCount - halfwayPoint].z), 
						new Vector3 (freeformListVertices.freeformList [0].x, freeformListVertices.freeformList [0].y, freeformListVertices.freeformList [0].z), 
						new Vector3 (freeformListVertices.freeformList [listCount].x, freeformListVertices.freeformList [listCount].y, freeformListVertices.freeformList [listCount].z), 
						new Vector3 (freeformListVertices.freeformList [halfwayPoint].x, freeformListVertices.freeformList [halfwayPoint].y, freeformListVertices.freeformList [halfwayPoint].z), 
						true, 
                        nonTwoSidedMaterial,
                        bShouldInvert,
                        false,
                        parentObject);
				}
			}
		
	}

	void GenerateContinuousPoints(ContinuousListV continuousListVertices, GameObject parentObject)
	{
		int vertexCounter = 0;

		for (int j = 0; j < continuousListVertices.continuousList.Count; j++) 
		{
			vertexCounter++;
			newMarkerRef = (GameObject)Instantiate (markerPrefab, new Vector3 (continuousListVertices.continuousList[j].x, continuousListVertices.continuousList[j].y, continuousListVertices.continuousList[j].z), markerPrefab.transform.rotation);
			newMarkerRef.transform.SetParent (parentObject.transform);

			int halfwayPoint = continuousListVertices.continuousList.Count / 2;

            bool bShouldInvert = false;

            //checks to see if the mesh has become inverted
            float negativeXCheck = continuousListVertices.continuousList[0].x - continuousListVertices.continuousList[1].x;
            float negativeZCheck = continuousListVertices.continuousList[0].z - continuousListVertices.continuousList[1].z;

            if (Mathf.Sign(negativeXCheck) == 1)
            {
                bShouldInvert = true;
            }

            else if (Mathf.Sign(negativeZCheck) == -1)
            {
                bShouldInvert = true;
            }


			if (j < halfwayPoint - 1)
			{
				//call create plane and pass it the last 4 vertices iterated upon
				CreatePlane (
					new Vector3 (continuousListVertices.continuousList [j].x, continuousListVertices.continuousList [j].y, continuousListVertices.continuousList [j].z), 
					new Vector3 (continuousListVertices.continuousList [j + 1].x, continuousListVertices.continuousList [j + 1].y, continuousListVertices.continuousList [j + 1].z), 
					new Vector3 (continuousListVertices.continuousList [halfwayPoint + j].x, continuousListVertices.continuousList [halfwayPoint + j].y, continuousListVertices.continuousList [halfwayPoint + j].z), 
					new Vector3 (continuousListVertices.continuousList [halfwayPoint + j + 1].x, continuousListVertices.continuousList [halfwayPoint + j + 1].y, continuousListVertices.continuousList [halfwayPoint + j + 1].z), 
					true, 
                    planeMaterial,
                    true,true, parentObject);
			} 
		}
		
	}


    //Adapted from http://wiki.unity3d.com/index.php?title=Triangulator
    void GeneratePolygonCaps(Vector2[] newVertices2d, Vector3 startPosition, GameObject parentObject)
    {
        go = new GameObject("PolygonCap");
        go.transform.position = startPosition;

        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(newVertices2d);
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[newVertices2d.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(newVertices2d[i].x + (0 - go.transform.position.x), 0, newVertices2d[i].y + (0 - go.transform.position.z));
        }

        // Create the mesh
        Mesh msh = new Mesh();
        msh.vertices = vertices;
        msh.triangles = indices;
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        // Set up game object with mesh;
        MeshRenderer renderer = go.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        renderer.material = nonTwoSidedMaterial;
        MeshFilter filter = go.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;

        go.transform.parent = parentObject.transform;
    }



    /// <summary>
    /// Creates a plane based on the vertices passed from the data file.
    /// </summary>
    private GameObject CreatePlane(Vector3 bottomRight, Vector3 bottomLeft, Vector3 topRight, Vector3 topLeft, bool bAddCollider, Material mat, bool bFlip, bool bPlane, GameObject parentObject)
	{
		go = new GameObject("Plane");

		//changes default position of new plane from 0,0,0 to the position of bottom right corner.
		go.transform.position = bottomRight;
		mf = go.AddComponent(typeof(MeshFilter)) as MeshFilter;
		mr = go.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

		m = new Mesh();

		//mesh pivot points are created at 0,0,0 by default. Mesh position needs shifting so it matches the pivot.
		Vector3 bottomRightAdjusted = bottomRight - go.transform.position;
		Vector3 bottomLeftAdjusted = bottomLeft - go.transform.position;
		Vector3 topRightAdjusted = topRight - go.transform.position;
		Vector3 topLeftAdjusted = topLeft - go.transform.position;

        //creates vertices for the plane. 
        if (bPlane)
        {
            m.vertices = new Vector3[]
            {
                bottomRightAdjusted,
                topRightAdjusted,
                topLeftAdjusted,
                bottomLeftAdjusted
            };
        }
        else
        {
            m.vertices = new Vector3[]
            {
                bottomLeftAdjusted,
                topLeftAdjusted,
                topRightAdjusted,
                bottomRightAdjusted
            };
        }

		m.uv = new Vector2[]
		{
			new Vector2(0,0),
            new Vector2(0,Vector3.Distance(bottomLeftAdjusted, topLeftAdjusted)),
            new Vector2(Vector3.Distance(topLeftAdjusted, topRightAdjusted),Vector3.Distance(bottomRightAdjusted, topRightAdjusted)),
            new Vector2(Vector3.Distance(bottomLeftAdjusted, bottomRightAdjusted),0)
		};

        //defines the order of vertices for the plane
        m.triangles = new int[]
        {
            0,1,2,0,2,3
        };

        mr.material = mat;


        if (bFlip)
        {
            m = ReverseNormals(m);
        }

        m.RecalculateBounds();
        m.RecalculateNormals();
        mf.mesh = m;

        if (bAddCollider)
        {
            (go.AddComponent(typeof(MeshCollider)) as MeshCollider).sharedMesh = m;
        }

		go.transform.SetParent (parentObject.transform);

		return go;

	}

	public GameObject CreateCube(Material mat)
	{
		newGameObject = new GameObject("Cube");
		meshFilter = newGameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
		meshRenderer = newGameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

		mesh = new Mesh();

		meshRenderer.material = mat;
		Color c = meshRenderer.material.color;
		c.a = 0.5f;
		meshRenderer.material.color = c;

		return newGameObject;

	}

	public void UpdateCube(Vector3 startLocation, float width, float height, float depth, bool bAddCollider)
	{
		mesh.vertices = new Vector3[]
		{
			//front
			new Vector3(startLocation.x, startLocation.y, startLocation.z), //left top front 0
			new Vector3(width, startLocation.y, startLocation.z), //right top front 1
			new Vector3(startLocation.x, height, startLocation.z), //left bottom front 2
			new Vector3(width, height, startLocation.z), //right bottom front 3

			//right
			new Vector3(startLocation.x, startLocation.y, startLocation.z),
			new Vector3(startLocation.x, startLocation.y, depth),
			new Vector3(startLocation.x, height, startLocation.z),
			new Vector3(startLocation.x, height, depth), 

			//bottom
			new Vector3(startLocation.x, startLocation.y, startLocation.z),
			new Vector3(width, startLocation.y, startLocation.z),
			new Vector3(startLocation.x, startLocation.y, depth),
			new Vector3(width, startLocation.y, depth), 

			//top
			new Vector3(startLocation.x, height, startLocation.z),
			new Vector3(width, height, startLocation.z),
			new Vector3(startLocation.x, height, depth),
			new Vector3(width, height, depth),

			//left
			new Vector3(width, startLocation.y, startLocation.z),
			new Vector3(width, height, startLocation.z),
			new Vector3(width, startLocation.y, depth),
			new Vector3(width, height, depth),

			//back
			new Vector3(startLocation.x, startLocation.y, depth),
			new Vector3(width, startLocation.y, depth),
			new Vector3(startLocation.x, height, depth),
			new Vector3(width, height, depth)
		};

		mesh.uv = new Vector2[]
		{
			new Vector2(0,0),
			new Vector2(0,1),
			new Vector2(1,1),
			new Vector2(1,0),

			new Vector2(0,0),
			new Vector2(0,1),
			new Vector2(1,1),
			new Vector2(1,0),

			new Vector2(0,0),
			new Vector2(0,1),
			new Vector2(1,1),
			new Vector2(1,0),

			new Vector2(0,0),
			new Vector2(0,1),
			new Vector2(1,1),
			new Vector2(1,0),

			new Vector2(0,0),
			new Vector2(0,1),
			new Vector2(1,1),
			new Vector2(1,0),

			new Vector2(0,0),
			new Vector2(0,1),
			new Vector2(1,1),
			new Vector2(1,0),
		};

		mesh.triangles = new int[]
		{
			//front
			2,1,0,
			1,2,3,

			//right
			4,5,6,
			5,7,6,

			//bottom
			8,9,11,
			10,8,11,

			//top
			15,13,12,
			12,14,15,

			//left
			16,17,19,
			19,18,16,

			//back
			20,21,23,
			23,22,20
		};

		//checks to see if the mesh has become inverted
		float negativeXCheck = startLocation.x - width;
		float negativeYCheck = startLocation.y - height;
		float negativeZCheck = startLocation.z - depth;

		if (Mathf.Sign(negativeXCheck) == -1)
		{
			print("X: " + negativeXCheck);
			mesh = ReverseNormals(mesh);
		}

		if (Mathf.Sign(negativeYCheck) == -1)
		{
			print("Y: " + negativeYCheck);
			mesh = ReverseNormals(mesh);
		}

		if (Mathf.Sign(negativeZCheck) == 1)
		{
			print("Z: " + negativeZCheck);
			mesh = ReverseNormals(mesh);
		}

		meshFilter.mesh = mesh;

		mesh.RecalculateBounds();
		mesh.RecalculateNormals();

	}

	//takes the normals of the mesh and reverses them. Stops mesh going inverted.
	//http://wiki.unity3d.com/index.php/ReverseNormals
	private Mesh ReverseNormals(Mesh mesh)
	{
		Vector3[] normals = mesh.normals;
		for (int i = 0; i < normals.Length; i++)
			normals[i] = -normals[i];

		mesh.normals = normals;

		for (int m = 0; m < mesh.subMeshCount; m++)
		{
			int[] triangles = mesh.GetTriangles(m);
			for (int i = 0; i < triangles.Length; i += 3)
			{
				int temp = triangles[i + 0];
				triangles[i + 0] = triangles[i + 1];
				triangles[i + 1] = temp;
			}
			mesh.SetTriangles(triangles, m);
		}

		return mesh;
	}

	public void SetMaterialOpaque()
	{
		Color c = meshRenderer.material.color;
		c.a = 1f;
		meshRenderer.material.color = c;
	}

	//Add collision mesh once cube is completed.
	public void AddCollisionMesh()
	{
		(newGameObject.AddComponent(typeof(MeshCollider)) as MeshCollider).sharedMesh = mesh;
		meshCollider = newGameObject.GetComponent<MeshCollider>();
		meshCollider.convex = true;
	}

}
