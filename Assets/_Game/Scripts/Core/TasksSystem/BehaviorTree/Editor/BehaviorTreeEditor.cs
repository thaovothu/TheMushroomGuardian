using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(BehaviorTree))]
public class BehaviorTreeEditor : Editor
{
    bool showData = true;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        BehaviorTree behaviorTree= (BehaviorTree)target;
        EditorGUILayout.Space(15f);

        if (GUILayout.Button("Open Viewer"))
        {
            BehaviorTreeViewer.ShowWindow();
        }

        EditorGUILayout.Space(15f);
        showData = EditorGUILayout.Foldout(showData, "Data");
        if (showData)
        {
            EditorGUI.indentLevel++;
            DrawData(behaviorTree.Data);
            EditorGUI.indentLevel--;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(behaviorTree);
        }
    }

    void DrawData(BehaviorTreeData data)
    {
        GUI.enabled = false;

        // foreach (var v in data.NPCList.List)
        // {
        //     object valObj = v.Value;
        //     UnityEngine.Object uobj = valObj as UnityEngine.Object;
        //     if (uobj != null)
        //     {
        //         EditorGUILayout.ObjectField(v.Name, uobj, typeof(UnityEngine.Object), true);
        //     }
        //     else
        //     {
        //         EditorGUILayout.LabelField(v.Name, v.Value != null ? v.Value.ToString() : "null");
        //     }
        // }

        foreach (var v in data.FloatList.List)
        {
            EditorGUILayout.FloatField(v.Name, v.Value);
        }

        foreach (var v in data.BoolList.List)
        {
            EditorGUILayout.Toggle(v.Name, v.Value);
        }

        foreach (var v in data.StringList.List)
        {
            EditorGUILayout.TextField(v.Name, v.Value);
        }
        foreach (var v in data.Vector3List.List)
        {
            EditorGUILayout.Vector3Field(v.Name, v.Value);
        }

        foreach (var v in data.Vector2List.List)
        {
            EditorGUILayout.Vector2Field(v.Name, v.Value);
        }
        foreach (var v in data.TransformList.List)
        {
            EditorGUILayout.ObjectField(v.Name, v.Value, typeof(Transform), true);
        }
        foreach (var v in data.GameObjectList.List)
        {
            EditorGUILayout.ObjectField(v.Name, v.Value, typeof(GameObject), true);
        }
        GUI.enabled = true;
    }
}

#endif