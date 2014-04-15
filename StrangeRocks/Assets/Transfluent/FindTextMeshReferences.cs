using System;
using System.Collections.Generic;
using System.IO;
using strange.examples.strangerocks;
using UnityEditor;
using UnityEngine;

public class FindTextMeshReferences : MonoBehaviour
{
	private static readonly List<string> blacklistStringsContaining = new List<string>
	{
		"XXXX",
	};
	//something that returns a mesh[]
	//TODO: reflection based solution?
	private static List<TextMesh> toExplicitlyIgnore(GameObject inPrefab = null)
	{
		//ignore all textmeshes referenced by all ButtonView components
		var listToIngore = new List<TextMesh>();

		//custom references -- TODO: replace with a reflection based solution
		//find gameobjects with [SerializeField] private or public vars and also define an OnLocalize

		var allButtons = new List<ButtonView>();
		if (inPrefab == null)
		{
			allButtons.AddRange(FindObjectsOfType<ButtonView>());
		}
		else
		{
			allButtons.AddRange(inPrefab.GetComponentsInChildren<ButtonView>(true));
		}
		allButtons.ForEach((ButtonView button) =>
		{
			if(button != null && button.labelMesh != null)
				listToIngore.Add(button.labelMesh);
		});

		var allMeshesInSource = new List<TextMesh>();
		if (inPrefab == null)
		{
			allMeshesInSource.AddRange(FindObjectsOfType<TextMesh>());
		}
		else
		{
			allMeshesInSource.AddRange(inPrefab.GetComponentsInChildren<TextMesh>(true));
		}

		foreach (TextMesh mesh in allMeshesInSource)
		{
			if(!shouldGlobalizeText(mesh.text))
			{
				listToIngore.Add(mesh);
			}
		}
		return listToIngore;
	}


	private static bool shouldGlobalizeText(string textIn)
	{
		foreach (string blacklist in blacklistStringsContaining)
		{
			if (textIn.Contains(blacklist))
				return false;
		}
		return true;
	}

	[MenuItem("Helpers/All of the above")]
	public static void UpdateReferences()
	{
		GetTextMeshReferencesFromPrefabs();
		var scene = EditorApplication.currentScene.Replace("Assets/","");
		Application.LoadLevel(scene);
		GetTextMeshReferences();
	}

	//NOTE you *must* be in the source language for this to not cause corruption issues!
	[MenuItem("Helpers/TestMesh In Current Scene")]
	public static TextMesh[] GetTextMeshReferences()
	{
		var meshes = FindObjectsOfType(typeof (TextMesh)) as TextMesh[];
		foreach (TextMesh mesh in meshes)
		{
			setTextMesh(mesh);
		}
		AssetDatabase.SaveAssets();
		return meshes;
	}

	[MenuItem("Helpers/TestMesh In All Scene map")]
	public static void GetTextMeshReferencesInScenes()
	{
		var scenePathToReferenceList = new Dictionary<string, TextMesh[]>();
		
		string[] sceneFiles = Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories);
		foreach (string scene in sceneFiles)
		{
			Debug.Log("Looking at scene file:" + scene);
			EditorApplication.OpenScene(scene);
			var textMeshes = GetTextMeshReferences();
			scenePathToReferenceList.Add(scene,textMeshes);
			
			foreach (TextMesh mesh in textMeshes)
			{
				Debug.Log("Externally lookin at text mesh named:" + mesh.gameObject.name);
			}
		}
		
	}

	private static void setTextMesh(TextMesh mesh)
	{
		var translatable = mesh.GetComponent<GlobalizeTextMesh>();
		if (!shouldGlobalizeText(mesh.text))
		{
			if (translatable != null)
			{
				DestroyImmediate(translatable);
			}
			return; //TODO: possibly allow for XXXX to be transalted to a field in a formatted text field?
		}
		
		if (translatable == null)
		{
			translatable = mesh.gameObject.AddComponent<GlobalizeTextMesh>();
			translatable.globalizationKey = mesh.text; //just use whatever the source text is upfront, and allow the user to 
		}

		translatable.textmesh = mesh;

		//should this be reversed?
		translatable.stringValue = mesh.text; //?????????????????????????????????????
	}

	public static List<GameObject> getAllPrefabReferences()
	{
		var retList = new List<GameObject>();
		string[] aMaterialFiles = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);
		foreach (string matFile in aMaterialFiles)
		{
			string assetPath = "Assets" + matFile.Replace(Application.dataPath, "").Replace('\\', '/');
			var go = (GameObject) AssetDatabase.LoadAssetAtPath(assetPath, typeof (GameObject));

			retList.Add(go);
		}
		return retList;
	}

	[MenuItem("Helpers/Textmeshes in prefabs")]
	public static void GetTextMeshReferencesFromPrefabs()
	{
		//var assets = AssetDatabase.LoadAllAssetsAtPath("Assets") as Object[];
		List<GameObject> assets = getAllPrefabReferences();

		Debug.Log("Assets:" + assets.Count);

		foreach (GameObject go in assets)
		{
			//Debug.Log("looking at path:" + AssetDatabase.GetAssetPath(go));
			if (go == null)
				continue;
			//Debug.Log("looking at go:" + go.gameObject);
			TextMesh[] textMeshSubObjects = go.GetComponentsInChildren<TextMesh>(true);
			if (textMeshSubObjects == null || textMeshSubObjects.Length == 0) continue;

			Debug.Log("gameobject has meshes:" + go.gameObject);
			foreach (TextMesh mesh in textMeshSubObjects)	
			{
				setTextMesh(mesh);
			}

			EditorUtility.SetDirty(go);
			EditorApplication.SaveAssets();
		}

		/*
		
		foreach (Object asset in assets)
		{
			GameObject go = asset as GameObject;
			Debug.Log("looking at path:"+ AssetDatabase.GetAssetPath(go));
			if (go == null)
				continue;
			Debug.Log("looking at go:" + go.gameObject);
			TextMesh[] textMeshSubObjects = go.GetComponentsInChildren<TextMesh>(true);
			if (textMeshSubObjects == null || textMeshSubObjects.Length == 0) continue;

			Debug.Log("gameobject has meshes:"+go.gameObject);
			foreach(TextMesh mesh in textMeshSubObjects)
			{
				setTextMesh(mesh);
			}

			EditorUtility.SetDirty(go);

			//PrefabUtility.SetPropertyModifications(go,);
		}
		 */
		AssetDatabase.SaveAssets();
		//then go through instances?
	}
}