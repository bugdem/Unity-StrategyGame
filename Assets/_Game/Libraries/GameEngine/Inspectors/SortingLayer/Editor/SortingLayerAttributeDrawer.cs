using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;

namespace GameEngine.Game.Core 
{
	[CustomPropertyDrawer(typeof(SortingLayerAttribute))]
	public class SortingLayerDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType != SerializedPropertyType.Integer)
				return;

			EditorGUI.LabelField(position, label);

			position.x += EditorGUIUtility.labelWidth;
			position.width -= EditorGUIUtility.labelWidth;

			string[] sortingLayerNames = GetSortingLayerNames();
			int[] sortingLayerIDs = GetSortingLayerIDs();

			int sortingLayerIndex = Mathf.Max(0, System.Array.IndexOf(sortingLayerIDs, property.intValue));
			sortingLayerIndex = EditorGUI.Popup(position, sortingLayerIndex, sortingLayerNames);
			property.intValue = sortingLayerIDs[sortingLayerIndex];
		}

		private string[] GetSortingLayerNames()
		{
			System.Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty(
					"sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
			return (string[])sortingLayersProperty.GetValue(null, new object[0]);
		}

		private int[] GetSortingLayerIDs()
		{
			System.Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty(
					"sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
			return (int[])sortingLayersProperty.GetValue(null, new object[0]);
		}

	} }