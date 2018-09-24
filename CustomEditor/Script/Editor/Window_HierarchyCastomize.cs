using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System.IO;
using System.Text;

public class Window_HierarchyCustomize : EditorWindow
{
    static List<string> opened = new List<string>();
    Object target;
    static List<string> fileList = new List<string>();
    

    enum Tab
    {
        Main,
        Option
    }

    Tab currentTab = Tab.Main;

    [MenuItem("Window/HierarchyCustomizer")]
    static public void Show()
    {
        EditorWindow.GetWindow(typeof(Window_HierarchyCustomize));
    }


    private void OnGUI()
    {
        bool was = CustomHierarchyData.On;
        CustomHierarchyData.On = EditorGUILayout.Toggle("Enable : ", CustomHierarchyData.On);
        if (CustomHierarchyData.On != was)
        {
            EditorApplication.RepaintHierarchyWindow();
        }
        if (GUILayout.Button("Load"))
        {
            CustomHierarchyData.Load();
        }
        if (GUILayout.Button("Save"))
        {
            CustomHierarchyData.Save();
        }

        GUILayout.BeginHorizontal();

        if(currentTab == Tab.Main)
        {
            
            if (GUILayout.Button("Main", new GUIStyle()
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            }, GUILayout.Width(100)))
            {
            }
            if (GUILayout.Button("Options", GUILayout.Width(100)))
            {
                currentTab = Tab.Option;
            }
        }
        else
        {
            if (GUILayout.Button("Main", GUILayout.Width(100)))
            {
                currentTab = Tab.Main;
            }
            if (GUILayout.Button("Options", new GUIStyle()
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            }, GUILayout.Width(100)))
            {
            }
        }

        GUILayout.EndHorizontal();
        
        if (currentTab == Tab.Main)
        {
            DrawMain();
        }
        else
        {
            DrawOptions();
        }
           
    }

    private void Update()
    {
        if (Selection.objects.Length > 0 && (target == null || Selection.objects[0].name != target.name))
        {
            target = Selection.objects[0];
            Repaint();
        }
        else if(Selection.objects.Length == 0)
        {
            target = null;
        }
    }

    private void OnInspectorUpdate()
    {
        fileList.Clear();
        if (Directory.Exists(CustomizeHierarchy.PathToFolder))
        {
            string[] files = Directory.GetFiles(CustomizeHierarchy.PathToFolder);
            for(int i = 0; i < files.Length; i++)
            {
                string[] p = files[i].Split('\\');
                if (files[i].Contains("meta"))
                    continue;
                fileList.Add(p[p.Length - 1]);
            }
        }
    }

    void DrawMain()
    {
        if (target)
        {
            if (CustomHierarchyData.Contains(target.name) == false)
            {
                if (GUILayout.Button("Add selected"))
                {
                    CustomHierarchyData.Add(target.name);
                }
            }
            else
            {
                if (GUILayout.Button("Remove selected"))
                {
                    CustomHierarchyData.Remove(target.name);
                }
            }
        }



        foreach (CustomHElement elem in CustomHierarchyData.list)
        {
            bool b = opened.Contains(elem.Name);

            b = EditorGUILayout.Foldout(b, elem.Name);
            if (b && opened.Contains(elem.Name) == false)
            {
                opened.Add(elem.Name);
            }
            else if (!b)
            {
                opened.Remove(elem.Name);
            }

            if (b)
            {
                EditorGUI.indentLevel++;
                Vector3 currentPos = GUILayoutUtility.GetLastRect().position;
                //EditorGUI.DrawRect(new Rect(currentPos.x, currentPos.y+16, EditorWindow.GetWindow(typeof(Window_HierarchyCustomize)).maxSize.x - EditorWindow.GetWindow(typeof(Window_HierarchyCustomize)).minSize.x, 76), Color.grey);
                elem.color = EditorGUILayout.ColorField("BGColor : ", elem.color);
                elem.fontColor = EditorGUILayout.ColorField("Font colot : ", elem.fontColor);
                elem.comment = EditorGUILayout.DelayedTextField("Comment: ", elem.comment);

                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    CustomHierarchyData.list.Remove(elem);
                    EditorApplication.RepaintHierarchyWindow();
                    return;
                }
                EditorGUI.indentLevel--;
            }
        }
    }

    void DrawOptions()
    {
        EditorGUILayout.BeginHorizontal();
        CustomizeHierarchy.PathToFolder = EditorGUILayout.TextField("Path to folders : ", CustomizeHierarchy.PathToFolder);
        if (GUILayout.Button("Default"))
        {
            CustomizeHierarchy.PathToFolder = "Assets\\CustomEditor\\styles";
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Label("Choosed style : " + CustomizeHierarchy.FileName);
        if (GUILayout.Button("Add new profile"))
        {
            string name = "Profile";
            if (Directory.Exists(CustomizeHierarchy.PathToFolder) == false)
            {
                Debug.LogError("Can't find directory for styles");
                return;
            }
            if (File.Exists(CustomizeHierarchy.PathToFolder + "/" + name + ".opt"))
            {
                int i = 1;
                name += "1";
                while (File.Exists(CustomizeHierarchy.PathToFolder + "/" + name + ".opt"))
                {
                    name = name.Replace(i.ToString(), (i + 1).ToString());
                    i++;
                }
            }
            File.Create(CustomizeHierarchy.PathToFolder + "/" + name + ".opt");
        }
        foreach (string file in fileList)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(file, GUILayout.Width(100)))
            {
                AssetDatabase.Refresh();
                CustomizeHierarchy.FileName = file;
                CustomHierarchyData.Load();
                CustomHierarchyData.Save();
            }
            GUILayout.Space(40);
            if (GUILayout.Button("DEL", GUILayout.Width(50)))
            {
                AssetDatabase.Refresh();
                if (File.Exists(CustomizeHierarchy.PathToFolder + "/" + file))
                {
                    File.Delete(CustomizeHierarchy.PathToFolder + "/" + file);
                    AssetDatabase.Refresh();
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}
