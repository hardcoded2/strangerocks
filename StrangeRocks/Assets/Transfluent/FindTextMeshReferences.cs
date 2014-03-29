using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FindTextMeshReferences : MonoBehaviour
{
	private static readonly List<string> blacklistStringsContaining = new List<string>
	{
		"XXXX",
	};

	private static bool shouldGlobalizeText(string textIn)
	{
		foreach (string blacklist in blacklistStringsContaining)
		{
			if (textIn.Contains(blacklist))
				return false;
		}
		return true;
	}

	//NOTE you *must* be in the source language for this to not cause corruption issues!
	[MenuItem("Helpers/TestMesh In Current Scene")]
	public static void GetTextMeshReferences()
	{
		var meshes = FindObjectsOfType(typeof (TextMesh)) as TextMesh[];
		foreach (TextMesh mesh in meshes)
		{
			setTextMesh(mesh);
		}
	}

	private static void setTextMesh(TextMesh mesh)
	{
		if (!shouldGlobalizeText(mesh.text))
			return; //TODO: possibly allow for XXXX to be transalted to a field in a formatted text field?

		var translatable = mesh.GetComponent<GlobalizeTextMesh>();
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
			// .. do whatever you like
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
			Debug.Log("looking at path:" + AssetDatabase.GetAssetPath(go));
			if (go == null)
				continue;
			Debug.Log("looking at go:" + go.gameObject);
			TextMesh[] textMeshSubObjects = go.GetComponentsInChildren<TextMesh>(true);
			if (textMeshSubObjects == null || textMeshSubObjects.Length == 0) continue;

			Debug.Log("gameobject has meshes:" + go.gameObject);
			foreach (TextMesh mesh in textMeshSubObjects)
			{
				setTextMesh(mesh);
			}

			EditorUtility.SetDirty(go);
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