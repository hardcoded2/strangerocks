﻿using transfluent;
using UnityEngine;
using GUILayout = transfluent.guiwrapper.GUILayout;

public class Sample1_OnGUIWrapped : MonoBehaviour
{
	private bool languageSelectToggle = true;

	public void OnGUI()
	{
		GUILayout.Label("options");

		//This is just to show that you can switch langauges too!
		languageSelectToggle = GUILayout.Toggle(languageSelectToggle, "Language Select");
		if(languageSelectToggle)
		{
			if(GUILayout.Button("English"))
				TranslationUtility.changeStaticInstanceConfig("en-us");
			if(GUILayout.Button("French"))
				TranslationUtility.changeStaticInstanceConfig("fr-fr");
			if(GUILayout.Button("German"))
				TranslationUtility.changeStaticInstanceConfig("de-de");
			if(GUILayout.Button("Backwards (test language)"))
				TranslationUtility.changeStaticInstanceConfig("xx-xx");
		}
	}
}