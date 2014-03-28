﻿using System.Collections.Generic;
using UnityEngine;

namespace transfluent
{
	public class TransfluentUtility
	{
		//TODO: keep sets of language/group and allow for explict load/unload statements
		//the implication of that is that any ongui/other client would need to declare set groups for their activiites in some way
		private static TransfluentUtilityInstance _instance = new TransfluentUtilityInstance();

		private static LanguageList _LanguageList;

		private TransfluentUtility()
		{
			changeStaticInstanceConfig(); //load default translation group info
		}

		public static TransfluentUtilityInstance getUtilityInstanceForDebugging()
		{
			return _instance;
		}

		//convert into a factory?
		public static bool changeStaticInstanceConfig(string destinationLanguageCode = "", string translationGroup = "")
		{
			TransfluentUtilityInstance tmpInstance = createNewInstance(destinationLanguageCode, translationGroup);
			if(tmpInstance != null)
			{
				_instance = tmpInstance;
				return true;
			}
			return false;
		}

		public static TransfluentUtilityInstance createNewInstance(string destinationLanguageCode = "", string group = "")
		{
			if(_LanguageList == null)
			{
				_LanguageList = ResourceLoadFacade.getLanguageList();
			}

			if(_LanguageList == null)
			{
				Debug.LogError("Could not load new language list");
				return null;
			}

			TransfluentLanguage dest = _LanguageList.getLangaugeByCode(destinationLanguageCode);
			GameTranslationSet destLangDB = GameTranslationGetter.GetTranslaitonSetFromLanguageCode(destinationLanguageCode);
			Dictionary<string, string> keysInLanguageForGroupSpecified = destLangDB != null
				? destLangDB.getGroup(group).getDictionaryCopy()
				: new Dictionary<string, string>();
			return new TransfluentUtilityInstance
			{
				allKnownTranslations = keysInLanguageForGroupSpecified,
				destinationLanguage = dest,
				groupBeingShown = group
			};
		}

		public static string get(string sourceText)
		{
			return _instance.getTranslation(sourceText);
		}

		//same format as string.format for now, not tokenized
		//ie "Hi, my name is {0}" instead of "Hi, my name is $NAME" or some other scheme
		public static string getFormatted(string sourceText, params object[] formatStrings)
		{
			return _instance.getFormattedTranslation(sourceText, formatStrings);
		}
	}

	//an interface for handling translaitons
	public class TransfluentUtilityInstance
	{
		public Dictionary<string, string> allKnownTranslations;

		public TransfluentLanguage destinationLanguage { get; set; }

		public string groupBeingShown { get; set; }

		public void setNewDestinationLanguage(Dictionary<string, string> transaltionsInSet)
		{
			allKnownTranslations.Clear();
			allKnownTranslations = transaltionsInSet;
		}

		//same format as string.format for now, not tokenized
		//ie "Hi, my name is {0}" instead of "Hi, my name is $NAME" or some other scheme
		public string getFormattedTranslation(string sourceText, params object[] formatStrings)
		{
			return string.Format(getTranslation(sourceText), formatStrings);
		}

		public string getTranslation(string sourceText)
		{
			if(allKnownTranslations != null && allKnownTranslations.ContainsKey(sourceText))
			{
				return allKnownTranslations[sourceText];
			}
			return sourceText;
		}
	}
}