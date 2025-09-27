using System;

namespace DynamicBatteryStorage
{
  /// <summary>
  /// This is a generic handler that parses a KSPField
  /// Ideally this field is populated by another mod then read by this
  /// </summary>
  public class GenericFieldDataHandler : ModuleDataHandler
  {
    private readonly string editorFieldName;
    private readonly string flightFieldName;

    private readonly double editorValueScalar = 1.0d;
    private readonly double flightValueScalar = 1.0d;

    private BaseField editorField;
    private BaseField flightField;

    public GenericFieldDataHandler(HandlerModuleData moduleData) : base(moduleData)
    {
      editorFieldName = moduleData.config.editorFieldName;
      flightFieldName = moduleData.config.flightFieldName;
      editorValueScalar = moduleData.config.editorValueScalar;
      flightValueScalar = moduleData.config.flightValueScalar;
    }

    public override bool Initialize(PartModule pm)
    {
      base.Initialize(pm);
      try
      {
        editorField = pm.Fields[editorFieldName];
      }
      catch (Exception)
      {
        Utils.Warn($"[GenericFieldDataHandler] Issue locating editor field name {editorFieldName} on {pm.moduleName}");
        return false;
      }
      try
      {
        flightField = pm.Fields[flightFieldName];
      }
      catch (Exception)
      {
        Utils.Warn($"[GenericFieldDataHandler] Issue locating flight field name {flightFieldName} on {pm.moduleName}");
        return false;
      }
      return true;
    }
    protected override double GetValueEditor()
    {
      double results = 0d;
      if (editorField != null)
      {
        double.TryParse(editorField.GetValue(pm)?.ToString(), out results);
      }
      return results * editorValueScalar;
    }
    protected override double GetValueFlight()
    {
      double results = 0d;
      if (flightField != null)
      {
        double.TryParse(flightField.GetValue(pm)?.ToString(), out results);
      }
      return results * flightValueScalar;
    }
  }
}
