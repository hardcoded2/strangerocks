using System;
using strange.examples.strangerocks;
using strange.examples.strangerocks.ui;
using transfluent;
using UnityEngine;
using System.Collections.Generic;

public class TranslationSelectView : MonoBehaviour
{
	[Serializable]
	public class LanguageToButtonMap
	{
		public string languageName;
		public ButtonView button;

		public void setButtonListener()
		{
			button.releaseSignal.AddListener(setLanguage);
		}

		private void setLanguage()
		{
			TranslationUtility.changeStaticInstanceConfig(languageName);
		}
	}

	public List<LanguageToButtonMap> languageMaps = new List<LanguageToButtonMap>();
	public ButtonView backButton;
	public StartLevelPanelView startLevelView;

	public void Start()
	{
		backButton.releaseSignal.AddListener(() =>
		{
			Debug.Log("Back button");
			startLevelView.gameObject.SetActive(true);
			gameObject.SetActive(false);
		});
		foreach (LanguageToButtonMap map in languageMaps)
		{
			map.setButtonListener();
		}
	}
}
