﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace transfluent.editor
{
	//TODO: remove static funcitons
	public class DownloadAllGameTranslations
	{
		// I don't know if I am going to expose this, but it is something to do
		//maybe as sub-functionality on the scriptableobject?  push/pull on the object itself?

		[MenuItem("Transfluent/Download All Transfluent data")]
		public static void doDownload()
		{
			TransfluentEditorWindowMediator mediator = getAuthenticatedMediator();
			if(mediator == null) return;

			List<string> allLanguageCodes = mediator.getAllLanguageCodes();
			downloadTranslationSetsFromLanguageCodeList(allLanguageCodes);
		}

		private static TransfluentEditorWindowMediator getAuthenticatedMediator()
		{
			var mediator = new TransfluentEditorWindowMediator();
			KeyValuePair<string, string> usernamePassword = mediator.getUserNamePassword();
			if(String.IsNullOrEmpty(usernamePassword.Key) || String.IsNullOrEmpty(usernamePassword.Value))
			{
				EditorUtility.DisplayDialog("Login please",
					"Please login using editor window before trying to use this functionality", "ok");
				TransfluentEditorWindow.Init();
				return null;
			}
			mediator.doAuth(usernamePassword.Key, usernamePassword.Value);
			return mediator;
		}

		public static void uploadTranslationSet(List<string> languageCodes, string groupid)
		{
			TransfluentEditorWindowMediator mediator = getAuthenticatedMediator();
			if(mediator == null) return;

			foreach(string languageCode in languageCodes)
			{
				try
				{
					GameTranslationSet set = GameTranslationGetter.GetTranslaitonSetFromLanguageCode(languageCode);
					var groupData = set.getGroup(groupid);
					var lang = ResourceLoadFacade.getLanguageList().getLangaugeByCode(languageCode);
					if(groupData.translations.Count > 0)
					{
						mediator.SaveGroupToServer(groupData, lang);
					}
				}
				catch
				{
				}
			}
		}

		public static void downloadTranslationSetsFromLanguageCodeList(List<string> languageCodes, string groupid = null)
		{
			TransfluentEditorWindowMediator mediator = getAuthenticatedMediator();
			if(mediator == null) return;

			foreach(string languageCode in languageCodes)
			{
				try
				{
					mediator.setCurrentLanguageFromLanguageCode(languageCode);
					TransfluentLanguage currentlanguage = mediator.GetCurrentLanguage();

					List<TransfluentTranslation> translations = mediator.knownTextEntries(groupid);
					//Debug.Log("CURRENT LANGUAGE:" + currentlanguage.code + " translation count:" + translations.Count);
					if(translations.Count > 0)
					{
						GameTranslationSet set = GameTranslationGetter.GetTranslaitonSetFromLanguageCode(languageCode) ??
												 ResourceCreator.CreateSO<GameTranslationSet>(
													 GameTranslationGetter.fileNameFromLanguageCode(languageCode));

						set.language = currentlanguage;
						var groupToTranslationMap = groupidToDictionaryMap(translations);
						foreach(KeyValuePair<string, Dictionary<string, string>> kvp in groupToTranslationMap)
						{
							set.mergeInSet(kvp.Key, kvp.Value);
						}

						EditorUtility.SetDirty(set);
						AssetDatabase.SaveAssets();
					}
				}
				catch(Exception e)
				{
					Debug.LogError("error while downloading translations:" + e.Message + " stack:" + e.StackTrace);
				}
			}
		}

		public static Dictionary<string, Dictionary<string, string>> groupidToDictionaryMap(List<TransfluentTranslation> translations)
		{
			var map = new Dictionary<string, Dictionary<string, string>>();
			foreach(TransfluentTranslation translation in translations)
			{
				string group = translation.group_id ?? "";
				if(!map.ContainsKey(group))
				{
					map.Add(group, new Dictionary<string, string>());
				}

				var dic = map[group];
				dic.Add(translation.text_id, translation.text);
			}
			return map;
		}
	}
}