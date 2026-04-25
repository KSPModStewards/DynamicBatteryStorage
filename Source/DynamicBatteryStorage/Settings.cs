using System;
using System.Collections.Generic;
using System.Reflection;
using UniLinq;
using UnityEngine;

namespace DynamicBatteryStorage
{
  [KSPAddon(KSPAddon.Startup.EveryScene, false)]
  public class DynamicBatteryStorage : MonoBehaviour
  {
    public static DynamicBatteryStorage Instance { get; private set; }

    protected void Awake()
    {
      Instance = this;
    }
    protected void Start()
    {
      Settings.Load();
      enabled = Settings.Enabled;
    }
  }


  public enum ResourcesSupported
  {
    Heat,
    Power
  }

  /// <summary>
  /// Static class to hold settings and configuration
  /// </summary>
  public static class Settings
  {

    public static bool Enabled = true;

    public static bool WeatherDrivenSolarPanel = false;
    public static bool Kopernicus = false;
    public static bool KopernicusMultiStar = false;

    public static float TimeWarpLimit = 100f;
    public static float BufferScaling = 1.75f;

    public static float UIIconAlertLevel = 0.5f;
    public static float UIIconCriticalLevel = 0.1f;
    public static int UIUpdateInterval = 3;

    public static bool DebugUI = true;
    public static bool DebugSettings = true;
    public static bool DebugModules = true;
    public static bool DebugHandlers = true;
    public static bool DebugDynamicStorage = true;
    public static bool DebugVesselData = true;
    public static bool DebugLoading = true;

    public static Dictionary<string, UIHandlerCategory> HandlerCategoryData;
    public static List<HandlerModuleData> HandlerPartModuleData;

    public static readonly string ELECTRICITY_RESOURCE_NAME = "ElectricCharge";

    private static readonly string CONFIG_NODE_NAME = "DynamicBatteryStorage/DYNAMICBATTERYSTORAGE";
    private static readonly string UI_HANDLER_NODE_NAME = "HANDLERCATEGORY";
    private static readonly string MODULE_HANDLER_NODE_NAME = "PARTMODULEHANDLER";

    /// <summary>
    /// Load data from configuration
    /// </summary>
    public static void Load()
    {
      ConfigNode settingsNode;

      DetectMods();

      Utils.Log("[Settings]: Started loading", Utils.LogType.Settings);
      if (GameDatabase.Instance.ExistsConfigNode(CONFIG_NODE_NAME))
      {
        Utils.Log("[Settings]: Located settings file", Utils.LogType.Settings);

        settingsNode = GameDatabase.Instance.GetConfigNode(CONFIG_NODE_NAME);

        settingsNode.TryGetValue("MinimumWarpFactor", ref TimeWarpLimit);

        settingsNode.TryGetValue("DebugVesselData", ref DebugVesselData);
        settingsNode.TryGetValue("DebugModules", ref DebugModules);
        settingsNode.TryGetValue("DebugHandlers", ref DebugHandlers);
        settingsNode.TryGetValue("DebugDynamicStorage", ref DebugDynamicStorage);

        settingsNode.TryGetValue("DebugSettings", ref DebugSettings);
        settingsNode.TryGetValue("DebugUI", ref DebugUI);
        settingsNode.TryGetValue("DebugLoading", ref DebugLoading);

        settingsNode.TryGetValue("BufferScaling ", ref BufferScaling);
        settingsNode.TryGetValue("UIUpdateInterval ", ref UIUpdateInterval);
        settingsNode.TryGetValue("UIIconAlertLevel ", ref UIIconAlertLevel);
        settingsNode.TryGetValue("UIIconCriticalLevel ", ref UIIconCriticalLevel);
        settingsNode.TryGetValue("Enabled", ref Enabled);

        if (Settings.DebugLoading)
        {
          Utils.Log("[Settings]: Loading handler categories", Utils.LogType.Settings);
        }
        HandlerCategoryData = new Dictionary<string, UIHandlerCategory>();
        ConfigNode[] categoryNodes = settingsNode.GetNodes(UI_HANDLER_NODE_NAME);

        foreach (ConfigNode node in categoryNodes)
        {
          UIHandlerCategory newCat = new UIHandlerCategory(node);
          HandlerCategoryData.Add(newCat.name, newCat);
        }
        if (Settings.DebugLoading)
        {
          Utils.Log("[Settings]: Loading handler modules", Utils.LogType.Settings);
        }
        HandlerPartModuleData = new List<HandlerModuleData>();
        ConfigNode[] partModuleNodes = settingsNode.GetNodes(MODULE_HANDLER_NODE_NAME);
        foreach (ConfigNode node in partModuleNodes)
        {
          HandlerModuleData newDat = new HandlerModuleData(node);
          HandlerPartModuleData.Add(newDat);
        }

      }
      else
      {
        Utils.Log("[Settings]: Couldn't find settings file, using defaults", Utils.LogType.Settings);
      }
      Utils.Log("[Settings]: Finished loading", Utils.LogType.Settings);
    }

