using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FixPrefabLink : EditorWindow
{
	public enum InputType
	{
		Parent, // A single prefab.
		SingleObject // A parent with many child objects that are all instances of the same prefab.
	}
    
	[Tooltip("What the input object is, single = single prefab, parent = parent object where all children are instances of the same prefab. Hover over each field's label for more info!")]
	public InputType inputType = InputType.Parent;
	public Transform input;
	public GameObject prefab;
	public Transform outputParent;
    
	private static List<int> _childIndices;

	[MenuItem("Tools/Fix Lost Prefab Link")]
	public static void ShowWindow()
	{
		GetWindow<FixPrefabLink>("Fix Lost Prefab Link");
	}
    
	private void OnGUI()
	{
		string inputTooltip = inputType == InputType.Parent
			? "The input transform of the parent to multiple prefabs"
			: "The input transform of a single prefab";
		
		EditorGUILayout.HelpBox("This tool takes an input Game Object in the currently open scene and restores it's prefab link as a new object", MessageType.Info);
		EditorGUILayout.Space();
		inputType = (InputType)EditorGUILayout.EnumPopup(new GUIContent("Input Type", "Input Type Explanations:\n" +
			"1. Parent - Input will be a parent containing multiple children that are all instances of the same prefab (that has lost it's link)\n" +
			"2. SingleObject - Input will be a single prefab (that has lost it's link)"), inputType);
		input = (Transform)EditorGUILayout.ObjectField(new GUIContent("Input", inputTooltip), input, typeof(Transform), true);
		prefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Prefab", "The prefab that the input should be relinked to"), prefab, typeof(GameObject), true);
		outputParent = (Transform)EditorGUILayout.ObjectField(new GUIContent("Output Parent", "The parent that the new linked prefabs will be instantiated on, can be null for no parent"), outputParent, typeof(Transform), true);
		EditorGUILayout.Space();
		if (GUILayout.Button("Fix Prefabs"))
		{
			FixPrefab();
		}
	}
    
	public void FixPrefab()
	{
		if (inputType == InputType.SingleObject && input.childCount != prefab.transform.childCount ||
		    inputType == InputType.Parent && input.GetChild(0).childCount != prefab.transform.childCount)
		{
			throw new IndexOutOfRangeException("The child structure of the prefab and the input prefab(s) does not match");
		}
        
		switch (inputType)
		{
			case InputType.SingleObject:
				RestorePrefabLink(input, prefab, outputParent);
				break;
            
			case InputType.Parent:
				foreach (Transform tf in input)
				{
					RestorePrefabLink(tf, prefab, outputParent);
				}
				break;
            
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private static void RestorePrefabLink(Transform obj, GameObject targetPrefab, Transform parent = null)
	{
		_childIndices = new List<int>();
		GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(targetPrefab, SceneManager.GetActiveScene());
		prefabInstance.transform.parent = parent;
		IterateObject(obj);
        
		void IterateObject(Transform tf)
		{
			if (tf.childCount > 0)
			{
				_childIndices.Add(-1);
				for (int i = 0; i < tf.childCount; i++)
				{
					_childIndices[_childIndices.Count - 1] = i;
					IterateObject(tf.GetChild(i));
				}
				_childIndices.RemoveAt(_childIndices.Count - 1);
			}
			Transform currentTarget = prefabInstance.transform;
			Transform currentSource = tf;
			foreach (int index in _childIndices)
			{
				currentTarget = currentTarget.GetChild(index);
			}
            
			CopyValues(currentSource, currentTarget);

			void CopyValues(Transform source, Transform target)
			{
				target.localPosition = source.localPosition;
				target.localRotation = source.localRotation;
				target.localScale = source.localScale;
			}
		}
	}
}