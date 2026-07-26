using Player;
using UnityEditor;

namespace Editor
{
    [CustomEditor(typeof(PlayerController))]
    public sealed class PlayerControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            var property = serializedObject.GetIterator();
            property.NextVisible(true);

            while (property.NextVisible(false))
            {
                if (property.name == "smoothedMovement")
                {
                    EditorGUILayout.PropertyField(property);

                    if (property.boolValue)
                    {
                        property.NextVisible(false); // moveSmoothTime
                        EditorGUILayout.PropertyField(property);
                        property.NextVisible(false); // moveSpeedSmoothTime
                        EditorGUILayout.PropertyField(property);
                    }
                    else
                    {
                        property.NextVisible(false); // Skip moveSmoothTime
                        property.NextVisible(false); // Skip moveSpeedSmoothTime
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(property, true);
                }
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}