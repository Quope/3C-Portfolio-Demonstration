#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;
using Object = UnityEngine.Object;

[CanEditMultipleObjects]
[CustomEditor(typeof(Object), true)]
public class UniversalButtonEditor : Editor
{
    static readonly Type[] unityTypesToIgnore = new Type[]
    {
        typeof(Transform), typeof(RectTransform), typeof(Camera), typeof(Light), typeof(MeshFilter),
        typeof(MeshRenderer), typeof(SkinnedMeshRenderer), typeof(Canvas), typeof(CanvasRenderer),
        typeof(SpriteRenderer), typeof(Animator), typeof(AudioSource), typeof(AudioListener)
    };

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var targetObject = target;
        var type = targetObject.GetType();

        // Ignore les types Unity de base pour éviter de polluer l'UI
        foreach (var t in unityTypesToIgnore)
            if (t == type) return;

        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var method in methods)
        {
            var attrs = method.GetCustomAttributes(typeof(ButtonAttribute), true);
            if (attrs.Length > 0 && method.GetParameters().Length == 0)
            {
                if (GUILayout.Button(method.Name))
                {
                    method.Invoke(targetObject, null);
                }
            }
        }
    }
}
#endif