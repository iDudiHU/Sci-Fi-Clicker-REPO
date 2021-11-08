using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Methods : MonoBehaviour
{   
    public  static List<T> CreateList<T>(int capacity) => Enumerable.Repeat(default(T), capacity).ToList();

    public static void UpgradeCheck<T>(ref List<T> List, int length) where T : new()
    {
        try
        {
            if (List.Count == 0) List = CreateList<T>(length);
            while(List.Count < length) List.Add(new T());
        }
        catch
        {
            List = CreateList<T>(length);
        }
    }

}
