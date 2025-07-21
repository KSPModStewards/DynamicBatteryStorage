using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using KSP.Localization;
using KSP.UI;
using KSP.UI.TooltipTypes;

namespace DynamicBatteryStorage.UI
{
  public class ToolbarAdvanced
  {
    public bool SectionEnabled { get; private set; }

    GameObject headerObject;
    Text headerText;
    Button headerButton;

    GameObject advancedObject;
    GameObject dbsObject;

    // Adavanced version
    Text userConsumptionText;
    Text userConsumptionUnits;
    InputField userConsumptionInput;

    Text userGenerationText;
    Text userGenerationUnits;
    InputField userGenerationInput;

    Text togglesText;

    Text variableOnText;
    Text variableOffText;
    Button variableOnButton;
    Button variableOffButton;

    Text constantOnText;
    Text constantOffText;
    Button constantOnButton;
    Button constantOffButton;

    // monitoring version
    Text vesselHeader;
    Text vesselStatus;
    Text vesselStatusValue;
    Text vesselSavedEC;
    Text vesselSavedECValue;

    Text bufferHeader;
    Text bufferPart;
    Text bufferPartValue;
    Text bufferSize;
    Text bufferSizeValue;
    Text bufferSavedEC;
    Text bufferSavedECValue;

    string ECunits;
    string ECFlowunits;
    string messageErrorNoController;
    string messageOfflineLowTimewarp;
    string messageEnabled;
    string messageDisabled;

    ToolbarPanel mainPanel;
    ModuleDynamicBatteryStorage controller;
    Vessel activeVessel;
    int partCount = -1;

