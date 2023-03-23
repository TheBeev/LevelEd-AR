using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class SaveUpload : MonoBehaviour {

	private FTPUtility ftpClient;
	private string fileName;

	// Use this for initialization
	void Start () 
	{
		ftpClient = new FTPUtility(@"server name", "username", "password");
	}
	
	public void SaveFile(List<GameObject> pointList, List<GameObject> depthList, List<FreeformListG> freeformList, List<ContinuousListG> continuousList, string participantNumber, string testType)
	{

		fileName = "/P_" + participantNumber + "_" + testType + ".dat";

		BinaryFormatter bf = new BinaryFormatter ();

		FileStream saveFile = File.Create (Application.persistentDataPath + fileName);

		WorldSaveData data = new WorldSaveData ();

        if (testType == "scene")
        {
            if (freeformList != null)
            {
                for (int i = 0; i < freeformList.Count; i++)
                {
                    FreeformListV tempVectorList = new FreeformListV();
                    string objectTag = freeformList[i].objectTag;
                    data.freeformPointLocationList.Add (tempVectorList);
                    for (int j = 0; j < freeformList[i].freeformList.Count; j++) 
                    {
                        PositionV pos = new PositionV (freeformList[i].freeformList[j].transform.position.x, freeformList[i].freeformList[j].transform.position.y, freeformList[i].freeformList[j].transform.position.z);
                        data.freeformPointLocationList[i].freeformList.Add (pos);
                        data.freeformPointLocationList[i].objectTag = objectTag;
                    }
                }   
            }

            if (continuousList != null)
            {
                for (int i = 0; i < continuousList.Count; i++)
                {
                    ContinuousListV tempVectorList = new ContinuousListV();
                    string wallTag = continuousList[i].wallTag;
                    data.continuousPointLocationList.Add (tempVectorList);
                    for (int j = 0; j < continuousList[i].continuousGList.Count; j++) 
                    {
                        PositionV pos = new PositionV (continuousList[i].continuousGList[j].transform.position.x, continuousList[i].continuousGList[j].transform.position.y, continuousList[i].continuousGList[j].transform.position.z);
                        data.continuousPointLocationList[i].continuousList.Add(pos);
                        data.continuousPointLocationList[i].wallTag = wallTag;
                    }
                }   
            }
        }

        if (freeformList != null && testType == "Task2Box" || testType == "Task3BoxWall" || testType == "Task4Walls")
		{
			for (int i = 0; i < freeformList.Count; i++)
			{
				FreeformListV tempVectorList = new FreeformListV();
				data.freeformPointLocationList.Add (tempVectorList);
				for (int j = 0; j < freeformList[i].freeformList.Count; j++) 
				{
					PositionV pos = new PositionV (freeformList[i].freeformList[j].transform.position.x, freeformList[i].freeformList[j].transform.position.y, freeformList[i].freeformList[j].transform.position.z);
					data.freeformPointLocationList[i].freeformList.Add (pos);
				}
			}	
		}

		if (continuousList != null && testType == "Task4Walls" || testType == "Task1Plane")
		{
			for (int i = 0; i < continuousList.Count; i++)
			{
				ContinuousListV tempVectorList = new ContinuousListV();
				data.continuousPointLocationList.Add (tempVectorList);
				for (int j = 0; j < continuousList[i].continuousGList.Count; j++) 
				{
					PositionV pos = new PositionV (continuousList[i].continuousGList[j].transform.position.x, continuousList[i].continuousGList[j].transform.position.y, continuousList[i].continuousGList[j].transform.position.z);
					data.continuousPointLocationList[i].continuousList.Add(pos);
				}
			}	
		}


		bf.Serialize (saveFile, data);

		saveFile.Close ();

		UploadFile ();

	}

    public void SaveFileQuest(List<GameObject> pointList, List<GameObject> depthList, List<FreeformListG> freeformList, List<ContinuousListG> continuousList, string participantNumber, string testType, ControllerListT controllerLocations)
    {
        /*
        fileName = "/P_" + participantNumber + "_" + testType + ".dat";

        BinaryFormatter bf = new BinaryFormatter();

        FileStream saveFile = File.Create(Application.persistentDataPath + fileName);

        WorldSaveData data = new WorldSaveData();

        
        if (pointList != null && testType == "Task1Plane")
        {
            foreach (GameObject item in pointList)
            {
                PositionV pos = new PositionV (item.transform.position.x, item.transform.position.y, item.transform.position.z);
                data.pointLocationList.Add (pos);
            }
        }*/

        /*
        if (depthList != null)
        {
            foreach (GameObject item in depthList)
            {
                PositionV pos = new PositionV (item.transform.position.x, item.transform.position.y, item.transform.position.z);
                data.depthPointLocationList.Add (pos);
            }
        }

        if (testType == "scene")
        {
            if (freeformList != null)
            {
                for (int i = 0; i < freeformList.Count; i++)
                {
                    FreeformListV tempVectorList = new FreeformListV();
                    string objectTag = freeformList[i].objectTag;
                    data.freeformPointLocationList.Add(tempVectorList);
                    for (int j = 0; j < freeformList[i].freeformList.Count; j++)
                    {
                        PositionV pos = new PositionV(freeformList[i].freeformList[j].transform.position.x, freeformList[i].freeformList[j].transform.position.y, freeformList[i].freeformList[j].transform.position.z);
                        data.freeformPointLocationList[i].freeformList.Add(pos);
                        data.freeformPointLocationList[i].objectTag = objectTag;
                    }
                }
            }

            if (continuousList != null)
            {
                for (int i = 0; i < continuousList.Count; i++)
                {
                    ContinuousListV tempVectorList = new ContinuousListV();
                    string wallTag = continuousList[i].wallTag;
                    data.continuousPointLocationList.Add(tempVectorList);
                    for (int j = 0; j < continuousList[i].continuousGList.Count; j++)
                    {
                        PositionV pos = new PositionV(continuousList[i].continuousGList[j].transform.position.x, continuousList[i].continuousGList[j].transform.position.y, continuousList[i].continuousGList[j].transform.position.z);
                        data.continuousPointLocationList[i].continuousList.Add(pos);
                        data.continuousPointLocationList[i].wallTag = wallTag;
                    }
                }
            }
        }

        if (freeformList != null && testType == "Task2Box" || testType == "Task3BoxWall" || testType == "Task4Walls")
        {
            for (int i = 0; i < freeformList.Count; i++)
            {
                FreeformListV tempVectorList = new FreeformListV();
                data.freeformPointLocationList.Add(tempVectorList);
                for (int j = 0; j < freeformList[i].freeformList.Count; j++)
                {
                    PositionV pos = new PositionV(freeformList[i].freeformList[j].transform.position.x, freeformList[i].freeformList[j].transform.position.y, freeformList[i].freeformList[j].transform.position.z);
                    data.freeformPointLocationList[i].freeformList.Add(pos);
                }
            }
        }

        if (continuousList != null && testType == "Task4Walls" || testType == "Task1Plane")
        {
            for (int i = 0; i < continuousList.Count; i++)
            {
                ContinuousListV tempVectorList = new ContinuousListV();
                data.continuousPointLocationList.Add(tempVectorList);
                for (int j = 0; j < continuousList[i].continuousGList.Count; j++)
                {
                    PositionV pos = new PositionV(continuousList[i].continuousGList[j].transform.position.x, continuousList[i].continuousGList[j].transform.position.y, continuousList[i].continuousGList[j].transform.position.z);
                    data.continuousPointLocationList[i].continuousList.Add(pos);
                }
            }
        }

        if (controllerLocations != null)
        {
            data.controllerPositions.leftControllerLocation = controllerLocations.leftControllerLocation;
            data.controllerPositions.rightControllerLocation = controllerLocations.rightControllerLocation;
        }
        else
        {
            print("controllers are empty");
        }


        //we serialize the data class object to a file at the saveFile location.
        bf.Serialize(saveFile, data);

        //we close the file stream.
        saveFile.Close();

        UploadFile();
        */

    }

    void UploadFile()
	{
		print ("made it to UploadFile");
		string location = Application.persistentDataPath + fileName;
		ftpClient.upload(fileName, @location);
	}

}
