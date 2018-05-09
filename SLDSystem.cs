//Optional Unity editor file
//Should create a new menu for you to preview what data is saved within your file.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using System.Linq;
using System;
using System.Reflection;
using System.Text;
#if UNITY_EDITOR
public class SLDSystem : EditorWindow {
    
    FileInfo[] FI;
    Vector2 ScrollPos = Vector2.zero;
    Vector2 BigScrollPos = Vector2.zero;
    Vector2 DataScroll = Vector2.zero;
    

    int DropDownSelection = 0;
    //string[] DropDownNames = { "Test1", "Test2" };
    List<string> DropDownNames = new List<string>();
    string DataList;

    //Makes the Window in the menu
    [MenuItem("Window/Save Load System Information")]
    public static void ShowWindow()
    {
        //Creates the physical window
        EditorWindow.GetWindow(typeof(SLDSystem));
    }

    [ExecuteInEditMode]
    void Update()
    {
        //Gets the file information
        DirectoryInfo FileData = new DirectoryInfo(Application.persistentDataPath);
        FI = FileData.GetFiles("*",SearchOption.TopDirectoryOnly);
    }

    void OnGUI()
    {
        DropDownNames.Clear();
        DropDownNames.Add("All");
        if (FI != null)
        {
            foreach (FileInfo I in FI)
            {
                DropDownNames.Add(Path.GetExtension(I.FullName));
            }
        }

        BigScrollPos = GUILayout.BeginScrollView(BigScrollPos, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height));
        
