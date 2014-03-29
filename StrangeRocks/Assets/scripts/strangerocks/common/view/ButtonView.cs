//A basic button View
//Notice how we use TWO different mediators to read the clicks:
//1. ButtonMouseMediator
//   reads MouseClicks and is useful in the Editor or for web/desktop
//2. ButtonTouchMediator
//   reads Touches and is useful on devices.

//Look at UIContext to see how we map the appropriate Mediator for the given
//platform.

using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using transfluent;
using UnityEngine;

namespace strange.examples.strangerocks
{
	public class ButtonView : View
	{
		public GameObject background;
		public string label;
		public TextMesh labelMesh;

		public Color normalColor = Color.red;
		public Color overColor = Color.magenta;
		public Color pressColor = Color.black;
		public Signal pressSignal = new Signal();
		public Signal releaseSignal = new Signal();


		protected override void Start()
		{
			base.Start();
			var bc = gameObject.AddComponent<BoxCollider>();
			bc.center = Vector3.zero;
			Vector3 size = Vector3.one;
			size.x /= background.transform.localScale.x;
			size.y /= background.transform.localScale.y;
			size.z /= background.transform.localScale.z;

			bc.size = background.transform.localScale;

			OnLocalize();
		}

		public void OnLocalize()
		{
			if (labelMesh != null)
			{
				Debug.Log(string.Format("label {0} translation: {1}", label, TransfluentUtility.get(label)));
				labelMesh.text = TransfluentUtility.get(label);
			}
		}

		internal void pressBegan()
		{
			pressSignal.Dispatch();
			background.renderer.material.color = pressColor;
		}

		internal void pressEnded()
		{
			releaseSignal.Dispatch();
			background.renderer.material.color = normalColor;
		}
	}
}