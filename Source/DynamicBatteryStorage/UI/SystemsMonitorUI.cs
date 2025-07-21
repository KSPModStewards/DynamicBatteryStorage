using UnityEngine;
using KSP.UI;
using KSP.UI.Screens;

namespace DynamicBatteryStorage.UI
{
  [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
  public class SystemsMonitorUI : MonoBehaviour
  {

    public static SystemsMonitorUI Instance { get; private set; }
    public bool WindowState { get { return showWindow; } }


    private Vessel activeVessel;
    private VesselDataManager vesselData;
    private EditorVesselDataManager editorVesselData;

    public VesselElectricalData ElectricalData
    {
      get
      {
        if (HighLogic.LoadedSceneIsFlight && vesselData)
        {
          return vesselData.ElectricalData;
        }
        else
        {
          if (editorVesselData)
          {
            return editorVesselData.ElectricalData;
          }
        }
        return null;
      }
    }

    // Control Vars
    protected static bool showWindow = false;

    // Panel
    public ToolbarPanel toolbarPanel;
    public ToolbarIconTag toolbarTag;

    // Stock toolbar button
    protected string toolbarUIIconURLOff = "DynamicBatteryStorage/UI/toolbar_off";
    protected string toolbarUIIconURLOn = "DynamicBatteryStorage/UI/toolbar_on";
    protected static ApplicationLauncherButton stockToolbarButton = null;

    protected virtual void Awake()
    {
      Utils.Log("[SystemsMonitorUI]: Initializing toolbar", Utils.LogType.UI);

      GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
      GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);

      GameEvents.onGUIApplicationLauncherUnreadifying.Add(new EventData<GameScenes>.OnEvent(OnGUIAppLauncherUnreadifying));
      GameEvents.onVesselChange.Add(new EventData<Vessel>.OnEvent(OnVesselChanged));

      Instance = this;
    }

    public void Start()
    {
      if (ApplicationLauncher.Ready)
        OnGUIAppLauncherReady();
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
        FindData();
    }

    protected void CreateToolbarPanel()
    {
      Utils.Log("[SystemsMonitorUI]: Creating toolbar panel", Utils.LogType.UI);
      GameObject newUIPanel = (GameObject)Instantiate(SystemsMonitorAssets.ToolbarPanelPrefab, Vector3.zero, Quaternion.identity);
      newUIPanel.transform.SetParent(UIMasterController.Instance.appCanvas.transform);
      newUIPanel.transform.localScale = Vector3.one;
      newUIPanel.transform.localPosition = Vector3.zero;

      toolbarPanel = newUIPanel.AddComponent<ToolbarPanel>();
      toolbarPanel.SetVisible(false);
      toolbarTag = new ToolbarIconTag();
      toolbarTag.Initialize(this);
      toolbarTag.Position(stockToolbarButton);
    }
    protected void DestroyToolbarPanel()
    {
      Utils.Log("[SystemsMonitorUI]: Destroying toolbar panel", Utils.LogType.UI);
      if (toolbarPanel != null)
      {
        Destroy(toolbarPanel.gameObject);
      }
    }

    bool pinnedOn = false;

    /// <summary>
    /// Hover state 
    /// </summary>
    public void SetHoverState(bool on)
    {
      if (pinnedOn)
        return;

      showWindow = on;
      toolbarPanel.SetVisible(showWindow);
    }
    public void SetClickedState(bool on)
    {
      pinnedOn = on;
      showWindow = pinnedOn;

      toolbarPanel.SetVisible(showWindow);
    }

    void Update()
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        if (FlightGlobals.ActiveVessel != null)
        {
          if (vesselData == null)
          {
            FindData();
          }
        }
      }
      if (HighLogic.LoadedSceneIsEditor)
      {
        if (editorVesselData == null)
        {
          FindData();
        }
      }
      if (ElectricalData != null && toolbarPanel != null)
      {
        toolbarPanel.SetElectricalData(ElectricalData);
      }

