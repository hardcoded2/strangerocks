using transfluent;
using UnityEngine;
using System.Collections;

public class GlobalizeTextMesh : MonoBehaviour
{
	public string globalizationKey;
	public string stringValue;

	public bool textIsManagedExternally;  //if someone else is managing this

	public TextMesh textmesh; //gets set in editor

	public void OnLocalize()
	{
		if(textIsManagedExternally) return;
		textmesh.text = TransfluentUtility.get(globalizationKey);
	}
}
