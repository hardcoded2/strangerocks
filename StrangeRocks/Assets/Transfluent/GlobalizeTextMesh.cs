using UnityEngine;
using System.Collections;

public class GlobalizeTextMesh : MonoBehaviour
{
	public string globalizationKey;
	public string stringValue;

	public bool doNotTranslate;

	public TextMesh textmesh; //gets set in editor

	public void Awake()
	{
		
	}

	public static void MakeSureAllTextMeshesHaveAGlobalizationComponent()
	{
		
	}
}
