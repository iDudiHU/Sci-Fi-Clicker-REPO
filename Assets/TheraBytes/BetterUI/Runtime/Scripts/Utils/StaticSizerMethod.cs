using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#pragma warning disable 0649 // never assigned warning

namespace TheraBytes.BetterUi
{
    [Serializable]
    public class StaticSizerMethod
    {

        [SerializeField] string assemblyName = "Assembly-CSharp";
        [SerializeField] string typeName;
        [SerializeField] string methodName;

        public float Invoke(Component caller, Vector2 optimizedResolution, Vector2 actualResolution, float optimizedDpi, float actualDpi)
        {
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
            if (assembly == null)
            {
                Debug.LogErrorFormat("Static Sizer Method: Assembly with name '{0}' could not be found.", assemblyName);
                return 0;
            }

            Type t = assembly.GetType(typeName, false);
            if(t == null)
            {
                Debug.LogErrorFormat("Static Sizer Method: Type '{0}' could not be found in assembly '{1}'. Make sure the name contains the full namespace.", typeName, assemblyName);
                return 0;
            }

            MethodInfo method = t.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
            if(method == null)
            {
                Debug.LogErrorFormat("Static Sizer Method: Method '{0}()' could not be found in Type '{0}'. Make sure it is declared public and static.", methodName, typeName);
                return 0;
            }

            try
            {
                object result = method.Invoke(null, new object[] { caller, optimizedResolution, actualResolution, optimizedDpi, actualDpi });
                return (float)result;
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Static Sizer Method: Method '{0}.{1}()' could be found but failed to be invoked (see details below). Make sure it has all parameters and returns a float.", methodName, typeName);

                Debug.LogException(ex);
                return 0;
            }
        }
    }
}


#pragma warning restore 0649
