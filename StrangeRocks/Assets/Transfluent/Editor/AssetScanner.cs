﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using Object = UnityEngine.Object;

namespace transfluent
{
	public class AssetScanner
	{
		[MenuItem("Translation/testScan")]
		public static void testscan()
		{
			AssetScanner scanner = new AssetScanner();
			/*var gos = scanner.getAllGameObjectsInScene();
			foreach (var go in gos)
			{
				//Debug.Log("name:"+go.name);
			}*/
			//scanner.searchGameObjects(scanner.getAllGameObjectsInScene());

			string activeSelectionPath = AssetDatabase.GetAssetPath(Selection.activeObject);
			activeSelectionPath = "Assets/art/scenes/level_block.prefab";
			Debug.Log("Active selection path:" + activeSelectionPath);
			//scanner.searchPrefab(activeSelectionPath);
			//scanner.searchGameObjects();


			scanner.searchScenes();

			//scanner.searchPrefabs();
		}

		private readonly List<GameSpecificMigration.IGameProcessor> _gameProcessors =
			new List<GameSpecificMigration.IGameProcessor>();
		List<GameObject> toIgnore = new List<GameObject>();
		private readonly GameSpecificMigration.CustomScriptProcessorState _customProcessorState;
		public AssetScanner()
		{
			_customProcessorState = new GameSpecificMigration.CustomScriptProcessorState(toIgnore, TranslationUtility.getUtilityInstanceForDebugging());
			_gameProcessors.Add(new GameSpecificMigration.ButtonViewProcessor());
			_gameProcessors.Add(new GameSpecificMigration.TextMeshProcessor());
		}

		public void searchPrefabs()
		{
			string[] aMaterialFiles = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);
			foreach(string matFile in aMaterialFiles)
			{
				string assetPath = "Assets" + matFile.Replace(Application.dataPath, "").Replace('\\', '/');
				var go = (GameObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));

				searchPrefab(assetPath);
			}
		}

		public void searchScenes()
		{
			string[] sceneFiles = Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories);
			foreach (string scene in sceneFiles)
			{
				Debug.LogError("Looking at scene file:" + scene);
				EditorApplication.OpenScene(scene);

				searchGameObjects(getAllGameObjectsInScene());

				EditorApplication.SaveScene(scene);

				AssetDatabase.SaveAssets();
			}
		}

		public void searchPrefab(string prefabLocation)
		{
			string assetPath = prefabLocation.Replace(Application.dataPath, "").Replace('\\', '/');
			if(!prefabLocation.StartsWith("Assets"))
			{
				assetPath = "Assets/" + assetPath;
			}
			var prefab = (GameObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));

			if(prefab == null)
			{
				Debug.LogError("could not load prefab at path: "+assetPath + " input:"+prefabLocation);
			}

			//getAllGameObjectsInPrefab()

			var originalScene = EditorApplication.currentScene;
			//cheat with a scene in order to make sure to get disabled components.  Feels odd, but I hope I find something better later
			EditorApplication.NewScene();

			//delete the main camera and any other default objects
			var gameobjects = GameObject.FindObjectsOfType<GameObject>();
			foreach(var existingGo in gameobjects)
			{
				GameObject.DestroyImmediate(existingGo);
			}

			GameObject instanceOfPrefab = GameObject.Instantiate(prefab) as GameObject;

			var listOfGameobjectsInPrefab = getAllGameObjectsInScene();

			searchGameObjects(listOfGameobjectsInPrefab);

			PrefabUtility.ReplacePrefab(instanceOfPrefab, prefab);
			
			EditorUtility.SetDirty(instanceOfPrefab);
			AssetDatabase.SaveAssets();

			GameObject.DestroyImmediate(instanceOfPrefab);
			string scene = originalScene.Replace("Assets/", "");
			EditorApplication.OpenScene(scene); //go back to the scene we started with
		}

		public void searchGameObjects(List<GameObject> allGameObjectsInScene)
		{
			//loop through the processors in order (so that gameobjects not directly linked can get blacklisted)
			foreach(GameSpecificMigration.IGameProcessor processor in _gameProcessors)
			{
				foreach(GameObject go in allGameObjectsInScene)
				{
					if(go == null)
						continue;
					if(toIgnore.Contains(go))
						continue;
					Debug.Log("Looking at go:" + go.name);

					processor.process(go, _customProcessorState);
				}
			}
		}
		
#if false
		public List<GameObject> getAllGameObjectsInPrefab(GameObject prefab)
		{
			var originalScene = EditorApplication.currentScene;
			//cheat with a scene in order to make sure to get disabled components.  Feels odd, but I hope I find something better later
			EditorApplication.NewScene();

			//delete the main camera and any other default objects
			var gameobjects = GameObject.FindObjectsOfType<GameObject>();
			foreach(var existingGo in gameobjects)
			{
				GameObject.DestroyImmediate(existingGo);
			}

			GameObject go = GameObject.Instantiate(prefab) as GameObject;

			var listOfGameobjectsInPrefab = getAllGameObjectsInScene();
			PrefabUtility.ReplacePrefab(go,prefab)
			//GameObject.DestroyImmediate(go);

			string scene = originalScene.Replace("Assets/", "");
			//EditorApplication.OpenScene(scene); //go back to the scene we started with

			return listOfGameobjectsInPrefab;
		}
#endif
		public List<GameObject> getAllGameObjectsInPrefabOldWay(GameObject prefab)
		{
			Component[] allcomponents = prefab.GetComponentsInChildren<Component>(true);
			var allGameobjectsWithThing = new List<GameObject>();
			foreach(var comp in allcomponents)
			{
				if(!allGameobjectsWithThing.Contains(comp.gameObject))
				{
					allGameobjectsWithThing.Add(comp.gameObject);
				}
			}
			return allGameobjectsWithThing;
		}

		//finding disabled objects
		//http://docs.unity3d.com/Documentation/ScriptReference/Resources.FindObjectsOfTypeAll.html
		public List<GameObject> getAllGameObjectsInScene()
		{
			var objectsInScene = new List<GameObject>();

			foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof (GameObject)) as GameObject[])
			{
				//this filter seems to miss a few important things.  a bunch of primitives show up
				if(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave || 
					go.hideFlags == HideFlags.HideInHierarchy || go.hideFlags == HideFlags.HideInInspector)
					continue;
				
				
				string assetPath = AssetDatabase.GetAssetPath(go.transform.root.gameObject);
				if(!String.IsNullOrEmpty(assetPath))
					continue;

				objectsInScene.Add(go);
			}

			return objectsInScene;
		}
	}
}
