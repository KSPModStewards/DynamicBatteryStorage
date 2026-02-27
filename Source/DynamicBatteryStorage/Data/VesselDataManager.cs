using System;
using UnityEngine;

namespace DynamicBatteryStorage
{
  /// <summary>
  /// This Vessel Module calculates and stores the Electrical and Core Heat data for a vessel
  /// </summary>
  public class VesselDataManager : VesselModule
  {
    public VesselElectricalData ElectricalData => GetElectricalData();

    VesselElectricalData electricalData;

    VesselElectricalData GetElectricalData()
    {
      if (electricalData != null)
        return electricalData;

      try
      {
        RefreshVesselData();
      }
      catch (Exception e)
      {
        Utils.Error("RefreshVesselData threw an exception");
        Debug.LogException(e);
        return null;
      }

      return electricalData;
    }

    // This module is only active in the flight scene for loaded vessels.
    public override bool ShouldBeActive()
    {
      if (!HighLogic.LoadedSceneIsFlight)
        return false;

      return vessel.loaded;
    }

    // Use OnEnable and OnDisable because ShouldBeActive will cause us to switch
    // between being enabled and disabled in response to events.
    void OnEnable()
    {
      GameEvents.onVesselGoOnRails.Add(RefreshVesselData);
      GameEvents.onVesselWasModified.Add(InvalidateVesselData);

      if (vessel == FlightGlobals.ActiveVessel)
        RefreshVesselData();
    }

    void OnDisable()
    {
      GameEvents.onVesselGoOnRails.Remove(RefreshVesselData);
      GameEvents.onVesselWasModified.Remove(InvalidateVesselData);

      electricalData = null;
    }

    /// <summary>
    /// Referesh the data, given a Vessel event
    /// </summary>
    protected void RefreshVesselData(Vessel eventVessel)
    {
      if (vessel != eventVessel)
        return;
      if (vessel == null || !vessel.loaded)
        return;

      if (Settings.DebugVesselData)
        Utils.Log($"[{GetType().Name}]: Refreshing VesselData from Vessel event", Utils.LogType.VesselData);
      RefreshVesselData();
    }

    /// <summary>
    /// Invalidate the current electrical data, given a vessel event.
    /// </summary>
    /// <param name="eventVessel"></param>
    protected void InvalidateVesselData(Vessel eventVessel)
    {
      if (vessel != eventVessel)
        return;

      electricalData = null;

      if (Settings.DebugVesselData)
        Utils.Log($"[{GetType().Name}]: Invalidated VesselData from Vessel event", Utils.LogType.VesselData);
    }

    /// <summary>
    /// Referesh the data classes
    /// </summary>
    protected void RefreshVesselData()
    {
      if (vessel == null || vessel.Parts == null || !vessel.loaded)
        return;

      electricalData = new VesselElectricalData(vessel.Parts);

      if (Settings.DebugVesselData)
        Utils.Log($"Dumping electrical database: \n{electricalData}", Utils.LogType.VesselData);
    }
  }
}
