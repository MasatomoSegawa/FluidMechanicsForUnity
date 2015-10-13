using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * プレハブをリソースから生成するシングルトンクラス.
 */ 
public class ResourceManager : Singleton<ResourceManager>
{
	private const string path = "PrefabResources";
	Object[] PrefabResources;

	// Use this for initialization
	void Awake ()
	{
		PrefabResources = Resources.LoadAll (path);

	}

	public void PrintObjects(){

		foreach (GameObject obj in PrefabResources) {
			Debug.Log (obj.name);
		}

	}

	public GameObject InstantiateResourceWithName(string name){

		foreach (GameObject obj in PrefabResources) {
			if (obj.name == name) {
				return Instantiate (obj)as GameObject;
			}
		}

		Debug.Log(name + "'s name don't exist");
		return null;
	}

	public GameObject GetPrefab(string name){

		foreach (GameObject obj in PrefabResources) {
			if (obj.name == name) {
				return obj;
			}
		}

		Debug.Log(name + "'s name don't exist");
		return null;
	}

	public GameObject InstantiateResourceWithTag(string tag){

		foreach (GameObject obj in PrefabResources) {
			if (obj.tag == tag) {
				return Instantiate (obj)as GameObject;
			}
		}

		Debug.Log(name + "'s Tag don't exist");
		return null;
	}

}
