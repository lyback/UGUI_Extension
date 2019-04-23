using System;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public class EditorHelper
{
    #region Excute Base
    public static void ExcuteAsset(string title, List<AssetObject> list, params Action<AssetObject>[] actions)
    {
        EditorUtility.DisplayProgressBar(title, "Please Wait......", 0);
        ClearConsole();
        AssetDatabase.Refresh();
        for (int i = 0; i < list.Count; i++)
        {
            var source = list[i];
            StringBuilder sb = new StringBuilder();
            sb.Append(source.name).Append("(").Append(i + 1).Append("/").Append(list.Count).Append(")");
            EditorUtility.DisplayProgressBar(title, sb.ToString(), i * 1.0f / list.Count);
            foreach (var action in actions)
            {
                action(source);
            }
        }
        EditorUtility.ClearProgressBar();
    }



    public static void ExcuteAssetOperat(Action<Object> action, SelectionMode Mode = SelectionMode.Deep)
    {
        ClearConsole();
        Object[] items = Selection.GetFiltered(typeof(Object), Mode);
        foreach (var item in items)
        {
            Debug.Log(item);
            action(item);
        }
        AssetDatabase.Refresh();
        Debug.Log("----------------Finish----------------");
    }
    #endregion

    #region Other

    public static void FindGameObjectInCurrentScene(Action<GameObject> action)
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] arrGO = scene.GetRootGameObjects();
        if (arrGO != null && arrGO.Length > 0)
        {
            for (int i = 0; i < arrGO.Length; i++)
            {
                action(arrGO[i]);
            }
        }
    }
    #endregion
    public static void ClearConsole()
    {
        // var logEntries = Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
        // System.Diagnostics.Debug.Assert(logEntries != null, "logEntries != null");
        // var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        // clearMethod.Invoke(null, null);
        var assembly = Assembly.GetAssembly(typeof(SceneView));
        Type logEntries = assembly.GetType("UnityEditor.LogEntries");
        var clearConsoleMethod = logEntries.GetMethod("Clear");
        clearConsoleMethod.Invoke(new object(), null);
    }
}