    /// <summary>
    /// Find any conflicting mods, return false if there are any.
    /// The result of this will be overridden by the Enable setting in the plugin configuration.
    /// </summary>
    private static void DetectMods()
    {
      foreach (var a in AssemblyLoader.loadedAssemblies)
      {
        // search for conflicting mods
        if (a.name.StartsWith("Kerbalism", StringComparison.Ordinal))
        {
          Utils.Log("[Settings]: Kerbalism detected. DBS will disable itself.", Utils.LogType.Any);
          Settings.Enabled = false;
        }
        // Search for kopernicus
        if (a.name.StartsWith("Kopernicus", StringComparison.Ordinal))
        {
          Utils.Log("[Settings]: Kopernicus detected", Utils.LogType.Any);
          Settings.Kopernicus = true;

          Assembly kopernicusAssembly = AssemblyLoader.loadedAssemblies.FirstOrDefault(b => b.assembly.GetName().Name == "Kopernicus")?.assembly;

          Type starType = kopernicusAssembly.GetType("Kopernicus.Components.KopernicusStar");
          var msObj = starType.GetField("UseMultiStarLogic",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).GetValue(null);
          KopernicusMultiStar = (bool)msObj;
          Utils.Log($"[Settings] Kopernicus Multi Star Logic is {KopernicusMultiStar}",Utils.LogType.Any);
        }
        // Search for wdsp
        if (a.name.StartsWith("WeatherDrivenSolarPanel", StringComparison.Ordinal))
        {
          WeatherDrivenSolarPanel = true;
          Utils.Log($"[Settings] Weather Dependent Solar Panel logic is  Multi Star Logic is {WeatherDrivenSolarPanel}", Utils.LogType.Any);
        }
          
      }
    }



    /// <summary>
    /// Returns a list of handler categories currently in the mod
    /// </summary>
    public static List<String> HandlerCategories
    {
      get { return new List<string>(Settings.HandlerCategoryData.Keys); }
    }

    /// <summary>
    /// Checks to see if a part module is supported
    /// </summary>
    /// <param name="moduleName">The partModule name to check for.</param>
    /// <param name="resourceType">The type of ResourcesSupported to support</param>
    public static bool IsSupportedPartModule(string moduleName, ResourcesSupported resourceType)
    {
      if (Settings.SupportedModules(resourceType).Contains(moduleName))
        return true;
      return false;
    }

    /// <summary>
    /// Gets the handler module data for a part module name
    /// </summary>
    /// <param name="moduleName">The partModule name to check for.</param>
    /// <param name="resourceType">The type of ResourcesSupported to support</param>
    public static HandlerModuleData GetPartModuleData(string moduleName, ResourcesSupported resourceType)
    {
      for (int i = 0; i < HandlerPartModuleData.Count; i++)
      {
        if (HandlerPartModuleData[i].resourceType == resourceType && HandlerPartModuleData[i].handledModule == moduleName)
          return HandlerPartModuleData[i];
      }
      return null;
    }

    /// <summary>
    /// Gets a list of supported part modules
    /// </summary>
    /// <param name="resourceType">The type of ResourcesSupported to support</param>
    public static List<String> SupportedModules(ResourcesSupported resourceType)
    {
      List<string> supportedModules = new List<string>();
      for (int i = 0; i < HandlerPartModuleData?.Count; i++)
      {
        if (HandlerPartModuleData[i]?.resourceType == resourceType)
          supportedModules.Add(HandlerPartModuleData[i]?.handledModule);
      }
      return supportedModules;
    }
  }
}
