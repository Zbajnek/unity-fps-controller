using Headbob;
using Player;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(PlayerLook))]
    public sealed class PlayerLookEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
        
            var property = serializedObject.GetIterator();
            property.NextVisible(true); // Skip m_Script
            
            while (property.NextVisible(false))
            {
                if (property.name == "smoothedLook")
                {
                    EditorGUILayout.PropertyField(property);
                    
                    if (property.boolValue)
                    {
                        property.NextVisible(false); // lookSmoothTime
                        EditorGUILayout.PropertyField(property);
                    }
                    else
                    {
                        property.NextVisible(false); // Skip lookSmoothTime
                    }
                }
                else if (property.name == "useHeadbob")
                {
                    EditorGUILayout.PropertyField(property);
                    
                    if (property.boolValue)
                    {
                        property.NextVisible(false); // headbobType
                        
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(property);
                        
                        HeadbobType newType = (HeadbobType)property.enumValueIndex;
                        
                        property.NextVisible(false); // headbob
                        
                        if (EditorGUI.EndChangeCheck() || property.managedReferenceValue == null)
                        {
                            property.managedReferenceValue = CreateHeadbobInstance(newType);
                        }
                        
                        EditorGUILayout.PropertyField(property, new GUIContent($"Headbob ({newType.ToString()})"), true);
                    }
                    else
                    {
                        property.NextVisible(false); // Skip headbobType
                        property.NextVisible(false); // Skip headbob
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(property, true);
                }
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