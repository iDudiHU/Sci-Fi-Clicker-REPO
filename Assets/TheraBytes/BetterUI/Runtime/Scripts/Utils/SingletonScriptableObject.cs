using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace TheraBytes.BetterUi
{
    public abstract class SingletonScriptableObject<T> : ScriptableObject
        where T : SingletonScriptableObject<T>
    {
        static T instance;
        static bool creatingInstance = false;

        public static T Instance
        {
            get
            {
                EnsureInstance();
                return instance;
            }
        }

        public static bool HasInstance { get { return instance != null; } }

        public static bool ScriptableObjectFileExists
        {
            get
            {
                if (HasInstance)
                    return true;

                string filePath = GetFilePathWithExtention(true);
                return File.Exists(filePath);
            }
        }

        public static T EnsureInstance()
        {
            if (instance == null)
            {
                if (creatingInstance) // don't go here
                    throw new Exception("Instance accessed during creation of instance.");

                creatingInstance = true;
                string filePath = GetFilePathWithExtention(false);

                string resourceFilePath = Path.GetFileNameWithoutExtension(
                        filePath.Split(new string[] { "Resources" }, StringSplitOptions.None).Last());

                var obj = Resources.Load(resourceFilePath);
                instance = obj as T; // note: in the debugger it might be displayed as null (which is not the case)

                if (obj == null)
                {
                    instance = CreateInstance<T>(); // note: in the debugger it might be displayed as null (which is not the case)

#if UNITY_EDITOR && !UNITY_CLOUD_BUILD
                    string completeFilePath = Path.Combine(Application.dataPath, filePath);
                    string directory = Path.GetDirectoryName(completeFilePath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    UnityEditor.AssetDatabase.CreateAsset(instance, "Assets/" + filePath);
                    UnityEditor.AssetDatabase.Refresh();

#else
                    Debug.LogErrorFormat(
                        "Could not find scriptable object of type '{0}'. Make sure it is instantiated inside Unity before building.", 
                        typeof(T));
#endif
                }

                creatingInstance = false;
            }


            return instance;
        }

        private static string GetFilePathWithExtention(bool fullPath)
        {
            Type t = typeof(T);
            var prop = t.GetProperty("FilePath", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic);

            if (prop == null) throw new Exception("No static Property 'FilePath' in " + t.ToString());

            string filePath = prop.GetValue(null, null) as string;

            if (filePath == null) throw new Exception("static property 'FilePath' is not a string or null in " + t.ToString());
            if (!filePath.Contains("Resources")) throw new Exception("static property 'FilePath' must contain a Resources folder.");
            if (filePath.Contains("Plugins")) throw new Exception("static property 'FilePath' must not contain a Plugin folder.");

            if (!filePath.EndsWith(".asset"))
                filePath += ".asset";

            return (fullPath)
                ? Path.Combine(Application.dataPath, filePath)
                : filePath;
        }
    }
}
