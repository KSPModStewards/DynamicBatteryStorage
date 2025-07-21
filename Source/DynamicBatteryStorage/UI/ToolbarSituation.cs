using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using KSP.Localization;
using KSP.UI;
using KSP.UI.TooltipTypes;
using System.Collections.Generic;
using System.Collections;

namespace DynamicBatteryStorage.UI
{
  public class ToolbarSituation
  {
    public CelestialBody SimSituationBody
    {
      get { return currentBody; }
    }
    public float SimSituationAltitude
    {
      get { return altitudeSlider.value * 1000f; }
    }
    public float SimSituationSolarAltitude
    {
      get { return solarAltitudeSlider.value; }
    }
    public float SimSituationPanelScale
    {
      get { return panelScalar; }
    }
    public double SimSituationEclipseTime
    {
      get { return darkTime; }
    }
    protected GameObject situationPanel;
    protected RectTransform situationData;

    protected Text situationTitle;
    protected Text bodyTitle;
    protected Dropdown bodyDropdown;

    protected GameObject altitudeObj;
    protected GameObject altitudeSliderObj;
    protected Text altitudeTitle;
    protected Slider altitudeSlider;
    protected Text altitudeLabel;
    protected InputField altitudeTextArea;


    protected Text solarAltitudeTitle;
    protected Slider solarAltitudeSlider;
    protected Text solarAltitudeLabel;
    protected InputField solarAltitudeTextArea;

    protected GameObject situationDataObj;
    protected GameObject situationHeaderObj;

    protected CelestialBody currentBody;
    protected CelestialBody homeBody;

    protected double referenceSolarAltitude = 0d;
    protected double referenceFurthestCBAltitude = 0d;
    protected float panelScalar;
    protected double darkTime;
    protected double bodySolarAltitude = 10000000d;
    protected double starLuminanceScale = 1.0;

    protected ToolbarPanel toolbar;

