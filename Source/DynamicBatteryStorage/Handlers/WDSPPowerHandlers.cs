using System;

namespace DynamicBatteryStorage
{

    /// <summary>
    /// Handler for Weather Driven Solar Panel
    /// </summary>
    public class WDSPPowerHandlers : ModuleDataHandler
    {
        public WDSPPowerHandlers(HandlerModuleData moduleData) : base(moduleData)
        { }

        public override bool Initialize(PartModule pm)
        {
            base.Initialize(pm);
            /// seppuku.exe
            if (Settings.WeatherDrivenSolarPanel)
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