        GUILayout.Label("Save/Load Data System Information", EditorStyles.boldLabel);
        GUILayout.Space(50);
        GUILayout.Label("Current Save Files", EditorStyles.boldLabel);
        DropDownSelection = EditorGUILayout.Popup("File Extensions", DropDownSelection, DropDownNames.ToArray(),GUILayout.Width(400));
        GUILayout.BeginArea(new Rect(10, 130, 485, 200),EditorStyles.helpBox);
        ScrollPos = GUILayout.BeginScrollView(ScrollPos,GUILayout.Width(480),GUILayout.Height(194));
        GUILayout.BeginVertical();
        if (FI == null) { GUILayout.EndVertical(); GUILayout.EndScrollView(); GUILayout.EndArea(); return; }
        foreach (FileInfo I in FI)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (Path.GetExtension(I.FullName) == DropDownNames[DropDownSelection] || DropDownNames[DropDownSelection] == "All")
            {
                if (GUILayout.Button(I.Name, GUILayout.Width(400), GUILayout.Height(30)))
                {
                    object obj = SaveLoadData.Load<object>(Path.GetFileNameWithoutExtension(I.Name));
                    DataList = LoadAndWrite(obj, I.Name.Split('.')[0], 10);
                    
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        GUILayout.Space(250);
        GUILayout.Label("File Data", EditorStyles.boldLabel);
        GUILayout.BeginArea(new Rect(10, 400, 505, 400), EditorStyles.helpBox);

        DataScroll = GUILayout.BeginScrollView(DataScroll, GUILayout.Width(495), GUILayout.Height(394));
        
        if(DataList != null)
            GUILayout.TextArea(DataList);
        GUILayout.EndScrollView();

        GUILayout.EndArea();
        GUILayout.Space(600);

        GUILayout.EndScrollView();
    }

    private static string Indent(int indentLevel)
    {
        string IndentionAmount = "      ";
        string Result="";
        for (int i = 0; i < indentLevel; i++)
        {
            Result += IndentionAmount;
        }
        return Result;//"".PadRight(indentLevel, '\t');
    }

    string LoadAndWrite(object obj, string label, int depth =1)
    {
        return LoadAndWrite(obj, depth, 0, label).ToString();
    }

    StringBuilder LoadAndWrite(object obj, int Depth, int Indention, string ObjName)
    {
        StringBuilder Output = new StringBuilder();
        if (Depth < 0) return Output;

        if (obj == null) return Output;
        Type ObjType = obj.GetType();
        
        if (ObjType.IsPrimitive || obj is string)
        {
            Output.AppendFormat("{0}({1}) {2} = {3}", Indent(Indention), ObjType.Name.ToString().Split('`')[0], ObjName, obj);
            Output.AppendLine();
        }
        else if (obj is ICollection)
        {
            string SpecialName = ObjName;
            if (SpecialName.Contains("List")&&Depth!= 10) SpecialName = SpecialName.Split('.')[3].Split('`')[0] + "(" + SpecialName.Split('.')[4].Split(']')[0] + ")";
            Output.AppendFormat("{0}({1}) {2} = {{", Indent(Indention), ObjType.Name.ToString().Split('`')[0], SpecialName);
            Output.AppendLine();
            Indention++;
        
            ICollection Collect = (ICollection)obj;
            int Count = 0;
            foreach(var element in Collect)
            {
                Output.Append(LoadAndWrite(element, Depth - 1, Indention, "Value #" + Count++));
            }
            Indention--;
        
            Output.AppendFormat("{0}}}", Indent(Indention));
            Output.AppendLine();
        }
        else
        {
            Output.AppendFormat("{0}({1}) {2} = {{", Indent(Indention), ObjType.Name, ObjName);
            Output.AppendLine();
            Indention++;
        
            var FieldInfos = ObjType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var PropInfos = ObjType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        
            foreach(var field in FieldInfos)
            {
                Output.Append(LoadAndWrite(field.GetValue(obj), Depth - 1, Indention, field.Name));
            }
        
            foreach(var prop in PropInfos)
            {
                Output.Append(
                        LoadAndWrite(
                            prop.GetValue(obj, null),
                            Depth - 1,
                            Indention,
                            prop.Name));
            }
        
            Indention--;
            Output.AppendFormat("{0}}}", Indent(Indention));
            Output.AppendLine();
        }
        
        return Output;



#if false //My original attempt, above is much better
        DataList = "";

        if (obj.GetType().IsPrimitive || obj is string)
        {
            DataList = obj.GetType().ToString().Split('.')[1] + " - " + obj.ToString();
        }
        else if (obj.GetType().IsArray || obj is IList)
        {
            Type DataType = obj.GetType();
            Type IsList = null;
            if (DataType.IsGenericType && DataType.GetGenericTypeDefinition() == typeof(List<>))
                IsList = DataType.GetGenericArguments()[0];

            if ((IsList != null && (IsList.IsPrimitive || IsList.ToString() == "System.String")) || (DataType.GetElementType() != null && DataType.GetElementType().IsPrimitive && obj.GetType().IsArray))
            {

                if (obj.GetType().IsArray)
                    DataList = "Array of " + DataType.GetElementType().ToString().Split('.')[1] + "s\n\n";
                else
                    DataList = "List of " + IsList.ToString().Split('.')[1] + "s\n\n";
                IEnumerable Values = obj as IEnumerable;
                int Count = 0;
                foreach (object o in Values)
                {
                    DataList += "Value #" + Count + ": " + o.ToString();
                    DataList += "\n";
                    Count++;

                }
            }
            else
            {
                if (IsList != null)
                {
                    DataList = "List of Class: " + IsList.ToString().Split('+')[1] + "\n\n";
                    IEnumerable Values = obj as IEnumerable;
                    int Count = 0;
                    foreach (object o in Values)
                    {
                        FieldInfo[] Info = IsList.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        DataList += "Value #" + Count + ":\n";

                        IEnumerable InfoValues = Info as IEnumerable;

                        //foreach (object io in InfoValues)
                        //{
                        //    //Debug.Log(o);
                        //    LoadAndWrite(io, Depth + 1);
                        //}

                        //for (int i = 0; i < Info.Length; i++)
                        //{
                        //    string Item = Info[i].ToString().Split('.')[1];
                        //    for (int j = 0; j < Depth; j++)
                        //    {
                        //        DataList += "   ";
                        //    }
                        //    
                        //    DataList += (Item.Split(' ')[0] + " - " + Item.Split(' ')[1] + ": " + Info[i].GetValue(o));
                        //    DataList += "\n";
                        //}
                        DataList += "\n\n";
                        Count++;
                    }

                }
                else
                {
                    DataList = "Array of Class: " + DataType.GetElementType().ToString().Split('+')[1] + "\n\n";
                    IEnumerable Values = obj as IEnumerable;
                    int Count = 0;
                    foreach (object o in Values)
                    {
                        FieldInfo[] Info = DataType.GetElementType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        DataList += "Value #" + Count + ":\n";


                        for (int i = 0; i < Info.Length; i++)
                        {
                            string Item = Info[i].ToString().Split('.')[1];
                            for (int j = 0; j < Depth; j++)
                            {
                                DataList += "   ";
                            }
                            DataList += (Item.Split(' ')[0] + " - " + Item.Split(' ')[1] + ": " + Info[i].GetValue(o));
                            DataList += "\n";
                        }
                        DataList += "\n\n";
                        Count++;
                    }
                }
            }

        }
        else
        {
            //Goes through data for a custom class
            Type DataType = obj.GetType();
            FieldInfo[] Info = DataType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            DataList = "Class: " + obj.ToString().Split('+')[1] + "\n\n";
            for (int i = 0; i < Info.Length; i++)
            {
                string Item = Info[i].ToString().Split('.')[1];
                DataList += (Item.Split(' ')[0] + " - " + Item.Split(' ')[1] + ": " + Info[i].GetValue(obj));
                DataList += "\n";
            }

        }

#endif
    }
}
#endif
