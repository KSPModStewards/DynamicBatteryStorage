
namespace DynamicBatteryStorage
{
  // RealBattery: Realistic behaving batteries; https://github.com/blackliner/RealBattery
  // NOTE: The VAB stuff here might need updates
  public class RealBatteryPowerHandler : ModuleDataHandler
  {
    public RealBatteryPowerHandler(HandlerModuleData moduleData) : base(moduleData)
    { }
    protected override double GetValueEditor()
    {
      double.TryParse(pm.Fields.GetValue("lastECpower").ToString(), out double results);
      if (results > 0)
      {
        producer = false;
        consumer = true;
      }
      else
      {
        producer = true;
        consumer = false;
      }

      return results * -1.0d; // positive value means the battery is charging --> consuming EC; thus negating result
    }
    protected override double GetValueFlight()
    {
      Utils.TryGetField(pm, "lastECpower", out double results);
      if (results > 0)
      {
        producer = false;
        consumer = true;
      }
      else
      {
        producer = true;
        consumer = false;
      }
      return results * -1.0d; // positive value means the battery is charging --> consuming EC; thus negating result
    }
  }
}
