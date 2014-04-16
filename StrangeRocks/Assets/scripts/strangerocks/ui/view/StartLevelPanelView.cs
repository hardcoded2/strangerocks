using System;
using strange.extensions.mediation.impl;
using UnityEngine;
using strange.extensions.signal.impl;

namespace strange.examples.strangerocks.ui
{
	public class StartLevelPanelView : View
	{
		[Inject]
		public IScreenUtil screenUtil{ get; set; }

		public ButtonView translationOptions;
		public GameObject translationOptionsPanelParent;

		public ButtonView startButton;
		public TextMesh level_field;
		public ScreenAnchor horizontalAnchor = ScreenAnchor.CENTER_HORIZONTAL;
		public ScreenAnchor verticalAnchor = ScreenAnchor.CENTER_VERTICAL;

		internal Signal proceedSignal = new Signal ();

		internal void Init()
		{
			startButton.releaseSignal.AddListener (onStartClick);
			translationOptions.releaseSignal.AddListener(onLanguageSelect);

			transform.localPosition = screenUtil.GetAnchorPosition(horizontalAnchor, verticalAnchor);
		}

		void onLanguageSelect()
		{
			translationOptionsPanelParent.transform.localPosition = Vector3.zero;
			translationOptionsPanelParent.SetActive(true);
			
			gameObject.SetActive(false);
		}
		
		internal void SetLevel(int value)
		{
			level_field.text = value.ToString ();
		}

		private void onStartClick()
		{
			proceedSignal.Dispatch ();
		}
	}
}

