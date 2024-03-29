﻿#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object = UnityEngine.Object;

namespace GameEngine.Library.Editor
{
    /// <summary>
    /// Unity's refusing to maintain EditorUtility.SetDirty, so for the times where you need it over
    /// Undo or SerializableObject, this is how to actually dirty objects.
    /// </summary>
    public static class GEEditorFix
    {
        public static void SetObjectDirty(Object o)
        {
            if (Application.isPlaying)
                return;

            if (o is GameObject)
            {
                SetObjectDirty((GameObject)o);
            }
            else if (o is Component)
            {
                SetObjectDirty((Component)o);
            }
            else
            {
                EditorUtility.SetDirty(o);
            }
        }

        public static void SetObjectDirty(GameObject go)
        {
            if (Application.isPlaying)
                return;

            HandlePrefabInstance(go);
            EditorUtility.SetDirty(go);

            //This stopped happening in EditorUtility.SetDirty after multi-scene editing was introduced.
            EditorSceneManager.MarkSceneDirty(go.scene);
        }

        public static void SetObjectDirty(Component comp)
        {
            if (Application.isPlaying)
                return;

            HandlePrefabInstance(comp.gameObject);
            EditorUtility.SetDirty(comp);

            //This stopped happening in EditorUtility.SetDirty after multi-scene editing was introduced.
            EditorSceneManager.MarkSceneDirty(comp.gameObject.scene);
        }

        // Some prefab overrides are not handled by Undo.RecordObject or EditorUtility.SetDirty.
        // eg. adding an item to an array/list on a prefab instance updates that the instance
        // has a different array count than the prefab, but not any data about the added thing
        private static void HandlePrefabInstance(GameObject gameObject)
        {
            var prefabType = PrefabUtility.GetPrefabAssetType(gameObject);
            if (prefabType == PrefabAssetType.Regular || prefabType == PrefabAssetType.Variant)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
            }
        }
    }
}
#endif