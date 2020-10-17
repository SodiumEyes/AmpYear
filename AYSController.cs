/**
 * AYSCController.cs
 *
 * AmpYear power management.
 * (C) Copyright 2015, Jamie Leighton
 * The original code and concept of AmpYear rights go to SodiumEyes on the Kerbal Space Program Forums, which was covered by GNU License GPL (no version stated).
 * As such this code continues to be covered by GNU GPL license.
 *
 * Concepts which are common to the Game Kerbal Space Program for which there are common code interfaces as such some of those concepts used
 * by this program were based on:-
 * Thunder Aerospace Corporation's Life Support for Kerbal Space Program.
 * Written by Taranis Elsu.
 * (C) Copyright 2013, Taranis Elsu
 * Which is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
 * creative commons license. See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>
 * for full details.
 *
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 *
 *  This file is part of AmpYear.
 *
 *  AmpYear is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  AmpYear is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with AmpYear.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using KSP.UI.Screens;
using UnityEngine;
using RSTUtils;
using RSTUtils.Extensions;
using System.Linq;
using System.Threading;
using KSP.Localization;

namespace AY
{
    internal class AYSCController : MonoBehaviour, ISavable
    {
        //GUI Properties
        private AppLauncherToolBar AYSCMenuAppLToolBar;
        private const float SCWINDOW_WIDTH = 450;
        private const float WINDOW_BASE_HEIGHT = 500;
        public Rect SCwindowPos = new Rect(40, Screen.height / 2 - 100, SCWINDOW_WIDTH, 200);
        private static int _windowId = new System.Random().Next();
        private static bool _debugging = false;
        private bool _toolTipsOn = true;
        private String _inputSmbdf = "";
        private double _inputVmbdf = 0f;
        private String _inputSrrt = "";
        private double _inputVrrt = 0f;
        private string _inputSccbdf = "";
        private double _inputVccbdf = 0f;
        private string _inputScbdf = "";
        private double _inputVcbdf = 0f;
        private string _inputSctt = "";
        private float _inputVctt = 0f;
        private string _inputSccuf = "";
        private double _inputVccuf = 0f;
        private string _inputSccrf = "";
        private double _inputVccrf = 0f;
        private string _inputScrrf = "";
        private double _inputVcrrf = 0f;
        private string _inputScmrf = "";
        private double _inputVcmrf = 0f;
        private string _inputScMinL = "";
        private double _inputVcMinL = 0f;
        private string _inputScMajL = "";
        private double _inputVcMajL = 0f;
        private string _inputSplw = "";
        private double _inputVplw = 0f;
        private string _inputSEmgcyShutOverrideCooldown = "";
        private double _inputVEmgcyShutOverrideCooldown = 0f;
        private bool _inputAppL = false;
        private bool _inputSdebug = _debugging;
        private bool _inputVdebug = _debugging;
        private bool _inputToolTipsOn = true;
        private bool _inputAYMonitoringUseEC = true;
        private Color _inputProdPartHighlightColor = Color.green;
        private Color _inputConsPartHighlightColor = Color.red;
        private string _inputSESPHighThreshold = "";
        private double _inputVESPHighThreshold = 0f;
        private string _inputSESPMediumThreshold = "";
        private double _inputVESPMediumThreshold = 0f;
        private string _inputSESPLowThreshold = "";
        private double _inputVESPLowThreshold = 0f;
        private List<KeyValuePair<ValidEmergencyPartModule, ESPValues>> _inputPartModuleEmergShutDnDflt = new List<KeyValuePair<ValidEmergencyPartModule, ESPValues>>();
        private bool LoadSettingsSC = true;
        public double CLIMATE_BASE_DRAIN_FACTOR = 1.0;
        public float CLIMATE_TARGET_TEMP = 20.0f;
        public double MASSAGE_BASE_DRAIN_FACTOR = 3.0;
        public double RECHARGE_RESERVE_THRESHOLD = 0.95;
        public double POWER_LOW_WARNING_AMT = 5;
        public double CRAZY_BASE_DRAIN_FACTOR = 0.05;
        public double CRAZY_CLIMATE_UNCOMF_FACTOR = 0.02;
        public double CRAZY_CLIMATE_REDUCE_FACTOR = 0.1;
        public double CRAZY_RADIO_REDUCE_FACTOR = 0.1;
        public double CRAZY_MASSAGE_REDUCE_FACTOR = 0.2;
        public double CRAZY_MINOR_LIMIT = 59;
        public double CRAZY_MAJOR_LIMIT = 89;
        private double ESPHighThreshold = 0.05;
        private double ESPMediumThreshold = 0.1;
        private double ESPLowThreshold = 0.2;
        private double EmgcyShutOverrideCooldown = 300;
        private bool AYMonitoringUseEC = true;
        private Color ProdPartHighlightColor = Color.green;
        private Color ConsPartHighlightColor = Color.red;
        private List<KeyValuePair<ValidEmergencyPartModule, ESPValues>> PartModuleEmergShutDnDflt = new List<KeyValuePair<ValidEmergencyPartModule, ESPValues>>();
        public bool Useapplauncher = false;
        private bool KKPresent = false;
        private Vector2 _bodscrollViewVector = Vector2.zero;
        private static string LOCK_ID = "AmpYear_KeyBinder";
        private string tmpToolTip;
        float prodPartRedValue = 1f;
        float prodPartGreenValue = 1f;
        float prodPartBlueValue = 1f;
        private Texture2D prodPartstyleTexture;
        private Texture2D consPartstyleTexture;
        float consPartRedValue = 1f;
        float consPartGreenValue = 1f;
        float consPartBlueValue = 1f;
        

        //AmpYear Savable settings
        private AYSettings _aYsettings;
        
        //Lifted this more or less directly from the Kerbal Engineer source. Thanks cybutek!
        /// <summary>
        ///     Gets and sets the input lock state.
        /// </summary>
        public bool InputLock
        {
            get
            {
                return InputLockManager.GetControlLock(LOCK_ID) != ControlTypes.None;
            }
            set
            {
                if (value)
                {
                    InputLockManager.SetControlLock(ControlTypes.KSC_ALL, LOCK_ID);
                }
                else
                {
                    InputLockManager.SetControlLock(ControlTypes.None, LOCK_ID);
                }
            }
        }

        public void Awake()
        {
            _aYsettings = AmpYear.Instance.AYsettings;
            AYSCMenuAppLToolBar = new AppLauncherToolBar(Localizer.Format("#autoLOC_AmpYear_1000001"), Localizer.Format("#autoLOC_AmpYear_1000001"), //#autoLOC_AmpYear_1000001 = AmpYear
                Textures.PathToolbarIconsPath + "/AYGreenOffTB",
                ApplicationLauncher.AppScenes.SPACECENTER,
                (Texture)Textures.IconGreenOn, (Texture)Textures.IconGreenOff,
                GameScenes.SPACECENTER);
            Utilities.Log_Debug("AYSCController Awake complete");
        }
        
        public void OnDestroy()
        {
            AYSCMenuAppLToolBar.Destroy();
            
            //if (GuiVisible) GuiVisible = !GuiVisible;
            InputLock = false;
        }

        public void Start()
        {
            Utilities.Log_Debug("AYSController Start");
            KKPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "KabinKraziness");
            Utilities.Log_Debug("Checked for mods");
            Utilities.Log_Debug(KKPresent ? "KabinKraziness present" : "KabinKraziness NOT present");

            //If TST Settings wants to use ToolBar mod, check it is installed and available. If not set the TST Setting to use Stock.
            if (!ToolbarManager.ToolbarAvailable && !_aYsettings.UseAppLauncher)
            {
                _aYsettings.UseAppLauncher = true;
            }

            AYSCMenuAppLToolBar.Start(_aYsettings.UseAppLauncher);

            prodPartstyleTexture = new Texture2D(1, 1);
            consPartstyleTexture = new Texture2D(1, 1);

            Utilities.Log_Debug("AYController Start complete");
        }

        /// <summary>
        ///     Called by unity every frame.
        /// </summary>
        protected virtual void Update()
        {
            UpdateInputLock();
        }

        //GUI Functions Follow

        private void OnGUI()
        {
            if (!AYSCMenuAppLToolBar.GuiVisible) return;
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                if (LoadSettingsSC)
                {
                    if (KKPresent)
                        LoadKKSettings();
                    _inputSccbdf = CLIMATE_BASE_DRAIN_FACTOR.ToString();
                    _inputVccbdf = CLIMATE_BASE_DRAIN_FACTOR;
                    _inputSctt = CLIMATE_TARGET_TEMP.ToString();
                    _inputVctt = CLIMATE_TARGET_TEMP;
                    _inputSmbdf = MASSAGE_BASE_DRAIN_FACTOR.ToString();
                    _inputVmbdf = MASSAGE_BASE_DRAIN_FACTOR;
                    _inputSrrt = (100 * RECHARGE_RESERVE_THRESHOLD).ToString();
                    _inputVrrt = RECHARGE_RESERVE_THRESHOLD;
                    _inputScbdf = CRAZY_BASE_DRAIN_FACTOR.ToString();
                    _inputVcbdf = CRAZY_BASE_DRAIN_FACTOR;
                    _inputSccuf = CRAZY_CLIMATE_UNCOMF_FACTOR.ToString();
                    _inputVccuf = CRAZY_CLIMATE_UNCOMF_FACTOR;
                    _inputSccrf = CRAZY_CLIMATE_REDUCE_FACTOR.ToString();
                    _inputVccrf = CRAZY_CLIMATE_REDUCE_FACTOR;
                    _inputScrrf = CRAZY_RADIO_REDUCE_FACTOR.ToString();
                    _inputVcrrf = CRAZY_RADIO_REDUCE_FACTOR;
                    _inputScmrf = CRAZY_MASSAGE_REDUCE_FACTOR.ToString();
                    _inputVcmrf = CRAZY_MASSAGE_REDUCE_FACTOR;
                    _inputScMinL = CRAZY_MINOR_LIMIT.ToString();
                    _inputVcMinL = CRAZY_MINOR_LIMIT;
                    _inputScMajL = CRAZY_MAJOR_LIMIT.ToString();
                    _inputVcMajL = CRAZY_MAJOR_LIMIT;
                    _inputSplw = POWER_LOW_WARNING_AMT.ToString();
                    _inputVplw = POWER_LOW_WARNING_AMT;
                    _inputAppL = Useapplauncher;
                    _inputSdebug = _debugging;
                    _inputVdebug = _debugging;
                    _inputToolTipsOn = _toolTipsOn;
                    _inputSESPHighThreshold = ESPHighThreshold.ToString();
                    _inputVESPHighThreshold = ESPHighThreshold;
                    _inputSESPMediumThreshold = ESPMediumThreshold.ToString();
                    _inputVESPMediumThreshold = ESPMediumThreshold;
                    _inputSESPLowThreshold = ESPLowThreshold.ToString();
                    _inputVESPLowThreshold = ESPLowThreshold;
                    _inputSEmgcyShutOverrideCooldown = EmgcyShutOverrideCooldown.ToString();
                    _inputVEmgcyShutOverrideCooldown = EmgcyShutOverrideCooldown;
                    _inputAYMonitoringUseEC = AYMonitoringUseEC;
                    _inputProdPartHighlightColor = ProdPartHighlightColor;
                    _inputConsPartHighlightColor = ConsPartHighlightColor;
                    _inputPartModuleEmergShutDnDflt.Clear();
                    foreach (KeyValuePair<ValidEmergencyPartModule, ESPValues> validentry in PartModuleEmergShutDnDflt)
                    {
                        _inputPartModuleEmergShutDnDflt.Add(new KeyValuePair<ValidEmergencyPartModule, ESPValues>(validentry.Key, validentry.Value));
                    }
                    LoadSettingsSC = false;
                }
                GUI.skin = HighLogic.Skin;
                SCwindowPos.ClampInsideScreen();
                SCwindowPos = GUILayout.Window(_windowId, SCwindowPos, WindowSc, Localizer.Format("#autoLOC_AmpYear_1000169"),		// #autoLOC_AmpYear_1000169 = AmpYear Power Manager Settings
                    GUILayout.Width(SCWINDOW_WIDTH), GUILayout.Height(WINDOW_BASE_HEIGHT));
                if (_toolTipsOn)
                    Utilities.DrawToolTip();
            }
        }

        private void WindowSc(int id)
        {
            //Init styles
            if (!Textures.StylesSet) Textures.SetupStyles();

            Utilities._TooltipStyle.normal.background = Textures.TooltipBox;
            Utilities._TooltipStyle.normal.textColor = new Color32(207, 207, 207, 255);
            Utilities._TooltipStyle.hover.textColor = Color.blue;

            GUIContent closeContent = new GUIContent(Textures.BtnRedCross, Localizer.Format("#autoLOC_AmpYear_1000007"));     //#autoLOC_AmpYear_1000007 = Close Window
            Rect closeRect = new Rect(SCwindowPos.width - 21, 4, 16, 16);
            if (GUI.Button(closeRect, closeContent, Textures.PartListbtnStyle))
            {
                AYSCMenuAppLToolBar.GuiVisible = false;
                LoadSettingsSC = true;
                return;
            }

            GUILayout.BeginVertical();
            _bodscrollViewVector = GUILayout.BeginScrollView(_bodscrollViewVector, GUILayout.Height(WINDOW_BASE_HEIGHT - 30), GUILayout.Width(SCWINDOW_WIDTH - 10));

            
            GUILayout.BeginHorizontal();
            GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000170"), Localizer.Format("#autoLOC_AmpYear_1000171")), Textures.StatusStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000170 = AmpYear Monitoring uses EC		// #autoLOC_AmpYear_1000171 = If ON AmpYear uses EC for it's monitoring functions. If Off it does not.
            _inputAYMonitoringUseEC = GUILayout.Toggle(_inputAYMonitoringUseEC, "", GUILayout.MinWidth(30.0F)); //you can play with the width of the text box
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000172"), Localizer.Format("#autoLOC_AmpYear_1000173")), Textures.StatusStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000172 = Reserve Battery Recharge Percentage		// #autoLOC_AmpYear_1000173 = The percentage of Main Power available before AmpYear begins recharging Reserve Batteries
            _inputSrrt = Regex.Replace(GUILayout.TextField(_inputSrrt, 3, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000174"), Localizer.Format("#autoLOC_AmpYear_1000175")), Textures.StatusStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000174 = Power Low Warning Percentage		// #autoLOC_AmpYear_1000175 = A Warning window will open when the percentage of Main Power Available reaches this percentage value
            _inputSplw = Regex.Replace(GUILayout.TextField(_inputSplw, 3, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box
            GUILayout.EndHorizontal();

            if (KKPresent)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000176"), Localizer.Format("#autoLOC_AmpYear_1000177")), Textures.StatusStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000176 = Climate Control Elec. Drain		// #autoLOC_AmpYear_1000177 = The amount of EC the Climate Control system will use per second
                _inputSccbdf = Regex.Replace(GUILayout.TextField(_inputSccbdf, 2, GUILayout.MinWidth(10.0F)), "[^.0-9]", ""); //you can play with the width of the text box
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000178"), Localizer.Format("#autoLOC_AmpYear_1000179")), Textures.StatusStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000178 = Climate Control Target Temp.		// #autoLOC_AmpYear_1000179 = The temperature in Celcius the Climate Control System will try to maintain in teh cabin
                _inputSctt = Regex.Replace(GUILayout.TextField(_inputSctt, 2, GUILayout.MinWidth(30.0F)), "[^.0-9]", ""); //you can play with the width of the text box
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000180"), Localizer.Format("#autoLOC_AmpYear_1000181")), Textures.StatusStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000180 = Massage Electrical Drain		// #autoLOC_AmpYear_1000181 = The amount of EC the Massage Chair system will use per second
                _inputSmbdf = Regex.Replace(GUILayout.TextField(_inputSmbdf, 2, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000182"), Localizer.Format("#autoLOC_AmpYear_1000183")), Textures.StatusStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000182 = Kraziness Base Growth Amount		// #autoLOC_AmpYear_1000183 = The percentage amount that Kraziness will grow per second
                _inputScbdf = Regex.Replace(GUILayout.TextField(_inputScbdf, 5, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000184"), Localizer.Format("#autoLOC_AmpYear_1000185")), Textures.StatusStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000184 = Temp Uncomfortable Kraziness		// #autoLOC_AmpYear_1000185 = The degrees Celcius where the actual cabin temperature is < less than or > greater than the target temperature. If this occurs Kraziness will go up faster
                _inputSccuf = Regex.Replace(GUILayout.TextField(_inputSccuf, 5, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000186"), Localizer.Format("#autoLOC_AmpYear_1000187")), Textures.StatusStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000186 = Climate Kraziness Reduction		// #autoLOC_AmpYear_1000187 = The percentage amount that Climate Control system will reduce Kraziness per second
                _inputSccrf = Regex.Replace(GUILayout.TextField(_inputSccrf, 5, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000188"), Localizer.Format("#autoLOC_AmpYear_1000189")), Textures.StatusStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000188 = Music Kraziness Reduction		// #autoLOC_AmpYear_1000189 = The percentage amount that the music system will reduce Kraziness per second
                _inputScrrf = Regex.Replace(GUILayout.TextField(_inputScrrf, 5, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000190"), Localizer.Format("#autoLOC_AmpYear_1000191")), Textures.StatusStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000190 = Massage Kraziness Reduction		// #autoLOC_AmpYear_1000191 = The percentage amount that the Massage chairs will reduce Kraziness per second
                _inputScmrf = Regex.Replace(GUILayout.TextField(_inputScmrf, 5, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000192"), Localizer.Format("#autoLOC_AmpYear_1000193")), Textures.StatusStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000192 = Kraziness Minor Problem Limit		// #autoLOC_AmpYear_1000193 = The percentage amount of Kraziness where minor bad things will start happening
                _inputScMinL = Regex.Replace(GUILayout.TextField(_inputScMinL, 5, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000194"), Localizer.Format("#autoLOC_AmpYear_1000195")), Textures.StatusStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000194 = Kraziness Major Problem Limit		// #autoLOC_AmpYear_1000195 = The percentage amount of Kraziness where major bad things will start happening
                _inputScMajL = Regex.Replace(GUILayout.TextField(_inputScMajL, 5, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box
                GUILayout.EndHorizontal();
            }

            if (!ToolbarManager.ToolbarAvailable)
            {
                GUI.enabled = false;
                tmpToolTip = Localizer.Format("#autoLOC_AmpYear_1000196");		// #autoLOC_AmpYear_1000196 = Not available unless ToolBar mod is installed
            }
            else
            {
                tmpToolTip =
                    Localizer.Format("#autoLOC_AmpYear_1000197");		// #autoLOC_AmpYear_1000197 = If ON Icon will appear in the stock Applauncher, if OFF Icon will appear in ToolBar mod
            }

            GUILayout.BeginHorizontal();
            GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000198"), tmpToolTip), Textures.StatusStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000198 = Use Application Launcher Button
            _inputAppL = GUILayout.Toggle(_inputAppL, "", GUILayout.MinWidth(30.0F)); //you can play with the width of the text box
            GUILayout.EndHorizontal();

            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000199"), Localizer.Format("#autoLOC_AmpYear_1000200")), Textures.StatusStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000199 = Debug Mode		// #autoLOC_AmpYear_1000200 = Creates logs of logging messages to help when there are problems, but will slow KSP down
            _inputSdebug = GUILayout.Toggle(_inputSdebug, "", GUILayout.MinWidth(30.0F)); //you can play with the width of the text box
            _inputVdebug = _inputSdebug;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000201"), Localizer.Format("#autoLOC_AmpYear_1000202")), Textures.StatusStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000201 = ToolTips On		// #autoLOC_AmpYear_1000202 = If On then you will see ToolTips like this one, If off - well you won't
            _inputToolTipsOn = GUILayout.Toggle(_inputToolTipsOn, "", GUILayout.MinWidth(30.0F)); //you can play with the width of the text box
            GUILayout.EndHorizontal();

            GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000203"), Localizer.Format("#autoLOC_AmpYear_1000204")), Textures.StatusStyle);		// #autoLOC_AmpYear_1000203 = EC Producer Module Highlight Color:		// #autoLOC_AmpYear_1000204 = The color that EC production modules will be highlighted with.\nMove the sliders to set the Color.
            GUILayout.BeginHorizontal();
            prodPartRedValue = _inputProdPartHighlightColor.r * 255f;
            prodPartGreenValue = _inputProdPartHighlightColor.g * 255f;
            prodPartBlueValue = _inputProdPartHighlightColor.b * 255f;
            prodPartRedValue = GUILayout.HorizontalSlider(prodPartRedValue, 0f, 255f);
            prodPartGreenValue = GUILayout.HorizontalSlider(prodPartGreenValue, 0f, 255f);
            prodPartBlueValue = GUILayout.HorizontalSlider(prodPartBlueValue, 0f, 255f);
            GUILayout.EndHorizontal();

            GUIStyle prodPartstyle = new GUIStyle();
            _inputProdPartHighlightColor = new Color(prodPartRedValue / 255f, prodPartGreenValue / 255f, prodPartBlueValue / 255f);
            prodPartstyleTexture.SetPixel(0, 0, _inputProdPartHighlightColor);
            prodPartstyleTexture.Apply();
            prodPartstyle.normal.background = prodPartstyleTexture;
            GUILayout.Box(new GUIContent(""), prodPartstyle, GUILayout.Height(20));


            GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000205"), Localizer.Format("#autoLOC_AmpYear_1000206")), Textures.StatusStyle);		// #autoLOC_AmpYear_1000205 = EC Consumer Module Highlight Color:		// #autoLOC_AmpYear_1000206 = The color that EC consumer modules will be highlighted with.\nMove the sliders to set the Color.
            GUILayout.BeginHorizontal();
            consPartRedValue = _inputConsPartHighlightColor.r * 255f;
            consPartGreenValue = _inputConsPartHighlightColor.g * 255f;
            consPartBlueValue = _inputConsPartHighlightColor.b * 255f;
            consPartRedValue = GUILayout.HorizontalSlider(consPartRedValue, 0f, 255f);
            consPartGreenValue = GUILayout.HorizontalSlider(consPartGreenValue, 0f, 255f);
            consPartBlueValue = GUILayout.HorizontalSlider(consPartBlueValue, 0f, 255f);
            GUILayout.EndHorizontal();

            GUIStyle consPartstyle = new GUIStyle();
            _inputConsPartHighlightColor = new Color(consPartRedValue / 255f, consPartGreenValue / 255f, consPartBlueValue / 255f);
            consPartstyleTexture.SetPixel(0, 0, _inputConsPartHighlightColor);
            consPartstyleTexture.Apply();
            consPartstyle.normal.background = consPartstyleTexture;
            GUILayout.Box(new GUIContent(""), consPartstyle, GUILayout.Height(20));
            
            GUILayout.BeginHorizontal();
            GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000207"), Localizer.Format("#autoLOC_AmpYear_1000208")), Textures.StatusStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000207 = ESP High Threshold		// #autoLOC_AmpYear_1000208 = The percentage of Main Power where Emergency Shutdow Procedure will shutdown/restart High priority modules. Must be >= 1%
            _inputSESPHighThreshold = Regex.Replace(GUILayout.TextField(_inputSESPHighThreshold, 5, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000209"), Localizer.Format("#autoLOC_AmpYear_1000210")), Textures.StatusStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000209 = ESP Medium Threshold		// #autoLOC_AmpYear_1000210 = The percentage of Main Power where Emergency Shutdow Procedure will shutdown/restart Medium priority modules. Must be >= High Threshold + 1%
            _inputSESPMediumThreshold = Regex.Replace(GUILayout.TextField(_inputSESPMediumThreshold, 5, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000211"), Localizer.Format("#autoLOC_AmpYear_1000212")), Textures.StatusStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000211 = ESP Low Threshold		// #autoLOC_AmpYear_1000212 = The percentage of Main Power where Emergency Shutdow Procedure will shutdown/restart Low priority modules. Must be >= Medium Threshold + 1%
            _inputSESPLowThreshold = Regex.Replace(GUILayout.TextField(_inputSESPLowThreshold, 5, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000213"), Localizer.Format("#autoLOC_AmpYear_1000214")), Textures.StatusStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000213 = Emergency Shutdown Cooldown Period		// #autoLOC_AmpYear_1000214 = The Cooldown period of time in seconds after an Emergency Shutdown is performed
            _inputSEmgcyShutOverrideCooldown = Regex.Replace(GUILayout.TextField(_inputSEmgcyShutOverrideCooldown, 5, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box
            GUILayout.EndHorizontal();

            GUILayout.Box(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000215"), Localizer.Format("#autoLOC_AmpYear_1000216")), Textures.SectionTitleStyle, GUILayout.Width(300));		// #autoLOC_AmpYear_1000215 = Emergency Shutdown Procedures - Module Defaults		// #autoLOC_AmpYear_1000216 = Default values for ESP system

            for (int i = _inputPartModuleEmergShutDnDflt.Count; i-- > 0;)
            {
                KeyValuePair<ValidEmergencyPartModule, ESPValues> validentry = _inputPartModuleEmergShutDnDflt[i];
                if (!KKPresent &&
                    (validentry.Key.Name == "ClimateControl" || validentry.Key.Name == "SmoothJazz" || validentry.Key.Name == "MassageChair"))
                    continue;
                GUILayout.BeginHorizontal();
                GUILayout.Box(Localizer.Format(validentry.Key.displayName), Textures.StatusStyle, GUILayout.Width(300));
                bool tmpBool = validentry.Value.EmergShutDnDflt;
                tmpBool = GUILayout.Toggle(tmpBool, new GUIContent("", Localizer.Format("#autoLOC_AmpYear_1000217")), GUILayout.MinWidth(30.0F)); //you can play with the width of the text box		// #autoLOC_AmpYear_1000217 = If ON any Parts with this Module will be included in Emergency Shutdown Procedures, If OFF then it won't be included
                List<GUIContent> tmpList = new List<GUIContent>();
                tmpList.Add(new GUIContent(Textures.BtnPriority1, Localizer.Format("#autoLOC_AmpYear_1000218")));		// #autoLOC_AmpYear_1000218 = Emergency Shutdown Procedures - Set Default for this Module to Priority One
                tmpList.Add(new GUIContent(Textures.BtnPriority2, Localizer.Format("#autoLOC_AmpYear_1000219")));		// #autoLOC_AmpYear_1000219 = Emergency Shutdown Procedures - Set Default for this Module to Priority Two
                tmpList.Add(new GUIContent(Textures.BtnPriority3, Localizer.Format("#autoLOC_AmpYear_1000220")));		// #autoLOC_AmpYear_1000220 = Emergency Shutdown Procedures - Set Default for this Module to Priority Three
                GUIContent[] tmpToggles = tmpList.ToArray();
                List<GUIStyle> tmpStylesList = new List<GUIStyle>();
                tmpStylesList.Add(Textures.PrioritybtnStyle);
                tmpStylesList.Add(Textures.PrioritybtnStyle);
                tmpStylesList.Add(Textures.PrioritybtnStyle);
                GUIStyle[] tmpStylesToggles = tmpStylesList.ToArray();
                int tmpESPPriority = Utilities.ToggleList((int)validentry.Value.EmergShutPriority - 1, tmpToggles, tmpStylesToggles, 20);
                var tmpIndex = _inputPartModuleEmergShutDnDflt.FindIndex(a => a.Key == validentry.Key);
                ESPValues tmpEspValues = new ESPValues(tmpBool, (ESPPriority)(tmpESPPriority + 1));
                _inputPartModuleEmergShutDnDflt[tmpIndex] = new KeyValuePair<ValidEmergencyPartModule, ESPValues>(validentry.Key, tmpEspValues);
                GUILayout.EndHorizontal();
            }
           
            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            if (!Double.TryParse(_inputSrrt, out _inputVrrt))
            {
                _inputVrrt = RECHARGE_RESERVE_THRESHOLD;
            }
            if (!Double.TryParse(_inputSplw, out _inputVplw))
            {
                _inputVplw = POWER_LOW_WARNING_AMT;
            }
            if (!Double.TryParse(_inputSESPHighThreshold, out _inputVESPHighThreshold))
            {
                _inputVESPHighThreshold = ESPHighThreshold;
            }
            if (!Double.TryParse(_inputSESPMediumThreshold, out _inputVESPMediumThreshold))
            {
                _inputVESPMediumThreshold = ESPMediumThreshold;
            }
            if (!Double.TryParse(_inputSESPLowThreshold, out _inputVESPLowThreshold))
            {
                _inputVESPLowThreshold = ESPLowThreshold;
            }
            //Apply rules to the ESPThresholds. High must be 1% or higher
            // Medium must be 5% more than High or higher.
            // Low must be 5% more than Medium or higher.
            if (_inputVESPHighThreshold < 1)
            {
                _inputVESPHighThreshold = ESPHighThreshold = 1;
                _inputSESPHighThreshold = _inputVESPHighThreshold.ToString();
                ScreenMessages.PostScreenMessage(
                                    Localizer.Format("#autoLOC_AmpYear_1000221"), 5.0f,		// #autoLOC_AmpYear_1000221 = ESP High Threshold must be 1% or greater. Reset to 1%
                                    ScreenMessageStyle.UPPER_CENTER);
            }
            if (_inputVESPMediumThreshold < ESPHighThreshold + 5)
            {
                _inputVESPMediumThreshold = ESPMediumThreshold = ESPHighThreshold + 5;
                _inputSESPMediumThreshold = _inputVESPMediumThreshold.ToString();
                ScreenMessages.PostScreenMessage(
                                    Localizer.Format("#autoLOC_AmpYear_1000222", _inputVESPMediumThreshold.ToString("##")), 5.0f,		// #autoLOC_AmpYear_1000222 = ESP Medium Threshold must be 5% More than High Threshhold. Reset to 
                                    ScreenMessageStyle.UPPER_CENTER);
            }
            if (_inputVESPLowThreshold < ESPMediumThreshold + 5)
            {
                _inputVESPLowThreshold = ESPLowThreshold = ESPMediumThreshold + 5;
                _inputSESPLowThreshold = _inputVESPLowThreshold.ToString();
                ScreenMessages.PostScreenMessage(
                                    Localizer.Format("#autoLOC_AmpYear_1000223", _inputVESPLowThreshold.ToString("##")), 5.0f,		// #autoLOC_AmpYear_1000223 = ESP Low Threshold must be 5% More than Medium Threshhold. Reset to 
                                    ScreenMessageStyle.UPPER_CENTER);
            }
            if (!Double.TryParse(_inputSEmgcyShutOverrideCooldown, out _inputVEmgcyShutOverrideCooldown))
            {
                _inputVEmgcyShutOverrideCooldown = EmgcyShutOverrideCooldown;
            }
            if (_inputVEmgcyShutOverrideCooldown < 30)
            {
                _inputVEmgcyShutOverrideCooldown = 30;
                _inputSEmgcyShutOverrideCooldown = "30";
                ScreenMessages.PostScreenMessage(
                                    Localizer.Format("#autoLOC_AmpYear_1000224"), 5.0f,ScreenMessageStyle.UPPER_CENTER);		// #autoLOC_AmpYear_1000224 = ESP Cooldown Period must be at least 30 secs. Reset to 30 secs.
            }

            if (KKPresent)
            {
                if (!Double.TryParse(_inputSccbdf, out _inputVccbdf))
                {
                    _inputVccbdf = CLIMATE_BASE_DRAIN_FACTOR;
                }
                if (!float.TryParse(_inputSctt, out _inputVctt))
                {
                    _inputVctt = CLIMATE_TARGET_TEMP;
                }
                if (!Double.TryParse(_inputSmbdf, out _inputVmbdf))
                {
                    _inputVmbdf = MASSAGE_BASE_DRAIN_FACTOR;
                }
                if (!Double.TryParse(_inputScbdf, out _inputVcbdf))
                {
                    _inputVcbdf = CRAZY_BASE_DRAIN_FACTOR;
                }
                if (!Double.TryParse(_inputSccuf, out _inputVccuf))
                {
                    _inputVccuf = CRAZY_CLIMATE_UNCOMF_FACTOR;
                }
                if (!Double.TryParse(_inputSccrf, out _inputVccrf))
                {
                    _inputVccrf = CRAZY_CLIMATE_REDUCE_FACTOR;
                }
                if (!Double.TryParse(_inputScrrf, out _inputVcrrf))
                {
                    _inputVcrrf = CRAZY_RADIO_REDUCE_FACTOR;
                }
                if (!Double.TryParse(_inputScmrf, out _inputVcmrf))
                {
                    _inputVcmrf = CRAZY_MASSAGE_REDUCE_FACTOR;
                }
                if (!Double.TryParse(_inputScMinL, out _inputVcMinL))
                {
                    _inputVcMinL = CRAZY_MINOR_LIMIT;
                }
                if (!Double.TryParse(_inputScMajL, out _inputVcMajL))
                {
                    _inputVcMajL = CRAZY_MAJOR_LIMIT;
                }
            }

            if (GUILayout.Button(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000225"), Localizer.Format("#autoLOC_AmpYear_1000226"))))		// #autoLOC_AmpYear_1000225 = Save Settings		// #autoLOC_AmpYear_1000226 = This will save you settings changes to the config.cfg file
            {
                CLIMATE_BASE_DRAIN_FACTOR = _inputVccbdf;
                CLIMATE_TARGET_TEMP = _inputVctt;
                MASSAGE_BASE_DRAIN_FACTOR = _inputVmbdf;
                RECHARGE_RESERVE_THRESHOLD = _inputVrrt / 100;
                POWER_LOW_WARNING_AMT = _inputVplw;
                CRAZY_BASE_DRAIN_FACTOR = _inputVcbdf;
                CRAZY_CLIMATE_UNCOMF_FACTOR = _inputVccuf;
                CRAZY_CLIMATE_REDUCE_FACTOR = _inputVccrf;
                CRAZY_RADIO_REDUCE_FACTOR = _inputVcrrf;
                CRAZY_MASSAGE_REDUCE_FACTOR = _inputVcmrf;
                CRAZY_MINOR_LIMIT = _inputVcMinL;
                CRAZY_MAJOR_LIMIT = _inputVcMajL;
                if (Useapplauncher != _inputAppL)
                {
                    Useapplauncher = _inputAppL;
                    AYSCMenuAppLToolBar.chgAppIconStockToolBar(Useapplauncher);
                }
                Useapplauncher = _inputAppL;
                _debugging = _inputVdebug;
                Utilities.debuggingOn = _debugging;
                _toolTipsOn = _inputToolTipsOn;
                ESPHighThreshold = _inputVESPHighThreshold;
                ESPMediumThreshold = _inputVESPMediumThreshold;
                ESPLowThreshold = _inputVESPLowThreshold;
                EmgcyShutOverrideCooldown = _inputVEmgcyShutOverrideCooldown;
                AYMonitoringUseEC = _inputAYMonitoringUseEC;
                ProdPartHighlightColor = _inputProdPartHighlightColor;
                ConsPartHighlightColor = _inputConsPartHighlightColor;
                //Textures.SetupHighLightStyles(ProdPartHighlightColor, ConsPartHighlightColor);
                PartModuleEmergShutDnDflt.Clear();
                foreach (KeyValuePair<ValidEmergencyPartModule, ESPValues> validentry in _inputPartModuleEmergShutDnDflt)
                {
                    PartModuleEmergShutDnDflt.Add(new KeyValuePair<ValidEmergencyPartModule, ESPValues>(validentry.Key, validentry.Value));
                }
                LoadSettingsSC = true;
                if (KKPresent)
                    SaveKKSettings();
            }
            if (GUILayout.Button(new GUIContent(Localizer.Format("#autoLOC_AmpYear_1000227"), Localizer.Format("#autoLOC_AmpYear_1000228"))))		// #autoLOC_AmpYear_1000227 = Reset Settings		// #autoLOC_AmpYear_1000228 = This will reload the settings from the config.cfg without saving your changes
            {
                LoadSettingsSC = true;
            }
            if (_toolTipsOn)
                Utilities.SetTooltipText();
            GUI.DragWindow();
           
        }

        /// <summary>
        ///     Updates the input lock.
        /// </summary>
        private void UpdateInputLock()
        {
            bool mouseOver = false; // position.MouseIsOver();
            if (AYSCMenuAppLToolBar.GuiVisible)
            {
                mouseOver = SCwindowPos.MouseIsOver();
                bool inputLock = InputLock;

                if (mouseOver && inputLock == false)
                {
                    InputLock = true;
                }
                else if (mouseOver == false && inputLock)
                {
                    InputLock = false;
                }
            }
            else
            {
                InputLock = false;
            }
        }


        //Class Load and Save of global settings
        public void Load(ConfigNode globalNode)
        {
            Utilities.Log_Debug("AYSCController Load");
            SCwindowPos.x = _aYsettings.SCwindowPosX;
            SCwindowPos.y = _aYsettings.SCwindowPosY;
            RECHARGE_RESERVE_THRESHOLD = _aYsettings.RECHARGE_RESERVE_THRESHOLD;
            POWER_LOW_WARNING_AMT = _aYsettings.POWER_LOW_WARNING_AMT;
            Useapplauncher = _aYsettings.UseAppLauncher;
            _debugging = _aYsettings.debugging;
            Utilities.debuggingOn = _debugging;
            _toolTipsOn = _aYsettings.TooltipsOn;
            ESPHighThreshold = _aYsettings.ESPHighThreshold;
            ESPMediumThreshold = _aYsettings.ESPMediumThreshold;
            ESPLowThreshold = _aYsettings.ESPLowThreshold;
            EmgcyShutOverrideCooldown = _aYsettings.EmgcyShutOverrideCooldown;
            AYMonitoringUseEC = _aYsettings.AYMonitoringUseEC;
            ProdPartHighlightColor = _aYsettings.ProdPartHighlightColor;
            ConsPartHighlightColor = _aYsettings.ConsPartHighlightColor;
            PartModuleEmergShutDnDflt.Clear();
            foreach (KeyValuePair<ValidEmergencyPartModule, ESPValues> validentry in _aYsettings.PartModuleEmergShutDnDflt)
            {
                PartModuleEmergShutDnDflt.Add(new KeyValuePair<ValidEmergencyPartModule, ESPValues>(validentry.Key, validentry.Value));
            }
            Utilities.Log_Debug("AYController Load end");
        }

        public void Save(ConfigNode globalNode)
        {
            Utilities.Log_Debug("AYSCController Save");
            _aYsettings.SCwindowPosX = SCwindowPos.x;
            _aYsettings.SCwindowPosY = SCwindowPos.y;
            _aYsettings.RECHARGE_RESERVE_THRESHOLD = RECHARGE_RESERVE_THRESHOLD;
            _aYsettings.POWER_LOW_WARNING_AMT = POWER_LOW_WARNING_AMT;
            _aYsettings.UseAppLauncher = Useapplauncher;
            _aYsettings.debugging = _debugging;
            Utilities.debuggingOn = _debugging;
            _aYsettings.TooltipsOn = _toolTipsOn;
            _aYsettings.ESPHighThreshold = ESPHighThreshold;
            _aYsettings.ESPMediumThreshold = ESPMediumThreshold;
            _aYsettings.ESPLowThreshold = ESPLowThreshold;
            _aYsettings.EmgcyShutOverrideCooldown = EmgcyShutOverrideCooldown;
            _aYsettings.AYMonitoringUseEC = AYMonitoringUseEC;
            _aYsettings.ProdPartHighlightColor = ProdPartHighlightColor;
            _aYsettings.ConsPartHighlightColor = ConsPartHighlightColor;
            _aYsettings.PartModuleEmergShutDnDflt.Clear();
            foreach (KeyValuePair<ValidEmergencyPartModule, ESPValues> validentry in PartModuleEmergShutDnDflt)
            {
                _aYsettings.PartModuleEmergShutDnDflt.Add(new KeyValuePair<ValidEmergencyPartModule, ESPValues>(validentry.Key, validentry.Value)); 
            }
            Textures.SetupHighLightStyles(ProdPartHighlightColor, ConsPartHighlightColor);
            Utilities.Log_Debug("AYSController Save end");
        }

        public void LoadKKSettings()
        {
            //KabinKraziness.Ikkaddon _KK = KKClient.GetKK();
            if (KKWrapper.APIReady)
            {
                CLIMATE_BASE_DRAIN_FACTOR = KKWrapper.KKactualAPI.CLMT_BSE_DRN_FTR;
                CLIMATE_TARGET_TEMP = KKWrapper.KKactualAPI.CLMT_TGT_TMP;
                MASSAGE_BASE_DRAIN_FACTOR = KKWrapper.KKactualAPI.MSG_BSE_DRN_FTR;
                CRAZY_BASE_DRAIN_FACTOR = KKWrapper.KKactualAPI.CRZ_BSE_DRN_FTR;
                CRAZY_CLIMATE_UNCOMF_FACTOR = KKWrapper.KKactualAPI.CRZ_CTE_UNC_FTR;
                CRAZY_CLIMATE_REDUCE_FACTOR = KKWrapper.KKactualAPI.CRZ_CTE_RED_FTR;
                CRAZY_RADIO_REDUCE_FACTOR = KKWrapper.KKactualAPI.CRZ_RDO_RED_FTR;
                CRAZY_MASSAGE_REDUCE_FACTOR = KKWrapper.KKactualAPI.CRZ_MSG_RED_FTR;
                CRAZY_MINOR_LIMIT = KKWrapper.KKactualAPI.CRZ_MINOR_LMT;
                CRAZY_MAJOR_LIMIT = KKWrapper.KKactualAPI.CRZ_MAJOR_LMT;
            }
        }

        public void SaveKKSettings()
        {
            //KabinKraziness.Ikkaddon _KK = KKClient.GetKK();
            if (KKWrapper.APIReady)
            {
                KKWrapper.KKactualAPI.CLMT_BSE_DRN_FTR = CLIMATE_BASE_DRAIN_FACTOR;
                KKWrapper.KKactualAPI.CLMT_TGT_TMP = CLIMATE_TARGET_TEMP;
                KKWrapper.KKactualAPI.MSG_BSE_DRN_FTR = MASSAGE_BASE_DRAIN_FACTOR;
                KKWrapper.KKactualAPI.CRZ_BSE_DRN_FTR = CRAZY_BASE_DRAIN_FACTOR;
                KKWrapper.KKactualAPI.CRZ_CTE_UNC_FTR = CRAZY_CLIMATE_UNCOMF_FACTOR;
                KKWrapper.KKactualAPI.CRZ_CTE_RED_FTR = CRAZY_CLIMATE_REDUCE_FACTOR;
                KKWrapper.KKactualAPI.CRZ_RDO_RED_FTR = CRAZY_RADIO_REDUCE_FACTOR;
                KKWrapper.KKactualAPI.CRZ_MSG_RED_FTR = CRAZY_MASSAGE_REDUCE_FACTOR;
                KKWrapper.KKactualAPI.CRZ_MINOR_LMT = CRAZY_MINOR_LIMIT;
                KKWrapper.KKactualAPI.CRZ_MAJOR_LMT = CRAZY_MAJOR_LIMIT;
            }
        }
    }
}