    public void Initialize(Transform root, ToolbarPanel parentPanel)
    {
      mainPanel = parentPanel;
      headerObject = root.FindDeepChild("AdvancedHeader").gameObject;
      advancedObject = root.FindDeepChild("AdvancedData").gameObject;
      dbsObject = root.FindDeepChild("DBSData").gameObject;

      headerText = Utils.FindChildOfType<Text>("AdvancedHeaderText", root);
      headerButton = Utils.FindChildOfType<Button>("AdvancedHeader", root);

      // Advanced version
      userConsumptionText = Utils.FindChildOfType<Text>("ManualConsumptionLabel", root);
      userConsumptionUnits = Utils.FindChildOfType<Text>("ManualConsumptionUnits", root);
      userConsumptionInput = Utils.FindChildOfType<InputField>("ManualConsumptionInput", root);

      userGenerationText = Utils.FindChildOfType<Text>("ManualGenerationLabel", root);
      userGenerationUnits = Utils.FindChildOfType<Text>("ManualGenerationUnits", root);
      userGenerationInput = Utils.FindChildOfType<InputField>("ManualGenerationInput", root);

      togglesText = Utils.FindChildOfType<Text>("ToggleQuickLabel", root);

      variableOnText = Utils.FindChildOfType<Text>("VariablesOnText", root);
      variableOffText = Utils.FindChildOfType<Text>("VariablesOffText", root);
      variableOnButton = Utils.FindChildOfType<Button>("VariablesOnButton", root);
      variableOffButton = Utils.FindChildOfType<Button>("VariablesOffButton", root);

      constantOnText = Utils.FindChildOfType<Text>("ConstantsOnText", root);
      constantOffText = Utils.FindChildOfType<Text>("ConstantsOffText", root);
      constantOnButton = Utils.FindChildOfType<Button>("ConstantsOnButton", root);
      constantOffButton = Utils.FindChildOfType<Button>("ConstantsOffButton", root);

      // monitoring version
      vesselHeader = Utils.FindChildOfType<Text>("VesselBlockHeaderLabel", root);
      vesselStatus = Utils.FindChildOfType<Text>("SystemStatusTitle", root);
      vesselStatusValue = Utils.FindChildOfType<Text>("SystemStatusValue", root);
      vesselSavedEC = Utils.FindChildOfType<Text>("SavedDataTitle", root);
      vesselSavedECValue = Utils.FindChildOfType<Text>("SavedDataValue", root);

      bufferHeader = Utils.FindChildOfType<Text>("BufferBlockHeaderLabel", root);
      bufferPart = Utils.FindChildOfType<Text>("BufferPartTitle", root);
      bufferPartValue = Utils.FindChildOfType<Text>("BufferPartValue", root);
      bufferSize = Utils.FindChildOfType<Text>("BufferSizeTitle", root);
      bufferSizeValue = Utils.FindChildOfType<Text>("BufferSizeValue", root);
      bufferSavedEC = Utils.FindChildOfType<Text>("SavedBufferDataTitle", root);
      bufferSavedECValue = Utils.FindChildOfType<Text>("SavedBufferDataValue", root);

      headerButton.onClick.AddListener(delegate { ToggleState(); });

      variableOffButton.onClick.AddListener(delegate { SetVariableState(false); });
      variableOnButton.onClick.AddListener(delegate { SetVariableState(true); });
      constantOffButton.onClick.AddListener(delegate { SetConstantState(false); });
      constantOnButton.onClick.AddListener(delegate { SetConstantState(true); });

      userConsumptionInput.contentType = InputField.ContentType.DecimalNumber;
      userConsumptionInput.characterValidation = InputField.CharacterValidation.Decimal;
      userConsumptionInput.onValueChanged.AddListener(delegate { OnManualGenerationChange(); });
      userGenerationInput.contentType = InputField.ContentType.DecimalNumber;
      userGenerationInput.characterValidation = InputField.CharacterValidation.Decimal;
      userGenerationInput.onValueChanged.AddListener(delegate { OnManualGenerationChange(); });


      SectionEnabled = false;
      advancedObject.SetActive(false);
      dbsObject.SetActive(false);
      Localize();

      SetupTooltips(root, Tooltips.FindTextTooltipPrefab());
    }
    protected void OnDestroy()
    {

      InputLockManager.RemoveControlLock("DBSFocus");
    }
    protected void Localize()
    {
      headerText.text = Localizer.Format("#LOC_DynamicBatteryStorage_AdvancedPanel_AdvancedTitle");
      ECunits = Localizer.Format("#LOC_DynamicBatteryStorage_UI_ElectricalUnits");
      ECFlowunits = Localizer.Format("#LOC_DynamicBatteryStorage_UI_ElectricalFlowUnits");
      userConsumptionUnits.text = userGenerationUnits.text = ECFlowunits;
      userConsumptionText.text = Localizer.Format("#LOC_DynamicBatteryStorage_AdvancedPanel_UserConsumption");
      userGenerationText.text = Localizer.Format("#LOC_DynamicBatteryStorage_AdvancedPanel_UserGeneration");

      togglesText.text = Localizer.Format("#LOC_DynamicBatteryStorage_AdvancedPanel_QuickTogglesTitle");
      variableOnText.text = Localizer.Format("#LOC_DynamicBatteryStorage_AdvancedPanel_Variable_On");
      variableOffText.text = Localizer.Format("#LOC_DynamicBatteryStorage_AdvancedPanel_Variable_Off");
      constantOnText.text = Localizer.Format("#LOC_DynamicBatteryStorage_AdvancedPanel_Constants_On");
      constantOffText.text = Localizer.Format("#LOC_DynamicBatteryStorage_AdvancedPanel_Constants_Off");

      // monitoring version
      vesselHeader.text = Localizer.Format("#LOC_DynamicBatteryStorage_AdvancedPanel_VesselTitle");
      vesselStatus.text = Localizer.Format("#LOC_DynamicBatteryStorage_AdvancedPanel_Vessel_SystemStatusTitle");
      vesselSavedEC.text = Localizer.Format("#LOC_DynamicBatteryStorage_AdvancedPanel_Vessel_SavedDataTitle");

      bufferHeader.text = Localizer.Format("#LOC_DynamicBatteryStorage_AdvancedPanel_BufferTitle");
      bufferPart.text = Localizer.Format("#LOC_DynamicBatteryStorage_AdvancedPanel_Buffer_PartTitle");
      bufferSize.text = Localizer.Format("#LOC_DynamicBatteryStorage_AdvancedPanel_Buffer_SizeTitle");
      bufferSavedEC.text = Localizer.Format("#LOC_DynamicBatteryStorage_AdvancedPanel_Buffer_SavedDataTitle");

      messageOfflineLowTimewarp = Localizer.Format("#LOC_DynamicBatteryStorage_AdvancedPanel_Vessel_SystemStatus_Offline", Settings.TimeWarpLimit);
      messageErrorNoController = Localizer.Format("#LOC_DynamicBatteryStorage_AdvancedPanel_Vessel_SystemStatus_NoController");
      messageEnabled = Localizer.Format("#LOC_DynamicBatteryStorage_AdvancedPanel_Vessel_SystemStatus_Enabled");
      messageDisabled = Localizer.Format("#LOC_DynamicBatteryStorage_AdvancedPanel_Vessel_SystemStatus_Disabled");

    }
    protected void SetupTooltips(Transform root, Tooltip_Text prefab)
    {
      Tooltips.AddTooltip(userConsumptionText.gameObject, prefab, Localizer.Format("#LOC_DynamicBatteryStorage_Tooltips_UserConsumption"));
      Tooltips.AddTooltip(userGenerationText.gameObject, prefab, Localizer.Format("#LOC_DynamicBatteryStorage_Tooltips_UserGeneration"));
      Tooltips.AddTooltip(constantOnButton.gameObject, prefab, Localizer.Format("#LOC_DynamicBatteryStorage_Tooltips_ConstantOn"));
      Tooltips.AddTooltip(constantOffButton.gameObject, prefab, Localizer.Format("#LOC_DynamicBatteryStorage_Tooltips_ConstantOff"));
      Tooltips.AddTooltip(variableOffButton.gameObject, prefab, Localizer.Format("#LOC_DynamicBatteryStorage_Tooltips_VariableOn"));
      Tooltips.AddTooltip(variableOnButton.gameObject, prefab, Localizer.Format("#LOC_DynamicBatteryStorage_Tooltips_VariableOff"));

    }

