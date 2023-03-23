using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARUtilities : MonoBehaviour {

	public static void Swap<T>(IList<T> list, int indexA, int indexB)
	{
		T tmp = list[indexA];
		list[indexA] = list[indexB];
		list[indexB] = tmp;
	}

}
