#define TRANSFLUENT_EXAMPLE
#if TRANSFLUENT_EXAMPLE
using strange.examples.strangerocks;
#endif //!TRANSFLUENT_EXAMPLE
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace transfluent
{
	public class GameSpecificMigration : MonoBehaviour
	{
		public static readonly List<string> blacklistStringsContaining = new List<string>
		{
			"XXXX",
		};
		public interface IGameProcessor
		{
			void process(GameObject go, CustomScriptProcessorState processorState);
		}
		public class CustomScriptProcessorState
		{
			private List<GameObject> _blackList;
			private ITranslationUtilityInstance _translationDB;

			public CustomScriptProcessorState(List<GameObject> blackList, ITranslationUtilityInstance translationDb)
			{
				_blackList = blackList;
				_translationDB = translationDb;
			}

			public void addToBlacklist(GameObject go)
			{
				if(go != null && _blackList.Contains(go) == false)
				{
					_blackList.Add(go);
				}
			}

			public void addToDB(string key, string value)
			{
				string currentGroup = _translationDB.groupBeingShown;
				TranslationConfigurationSO config = ResourceLoadFacade.LoadConfigGroup(_translationDB.groupBeingShown);
				var translationDictionary = _translationDB.allKnownTranslations;
				GameTranslationSet gameTranslationSet =
					GameTranslationGetter.GetTranslaitonSetFromLanguageCode(config.sourceLanguage.code);

				bool exists = translationDictionary.ContainsKey(key);
				if(!exists)
				{
					translationDictionary.Add(key, key);
				}

				gameTranslationSet.mergeInSet(currentGroup, translationDictionary);
				//_translationDB.allKnownTranslations.Add(key,value);
			}
		}

		public class ButtonViewProcessor : IGameProcessor
		{
			public void process(GameObject go,CustomScriptProcessorState processorState)
			{
				var button = go.GetComponent<ButtonView>();
				if(button == null) return;
				if(button.labelMesh != null)
				{
					string newKey = button.label;
					button.labelData.globalizationKey = newKey;

					processorState.addToDB(newKey, newKey);
					processorState.addToBlacklist(go);

					//make sure the button gets saved properly when the scene is closed
					//custom script objects have to manually declare themselves as "dirty"
					EditorUtility.SetDirty(button);
				}
			}

		}
		public class TextMeshProcessor : IGameProcessor
		{
			public void process(GameObject go, CustomScriptProcessorState processorState)
			{
				var textMesh = go.GetComponent<TextMesh>();
				if(textMesh == null) return;

				string newKey = textMesh.text;
				processorState.addToDB(newKey, newKey);
				processorState.addToBlacklist(go);
				
				var translatable = textMesh.GetComponent<LocalizedTextMesh>();

				if(translatable == null)
				{
					translatable = textMesh.gameObject.AddComponent<LocalizedTextMesh>();
					translatable.textmesh = textMesh; //just use whatever the source text is upfront, and allow the user to
				}
				translatable.localizableText.globalizationKey = textMesh.text;
				//For textmesh specificially, this setDirty is not needed according to http://docs.unity3d.com/Documentation/ScriptReference/EditorUtility.SetDirty.html
				//EditorUtility.SetDirty(textMesh);
			}

		}
		
		//ignore all textmeshes referenced by all ButtonView components
		public static void toExplicitlyIgnore(List<TextMesh> toIgnore, GameObject inPrefab = null)
		{
#if TRANSFLUENT_EXAMPLE
			//or maybe just find this class with reflection?
			//custom references -- replace with a reflection based solution maybe?
			//   find gameobjects with [SerializeField] private or public vars and also define an OnLocalize

			var allButtons = new List<ButtonView>();
			if(inPrefab == null)
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
				{
					toIgnore.Add(button.labelMesh);
					string newKey = button.label;
					button.labelData.globalizationKey = newKey;

					FindTextMeshReferences.setKeyInDefaultLanguageDB(newKey, newKey);

					//TODO: ensure that this is set to the source language of the game config before adding
					EditorUtility.SetDirty(button);
				}
			});
#endif //!TRANSFLUENT_EXAMPLE
		}

		public static void toExplicitlyIgnore(List<GUIText> toIgnore, GameObject inPrefab = null)
		{

		}
	}
}