      /// Set the window position
      if (showWindow)
      {
        if (HighLogic.LoadedSceneIsFlight)
        {
          if (toolbarPanel != null && stockToolbarButton != null)
          {
            toolbarPanel.SetToolbarPosition(stockToolbarButton.GetAnchorUL(), new Vector2(1, 0.5f));
          }
        }
        if (HighLogic.LoadedSceneIsEditor)
        {
          if (stockToolbarButton != null)
          {
            toolbarPanel.SetToolbarPosition(stockToolbarButton.GetAnchorUR(), new Vector2(1, 0));
          }
        }
      }
      if (toolbarTag != null && ElectricalData != null)
      {
        toolbarTag.Update(ElectricalData);
      }
    }
    public void OnVesselChanged(Vessel v)
    {
      Utils.Log($"[SystemsMonitorUI]: OnVesselChanged Fired to {v.vesselName}", Utils.LogType.UI);
      ResetToolbarPanel();
    }
    public void FindData()
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        activeVessel = FlightGlobals.ActiveVessel;
        if (activeVessel != null)
        {
          vesselData = activeVessel.GetComponent<VesselDataManager>();
          Utils.Log("[SystemsMonitorUI]: Located Flight data", Utils.LogType.UI);
        }
      }
      if (HighLogic.LoadedSceneIsEditor)
      {
        editorVesselData = EditorVesselDataManager.Instance;
        Utils.Log("[SystemsMonitorUI]: Located Editor data", Utils.LogType.UI);
      }
    }

    void ResetToolbarPanel()
    {
      FindData();
    }
    #region Stock Toolbar Methods
    public void OnDestroy()
    {
      Utils.Log("[SystemsMonitorUI]: OnDestroy Fired", Utils.LogType.UI);
      // Remove the stock toolbar button
      GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
      if (stockToolbarButton != null)
      {
        ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
      }
      GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
      GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnGUIAppLauncherDestroyed);
      GameEvents.onGUIApplicationLauncherUnreadifying.Remove(new EventData<GameScenes>.OnEvent(OnGUIAppLauncherUnreadifying));
      GameEvents.onVesselChange.Remove(new EventData<Vessel>.OnEvent(OnVesselChanged));
    }
    protected void OnToolbarButtonHover()
    {
      Utils.Log("[SystemsMonitorUI]: Toolbar Button Hover On", Utils.LogType.UI);

      stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(showWindow ? toolbarUIIconURLOn : toolbarUIIconURLOff, false));
      SetHoverState(true);
    }
    protected void OnToolbarButtonHoverOut()
    {
      Utils.Log("[SystemsMonitorUI]: Toolbar Button Hover Out", Utils.LogType.UI);

      stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(showWindow ? toolbarUIIconURLOn : toolbarUIIconURLOff, false));
      SetHoverState(false);
    }
    protected void OnToolbarButtonOn()
    {
      Utils.Log("[SystemsMonitorUI]: Toolbar Button  On", Utils.LogType.UI);

      stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(showWindow ? toolbarUIIconURLOn : toolbarUIIconURLOff, false));
      SetClickedState(true);
    }
    protected void OnToolbarButtonOff()
    {
      Utils.Log("[SystemsMonitorUI]: Toolbar Button Off", Utils.LogType.UI);

      stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(showWindow ? toolbarUIIconURLOn : toolbarUIIconURLOff, false));
      SetClickedState(false);
    }
    protected void OnToolbarButtonToggle()
    {
      Utils.Log("[SystemsMonitorUI]: Toolbar Button Toggled", Utils.LogType.UI);

      stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(showWindow ? toolbarUIIconURLOn : toolbarUIIconURLOff, false));
      //SetAppState(!showWindow, !showWindow);
    }


    protected void OnGUIAppLauncherReady()
    {
      showWindow = false;

      Utils.Log("[SystemsMonitorUI]: App Launcher Ready", Utils.LogType.UI);
      if (ApplicationLauncher.Ready && stockToolbarButton == null && Settings.Enabled)
      {
        if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
          stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(
              OnToolbarButtonOn,
              OnToolbarButtonOff,
              OnToolbarButtonHover,
              OnToolbarButtonHoverOut,
              DummyVoid,
              DummyVoid,
              ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.FLIGHT,
              (Texture)GameDatabase.Instance.GetTexture(toolbarUIIconURLOff, false));
      }
      CreateToolbarPanel();
    }

    protected void OnGUIAppLauncherDestroyed()
    {

      Utils.Log("[SystemsMonitorUI]: App Launcher Destroyed", Utils.LogType.UI);
      if (stockToolbarButton != null)
      {
        ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
        stockToolbarButton = null;
      }
      DestroyToolbarPanel();
    }


    protected void OnGUIAppLauncherUnreadifying(GameScenes scene)
    {

      Utils.Log("[SystemsMonitorUI]: App Launcher Unready", Utils.LogType.UI);

      DestroyToolbarPanel();
    }

    protected void onAppLaunchToggleOff()
    {
      Utils.Log("[SystemsMonitorUI]: App Launcher Toggle Off", Utils.LogType.UI);
      stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(toolbarUIIconURLOff, false));
    }

    protected void DummyVoid() { }

    public void ResetAppLauncher()
    {

      Utils.Log("[SystemsMonitorUI]: Reset App Launcher", Utils.LogType.UI);
      FindData();
      if (stockToolbarButton == null && Settings.Enabled)
      {
        stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(
              OnToolbarButtonOn,
              OnToolbarButtonOff,
              OnToolbarButtonHover,
              OnToolbarButtonHoverOut,
            DummyVoid,
            DummyVoid,
            ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.FLIGHT,
            (Texture)GameDatabase.Instance.GetTexture(toolbarUIIconURLOff, false));
      }
    }
    #endregion
  }
}
