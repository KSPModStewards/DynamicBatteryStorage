using System;
using UnityEngine;
using UnityEngine.UI;
using KSP.Localization;
using KSP.UI;
using KSP.UI.TooltipTypes;

namespace DynamicBatteryStorage.UI
{
  public class ToolbarPower
  {
    protected ToolbarDetailPanel detailPanel;
    protected Text powerPanelText;

    protected Text powerText;
    protected Image powerFlowIconUp;
    protected Image powerFlowIconDown;

    protected Text batteryText;
    protected Text chargeTimeText;

    protected GameObject solarEfficiencyObject;
    protected GameObject eclipseTimeObject;
    protected Text solarEfficiencyText;
    protected Text eclipseTimeText;

    protected Text powerGeneratedTitleText;
    protected Text powerGeneratedValueText;
    protected Button powerGeneratedButton;
    protected Text powerConsumedTitleText;
    protected Text powerConsumedValueText;
    protected Button powerConsumedButton;

    protected float userGeneration;
    protected float userConsumption;

    private Color32 colorGoodText;
    private Color32 colorBadText;
    private Color32 colorWarningText;

    private string powerFlowUnits = "";
    private string timeUntilBatteryDepletedSlug = "";
    private string timeUntilBatteryChargedSlug = "";
    private string timeUntilBatteryChargedFromZeroSlug = "";
    private string batteryNoChangeSlug = "";
    private string batteryFullSlug = "";
    private string batteryEmptySlug = "";

    public void Initialize(Transform root, ToolbarDetailPanel details)
    {
      detailPanel = details;
      powerPanelText = Utils.FindChildOfType<Text>("PowerHeaderText", root);

      powerText = Utils.FindChildOfType<Text>("PowerFlowValue", root);

      powerFlowIconUp = Utils.FindChildOfType<Image>("PowerFlowIconUp", root);
      powerFlowIconDown = Utils.FindChildOfType<Image>("PowerFlowIconDown", root);
      batteryText = Utils.FindChildOfType<Text>("BatteryValue", root);
      chargeTimeText = Utils.FindChildOfType<Text>("ChargeTimeValue", root);

      solarEfficiencyObject = root.FindDeepChild("EfficiencyData").gameObject;
      eclipseTimeObject = root.FindDeepChild("EclipseData").gameObject;

      solarEfficiencyText = Utils.FindChildOfType<Text>("EfficiencyValue", root);
      eclipseTimeText = Utils.FindChildOfType<Text>("EclipseTimeValue", root);

      powerGeneratedTitleText = Utils.FindChildOfType<Text>("GenerationTitle", root);
      powerGeneratedValueText = Utils.FindChildOfType<Text>("GenerationValue", root);
      powerGeneratedButton = Utils.FindChildOfType<Button>("GenerationDetailButton", root);
      powerConsumedTitleText = Utils.FindChildOfType<Text>("ConsumptionTitle", root);
      powerConsumedValueText = Utils.FindChildOfType<Text>("ConsumptionValue", root);
      powerConsumedButton = Utils.FindChildOfType<Button>("ConsumptionDetailButton", root);


      HexColorField.HexToColor("B4D455", out colorGoodText);
      HexColorField.HexToColor("FE4901", out colorBadText);
      HexColorField.HexToColor("FEA601", out colorWarningText);

      if (!HighLogic.LoadedSceneIsEditor)
      {
        eclipseTimeObject.SetActive(false);
        solarEfficiencyObject.SetActive(false);
      }
      powerGeneratedButton.onClick.AddListener(delegate { detailPanel.SetPanelMode(DetailPanelMode.ProducerPower, powerGeneratedButton.GetComponent<RectTransform>()); });
      powerConsumedButton.onClick.AddListener(delegate { detailPanel.SetPanelMode(DetailPanelMode.ConsumerPower, powerConsumedButton.GetComponent<RectTransform>()); });

      Localize();

      SetupTooltips(root, Tooltips.FindTextTooltipPrefab());
      SetNoVesselValues();
    }
    protected void SetupTooltips(Transform root, Tooltip_Text prefab)
    {
      Tooltips.AddTooltip(root.FindDeepChild("PowerFlow").gameObject, prefab, Localizer.Format("#LOC_DynamicBatteryStorage_Tooltips_PowerFlow"));
      Tooltips.AddTooltip(root.FindDeepChild("BatteryData").gameObject, prefab, Localizer.Format("#LOC_DynamicBatteryStorage_Tooltips_BatteryInfo"));
      Tooltips.AddTooltip(root.FindDeepChild("ChargeTimeData").gameObject, prefab, Localizer.Format("#LOC_DynamicBatteryStorage_Tooltips_ChargeTimeInfo"));
      Tooltips.AddTooltip(root.FindDeepChild("EfficiencyData").gameObject, prefab, Localizer.Format("#LOC_DynamicBatteryStorage_Tooltips_SolarEfficiency"));
      Tooltips.AddTooltip(root.FindDeepChild("EclipseData").gameObject, prefab, Localizer.Format("#LOC_DynamicBatteryStorage_Tooltips_EclipseTime"));
      Tooltips.AddTooltip(powerGeneratedButton.gameObject, prefab, Localizer.Format("#LOC_DynamicBatteryStorage_Tooltips_GenerationDetail"));
      Tooltips.AddTooltip(powerConsumedButton.gameObject, prefab, Localizer.Format("#LOC_DynamicBatteryStorage_Tooltips_ConsumptionDetail"));
    }

