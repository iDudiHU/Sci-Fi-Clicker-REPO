using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    public delegate bool TryParseDelegate<T>(string input, out T result);
    public static class ParseHelper
    {
        const string NULL_STRING = "<null>";
        public static bool TryParse<T>(string input, out T result)
        {
            Type type = typeof(T);
            if(type == typeof(string))
            {
                if (input == NULL_STRING)
                    result = default(T);

                result = (T)(object)input;
                return true;
            }

            if (TryParseExplicitType<bool, T>(input, bool.TryParse, out result))
                return true;

            if (TryParseExplicitType<int, T>(input, int.TryParse, out result)) 
                return true;

            if (TryParseExplicitType<float, T>(input, float.TryParse, out result))
                return true;

            if (TryParseExplicitType<Vector2, T>(input, TryParseVector2, out result))
                return true;

            Debug.LogError("No TryParse method defined for type " + type.Name);
            return false;
        }

        private static bool TryParseExplicitType<T, U>(string input, TryParseDelegate<T> tryParseMethod, out U result)
        {
            T innerResult;
            if (tryParseMethod(input, out innerResult))
            {
                result = (U)(object)innerResult;
                return true;
            }

            result = default(U);
            return false;
        }

        public static string ToParsableString<T>(T value)
        {
            if(typeof(T) == typeof(string))
            {
                if (value == null)
                    return NULL_STRING;

                return value.ToString();
            }

            if(value is bool || value is int || value is float)
            {
                return value.ToString();
            }

            if (value is Vector2)
            {
                Vector2 v2 = (Vector2)(object)value;
                return string.Format("{0}:{1}", v2.x, v2.y);
            }

            throw new NotImplementedException();
        }

        static bool TryParseVector2(string input, out Vector2 result)
        {
            string[] parts = input.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                result = default(Vector2);
                return false;
            }

            float x, y;
            if(!float.TryParse(parts[0], out x) || !float.TryParse(parts[1], out y))
            {
                result = default(Vector2);
                return false;
            }

            result = new Vector2(x, y);
            return true;
        }
    }
}
