using System;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicBatteryStorage
{
  public static class Utils
  {
    public enum LogType
    {
      UI,
      Settings,
      Modules,
      Handlers,
      DynamicStorage,
      VesselData,
      Loading,
      Any
    }
    public static string logTag = "DBS";
    /// <summary>
    /// Log a message with the mod name tag prefixed
    /// </summary>
    /// <param name="str">message string </param>
    public static void Log(string str, LogType logType)
    {
      bool doLog = false;
      if (logType == LogType.Settings && Settings.DebugSettings) doLog = true;
      if (logType == LogType.UI && Settings.DebugUI) doLog = true;
      if (logType == LogType.Loading && Settings.DebugLoading) doLog = true;
      if (logType == LogType.Modules && Settings.DebugModules) doLog = true;
      if (logType == LogType.Handlers && Settings.DebugHandlers) doLog = true;
      if (logType == LogType.DynamicStorage && Settings.DebugDynamicStorage) doLog = true;
      if (logType == LogType.VesselData && Settings.DebugVesselData) doLog = true;
      if (logType == LogType.Any) doLog = true;

      if (doLog)
        Debug.Log(String.Format("[{0}]{1}", logTag, str));
    }

    public static void Warn(string toLog)
    {
      Debug.LogWarning(String.Format("[{0}]: {1}", logTag, toLog));
    }
    public static void Error(string toLog)
    {
      Debug.LogError(String.Format("[{0}]: {1}", logTag, toLog));
    }

    public static bool TryGetField(PartModule pm, string fieldName, out bool result)
    {
        result = false;
        string field = pm?.Fields?.GetValue(fieldName)?.ToString();
        return bool.TryParse(field, out result);
    }

    public static bool TryGetField(PartModule pm, string fieldName, out double result)
    {
        result = 0d;
        string field = pm?.Fields?.GetValue(fieldName)?.ToString();
        return double.TryParse(field, out result);
    }
    
    public static bool TryGetField(PartModule pm, string fieldName, out float result)
    {
        result = 0f;
        string field = pm?.Fields?.GetValue(fieldName)?.ToString();
        return float.TryParse(field, out result);
    }

    public static bool TryParseEnum<T>(string str, bool caseSensitive, out T value) where T : struct
    {
      // Can't make this a type constraint...
      if (!typeof(T).IsEnum)
      {
        throw new ArgumentException("Type parameter must be an enum");
      }
      var names = Enum.GetNames(typeof(T));
      value = (Enum.GetValues(typeof(T)) as T[])[0];  // For want of a better default
      foreach (var name in names)
      {
        if (String.Equals(name, str, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
        {
          value = (T)Enum.Parse(typeof(T), name);
          return true;
        }
      }
      return false;
    }
    /// <summary>
    /// Get a reference in a child of a type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static T FindChildOfType<T>(string name, Transform parent)
    {
      T result = default(T);
      try
      {
        result = parent.FindDeepChild(name).GetComponent<T>();
      }
      catch (NullReferenceException e)
      {
        Debug.LogError($"Couldn't find {name} in children of {parent.name}");
      }
      return result;
    }
  }
}

public static class TransformDeepChildExtension
{
  //Breadth-first search
  public static Transform FindDeepChild(this Transform aParent, string aName)
  {
    Queue<Transform> queue = new Queue<Transform>();
    queue.Enqueue(aParent);
    while (queue.Count > 0)
    {
      var c = queue.Dequeue();
      if (c.name == aName)
        return c;
      foreach (Transform t in c)
        queue.Enqueue(t);
    }
    return null;
  }
}


