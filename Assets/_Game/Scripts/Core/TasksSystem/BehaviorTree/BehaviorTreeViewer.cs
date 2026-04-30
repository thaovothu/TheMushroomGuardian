using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BehaviorTreeViewer : EditorWindow
{
    private Vector2 scrollPosition;
    BehaviorTree behaviorTree;
    Task Root;
    int depth = 0;
    float indentSize = 20f;
    [MenuItem("Task System/ Behavior Tree Viewer")]
    public static void ShowWindow()
    {
        GetWindow<BehaviorTreeViewer>("Behavior Tree Viewer");
    }
    private void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChanged;
        EditorApplication.update += OnUpdate;
    }
    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
        EditorApplication.update -= OnUpdate;
    }
    void OnSelectionChanged()
    {
        if (Selection.activeGameObject == null)
        {
            return;
        }

        BehaviorTree bt = Selection.activeGameObject.GetComponent<BehaviorTree>();

        if(bt == null)
        {
            bt = Selection.activeGameObject.GetComponentInChildren<BehaviorTree>();
        }

        if (bt != null)
        {
            behaviorTree = bt;
            Root = bt.Root;
        }
    }

    void OnUpdate()
    {
        if (Root == null)
        {
            OnSelectionChanged();
        }

        Repaint();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        depth = 0;
    
        if (Root != null)
        {
            if (behaviorTree != null)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Behavior Tree (Restarts: {behaviorTree.Restarts})");
                GUILayout.EndHorizontal(); 
            }
            DrawTask(Root);
        }
        else
        {
            EditorGUILayout.LabelField("Select a BehaviorTree Obj to begin");
        }

        EditorGUILayout.EndScrollView();
    }

    Color GetTaskTextColor(Task task)
    {
        Color targetColor = task.CompareStatus(TaskStatus.Running) ? Color.green : GUI.skin.label.normal.textColor;
    
        if (task.Status == TaskStatus.Failure)
        {
            targetColor = Color.red;
        }
        else if (task.Status == TaskStatus.Success)
        {
            targetColor = Color.cyan;
        }
        return targetColor;
    }
    void DrawUtilityLabel(Task task)
    {
        Color oldColor = GUI.color;
        GUI.color = Color.yellow;

        string s = $"[Utility= {Mathf.Round(task.GetUtility() * 10.0f)/10.0f}]";
        EditorGUILayout.LabelField(s);
        GUI.color = oldColor;
    }

    void DrawTask(Task task, bool drawUtility = false)
    {
        Color oldColor = GUI.color;
        GUI.color = GetTaskTextColor(task);

        if (task is Composite)
        {
            DrawComposite((Composite)task, drawUtility);
        }
        else if (task is Decorator)
        {
            DrawDecorator((Decorator)task, drawUtility);
        }
        else
        {
            BeginTask(task);
            EditorGUILayout.LabelField(task.FullName);

            if (drawUtility)
            {
                DrawUtilityLabel(task);
            }
            EndTask(task);
        }
        GUI.color = oldColor;
    }

    void BeginTask(Task task)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(indentSize * (depth + 1));
    }

    void EndTask(Task task)
    {
        GUILayout.EndHorizontal();

        float y = GUILayoutUtility.GetLastRect().y;

        Color color = task.CompareStatus(TaskStatus.Running) ? Color.green : new Color(0.2f,0.2f,0.2f);
        Rect rect = new Rect(0, y, indentSize, EditorGUIUtility.singleLineHeight);

        EditorGUI.DrawRect(rect, color);

        if (task == Root)
        {
            return;
        }

        float arrowThickness = 1f;
        float arrowLength = (indentSize + arrowThickness)/2f;

        float x = indentSize * depth + indentSize * 0.5f - arrowThickness/2f;
        EditorGUI.DrawRect(rect, GUI.skin.label.normal.textColor);

        y += indentSize * 0.5f - arrowThickness;

        rect = new Rect(x,y,arrowLength - arrowThickness, arrowThickness);
        EditorGUI.DrawRect(rect, GUI.skin.label.normal.textColor);
    }

    void DrawDecorator( Decorator task, bool drawUtility = false)
    {
        BeginTask(task);

        task.ViewerShowFoldout = EditorGUILayout.Foldout(task.ViewerShowFoldout, task.FullName);

        if (drawUtility)
        {
            DrawUtilityLabel(task);
        }

        EndTask(task);

        depth++;

        if (task.ViewerShowFoldout)
        {
            DrawTask(task.Child);
        }
        depth--;

    }

    void DrawComposite(Composite task, bool drawUtility = false)
    {
        bool isUtilitySelector = task is UtilitySelector;
        BeginTask(task);

        task.ViewerShowFoldout = EditorGUILayout.Foldout(task.ViewerShowFoldout, task.FullName);

        if (drawUtility)
        {
            DrawUtilityLabel(task);
        }

        EndTask(task);

        depth++;

        if (task.ViewerShowFoldout)
        {
            foreach (Task childTask in task.Tasks)
            {
                DrawTask(childTask, isUtilitySelector);
            }
        }
        depth--;

    }
}
