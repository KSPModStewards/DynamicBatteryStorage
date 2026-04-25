namespace DynamicBatteryStorage
{
  /// <summary>
  /// Handler for Kopernicus > v209 solar panel type
  /// </summary>
  public class KopernicusSolarPanelPowerHandler : ModuleDataHandler
  {   
    public KopernicusSolarPanelPowerHandler(HandlerModuleData moduleData) : base(moduleData)
    { }

    public override bool Initialize(PartModule pm)
    {
      base.Initialize(pm);
      /// If kopernicus is using the multistar settings, we need to only show this thing in flight as the new handler wipes the data in editor
      /// We need to rely on the 
      if (Settings.KopernicusMultiStar)
      {
        return HighLogic.LoadedSceneIsFlight;
      }
      return true;
    }

    protected override double GetValueEditor()
    {
      /// Invalid in editor
      return 0f;
    }

    protected override double GetValueFlight()
    {
      Utils.TryGetField(pm, "currentOutput", out double results);
      return results;
    }
  }
}