    public void Update(VesselElectricalData data)
    {
      if (HighLogic.LoadedSceneIsEditor)
      {
        UpdateAdvanced();
      }
      if (HighLogic.LoadedSceneIsFlight)
      {
        UpdateDBS();
      }
    }

    protected void UpdateAdvanced()
    {
      /// what, all event driven? you monster
      if (userGenerationInput.isFocused || userConsumptionInput
        .isFocused)
      {
        if (InputLockManager.IsUnlocked(ControlTypes.KEYBOARDINPUT))
        {
          InputLockManager.SetControlLock(ControlTypes.KEYBOARDINPUT, "DBSFocus");
        }
       
      }
      else
      {
        if (InputLockManager.IsLocked(ControlTypes.KEYBOARDINPUT))
        {
          InputLockManager.RemoveControlLock("DBSFocus");
        }
      }
    }
    protected void UpdateDBS()
    {
      if (!Settings.Enabled)
      {
        vesselStatusValue.text = messageDisabled;
        bufferSizeValue.text = "-";
        bufferPartValue.text = "-";
        bufferSavedECValue.text = "-";
        vesselSavedECValue.text = "-";
        return;
      }
      if (controller == null)
      {
        vesselStatusValue.text = messageErrorNoController;

        if (FlightGlobals.ActiveVessel != null)
        {
          if (activeVessel != null)
          {
            if (partCount != activeVessel.parts.Count || activeVessel != FlightGlobals.ActiveVessel)
              RefreshController();
          }
          else
          {
            RefreshController();
          }
        }
        if (activeVessel != null)
        {
          if (partCount != activeVessel.parts.Count || activeVessel != FlightGlobals.ActiveVessel)
            RefreshController();
        }
      }
      else
      {
        if (!controller.AnalyticMode)
        {
          vesselStatusValue.text = messageOfflineLowTimewarp;
        }
        else if (controller.BufferPart == null)
        {
          vesselStatusValue.text = messageErrorNoController;
        }
        else
        {
          vesselStatusValue.text = messageEnabled;
          bufferSizeValue.text = String.Format("{0:F2} {1}", controller.BufferSize, ECunits);
          bufferPartValue.text = String.Format("{0}", controller.BufferPart.partInfo.title);
          bufferSavedECValue.text = String.Format("{0:F1} {1} : {2:F1} {3}", controller.MaxEC, ECunits, controller.SavedMaxEC, ECunits);
          vesselSavedECValue.text = String.Format("{0:F1} {1}", controller.SavedVesselMaxEC, ECunits);
        }
      }
    }
    protected void RefreshController()
    {
      activeVessel = FlightGlobals.ActiveVessel;
      partCount = activeVessel.parts.Count;

      controller = activeVessel.GetComponent<ModuleDynamicBatteryStorage>();
    }
    public void ToggleState()
    {
      SetTabState(!SectionEnabled);
    }
    public void SetTabState(bool state)
    {
      SectionEnabled = state;
      if (HighLogic.LoadedSceneIsEditor)
      {
        advancedObject.SetActive(state);
      }
      else
      {
        dbsObject.SetActive(state);
      }
      mainPanel.DetailUI.RecalculatePanelPositionData();
    }
    public void OnManualGenerationChange()
    {


      mainPanel.PowerUI.SetManualPowerInputs(
        ValidateManualInput(userGenerationInput),
        ValidateManualInput(userConsumptionInput));
    }

    protected float ValidateManualInput(InputField handler)
    {
      string input = handler.text;

      if (!float.TryParse(input, out float result))
      {
        result = 0f;
      }
      return result;
    }
    public void SetConstantState(bool state)
    {
      mainPanel.SetConstantHandlers(state);
    }
    public void SetVariableState(bool state)
    {
      mainPanel.SetVariableHandlers(state);
    }
  }
}