    protected void SetNoVesselValues()
    {
      powerText.text = "-";
      batteryText.text = "-";

      chargeTimeText.text = "-";
      powerConsumedValueText.text = "-";
      powerGeneratedValueText.text = "-";

      powerFlowIconUp.color = new Color(0.3f, 0.3f, 0.3f);
      powerFlowIconDown.color = new Color(0.3f, 0.3f, 0.3f);
    }
    protected void Localize()
    {
      powerPanelText.text = Localizer.Format("#LOC_DynamicBatteryStorage_PowerPanel_PowerTitle");
      powerGeneratedTitleText.text = Localizer.Format("#LOC_DynamicBatteryStorage_PowerPanel_PowerGeneratedTitle");
      powerConsumedTitleText.text = Localizer.Format("#LOC_DynamicBatteryStorage_PowerPanel_PowerConsumedTitle");

      powerFlowUnits = Localizer.Format("#LOC_DynamicBatteryStorage_UI_ElectricalFlowUnits");

      timeUntilBatteryDepletedSlug = Localizer.Format("#LOC_DynamicBatteryStorage_PowerPanel_BatteryTimeDepletion");
      timeUntilBatteryChargedSlug = Localizer.Format("#LOC_DynamicBatteryStorage_PowerPanel_BatteryTimeCharging");
      timeUntilBatteryChargedFromZeroSlug = Localizer.Format("#LOC_DynamicBatteryStorage_PowerPanel_BatteryTimeFullCharge");
      batteryNoChangeSlug = Localizer.Format("#LOC_DynamicBatteryStorage_PowerPanel_BatteryTimeStable");
      batteryFullSlug = Localizer.Format("#LOC_DynamicBatteryStorage_PowerPanel_BatteryTimeFull");
      batteryEmptySlug = Localizer.Format("#LOC_DynamicBatteryStorage_PowerPanel_BatteryTimeEmpty");
    }
    public void Update(VesselElectricalData data)
    {
      UpdatePowerFlowAndBatteryFields(data);
    }
    protected void UpdatePowerFlowAndBatteryFields(VesselElectricalData data)
    {
      double netPower = data.CurrentConsumption + data.GetSimulatedElectricalProdution() + userGeneration - userConsumption;
      data.GetElectricalChargeLevels(out double EC, out double maxEC);

      if (maxEC > 0)
      {
        batteryText.text = String.Format("{0:F0} / {1:F0} ({2:F1}%)", EC, maxEC, EC / maxEC * 100d);
      }
      else
      {
        batteryText.text = "-";
      }
      if (EC <= 0.001)
      {
        batteryText.color = colorBadText;
      }
      else if (EC / maxEC < 0.25f)
      {
        batteryText.color = colorWarningText;
      }
      else
      {
        batteryText.color = colorGoodText;
      }
      powerText.text = String.Format("{0:F2} {1}", Math.Abs(netPower), powerFlowUnits);
      if (netPower == 0d)
      {
        /// no power - hide flow icon, set charge metrics to say no change
        chargeTimeText.text = String.Format("{0}", batteryNoChangeSlug);

        powerFlowIconUp.color = new Color(0.3f, 0.3f, 0.3f);
        powerFlowIconDown.color = new Color(0.3f, 0.3f, 0.3f);
        powerText.color = colorGoodText;
      }
      else if (netPower > 0d)
      {
        /// positive power - flow icon up, set charge metrics to charging or full

        powerFlowIconUp.color = colorGoodText;
        powerFlowIconDown.color = new Color(0.3f, 0.3f, 0.3f);
        powerText.color = colorGoodText;
        if (HighLogic.LoadedSceneIsFlight)
        {
          if ((maxEC - EC) < 0.001d)
          {
            chargeTimeText.text = batteryFullSlug;
            chargeTimeText.color = colorGoodText;
          }
          else
          {
            chargeTimeText.text = String.Format("{0} {1}", timeUntilBatteryChargedSlug, FormatUtils.FormatTimeString((maxEC - EC) / netPower));
            chargeTimeText.color = colorGoodText;
          }
        }
        else
        {
          chargeTimeText.text = String.Format("{0} {1}", timeUntilBatteryChargedFromZeroSlug, FormatUtils.FormatTimeString(maxEC / netPower));
          chargeTimeText.color = colorGoodText;
        }
      }
      else
      {
        /// negative power - flow icon up, set charge metrics to discharging or empty

        powerFlowIconUp.color = new Color(0.3f, 0.3f, 0.3f);
        powerFlowIconDown.color = colorWarningText;
        powerText.color = colorWarningText;

        if (EC < 0.1d)
        {
          chargeTimeText.text = batteryEmptySlug;
          chargeTimeText.color = colorBadText;
        }
        else
        {
          chargeTimeText.text = String.Format("{0} {1}", timeUntilBatteryDepletedSlug, FormatUtils.FormatTimeString(EC / -netPower));
          chargeTimeText.color = colorWarningText;
        }
      }
      powerConsumedValueText.text = String.Format("▼ {0:F2} {1}",
        Math.Abs(data.CurrentConsumption) + userConsumption,
        powerFlowUnits);
      powerGeneratedValueText.text = String.Format("▲ {0:F2} {1}",
        Math.Abs(data.GetSimulatedElectricalProdution()) + userGeneration,
        powerFlowUnits);
    }

    public void SetManualPowerInputs(float input, float output)
    {
      userConsumption = output;
      userGeneration = input;
    }
    public void UpdateSolarFields(double panelScale, double timeInDark)
    {
      solarEfficiencyText.text = String.Format("{0:F1}%", panelScale * 100f);
      eclipseTimeText.text = FormatUtils.FormatTimeString(timeInDark);
    }

  }
}
