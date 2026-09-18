using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DoorSceneTransition))]
public sealed class DoorSceneTransitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var path = serializedObject.FindProperty("destinationScenePath");
        var current = AssetDatabase.LoadAssetAtPath<SceneAsset>(path.stringValue);
        EditorGUI.BeginChangeCheck();
        var selected = (SceneAsset)EditorGUILayout.ObjectField(
            new GUIContent("이동할 Scene"), current, typeof(SceneAsset), false);
        if (EditorGUI.EndChangeCheck())
        {
            path.stringValue = selected == null ? string.Empty : AssetDatabase.GetAssetPath(selected);
            if (selected != null)
            {
                var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
                var entry = scenes.Find(scene => scene.path == path.stringValue);
                if (entry == null)
                    scenes.Add(new EditorBuildSettingsScene(path.stringValue, true));
                else
                    entry.enabled = true;
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }

        EditorGUILayout.HelpBox("문을 가리키고 트리거를 누르면 지정한 Scene으로 이동합니다. Scene을 지정하면 Build Settings에도 활성 등록됩니다.", MessageType.Info);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("leftController"), new GUIContent("왼손 컨트롤러 (선택)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rightController"), new GUIContent("오른손 컨트롤러 (선택)"));
        EditorGUILayout.HelpBox("컨트롤러를 둘 다 비우면 현재 문을 가리키는 XR 입력 인터랙터의 Activate 입력을 사용합니다.", MessageType.None);
        serializedObject.ApplyModifiedProperties();
    }
}
