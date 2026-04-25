namespace DynamicBatteryStorage
{
  public class SCANsatPowerHandler : ModuleDataHandler
  {
    ModuleResource resource;
    public SCANsatPowerHandler(HandlerModuleData moduleData) : base(moduleData)
    { }

    public override bool Initialize(PartModule pm)
    {
      base.Initialize(pm);
      for (int i = pm.resHandler.inputResources.Count - 1; i >= 0; i--)
      {
        if (pm.resHandler.inputResources[i].id == PartResourceLibrary.ElectricityHashcode)
        {
          resource = pm.resHandler.inputResources[i];
          return true;
        }
      }
      return false;
    }
    protected override double GetValueEditor()
    {
      return -resource.rate;
    }
    protected override double GetValueFlight()
    {
      Utils.TryGetField(pm, "scanning", out bool isScanning);
      if (isScanning)
      {
        return -resource.rate;
      }
      return 0f;
    }
  }
}
