using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Sztorm.CylinderCollider
{
    [CustomEditor(typeof(CylinderCollider)), CanEditMultipleObjects]
    internal sealed class CylinderColliderEditor : Editor
    {
        private static readonly MethodInfo GenerateCollidersMethod = typeof(CylinderCollider)
            .GetMethod("GenerateColliders", BindingFlags.NonPublic | BindingFlags.Instance);

        public override void OnInspectorGUI()
        {
            bool prevGuiState = GUI.enabled;

            if (Application.isPlaying)
            {
                GUI.enabled = false;
            }
            DrawDefaultInspector();
            CylinderCollider unboxedTarget = (CylinderCollider)target; 

            if (GUILayout.Button("Generate Collider"))
            {
                GenerateCollidersMethod.Invoke(unboxedTarget, Array.Empty<object>());
            }
            GUI.enabled = prevGuiState;
        }
    }
}
