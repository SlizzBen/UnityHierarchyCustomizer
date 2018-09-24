using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

[InitializeOnLoad]
public class CustomizeHierarchy : MonoBehaviour
{
    public static string PathToFolder = "Assets\\CustomEditor\\styles";
    public static string FileName = "Profile.opt";

    private static Vector2 offset = new Vector2(0, 2);
    static CustomizeHierarchy()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;

        if (PlayerPrefs.HasKey("CustomizeHierarchy"))
        {
            string a = PlayerPrefs.GetString("CustomizeHierarchy");
            PathToFolder = a.Split(',')[0];
            FileName = a.Split(',')[1];
        }
    }

    private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        if (CustomHierarchyData.On == false)
            return;
        Color fontColor = Color.blue;
        Color backgroundColor = new Color(1f, 0f, 0f);

        var obj = EditorUtility.InstanceIDToObject(instanceID);
        
        if (obj != null)
        {
            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
           

            if (CustomHierarchyData.Contains(obj.name))
            {
                string comment = "";
                Rect offsetRect = new Rect(selectionRect.position + offset, selectionRect.size);
                if (Selection.instanceIDs.Contains(instanceID))
                {
                    fontColor = Color.white;
                    backgroundColor = new Color(0.24f, 0.48f, 0.90f);
                    EditorGUI.DrawRect(selectionRect, backgroundColor);
                }
                else
                {
                    CustomHElement element = CustomHierarchyData.GetElement(obj.name);
                    fontColor = element.fontColor;
                    backgroundColor = element.color;
                    comment = element.comment;
                    
                    Texture2D t = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CustomEditor/img/back.png", typeof(Texture2D));
                    Texture2D text = new Texture2D(t.width, t.height);
                    for(int x = 0; x < t.width; x++)
                    {
                        for(int y = 0; y < t.height; y++)
                        {
                            Color curColor = backgroundColor;
                            curColor.a = t.GetPixel(x, y).a;
                            
                            text.SetPixel(x, y, curColor);
                        }
                    }
                    text.wrapMode = TextureWrapMode.Clamp;
                    text.Apply();
                    GUI.DrawTexture(selectionRect, text);
                }
                
                
                EditorGUI.LabelField(offsetRect, obj.name+" "+ comment, new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = fontColor },
                    fontStyle = FontStyle.Bold
                });
            }
        }
    }
    
}

[InitializeOnLoad]
public class CustomHierarchyData : MonoBehaviour
{

    public static List<CustomHElement> list = new List<CustomHElement>();
    public static bool On = false;

    static CustomHierarchyData()
    {
        Load();
    }


    public static void Load()
    {
        string path = CustomizeHierarchy.PathToFolder;
        if (Directory.Exists(path) == false)
        {
            return;
        }
        string file = CustomizeHierarchy.FileName;
        if (File.Exists(path + "/" + file) == false)
            return;
        string json = File.ReadAllText(path + "/" + file);
        SaveCustomData s  = JsonUtility.FromJson<SaveCustomData>(json);
        if(s != null)
        {
            list = s.list;
            On = s.on;
        }
        else
        {
            list.Clear();
            On = true;
        }
        EditorApplication.RepaintHierarchyWindow();
    }

    private void Update()
    {
        Load();
    }

    static public void Save()
    {
        string path = CustomizeHierarchy.PathToFolder;

        string p = CustomizeHierarchy.PathToFolder + "," + CustomizeHierarchy.FileName;
        PlayerPrefs.SetString("CustomizeHierarchy", p);

        if (Directory.Exists(path) == false)
        {
            Directory.CreateDirectory(path);
        }
        string file = CustomizeHierarchy.FileName;

        SaveCustomData d = new SaveCustomData();
        d.list = list;
        d.on = On;
        File.WriteAllText(path + "/" + file, JsonUtility.ToJson(d));
    }

    static public void Add(string s)
    {
        list.Add(new CustomHElement()
        {
            Name = s,
            color = Color.red,
            fontColor = Color.black
        });
    }

    static public bool Contains(string s)
    {
        foreach(CustomHElement el in list)
        {
            if(el.Name == s)
            {
                return true;
            }
        }
        return false;
    }

    static public void Remove(string s)
    {
        foreach(CustomHElement el in list)
        {
            if(el.Name == s)
            {
                list.Remove(el);
                return;
            }
        }
    }

    static public Color GetColor(string s)
    {
        foreach (CustomHElement el in list)
        {
            if (el.Name == s)
            {
                return el.color;
            }
        }
        return Color.white;
    }

    static public Color GetFontColor(string s)
    {
        foreach (CustomHElement el in list)
        {
            if (el.Name == s)
            {
                return el.fontColor;
            }
        }
        return Color.white;
    }

    static public CustomHElement GetElement(string s)
    {
        foreach (CustomHElement el in list)
        {
            if (el.Name == s)
            {
                return el;
            }
        }
        return null;
    }
}

[System.Serializable]
public class CustomHElement
{
    public string Name;
    public Color color = Color.white;
    public Color fontColor = Color.black;
    public string comment = "";
}

[System.Serializable]
public class SaveCustomData
{
    public List<CustomHElement> list = new List<CustomHElement>();
    public bool on = false;
}