    public void Initialize(Transform root, ToolbarPanel parentPanel)
    {
      toolbar = parentPanel;
      situationHeaderObj = root.FindDeepChild("SituationHeader").gameObject;
      situationDataObj = root.FindDeepChild("SituationData").gameObject;
      situationTitle = root.FindDeepChild("SituationHeaderText").GetComponent<Text>();
      bodyTitle = root.FindDeepChild("BodyLabel").GetComponent<Text>();
      bodyDropdown = root.FindDeepChild("BodyDropdown").GetComponent<Dropdown>();

      altitudeObj = root.FindDeepChild("BodyAltRow").gameObject;
      altitudeSliderObj = root.FindDeepChild("BodyAltSliderRow").gameObject;

      altitudeTitle = root.FindDeepChild("BodyAltLabel").GetComponent<Text>();
      altitudeSlider = root.FindDeepChild("BodyAltSlider").GetComponent<Slider>();
      altitudeLabel = root.FindDeepChild("BodyAltUnits").GetComponent<Text>();
      altitudeTextArea = root.FindDeepChild("BodyAltInput").GetComponent<InputField>();

      solarAltitudeTitle = root.FindDeepChild("AltLabel").GetComponent<Text>();
      solarAltitudeSlider = root.FindDeepChild("AltSlider").GetComponent<Slider>();
      solarAltitudeLabel = root.FindDeepChild("AltUnits").GetComponent<Text>();
      solarAltitudeTextArea = root.FindDeepChild("AltInput").GetComponent<InputField>();

      situationData = situationDataObj.GetComponent<RectTransform>();

      if (HighLogic.LoadedSceneIsEditor)
      {
        homeBody = FlightGlobals.GetHomeBody();
        currentBody = homeBody;
        referenceSolarAltitude = FlightGlobals.getAltitudeAtPos(homeBody.getPositionAtUT(0d), FlightGlobals.Bodies[0]) / 1000000d;
        referenceFurthestCBAltitude = 0d;
        for (int i = 0; i < FlightGlobals.Bodies.Count; i++)
        {
          if (!FlightGlobals.Bodies[i].isStar)
          {
            referenceFurthestCBAltitude = UtilMath.Max(
              FlightGlobals.Bodies[i].orbit.ApA + FlightGlobals.Bodies[i].sphereOfInfluence,
              FlightGlobals.Bodies[i].orbit.PeA + FlightGlobals.Bodies[i].sphereOfInfluence,
              referenceFurthestCBAltitude);
          }
        }
        bodyDropdown.ClearOptions();
        bodyDropdown.AddOptions(FlightGlobals.Bodies.Select(x => x.bodyDisplayName.LocalizeRemoveGender()).ToList());
        for (int i = 0; i < bodyDropdown.options.Count; i++)
        {
          if (bodyDropdown.options[i].text == currentBody.bodyDisplayName.LocalizeRemoveGender())
            bodyDropdown.SetValueWithoutNotify(i);
        }
        bodyDropdown.onValueChanged.AddListener(delegate { OnBodyDropdownChange(); });
        solarAltitudeSlider.onValueChanged.AddListener(delegate { OnSolarAltSliderChange(); });
        solarAltitudeTextArea.onValueChanged.AddListener(delegate { OnSolarAltInputChange(); });
        altitudeSlider.onValueChanged.AddListener(delegate { OnAltSliderChange(); });
        altitudeTextArea.onValueChanged.AddListener(delegate { OnAltInputChange(); });

        SetBody(currentBody);

        SetVisible(true);
      }
      else
      {
        SetVisible(false);
      }
      Localize();

      SetupTooltips(root, Tooltips.FindTextTooltipPrefab());
    }
    protected void Localize()
    {
      situationTitle.text = Localizer.Format("#LOC_DynamicBatteryStorage_SituationPanel_SituationTitle");
      bodyTitle.text = Localizer.Format("#LOC_DynamicBatteryStorage_SituationPanel_SituationBody");
      altitudeTitle.text = Localizer.Format("#LOC_DynamicBatteryStorage_SituationPanel_AltitudeTitle");
      altitudeLabel.text = Localizer.Format("#LOC_DynamicBatteryStorage_SituationPanel_AltitudeUnits");
      solarAltitudeTitle.text = Localizer.Format("#LOC_DynamicBatteryStorage_SituationPanel_SolarAltitudeTitle");
      solarAltitudeLabel.text = Localizer.Format("#LOC_DynamicBatteryStorage_SituationPanel_SolarAltitudeUnits");
    }
    protected void SetupTooltips(Transform root, Tooltip_Text prefab)
    {
      Tooltips.AddTooltip(root.FindDeepChild("BodyLabel").gameObject, prefab, Localizer.Format("#LOC_DynamicBatteryStorage_Tooltips_SituationBody"));
      Tooltips.AddTooltip(root.FindDeepChild("AltLabel").gameObject, prefab, Localizer.Format("#LOC_DynamicBatteryStorage_Tooltips_SolarAlt"));
      Tooltips.AddTooltip(root.FindDeepChild("BodyAltLabel").gameObject, prefab, Localizer.Format("#LOC_DynamicBatteryStorage_Tooltips_BodyAlt"));
    }
    Assembly kopernicusAssembly;
    protected void SetReferenceToStarBody(CelestialBody newStar)
    {
      /// This needs to talk to Kopernicus and ask for the star luminosity, somehow.
      starLuminanceScale = 1d;
      if (Settings.Kopernicus)
      {
        //var assembly = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.assembly.GetName().Name == "Kopernicus")?.assembly;
        //var starType = assembly.GetType("Kopernicus.Components.KopernicusStar");
        //var cbStarDictObj = starType.GetField("CelestialBodies",
        //  BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).GetValue(null);
        //Debug.Log($"BLA 1 {cbStarDictObj}");
        //IDictionary cbStarDict = cbStarDictObj as IDictionary;
        //Debug.Log($"BLA 2 {cbStarDict}");
        //object kopStarInstance = cbStarDict[newStar];
        //Debug.Log($"BLA 3 {kopStarInstance}");
        //FieldInfo shifterInfo = kopStarInstance.GetType().GetField("shifter", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        //Debug.Log($"BLA 4 {shifterInfo}");
        //var shifterInstance = shifterInfo.GetValue(kopStarInstance);
        //var lumInfo = shifterInstance.GetType().GetField("solarLuminosity", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        //starLuminanceScale = (double)lumInfo.GetValue(shifterInstance) / PhysicsGlobals.SolarLuminosityAtHome;
        //Debug.Log($"BLA 8 {starLuminanceScale}");

        if (kopernicusAssembly == null)
        {
          kopernicusAssembly = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.assembly.GetName().Name == "Kopernicus")?.assembly;
        }
        Type starType = kopernicusAssembly.GetType("Kopernicus.Components.KopernicusStar");
        var cbStarDictObj = starType.GetField("CelestialBodies",
          BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).GetValue(null);
        IDictionary cbStarDict = cbStarDictObj as IDictionary;
        object kopStarInstance = cbStarDict[newStar];

        FieldInfo shifterInfo = kopStarInstance.GetType().GetField("shifter", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        var shifterInstance = shifterInfo.GetValue(kopStarInstance);
        FieldInfo lumInfo = shifterInstance.GetType().GetField("solarLuminosity", BindingFlags.Public | BindingFlags.Instance);
        FieldInfo lightInfo = shifterInstance.GetType().GetField("givesOffLight", BindingFlags.Public | BindingFlags.Instance);

        if ((bool)lightInfo.GetValue(shifterInstance))
        {
          starLuminanceScale = (double)lumInfo.GetValue(shifterInstance) / PhysicsGlobals.SolarLuminosityAtHome;
        }
        else
        {
          starLuminanceScale = 0d;
        }
      }
      Utils.Log($"[ToolbarSituation]: Set star luminance scale to {starLuminanceScale}", Utils.LogType.UI);
    }
    void SetBody(CelestialBody b)
    {
      double defaultHeightAboveAtmo = 10000d;
      double mToKmScale = 1000d;
      double mToMmScale = 1000000d;
      /// Find the relevant star 
      CelestialBody starBody = b;

      if (!currentBody.isStar)
      {
        bodySolarAltitude = 0d;
        while (starBody?.orbit?.referenceBody != null)
        {
          if (starBody.isStar)
          {
            break;
          }
          bodySolarAltitude += starBody.orbit.ApR;
          starBody = starBody.orbit.referenceBody;
        }
        altitudeSliderObj.SetActive(true);
        altitudeObj.SetActive(true);
      }
      else
      {
        starBody = b;
        altitudeSliderObj.SetActive(false);
        altitudeObj.SetActive(false);
      }

      SetReferenceToStarBody(starBody);

      altitudeSlider.maxValue = (float)(b.sphereOfInfluence / mToKmScale);
      altitudeSlider.minValue = 0;
      altitudeSlider.SetValueWithoutNotify((float)((b.atmosphereDepth + defaultHeightAboveAtmo) / mToKmScale));
      altitudeTextArea.SetTextWithoutNotify(altitudeSlider.value.ToString("F0"));

      solarAltitudeSlider.maxValue = (float)(referenceFurthestCBAltitude / mToMmScale);
      solarAltitudeSlider.minValue = 0.0001f;
      solarAltitudeSlider.SetValueWithoutNotify((float)(bodySolarAltitude / mToMmScale));
      solarAltitudeTextArea.SetTextWithoutNotify(solarAltitudeSlider.value.ToString("F0"));

      RecalculateSolarParameters();
    }
    public void SetVisible(bool visibility)
    {

      situationHeaderObj.SetActive(visibility);
      situationDataObj.SetActive(visibility);
    }


