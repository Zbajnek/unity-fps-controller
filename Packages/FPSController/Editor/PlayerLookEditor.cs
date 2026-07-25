using System;
using Headbob;
using Player;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(PlayerLook))]
    public sealed class PlayerLookEditor : UnityEditor.Editor
    {
        private SerializedProperty _smoothedLook;
        private SerializedProperty _lookSmoothTime;

        private SerializedProperty _useHeadbob;
        private SerializedProperty _headbobType;
        private SerializedProperty _headbob;

        private void OnEnable()
        {
            _smoothedLook = serializedObject.FindProperty("smoothedLook");
            _lookSmoothTime = serializedObject.FindProperty("lookSmoothTime");
            
            _useHeadbob = serializedObject.FindProperty("useHeadbob");
            _headbobType = serializedObject.FindProperty("headbobType");
            _headbob = serializedObject.FindProperty("headbob");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawPropertiesExcluding(serializedObject, "smoothedLook", "lookSmoothTime", "useHeadbob", "headbobType", "headbob");
            
            EditorGUILayout.PropertyField(_smoothedLook);

            if (_smoothedLook.boolValue)
            {
                EditorGUILayout.PropertyField(_lookSmoothTime);
            }
            
            EditorGUILayout.PropertyField(_useHeadbob);

            if (_useHeadbob.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_headbobType);
                
                HeadbobType newType = (HeadbobType)_headbobType.enumValueIndex;
                if (EditorGUI.EndChangeCheck() || _headbob.managedReferenceValue == null)
                {
                    _headbob.managedReferenceValue = CreateHeadbobInstance(newType);
                }
                
                EditorGUILayout.PropertyField(_headbob, new GUIContent($"Headbob ({newType.ToString()})"), true);
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private static BaseHeadbob CreateHeadbobInstance(HeadbobType type)
        {
            return type switch
            {
                HeadbobType.Simple => new SimpleHeadbob(),
                HeadbobType.Realistic => new RealisticHeadbob(),
                _ => null
            };
        }
    }
}