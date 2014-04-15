﻿using System;
using transfluent;
using UnityEngine;

public class GlobalizeTextMesh : MonoBehaviour
{
	public bool textIsManagedExternally;  //if someone else is managing this

	public TextMesh textmesh; //gets set in editor

	public LocalizeUtil localizableText = new LocalizeUtil();

	public void OnLocalize()
	{
		if(textIsManagedExternally) return;
		localizableText.OnLocalize();
		textmesh.text = localizableText.current;
	}
	
#if UNITY_EDITOR
	public void OnValidate()
	{
		textmesh.text = localizableText.current;  //make sure to update the textmesh
	}
#endif

	public void Start()
	{
		textmesh.text = localizableText.current;
	}
}

//TODO: Change the name
[Serializable]
public class LocalizeUtil
{
	public string globalizationKey;
	//[SerializeField]
	private string _current;
	
	/*
	public LocalizeUtil(){}
	public LocalizeUtil(string key)
	{
		globalizationKey = key;
	}
	*/
	public string current
	{
		get
		{
			if(string.IsNullOrEmpty(globalizationKey))
				return "";
			return _current ?? (_current = TranslationUtility.get(globalizationKey));
		}
		//set { globalizationKey = _current; }
	}

	public string OnLocalize()
	{
		_current = null;
		return current;
	}

	public static implicit operator string(LocalizeUtil util)
	{
		return util.current;
	}
	/*public static implicit operator LocalizeUtil(string globalizationKey)
	{
		return new LocalizeUtil(globalizationKey);
	}*/
	public override string ToString()
	{
		return current;
	}
}