    public void OnBodyDropdownChange()
    {
      Utils.Log($"[ToolbarSituation]: Selected body {bodyDropdown.options[bodyDropdown.value].text}", Utils.LogType.UI);
      foreach (CelestialBody body in FlightGlobals.Bodies)
      {
        if (body.bodyDisplayName.LocalizeRemoveGender() == bodyDropdown.options[bodyDropdown.value].text)
        {
          currentBody = body;
          SetBody(currentBody);
        }
      }
    }
    public void OnSolarAltSliderChange()
    {
      solarAltitudeTextArea.SetTextWithoutNotify(solarAltitudeSlider.value.ToString("F0"));
      RecalculateSolarParameters();
    }
    public void OnAltSliderChange()
    {
      altitudeTextArea.SetTextWithoutNotify(altitudeSlider.value.ToString("F0"));
      RecalculateSolarParameters();
    }
    public void OnSolarAltInputChange()
    {
      solarAltitudeSlider.SetValueWithoutNotify(float.Parse(solarAltitudeTextArea.text));
      RecalculateSolarParameters();
    }

    public void OnAltInputChange()
    {
      altitudeSlider.SetValueWithoutNotify(float.Parse(altitudeTextArea.text));
      RecalculateSolarParameters();
    }

    protected void RecalculateSolarParameters()
    {
      darkTime = CalculateOcclusionTime(currentBody, altitudeSlider.value);
      panelScalar = CalculatePanelScalar(solarAltitudeSlider.value);
      toolbar.UpdateSolarHandlerData();
    }


    /// <summary>
    /// Calculates a simulated eclipse time for orbit around a specific body
    /// </summary>
    protected double CalculateOcclusionTime(CelestialBody body, double bodyOrbitHeight)
    {
      if (bodyOrbitHeight == 0d)
      {
        return body.rotationPeriod /2d;

      }
      else
      {
        // This is a geometric approximation assuming a circular equatorial orbit and a cylindrical solar occlusion
        // Note the scaling factors for KM here
        double scaling = 1000.0d;
        double bodyRadius = body.Radius / scaling;
        double orbitDistance = (bodyRadius + bodyOrbitHeight);

        double eclipseFraction = (1d / Math.PI) * Math.Acos(Math.Sqrt(Math.Pow(bodyOrbitHeight, 2) + 2d * bodyRadius * bodyOrbitHeight) / (orbitDistance));
        double period = (2 * Math.PI) / (Math.Sqrt((body.gravParameter / Math.Pow(scaling, 3)) / Math.Pow(orbitDistance, 3)));
        return period * eclipseFraction;
      }
    }

    /// <summary>
    /// Calculates the scaling factor for solar panel power at this solar altitude
    /// </summary>
    protected float CalculatePanelScalar(double solarAltitude)
    {
      return (float)((1d / (solarAltitude * solarAltitude)) * (referenceSolarAltitude * referenceSolarAltitude) * starLuminanceScale);
    }
  }
}

