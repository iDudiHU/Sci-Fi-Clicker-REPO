using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace TheraBytes.BetterUi.Editor
{
    // //////////
    // STRING //
    // ////////
    public class StoredEditorString : StoredEditorValue<string>
    {
        public StoredEditorString(string id, string defaultValue)
            : base(id, defaultValue)
        { }

        protected override string GetPrefValue(string id)
        {
            return EditorPrefs.GetString(id);
        }

        protected override void SavePrefValue(string id, string value)
        {
            EditorPrefs.SetString(id, value);
        }
    }

    // /////////
    // FLOAT //
    // ///////
    public class StoredEditorFloat : StoredEditorValue<float>
    {
        public StoredEditorFloat(string id, float defaultValue)
            : base(id, defaultValue)
        { }

        protected override float GetPrefValue(string id)
        {
            return EditorPrefs.GetInt(id);
        }

        protected override void SavePrefValue(string id, float value)
        {
            EditorPrefs.SetFloat(id, value);
        }
    }

    // ///////
    // INT //
    // /////
    public class StoredEditorInt : StoredEditorValue<int>
    {
        public StoredEditorInt(string id, int defaultValue)
            : base(id, defaultValue)
        { }

        protected override int GetPrefValue(string id)
        {
            return EditorPrefs.GetInt(id);
        }

        protected override void SavePrefValue(string id, int value)
        {
            EditorPrefs.SetInt(id, value);
        }
    }

    // ////////
    // BOOL //
    // //////
    public class StoredEditorBool : StoredEditorValue<bool>
    {
        public StoredEditorBool(string id, bool defaultValue)
            : base(id, defaultValue)
        { }

        protected override bool GetPrefValue(string id)
        {
            return EditorPrefs.GetBool(id);
        }

        protected override void SavePrefValue(string id, bool value)
        {
            EditorPrefs.SetBool(id, value);
        }
    }

    // //////////////////////
    // GENERIC BASE CLASS //
    // ////////////////////
    public abstract class StoredEditorValue<T>
    {
        public T Value
        {
            get
            {
                if (EditorPrefs.HasKey(id))
                    return GetPrefValue(id);

                return defaultValue;
            }
            set
            {
                SavePrefValue(id, value);
            }
        }

        T defaultValue;
        string id;

        protected StoredEditorValue(string id, T defaultValue)
        {
            this.id = id;
            this.defaultValue = defaultValue;
        }

        protected abstract void SavePrefValue(string id, T value);
        protected abstract T GetPrefValue(string id);


        public static implicit operator T(StoredEditorValue<T> self)
        {
            return self.Value;
        }
        
    }
}
