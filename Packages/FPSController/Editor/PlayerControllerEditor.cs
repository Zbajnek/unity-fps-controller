using Scripts.Player;
using UnityEditor;

namespace Editor
{
    [CustomEditor(typeof(PlayerController))]
    public sealed class PlayerControllerEditor : UnityEditor.Editor
    {
        private SerializedProperty _smoothedMovement;
        private SerializedProperty _moveSmoothTime;
        private SerializedProperty _moveSpeedSmoothTime;
        
        private void OnEnable()
        {
            _smoothedMovement = serializedObject.FindProperty("smoothedMovement");
            _moveSmoothTime = serializedObject.FindProperty("moveSmoothTime");
            _moveSpeedSmoothTime = serializedObject.FindProperty("moveSpeedSmoothTime");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawPropertiesExcluding(serializedObject, "smoothedMovement", "moveSmoothTime", "moveSpeedSmoothTime");
            
            EditorGUILayout.PropertyField(_smoothedMovement);
            
            if (_smoothedMovement.boolValue)
            {
                EditorGUILayout.PropertyField(_moveSmoothTime);
                EditorGUILayout.PropertyField(_moveSpeedSmoothTime);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}