using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    public static class SerializedPropertyUtil
    {

        public static void Copy(SerializedProperty source, SerializedProperty target)
        {
            //Debug.LogFormat("source: {0} - {1} \ntarget: {2} - {3}", 
            //    source.name, source.propertyType, target.name, target.propertyType);

            if (source.isArray)
            {
                target.arraySize = source.arraySize;
                target.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                for (int i = 0; i < source.arraySize; i++)
                {
                    Copy(source.GetArrayElementAtIndex(i), target.GetArrayElementAtIndex(i));
                }
            }
            else if (source.hasChildren)
            {
                var endSource = source.GetEndProperty(true).propertyPath;
                var endTarget = target.GetEndProperty(true).propertyPath;
                
                source.NextVisible(true);
                target.NextVisible(true);

                while (source.propertyPath != endSource && target.propertyPath != endTarget)
                {
                    Copy(source, target);

                    bool enterChildren = !(source.isArray);
                    bool stop = (source.propertyPath == endSource) || (target.propertyPath == endTarget);
                    try
                    {
                        stop = stop || !(source.NextVisible(enterChildren));
                        stop = stop || !(target.NextVisible(enterChildren));
                    }
                    catch (Exception ex) 
                    {
                        stop = true;
                        Debug.LogErrorFormat("An Error occured: {0}", ex);
                    }

                    if (stop || source.propertyType != target.propertyType)
                        break;
                }
            }
            else
            {
                object val = GetValue(source);
                SetValue(target, val);
            }
        }

        public static object GetValue(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.ObjectReference: return prop.objectReferenceValue;
                case SerializedPropertyType.Integer: return prop.intValue;
                case SerializedPropertyType.Boolean: return prop.boolValue;
                case SerializedPropertyType.Float: return prop.floatValue;
                case SerializedPropertyType.String: return prop.stringValue;
                case SerializedPropertyType.Color: return prop.colorValue;
                case SerializedPropertyType.Enum: return prop.enumValueIndex;
                case SerializedPropertyType.Vector2: return prop.vector2Value;
                case SerializedPropertyType.Vector3: return prop.vector3Value;
                case SerializedPropertyType.Vector4: return prop.vector4Value;
                case SerializedPropertyType.Rect: return prop.rectValue;
                case SerializedPropertyType.ArraySize: return prop.arraySize;
                case SerializedPropertyType.AnimationCurve: return prop.animationCurveValue;
                case SerializedPropertyType.Bounds: return prop.boundsValue;
                case SerializedPropertyType.Quaternion: return prop.quaternionValue;

                case SerializedPropertyType.Generic: return prop.objectReferenceValue;
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.Character:
                default:
                    throw new ArgumentException();
            }
        }


        public static void SetValue(SerializedProperty prop, object value)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = (UnityEngine.Object)value;
                    break;

                case SerializedPropertyType.Integer:        prop.intValue = (int)value;               break;
                case SerializedPropertyType.Boolean:        prop.boolValue = (bool)value;             break;
                case SerializedPropertyType.Float:          prop.floatValue = (float)value;           break;
                case SerializedPropertyType.String:         prop.stringValue = (string)value;         break;
                case SerializedPropertyType.Color:          prop.colorValue = (Color)value;           break;
                case SerializedPropertyType.Enum:           prop.enumValueIndex = (int)value;         break;
                case SerializedPropertyType.Vector2:        prop.vector2Value = (Vector2)value;       break;
                case SerializedPropertyType.Vector3:        prop.vector3Value = (Vector3)value;       break;
                case SerializedPropertyType.Vector4:        prop.vector4Value = (Vector4)value;       break;
                case SerializedPropertyType.Rect:           prop.rectValue = (Rect)value;             break;
                case SerializedPropertyType.ArraySize:      prop.arraySize = (int)value;              break;
                case SerializedPropertyType.Bounds:         prop.boundsValue = (Bounds)value;         break;
                case SerializedPropertyType.Quaternion:     prop.quaternionValue = (Quaternion)value; break;
                case SerializedPropertyType.AnimationCurve:
                    prop.animationCurveValue = (AnimationCurve)value;
                    break;

                case SerializedPropertyType.Generic:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.Character:
                default:
                    throw new ArgumentException();
            }
        }
    }
}
