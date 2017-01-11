/**
 * AYController.cs
 *
 * AmpYear power management.
 * (C) Copyright 2015, Jamie Leighton
 * The original code and concept of AmpYear rights go to SodiumEyes on the Kerbal Space Program Forums, which was covered by GNU License GPL (no version stated).
 * As such this code continues to be covered by GNU GPL license.
 * Parts of this code were copied from Fusebox by the user ratzap on the Kerbal Space Program Forums, which is covered by GNU License GPLv2.
 * Concepts which are common to the Game Kerbal Space Program for which there are common code interfaces as such some of those concepts used
 * by this program were based on:
 * Thunder Aerospace Corporation's Life Support for Kerbal Space Program.
 * Written by Taranis Elsu.
 * (C) Copyright 2013, Taranis Elsu
 * Which is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
 * creative commons license. See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>
 * for full details.
 *
 * Thanks go to both ratzap and Taranis Elsu for their code.
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
using UnityEngine;
using RSTUtils;
using RSTUtils.Extensions;

namespace AY
{
    public partial class AYController : MonoBehaviour
    {
        //GUI Properties
        //private IButton _button1;
        private IconAlertState _iconAlertState = IconAlertState.GREEN;
        private AppLauncherToolBar AYMenuAppLToolBar;
        //private ApplicationLauncherButton _stockToolbarButton = null; // Stock Toolbar Button
        private readonly double[] _reserveTransferIncrements = new double[3] { 0.25, 0.1, 0.01 };
        private String[] incrementPercentString = new String[3];
        private bool[] _guiSectionEnableFlag = new bool[LoadGlobals.GuiSectionArrayCache.Length];
        private const float FWINDOW_WIDTH = 220;
        private const float EWINDOW_WIDTH = 220;
        private const float WINDOW_BASE_HEIGHT = 140;
        private Vector2 _plProdscrollViewVector = Vector2.zero;
        private Vector2 _plConsscrollViewVector = Vector2.zero;
        private Vector2 _dSscrollViewVector = Vector2.zero;
        private GUILayoutOption[] SubsystemButtonOptions;
        private static int _fwindowId;
        private static int _ewindowId;
        private static int _dwindowId;
        private static int _wwindowId;
        private static int _swindowId;
        private static string LOCK_ID = "AmpYear_KeyBinder";
        private bool[] _subsystemToggle = new bool[LoadGlobals.SubsystemArrayCache.Length]; // Enum.GetValues(typeof(Subsystem)).Length];
        private double[] _subsystemDrain = new double[LoadGlobals.SubsystemArrayCache.Length]; //Enum.GetValues(typeof(Subsystem)).Length];
        private bool _managerEnabled = true;
        private bool _showCrew = false;
        private bool _showParts = false;
        private double _timeLastElectricity;
        private double _lastUpdate;
        private float _powerUpTime = 0.0f;
        public static bool EmgcyShutActive = false;
        public static bool EmgcyShutOverride = false;
        public static bool EmgcyShutOverrideStarted = false;
        public static double EmgcyShutOverrideTmeStarted = 0;
        private static bool _checkLowEcWarning = true;
        private static bool _lowEcWarningWindowDisplay = false;
        internal static bool ShowDarkSideWindow = false;
        private CelestialBody _bodyTarget;
        private int _darkTargetSelection = -1;
        private int _selectedDarkTarget = -1;
        //private string _selectedHighlitePart = "";
        private int _showDarkOrbit = 100;
        private List<CelestialBody> _darkBodies = new List<CelestialBody>();
        private Rect _fwindowPos = new Rect(40, Screen.height / 2 - 100, FWINDOW_WIDTH, 200); // Flight Window position and size
        private Rect _ewindowPos = new Rect(40, Screen.height / 2 - 100, EWINDOW_WIDTH, 200); // Editor Window position and size
        private Rect _dwindowPos = new Rect(40, Screen.height / 2 - 100, 320, 200); // DarkSide Window position and size
        private Rect _epLwindowPos = new Rect(270, Screen.height / 2 - 100, 650, 600); //Extended Parts List Window position and size
        private float _eplPartName, _eplPartModuleName, _eplec, _eplProdListHeight, _eplConsListHeight;
        private Rect _eplHeaders2, _eplProdlistbox, _eplConslistbox;
        private bool _includeStoredEc = true;
        private bool _includeSoredRp = true;
        private bool _lockReservePower;  //Blocks usage of ReservePower automatically
        private bool PrtProdEditorIncludeAll = true;
        private bool PrtProdESPIncludeAll = true;
        private bool PrtConsEditorIncludeAll = true;
        private bool PrtConsESPIncludeAll = true;
        private double timeRemainReserve;
        private double timeRemainMains;
        private double powerPercent;
        private double reservePercent;
        private string ststext = "";
        private bool tmpShowDarkSideWindow;
        private float _totalProdPower = 0f;
        private string partModuleName = string.Empty;
        private int Orbit;
        private string strOrbit;
        private int tmpESPPriority;
        private bool tmpPrtConsEditorIncludeAll, tmpPrtConsEmergShutDnIncludeAll, tmpPrtConsOneAll, tmpPrtConsThreeAll, tmpPrtConsTwoAll;

        private bool tmpPrtEditorInclude,
            tmpPrtEmergShutDnInclude,
            tmpPrtProdEditorIncludeAll,
            tmpPrtProdEmergShutDnIncludeAll;

        private double tmpPrtPowerV;
        private string tmpPrtPower, Units;
        private bool tmpPrtProdOneAll, tmpPrtProdThreeAll, tmpPrtProdTwoAll;
        private float _totalConsPower;
        private Vector3d sun_dir;
        private double sun_dist;
        private double darkTime, eCdifference, ECprodfordarkTime, ECreqdfordarkTime, ECreqdfordarkTimeSurface, ECprodfordarkTimeSurface, powerSupply, rotPeriod;
        private uint tmpPartKey;

        #region GUIFunctions
        
        private ControlTypes EditorLocks = ControlTypes.EDITOR_ICON_HOVER | ControlTypes.EDITOR_ICON_PICK | ControlTypes.EDITOR_TAB_SWITCH | ControlTypes.EDITOR_PAD_PICK_PLACE | 
            ControlTypes.EDITOR_PAD_PICK_COPY | ControlTypes.EDITOR_GIZMO_TOOLS | ControlTypes.EDITOR_ROOT_REFLOW | ControlTypes.EDITOR_SYM_SNAP_UI | ControlTypes.EDITOR_EDIT_STAGES | 
            ControlTypes.EDITOR_UNDO_REDO | ControlTypes.EDITOR_MODE_SWITCH | ControlTypes.EDITOR_UI;

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
                    InputLockManager.SetControlLock(EditorLocks | ControlTypes.UI_DIALOGS, LOCK_ID);
                }
                else
                {
                    InputLockManager.SetControlLock(ControlTypes.None, LOCK_ID);
                }
            }
        }

        //GUI Functions Follow

        private void OnGUI()
        {
            try
            {
                if (!Textures.StylesSet) Textures.SetupStyles();
            }
            catch (Exception ex)
            {
                Utilities.Log("Unable to set GUI Styles to draw the GUI");
                Utilities.Log("Exception: {0}", ex);
            }
            try
            {
                SetIcon();
            }
            catch (Exception ex)
            {
                Utilities.Log("Unable to set GUI Icon");
                Utilities.Log("Exception: {0}", ex);
            }

            if (!AYMenuAppLToolBar.GuiVisible || AYMenuAppLToolBar.gamePaused || AYMenuAppLToolBar.hideUI || (!Utilities.GameModeisEditor && !Utilities.GameModeisFlight)) return;

            GUI.skin = HighLogic.Skin;
            if (Utilities.GameModeisFlight)  //FlightScene GUI
            {
                try
                {
                    _fwindowPos.ClampInsideScreen();
                    _fwindowPos = GUILayout.Window(_fwindowId, _fwindowPos, WindowF, "AmpYear", GUILayout.Width(FWINDOW_WIDTH), GUILayout.Height(WINDOW_BASE_HEIGHT));
                    if (_showParts)
                    {
                        _epLwindowPos.ClampToScreen();
                        _epLwindowPos = GUILayout.Window(_swindowId, _epLwindowPos, WindowScrollParts, "AmpYear Parts List", GUILayout.Width(_epLwindowPos.width), GUILayout.Height(_epLwindowPos.height), GUILayout.MinWidth(150), GUILayout.MinHeight(150));
                    }
                    CheckPowerLowWarning();
                    if (_lowEcWarningWindowDisplay)
                    {
                        PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "AmpYear Warning!",
                            "Ship Electric charge has dropped below the Warp Warning Percentage.\n This will not trigger again until Electric charge > Warning Percentage again.", "OK", false, HighLogic.UISkin);
                        _lowEcWarningWindowDisplay = false;
                    }
                }
                catch (Exception ex)
                {
                    Utilities.Log("Unable to draw FlightScene GUI");
                    Utilities.Log("Exception: {0}", ex);
                }
            }

            if (Utilities.GameModeisEditor)
            {
                try
                {
                    _ewindowPos.ClampInsideScreen();
                    _ewindowPos = GUILayout.Window(_ewindowId, _ewindowPos, WindowE, "AmpYear", GUILayout.Width(EWINDOW_WIDTH), GUILayout.Height(WINDOW_BASE_HEIGHT));
                    if (_showParts)
                    {
                        _epLwindowPos.ClampToScreen();
                        _epLwindowPos = GUILayout.Window(_swindowId, _epLwindowPos, WindowScrollParts, "AmpYear Parts List", GUILayout.Width(_epLwindowPos.width), GUILayout.Height(_epLwindowPos.height), GUILayout.MinWidth(150), GUILayout.MinHeight(150));
                    }
                }
                catch (Exception ex)
                {
                    Utilities.Log("Unable to draw EditorScene GUI");
                    Utilities.Log("Exception: {0}", ex);
                }
            }

            if (ShowDarkSideWindow)
            {
                try
                {
                    _dwindowPos.ClampToScreen();
                    _dwindowPos = GUILayout.Window(_dwindowId, _dwindowPos, WindowD, "AmpYear Dark-Side & Solar SOI", GUILayout.MinWidth(330), GUILayout.MinHeight(320));
                }
                catch (Exception ex)
                {
                    Utilities.Log("Unable to draw DarkSide GUI");
                    Utilities.Log("Exception: {0}", ex);
                }
            }
            
            if (AYsettings.TooltipsOn)
                Utilities.DrawToolTip();
        }

        /// <summary>
        ///     Called by unity every frame.
        /// </summary>
        protected virtual void Update()
        {
            UpdateInputLock();
        }

        private void CheckPowerLowWarning()
        {
            double chrgpct = TotalElectricCharge / TotalElectricChargeCapacity * 100;
            if (_checkLowEcWarning && !_lowEcWarningWindowDisplay) // If Warning is on check if it's triggered
            {
                if ((TimeWarp.CurrentRate > 0) && (chrgpct < AYsettings.POWER_LOW_WARNING_AMT)) // We have hit the warning stop timewarp and show warning
                {
                    Utilities.Log_Debug("cutting timewarp power warning limit reached");
                    TimeWarp.SetRate(0, false);
                    _checkLowEcWarning = false;
                    _lowEcWarningWindowDisplay = true;
                }
            }
            if ((chrgpct > AYsettings.POWER_LOW_WARNING_AMT) && !_checkLowEcWarning) // Reset the Warning indicator
            {
                Utilities.Log_Debug("Reset power warning");
                _checkLowEcWarning = true;
            }
        }

        private void WindowF(int id)
        {
            _iconAlertState = IconAlertState.GREEN;
            GUIContent closeContent = new GUIContent(Textures.BtnRedCross, "Close Window");
            Rect closeRect = new Rect(_fwindowPos.width - 21, 4, 16, 16);
            if (GUI.Button(closeRect, closeContent, Textures.PartListbtnStyle))
            {
                AYMenuAppLToolBar.onAppLaunchToggle();
                return;
            }
            AYsettings.showSI = GUI.Toggle(new Rect(_fwindowPos.width - 45, 4, 16, 16), AYsettings.showSI, new GUIContent(Textures.BtnIS, "Toggle the display to use EC or SI units"), Textures.PartListbtnStyle);//, SubsystemButtonOptions);


            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            for (int i = LoadGlobals.GuiSectionArrayCache.Length - 1; i >= 0; --i)
            {
                if (LoadGlobals.GuiSectionArrayCache[i] == GUISection.LUXURY)
                {
                    if (KKPresent)
                    {
                        _guiSectionEnableFlag[(int)LoadGlobals.GuiSectionArrayCache[i]]
                        = GUILayout.Toggle(_guiSectionEnableFlag[(int)LoadGlobals.GuiSectionArrayCache[i]], GuiSectionName(LoadGlobals.GuiSectionArrayCache[i]), GUI.skin.button);
                    }
                    else
                    {
                        _guiSectionEnableFlag[(int)LoadGlobals.GuiSectionArrayCache[i]] = false;
                    }
                }
                else
                {
                    _guiSectionEnableFlag[(int)LoadGlobals.GuiSectionArrayCache[i]]
                    = GUILayout.Toggle(_guiSectionEnableFlag[(int)LoadGlobals.GuiSectionArrayCache[i]], GuiSectionName(LoadGlobals.GuiSectionArrayCache[i]), GUI.skin.button);
                }
            }
            GUILayout.EndHorizontal();

            //Manager status+drain
            if (TimewarpIsValid)
            {
                GUILayout.BeginHorizontal();
                if (_rt2Present && !RT2UnderControl)
                {
                    GUI.enabled = false;
                    GUILayout.Label(new GUIContent("Remote Tech - No Control", "No Remote Tech Connection or Local Control - Unable to use AmpYear Functions."), Textures.AlertStyle);
                }
                _managerEnabled = GUILayout.Toggle(_managerEnabled, new GUIContent("Manager", "Turn on to Enable the AmpYear Management Unit"), Textures.SubsystemButtonStyle, SubsystemButtonOptions);
                GUI.enabled = true;
                if (ManagerIsActive)
                    ConsumptionLabel(ManagerCurrentDrain, false);
                else
                    ConsumptionLabel(managerActiveDrain, true);
                GUILayout.EndHorizontal();
            }

            //Manager status label
            if (hasPower || HasReservePower)
            {
                if (ManagerIsActive)
                {
                    if (hasPower)
                    {
                        if (TotalElectricChargeCapacity > 0.0)
                        {
                            powerPercent = TotalElectricCharge / TotalElectricChargeCapacity * 100.0;
                            if (powerPercent < 20.00)
                            {
                                GUILayout.Label(new GUIContent("Power: " + powerPercent.ToString("0.00") + '%', "The Total Percentage of Main Power stored as a percentage of total capacity"), Textures.AlertStyleLeft);
                                SetIconalertstate(IconAlertState.RED);
                            }
                            else
                            {
                                if (powerPercent < 35.00)
                                {
                                    GUILayout.Label(new GUIContent("Power: " + powerPercent.ToString("0.00") + '%', "The Total Percentage of Main Power stored as a percentage of total capacity"), Textures.WarningStyleLeft);

                                    SetIconalertstate(IconAlertState.YELLOW);
                                }
                                else
                                    GUILayout.Label(new GUIContent("Power: " + powerPercent.ToString("0.00") + '%', "The Total Percentage of Main Power stored as a percentage of total capacity"), Textures.StatusStyleLeft);
                            }
                            if (AYsettings.showSI)
                            {
                                tmpPrtPowerV = Utilities.ConvertECtoSI(TotalPowerDrain, out Units);
                                tmpPrtPower = tmpPrtPowerV.ToString("0.##") + Units;
                            }
                            else
                            {
                                tmpPrtPower = TotalPowerDrain.ToString("0.##");
                            }
                            if (TotalPowerDrain > TotalPowerProduced)
                            {
                                GUILayout.Label(new GUIContent("Power Drain : " + tmpPrtPower, "The Total Power Drain on this vessel"), Textures.AlertStyleLeft);
                                SetIconalertstate(IconAlertState.RED);
                            }
                            else
                                GUILayout.Label(new GUIContent("Power Drain : " + tmpPrtPower, "The Total Power Drain on this vessel"), Textures.StatusStyleLeft);

                            if (AYsettings.showSI)
                            {
                                tmpPrtPowerV = Utilities.ConvertECtoSI(TotalPowerProduced, out Units);
                                tmpPrtPower = tmpPrtPowerV.ToString("0.##") + Units;
                            }
                            else
                            {
                                tmpPrtPower = TotalPowerProduced.ToString("0.##");
                            }
                            if (TotalPowerProduced > 0)
                                GUILayout.Label(new GUIContent("Power Prod : " + tmpPrtPower, "The Total Power Production of this vessel"), Textures.StatusStyleLeft);
                            else
                            {
                                GUILayout.Label(new GUIContent("Power Prod : " + tmpPrtPower, "The Total Power Production of this vessel"), Textures.AlertStyleLeft);
                                SetIconalertstate(IconAlertState.YELLOW);
                            }


                            //Time Remaining in Main batteries
                            timeRemainMains = TotalElectricCharge / TotalPowerDrain;
                            if (timeRemainMains < 300) //5 mins
                            {
                                GUILayout.Label(new GUIContent("Mains Time: " + KSPUtil.PrintTimeCompact((int)timeRemainMains, false), "Time remaining in Main Power Batteries"), Textures.AlertStyleLeft);
                                SetIconalertstate(IconAlertState.RED);
                            }
                            else
                            {
                                if (timeRemainMains < 1800) //30 mins
                                {
                                    GUILayout.Label(new GUIContent("Mains Time: " + KSPUtil.PrintTimeCompact((int)timeRemainMains, false), "Time remaining in Main Power Batteries"), Textures.WarningStyleLeft);
                                    SetIconalertstate(IconAlertState.YELLOW);
                                }
                                else
                                    GUILayout.Label(new GUIContent("Mains Time: " + KSPUtil.PrintTimeCompact((int)timeRemainMains, false), "Time remaining in Main Power Batteries"), Textures.StatusStyleLeft);
                            }

                            //Time Remaining in Reserver Batteries
                            timeRemainReserve = TotalReservePower / TotalPowerDrain;
                            GUILayout.Label(
                                    new GUIContent(
                                        "Reserve Time: " + KSPUtil.PrintTimeCompact((int) timeRemainReserve, false),
                                        "Time remaining in Reserve Power Batteries"),
                                    timeRemainReserve < 30 ? Textures.AlertStyle : Textures.StatusStyleLeft);

                            
                            if (_lockReservePower)
                                GUILayout.Label(new GUIContent("Reserve Power Isolated","Reserve Power Isolation Switch is ON"),Textures.WarningStyleLeft);

                            if (TotalElectricChargeFlowOff > 0)
                            {
                                if (AYsettings.showSI)
                                {
                                    tmpPrtPowerV = Utilities.ConvertECtoSI(TotalElectricChargeFlowOff, out Units);
                                    tmpPrtPower = tmpPrtPowerV.ToString("0.##") + Units;
                                }
                                else
                                {
                                    tmpPrtPower = TotalElectricChargeFlowOff.ToString("0.##");
                                }
                                GUILayout.Label(new GUIContent("Disabled EC: " + tmpPrtPower, "You still have Main EC power, but you have disabled it in the part right click menu."), Textures.WarningStyleLeft);
                            }
                            if (TotalReservePowerFlowOff > 0)
                            {
                                if (AYsettings.showSI)
                                {
                                    tmpPrtPowerV = Utilities.ConvertECtoSI(TotalReservePowerFlowOff, out Units);
                                    tmpPrtPower = tmpPrtPowerV.ToString("0.##") + Units;
                                }
                                else
                                {
                                    tmpPrtPower = TotalReservePowerFlowOff.ToString("0.##");
                                }
                                GUILayout.Label(new GUIContent("Disabled ReservePower: " + tmpPrtPower, "You still have ReservePower, but you have disabled it in the part right click menu."), Textures.WarningStyleLeft);
                            }
                        }
                    }
                    else
                    {
                        GUILayout.Label(new GUIContent("Running on Reserve Power!", "Main EC power is exhausted, we are running on Reserve Power. Until that runs out..."), Textures.AlertStyleLeft);
                        if (TotalElectricChargeFlowOff > 0)
                        {
                            if (AYsettings.showSI)
                            {
                                tmpPrtPowerV = Utilities.ConvertECtoSI(TotalElectricChargeFlowOff, out Units);
                                tmpPrtPower = tmpPrtPowerV.ToString("0.##") + Units;
                            }
                            else
                            {
                                tmpPrtPower = TotalElectricChargeFlowOff.ToString("0.##");
                            }
                            GUILayout.Label(new GUIContent("Disabled EC: " + tmpPrtPower, "You still have Main EC power, but you have disabled it in the part right click menu."), Textures.AlertStyleLeft);
                        }
                        if (TotalReservePowerFlowOff > 0)
                        {
                            if (AYsettings.showSI)
                            {
                                tmpPrtPowerV = Utilities.ConvertECtoSI(TotalReservePowerFlowOff, out Units);
                                tmpPrtPower = tmpPrtPowerV.ToString("0.##") + Units;
                            }
                            else
                            {
                                tmpPrtPower = TotalReservePowerFlowOff.ToString("0.##");
                            }
                            GUILayout.Label(new GUIContent("Disabled ReservePower: " + tmpPrtPower, "You still have ReservePower, but you have disabled it in the part right click menu."), Textures.AlertStyleLeft);
                        }
                        SetIconalertstate(IconAlertState.RED);
                    }
                }
                else
                {
                    if (TimewarpIsValid)
                    {
                        GUILayout.Label(new GUIContent("Manager Disabled", "The AmpYear Power Management Unit has been disabled"), Textures.WarningStyleLeft);
                        SetIconalertstate(IconAlertState.GRAY);
                    }
                    else
                    {
                        GUILayout.Label(new GUIContent("Auto-Hibernation", "AmpYear functions are disabled at High rates of Time Warp"), Textures.StatusStyleLeft);
                    }
                }
            }
            else
            {
                GUILayout.Label(new GUIContent("Insufficient Power", "There is insufficient Power to run ANY of the vessels systems"), Textures.AlertStyleLeft);
                if (TotalElectricChargeFlowOff > 0)
                {
                    if (AYsettings.showSI)
                    {
                        tmpPrtPowerV = Utilities.ConvertECtoSI(TotalElectricChargeFlowOff, out Units);
                        tmpPrtPower = tmpPrtPowerV.ToString("0.##") + Units;
                    }
                    else
                    {
                        tmpPrtPower = TotalElectricChargeFlowOff.ToString("0.##");
                    }
                    GUILayout.Label(new GUIContent("Disabled EC: " + tmpPrtPower, "You still have Main EC power, but you have disabled it in the part right click menu."), Textures.AlertStyleLeft);
                }
                if (TotalReservePowerFlowOff > 0)
                {
                    if (AYsettings.showSI)
                    {
                        tmpPrtPowerV = Utilities.ConvertECtoSI(TotalReservePowerFlowOff, out Units);
                        tmpPrtPower = tmpPrtPowerV.ToString("0.##") + Units;
                    }
                    else
                    {
                        tmpPrtPower = TotalReservePowerFlowOff.ToString("0.##");
                    }
                    GUILayout.Label(new GUIContent("Disabled ReservePower: " + tmpPrtPower, "You still have ReservePower, but you have disabled it in the part right click menu."), Textures.AlertStyleLeft);
                }
                SetIconalertstate(IconAlertState.RED);
            }

            //if (AYsettings.Craziness_Function)
            if (KKPresent)
            {
                KKKrazyWrngs();
            }

            
            //Subsystems
            if (ManagerIsActive && GuiSectionEnabled(GUISection.SUBSYSTEM))
            {
                GUILayout.Label("Subsystems", Textures.SectionTitleStyle);
                for (int i = LoadGlobals.SubsystemArrayCache.Length - 1; i >= 0; --i)
                {
                    if (!SubsystemIsLuxury(LoadGlobals.SubsystemArrayCache[i]) && SubsystemVisible(LoadGlobals.SubsystemArrayCache[i]))
                    {
                        GUILayout.BeginHorizontal();
                        SubsystemButton(LoadGlobals.SubsystemArrayCache[i]);
                        SubsystemConsumptionLabel(LoadGlobals.SubsystemArrayCache[i]);
                        GUILayout.EndHorizontal();
                    }
                }
            }

            if (GuiSectionEnabled(GUISection.SUBSYSTEM))
            {
                if (_rt2Present && !RT2UnderControl) GUI.enabled = false;
                GUILayout.BeginHorizontal();
                _showCrew = GUILayout.Toggle(_showCrew, new GUIContent("ShowCrew", "Show a list of crew on board the current vessel"), Textures.SubsystemButtonStyle, SubsystemButtonOptions);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EmgcyShutActive = GUILayout.Toggle(EmgcyShutActive, new GUIContent("Emergency SP Auto Active","Activate Automatic Emergency Shutdown Procedures"), Textures.SubsystemButtonStyle, SubsystemButtonOptions);
                GUILayout.EndHorizontal();
                if (Emergencypowerdownactivated || Emergencypowerdownreset)  //Do a status if Auto ESP is processing
                {
                    ststext = "";
                    if (Emergencypowerdownactivated)
                    {
                        ststext = "PwrDown ";
                    }
                    else
                    {
                        ststext = "PwrUp ";
                    }
                    switch (_espPriority)
                    {
                        case ESPPriority.HIGH:
                            ststext += "High";
                            break;
                        case ESPPriority.MEDIUM:
                            ststext += "Medium";
                            break;
                        case ESPPriority.LOW:
                            ststext += "Low";
                            break;
                    }
                    GUILayout.Label(new GUIContent("Auto ESP Active: " + ststext, "Automatic ESP is Actively processing"), Textures.AlertStyleLeft);
                }
                GUILayout.BeginHorizontal();
                GUIContent btntext = new GUIContent("Emerg. SP Manual",
                            EmgcyShutActive
                                ? "Manually Activate Emergency Shutdown Procedures right now."
                                : "Automatic ESP (above) must be turned on first for manual override to become available.");
                if (!EmgcyShutActive || EmgcyShutOverride || EmgcyShutOverrideTmeStarted > 0)
                {
                    GUI.enabled = false;
                    if (EmgcyShutOverride)
                        btntext = new GUIContent("Emerg. SP Manual", "Manual Emergency Shutdown Process has been activated.");
                    if (EmgcyShutOverrideTmeStarted > 0)
                    {
                        double tmeRemaining = AYsettings.EmgcyShutOverrideCooldown - (Planetarium.GetUniversalTime() - EmgcyShutOverrideTmeStarted);
                        string tmeRemStr = KSPUtil.PrintTimeCompact(tmeRemaining, true);
                        btntext = new GUIContent("Manual CoolDown Rem:\n" + tmeRemStr, "Manual Emergency Shutdown Process is in cooldown mode.");
                    }
                }
                if (GUILayout.Button(btntext))
                {
                    if (EmgcyShutActive)
                    {
                        Utilities.Log_Debug("emergency shutdown manual override activated");
                        EmgcyShutActive = true;
                        EmgcyShutOverride = true;
                        _espPriority = ESPPriority.LOW;
                    }
                    else
                    {
                        Utilities.Log_Debug("emergency shutdown manual override NOT activated - automatic not activated first");
                        EmgcyShutActive = false;
                        EmgcyShutOverride = false;
                        ScreenMessages.PostScreenMessage(
                                "You must activate the Automatic ESP system before you can activate the manual override button", 5.0f,
                                ScreenMessageStyle.UPPER_CENTER);
                    }
                    
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                _showParts = GUILayout.Toggle(_showParts, new GUIContent("ShowParts", "Show all the Parts and PartModules list in the current vessel"), Textures.SubsystemButtonStyle, SubsystemButtonOptions);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                tmpShowDarkSideWindow = ShowDarkSideWindow;
                ShowDarkSideWindow = GUILayout.Toggle(ShowDarkSideWindow, new GUIContent("Dark-Side & Solar Calcs", "Open the Dark-Side and Solar Panel SOI Calculator"), Textures.SubsystemButtonStyle, SubsystemButtonOptions);
                if (tmpShowDarkSideWindow != ShowDarkSideWindow)
                {
                    AYVesselPartLists.ResetSolarPartToggles();
                }
                GUILayout.EndHorizontal();
            }

            //Luxury
            if (ManagerIsActive && GuiSectionEnabled(GUISection.LUXURY))
            {
                GUILayout.Label("Luxury", Textures.SectionTitleStyle);
                if (_rt2Present && !RT2UnderControl) GUI.enabled = false;
                for (int i = LoadGlobals.SubsystemArrayCache.Length - 1; i >= 0; --i)
                {
                    if (SubsystemIsLuxury(LoadGlobals.SubsystemArrayCache[i]) && SubsystemVisible(LoadGlobals.SubsystemArrayCache[i]))
                    {
                        GUILayout.BeginHorizontal();
                        SubsystemButton(LoadGlobals.SubsystemArrayCache[i]);
                        SubsystemConsumptionLabel(LoadGlobals.SubsystemArrayCache[i]);
                        GUILayout.EndHorizontal();
                    }
                }
                GUI.enabled = true;
            }

            //Reserve
            if (ManagerIsActive && GuiSectionEnabled(GUISection.RESERVE))
            {
                GUILayout.Label("Reserve Power", Textures.SectionTitleStyle);

                //Reserve status label
                if (TotalReservePowerCapacity > 0.0)
                {
                    if (HasReservePower)
                    {
                        reservePercent = TotalReservePower / TotalReservePowerCapacity * 100.0;
                        if (reservePercent < 20.0)
                            GUILayout.Label(new GUIContent("Reserve Power: " + reservePercent.ToString("0.00") + '%', "Percentage of Reserve Power Available"), Textures.AlertStyleLeft);
                        else
                        {
                            GUILayout.Label(
                                new GUIContent("Reserve Power: " + reservePercent.ToString("0.00") + '%',
                                    "Percentage of Reserve Power Available"),
                                reservePercent < 40.0 ? Textures.WarningStyle : Textures.StatusStyleLeft);
                        }
                    }
                    else
                        GUILayout.Label("Reserve Power Depleted", Textures.AlertStyleLeft);
                }
                else
                    GUILayout.Label("Reserve Power not Found!", Textures.AlertStyleLeft);

                //Reserve transfer
                //String[] incrementPercentString = new String[_reserveTransferIncrements.Length];
                //if (_rt2Present && !RT2UnderControl) GUI.enabled = false;
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("XFer Reserve to Main", "Transfer a percentage of Reserve Power to Mains Power"));
                for (int i = 0; i < _reserveTransferIncrements.Length; i++)
                {
                    incrementPercentString[i] = (_reserveTransferIncrements[i] * 100).ToString("F0") + '%';
                    if (GUILayout.Button(new GUIContent(incrementPercentString[i], "Transfer " + incrementPercentString[i] + " of Reserve Power to Mains EC")))
                        TransferReserveToMain(TotalReservePowerCapacity * _reserveTransferIncrements[i]);
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("XFer Main to Reserve", "Transfer a percentage of Main Power to Reserve Power"));
                for (int i = 0; i < _reserveTransferIncrements.Length; i++)
                {
                    if (GUILayout.Button(new GUIContent(incrementPercentString[i], "Transfer " + incrementPercentString[i] + " of Mains EC to Reserve Power")))
                        TransferMainToReserve(TotalReservePowerCapacity * _reserveTransferIncrements[i]);
                }
                GUILayout.EndHorizontal();
                GUI.enabled = true;
            }

            if (GuiSectionEnabled(GUISection.RESERVE))
            {
                //if (_rt2Present && !RT2UnderControl) GUI.enabled = false;
                GUILayout.BeginHorizontal();
                _lockReservePower = GUILayout.Toggle(_lockReservePower, new GUIContent("Isolate Reserve Power", "Isolation Switch for Reserve Power, ReservePower will not be used if this switch is on"), Textures.SubsystemButtonStyle, SubsystemButtonOptions);
                GUILayout.EndHorizontal();
                GUI.enabled = true;
            }

            //ShowCrew
                if (_showCrew)
            {
                GUILayout.Label("Crew", Textures.SectionTitleStyle);
                //VslRstr = FlightGlobals.ActiveVessel.GetVesselCrew();
                if (VslRstr.Count > 0)
                {
                    for (int i = VslRstr.Count - 1; i >= 0; --i)
                    {
                        GUILayout.Label(VslRstr[i].name + " - " + VslRstr[i].experienceTrait.Title, Textures.StatusStyleLeft);
                    }
                }
                else //if (timewarpIsValid)
                    GUILayout.Label("No Crew OnBoard", Textures.WarningStyleLeft);
            }

            GUILayout.EndVertical();
            if (AYsettings.TooltipsOn)
                Utilities.SetTooltipText();
            GUI.DragWindow();
        }

        private void WindowE(int id)
        {
            _iconAlertState = IconAlertState.GREEN;
            GUIContent closeContent = new GUIContent(Textures.BtnRedCross, "Close Window");
            Rect closeRect = new Rect(_ewindowPos.width - 21, 4, 16, 16);
            if (GUI.Button(closeRect, closeContent, Textures.PartListbtnStyle))
            {
                AYMenuAppLToolBar.onAppLaunchToggle();
                return;
            }
            AYsettings.showSI = GUI.Toggle(new Rect(_ewindowPos.width - 45, 4, 16, 16), AYsettings.showSI, new GUIContent(Textures.BtnIS, "Toggle the display to use EC or SI units"), Textures.PartListbtnStyle);//, SubsystemButtonOptions);
            GUILayout.BeginVertical();

            //Manager status+drain
            GUILayout.BeginHorizontal();
            _managerEnabled = GUILayout.Toggle(_managerEnabled, new GUIContent("Manager", "Turn on to activate the AmpYear Power Management Unit"), Textures.SubsystemButtonStyle, SubsystemButtonOptions);
            if (ManagerIsActive)
                ConsumptionLabel(ManagerCurrentDrain, false);
            else
                ConsumptionLabel(managerActiveDrain, true);
            GUILayout.EndHorizontal();
            _showParts = GUILayout.Toggle(_showParts, new GUIContent("ShowParts", "Show all the Parts and PartModules list in the current vessel"), Textures.SubsystemButtonStyle, SubsystemButtonOptions);
            tmpShowDarkSideWindow = ShowDarkSideWindow;
            ShowDarkSideWindow = GUILayout.Toggle(ShowDarkSideWindow, new GUIContent("Dark-Side & Solar Calcs", "Open the Dark-Side and Solar Panel SOI Calculator"), Textures.SubsystemButtonStyle, SubsystemButtonOptions);
            if (tmpShowDarkSideWindow != ShowDarkSideWindow)
            {
                AYVesselPartLists.ResetSolarPartToggles();
            }
            //ShowSOIWindow = GUILayout.Toggle(ShowSOIWindow, new GUIContent("Select Solar Panel SOI", "Open the Solar Panel SOI Selector"), Textures.SubsystemButtonStyle, SubsystemButtonOptions);
            //Power Capacity
            if (AYsettings.showSI)
            {
                tmpPrtPowerV = Utilities.ConvertECtoSI(TotalElectricChargeCapacity, out Units);
                tmpPrtPower = tmpPrtPowerV.ToString("0.00") + Units;
            }
            else
            {
                tmpPrtPower = TotalElectricChargeCapacity.ToString("0.00");
            }
            GUILayout.Label(new GUIContent("Power Capacity: " + tmpPrtPower, "Total Power Capacity of this vessel"), Textures.StatusStyleLeft);
            if (AYsettings.showSI)
            {
                tmpPrtPowerV = Utilities.ConvertECtoSI(TotalPowerDrain, out Units);
                tmpPrtPower = tmpPrtPowerV.ToString("0.00") + Units;
            }
            else
            {
                tmpPrtPower = TotalPowerDrain.ToString("0.00");
            }
            if (TotalPowerDrain > TotalPowerProduced)
            {
                GUILayout.Label(new GUIContent("Power Drain : " + tmpPrtPower, "The Total Power Drain on this vessel"), Textures.AlertStyleLeft);
                SetIconalertstate(IconAlertState.RED);
            }
            else
                GUILayout.Label(new GUIContent("Power Drain : " + tmpPrtPower, "The Total Power Drain on this vessel"), Textures.StatusStyleLeft);
            if (AYsettings.showSI)
            {
                tmpPrtPowerV = Utilities.ConvertECtoSI(TotalPowerProduced, out Units);
                tmpPrtPower = tmpPrtPowerV.ToString("0.00") + Units;
            }
            else
            {
                tmpPrtPower = TotalPowerProduced.ToString("0.00");
            }
            if (TotalPowerProduced > 0)
                GUILayout.Label("Power Prod : " + tmpPrtPower, Textures.StatusStyleLeft);
            else
            {
                GUILayout.Label(new GUIContent("Power Prod : " + tmpPrtPower, "The Total Power Production of this vessel"), Textures.AlertStyleLeft);
                SetIconalertstate(IconAlertState.YELLOW);
            }

            //Time Remaining in Main batteries
            double timeRemainMains = TotalElectricCharge / TotalPowerDrain;
            if (timeRemainMains < 300) //5 mins
            {
                GUILayout.Label(new GUIContent("Mains Time: " + KSPUtil.PrintTimeCompact((int)timeRemainMains, false), "The time remaining of Main EC stored based on current power production and usage"), Textures.AlertStyleLeft);
                SetIconalertstate(IconAlertState.RED);
            }
            else
            {
                if (timeRemainMains < 1800) //30 mins
                {
                    GUILayout.Label(new GUIContent("Mains Time: " + KSPUtil.PrintTimeCompact((int)timeRemainMains, false), "The time remaining of Main EC stored based on current power production and usage"), Textures.WarningStyleLeft);
                    SetIconalertstate(IconAlertState.YELLOW);
                }
                else
                    GUILayout.Label(new GUIContent("Mains Time: " + KSPUtil.PrintTimeCompact((int)timeRemainMains, false), "The time remaining of Main EC stored based on current power production and usage"), Textures.StatusStyleLeft);
            }

            //Time Remaining in Reserver Batteries
            double timeRemainReserve = TotalReservePower / TotalPowerDrain;
            if (timeRemainReserve < 30)
            {
                GUILayout.Label(new GUIContent("Reserve Time: " + KSPUtil.PrintTimeCompact((int)timeRemainReserve, false), "The time remaining of Main EC stored based on current power production and usage"), Textures.AlertStyleLeft);
                SetIconalertstate(IconAlertState.YELLOW);
            }
            else
                GUILayout.Label(new GUIContent("Reserve Time: " + KSPUtil.PrintTimeCompact((int)timeRemainReserve, false), "The time remaining of Main EC stored based on current power production and usage"), Textures.StatusStyleLeft);

            //Reserve
            GUILayout.Label("Reserve Power", Textures.SectionTitleStyle);
            //Reserve status label
            if (AYsettings.showSI)
            {
                tmpPrtPowerV = Utilities.ConvertECtoSI(TotalReservePowerCapacity, out Units);
                tmpPrtPower = tmpPrtPowerV.ToString("0.00") + Units;
            }
            else
            {
                tmpPrtPower = TotalReservePowerCapacity.ToString("0.00");
            }
            GUILayout.Label(new GUIContent("Reserve Capacity: " + tmpPrtPower, "The total capacity of ReservePower of the current vessel"), Textures.StatusStyleLeft);
            GUILayout.EndVertical();
            if (AYsettings.TooltipsOn)
                Utilities.SetTooltipText();
            GUI.DragWindow();
        }

        private void WindowScrollParts(int id)
        {
            GUIContent closeContent = new GUIContent(Textures.BtnRedCross, "Close Window");
            Rect closeRect = new Rect(_epLwindowPos.width - 21, 4, 16, 16);
            if (GUI.Button(closeRect, closeContent, Textures.PartListbtnStyle))
            {
                _showParts = false; 
                return;
            }
            //Rect showSIRect = new Rect(_epLwindowPos.width - 45, 4, 16, 16);
            AYsettings.showSI = GUI.Toggle(new Rect(_epLwindowPos.width - 45, 4, 16, 16), AYsettings.showSI,  new GUIContent(Textures.BtnIS, "Toggle the display to use EC or SI units"), Textures.PartListbtnStyle);//, SubsystemButtonOptions);
            GUILayout.BeginVertical();
            GUILayout.Label("Power Production Parts", Textures.PartListStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("", Textures.PartListpartHeadingStyle, GUILayout.Width(5));
            GUILayout.Label(new GUIContent("Calc", "If on, this PartModule will be Included in AmpYear Power Calculations"), Textures.PartListpartHeadingStyle, GUILayout.Width(28));
            GUILayout.Label(new GUIContent("ESP", "If on, this PartModule will be Included in Emergency Shutdown Procedures"), Textures.PartListpartHeadingStyle, GUILayout.Width(24));
            GUILayout.Label(new GUIContent("Priority", "PartModule Priority in Emergency Shutdown Procedures"), Textures.PartListpartHeadingStyle, GUILayout.Width(80));
            GUILayout.Label(new GUIContent("PartTitle", "The Title of the Part"), Textures.PartListpartHeadingStyle, GUILayout.Width(_eplPartName));
            GUILayout.Label(new GUIContent("Module", "The name of the PartModule"), Textures.PartListpartHeadingStyle, GUILayout.Width(_eplPartModuleName));
            if (!AYsettings.showSI)
            {
                GUILayout.Label(new GUIContent("EC", "Electric Charge produced by this PartModule"),Textures.PartListpartHeadingStyle, GUILayout.Width(_eplec));
            }
            else
            {
                GUILayout.Label(new GUIContent("SI", "Electric Charge produced by this PartModule, in Système international Units"), Textures.PartListpartHeadingStyle, GUILayout.Width(_eplec));
            }
            //AYsettings.showSI = GUILayout.Toggle(AYsettings.showSI, new GUIContent("Units", "Toggle the display to use EC or SI units"), Textures.SubsystemButtonStyle, SubsystemButtonOptions);

            GUILayout.EndHorizontal();
            // Begin the ScrollView
            _plProdscrollViewVector = GUILayout.BeginScrollView(_plProdscrollViewVector, true, true, GUILayout.Height(_eplProdListHeight));
            // Put something inside the ScrollView
            if (AYVesselPartLists.VesselProdPartsList.Count == 0)
                GUILayout.Label("No Power Producing Parts", Textures.PartListPartStyle);
            _totalProdPower = 0f;
            foreach (var entry in AYVesselPartLists.VesselProdPartsList)
            {
                partModuleName = string.Empty;
                tmpPartKey = AYVesselPartLists.GetPartKeyVals(entry.Key, out partModuleName);
                partModuleName = Utilities.RemoveSubStr(partModuleName, "Module");
                if (entry.Value.PrtEditorInclude)
                    _totalProdPower += entry.Value.PrtPowerF;
                if (ShowDarkSideWindow && entry.Value.PrtSolarDependant)
                {
                    GUI.enabled = false;
                    entry.Value.PrtEditorInclude = false;
                    GUILayout.BeginHorizontal();
                    bool tmpPrtEditorInclude = GUILayout.Toggle(entry.Value.PrtEditorInclude, 
                        new GUIContent(Textures.BtnIncInCalcs, "Include this PartModule in AmpYear Calcs"), Textures.PartListbtnStyle, GUILayout.Width(20));
                    entry.Value.PrtEditorInclude = tmpPrtEditorInclude;
                    bool tmpPrtEmergShutDnInclude = GUILayout.Toggle(entry.Value.PrtEmergShutDnInclude, 
                        new GUIContent(Textures.BtnEspInc, "Include this PartModule in Emergency Shutdown Procedures"), Textures.PartListbtnStyle, GUILayout.Width(20));
                    entry.Value.PrtEmergShutDnInclude = tmpPrtEmergShutDnInclude;
                    List<GUIContent> tmpList = new List<GUIContent>();
                    tmpList.Add(new GUIContent(Textures.BtnPriority1, entry.Value.ValidprtEmergShutDn ? "Emergency Shutdown Procedures - Priority One" : "Part not Supported by Emergency Shutdown Procedure"));
                    tmpList.Add(new GUIContent(Textures.BtnPriority2, entry.Value.ValidprtEmergShutDn ? "Emergency Shutdown Procedures - Priority Two" : "Part not Supported by Emergency Shutdown Procedure"));
                    tmpList.Add(new GUIContent(Textures.BtnPriority3, entry.Value.ValidprtEmergShutDn ? "Emergency Shutdown Procedures - Priority Three" : "Part not Supported by Emergency Shutdown Procedure"));
                    GUIContent[] tmpToggles = tmpList.ToArray();
                    List<GUIStyle> tmpStylesList = new List<GUIStyle>();
                    tmpStylesList.Add(Textures.PartListbtnStyle);
                    tmpStylesList.Add(Textures.PartListbtnStyle);
                    tmpStylesList.Add(Textures.PartListbtnStyle);
                    GUIStyle[] tmpStylesToggles = tmpStylesList.ToArray();
                    int tmpESPPriority = Utilities.ToggleList((int)entry.Value.PrtEmergShutDnPriority - 1, tmpToggles, tmpStylesToggles, 20);
                    entry.Value.PrtEmergShutDnPriority = (ESPPriority)(tmpESPPriority + 1);
                    GUILayout.Label(entry.Value.PrtTitle, entry.Value.PrtEditorInclude ? Textures.PartListPartStyle : Textures.PartListPartGrayStyle, GUILayout.Width(_eplPartName));
                    GUILayout.Label(partModuleName, entry.Value.PrtEditorInclude ? Textures.PartListPartStyle : Textures.PartListPartGrayStyle, GUILayout.Width(_eplPartModuleName));
                    if (AYsettings.showSI)
                    {
                        tmpPrtPowerV = Utilities.ConvertECtoSI(entry.Value.PrtPowerF, out Units);
                        tmpPrtPower = tmpPrtPowerV.ToString("####0.###") + Units;
                    }
                    else
                    {
                        tmpPrtPower = entry.Value.PrtPower;
                    }
                    GUILayout.Label(tmpPrtPower, entry.Value.PrtEditorInclude ? Textures.PartListPartStyle : Textures.PartListPartGrayStyle, GUILayout.Width(_eplec));
                    GUILayout.EndHorizontal();
                    GUI.enabled = true;
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    tmpPrtEditorInclude = GUILayout.Toggle(entry.Value.PrtEditorInclude, 
                        new GUIContent(Textures.BtnIncInCalcs, "Include this PartModule in AmpYear Calcs"), Textures.PartListbtnStyle, GUILayout.Width(20));
                    entry.Value.PrtEditorInclude = tmpPrtEditorInclude;
                    if (!entry.Value.ValidprtEmergShutDn)
                        GUI.enabled = false;
                    tmpPrtEmergShutDnInclude = GUILayout.Toggle(entry.Value.PrtEmergShutDnInclude, 
                        new GUIContent(Textures.BtnEspInc, GUI.enabled ? "Include this PartModule in Emergency Shutdown Procedures" : "Part not Supported by Emergency Shutdown Procedure"), Textures.PartListbtnStyle, GUILayout.Width(20));
                    entry.Value.PrtEmergShutDnInclude = tmpPrtEmergShutDnInclude;
                    List<GUIContent> tmpList = new List<GUIContent>();
                    tmpList.Add(new GUIContent(Textures.BtnPriority1, entry.Value.ValidprtEmergShutDn ? "Emergency Shutdown Procedures - Priority One" : "Part not Supported by Emergency Shutdown Procedure"));
                    tmpList.Add(new GUIContent(Textures.BtnPriority2, entry.Value.ValidprtEmergShutDn ? "Emergency Shutdown Procedures - Priority Two" : "Part not Supported by Emergency Shutdown Procedure"));
                    tmpList.Add(new GUIContent(Textures.BtnPriority3, entry.Value.ValidprtEmergShutDn ? "Emergency Shutdown Procedures - Priority Three" : "Part not Supported by Emergency Shutdown Procedure"));
                    GUIContent[] tmpToggles = tmpList.ToArray();
                    List<GUIStyle> tmpStylesList = new List<GUIStyle>();
                    tmpStylesList.Add(Textures.PartListbtnStyle);
                    tmpStylesList.Add(Textures.PartListbtnStyle);
                    tmpStylesList.Add(Textures.PartListbtnStyle);
                    GUIStyle[] tmpStylesToggles = tmpStylesList.ToArray();
                    tmpESPPriority = Utilities.ToggleList((int)entry.Value.PrtEmergShutDnPriority - 1, tmpToggles, tmpStylesToggles, 20);
                    entry.Value.PrtEmergShutDnPriority = (ESPPriority)(tmpESPPriority + 1);
                    GUI.enabled = true;
                    GUILayout.Label(entry.Value.PrtTitle, entry.Value.PrtEditorInclude ? Textures.PartListPartStyle : Textures.PartListPartGrayStyle, GUILayout.Width(_eplPartName));
                    GUILayout.Label(partModuleName, entry.Value.PrtEditorInclude ? Textures.PartListPartStyle : Textures.PartListPartGrayStyle, GUILayout.Width(_eplPartModuleName));
                    //GUILayout.Label(entry.Value.PrtPower, entry.Value.PrtEditorInclude ? Textures.PartListPartStyle : Textures.PartListPartGrayStyle, GUILayout.Width(_eplec));
                    if (AYsettings.showSI)
                    {
                        tmpPrtPowerV = Utilities.ConvertECtoSI(entry.Value.PrtPowerF, out Units);
                        tmpPrtPower = tmpPrtPowerV.ToString("####0.###") + Units;
                    }
                    else
                    {
                        tmpPrtPower = entry.Value.PrtPower;
                    }
                    GUILayout.Label(tmpPrtPower, entry.Value.PrtEditorInclude ? Textures.PartListPartStyle : Textures.PartListPartGrayStyle, GUILayout.Width(_eplec));
                    GUILayout.EndHorizontal();
                    entry.Value.PrtUserEditorInclude = entry.Value.PrtEditorInclude;
                    entry.Value.PrtEmergShutDnInclude = entry.Value.PrtEmergShutDnInclude;
                }

            }
            if (AYVesselPartLists.VesselProdPartsList.Count > 0) //This is very clunky but we get there in the end... and KSP 1.1 will probably mean have to re-do this anyway
            {
                GUILayout.BeginHorizontal();
                tmpPrtProdEditorIncludeAll = false;
                tmpPrtProdEditorIncludeAll = GUILayout.Toggle(PrtProdEditorIncludeAll,
                    new GUIContent(Textures.BtnIncInCalcs, "Toggle Include in AmpYear Calcs for all listed part modules above"), Textures.PartListbtnStyle, GUILayout.Width(20));
                if (tmpPrtProdEditorIncludeAll != PrtProdEditorIncludeAll)
                {
                    PrtProdEditorIncludeAll = !PrtProdEditorIncludeAll;
                    foreach (var entry in AYVesselPartLists.VesselProdPartsList)
                    {
                        entry.Value.PrtEditorInclude = PrtProdEditorIncludeAll;
                    }
                }
                tmpPrtProdEmergShutDnIncludeAll = false;
                tmpPrtProdEmergShutDnIncludeAll = GUILayout.Toggle(PrtProdESPIncludeAll,
                    new GUIContent(Textures.BtnEspInc, "Toggle Include in Emergency Shutdown Procedures for all listed part modules above"), Textures.PartListbtnStyle, GUILayout.Width(20));
                if (tmpPrtProdEmergShutDnIncludeAll != PrtProdESPIncludeAll)
                {
                    PrtProdESPIncludeAll = !PrtProdESPIncludeAll;
                    foreach (var entry in AYVesselPartLists.VesselProdPartsList)
                    {
                        if (entry.Value.ValidprtEmergShutDn)
                            entry.Value.PrtEmergShutDnInclude = PrtProdESPIncludeAll;
                    }
                }
                tmpPrtProdOneAll = false;
                tmpPrtProdOneAll = GUILayout.Toggle(tmpPrtProdOneAll,
                    new GUIContent(Textures.BtnPriority1, "Emergency Shutdown Procedures - Set all parts to Priority One"), Textures.PartListbtnStyle, GUILayout.Width(20));
                if (tmpPrtProdOneAll)
                {
                    foreach (var entry in AYVesselPartLists.VesselProdPartsList)
                    {
                        if (entry.Value.ValidprtEmergShutDn)
                            entry.Value.PrtEmergShutDnPriority = ESPPriority.HIGH;
                    }
                }
                tmpPrtProdTwoAll = false;
                tmpPrtProdTwoAll = GUILayout.Toggle(tmpPrtProdTwoAll,
                    new GUIContent(Textures.BtnPriority2, "Emergency Shutdown Procedures - Set all parts to Priority Two"), Textures.PartListbtnStyle, GUILayout.Width(20));
                if (tmpPrtProdTwoAll)
                {
                    foreach (var entry in AYVesselPartLists.VesselProdPartsList)
                    {
                        if (entry.Value.ValidprtEmergShutDn)
                            entry.Value.PrtEmergShutDnPriority = ESPPriority.MEDIUM;
                    }
                }
                tmpPrtProdThreeAll = false;
                tmpPrtProdThreeAll = GUILayout.Toggle(tmpPrtProdThreeAll,
                    new GUIContent(Textures.BtnPriority3, "Emergency Shutdown Procedures - Set all parts to Priority Three"), Textures.PartListbtnStyle, GUILayout.Width(20));
                if (tmpPrtProdThreeAll)
                {
                    foreach (var entry in AYVesselPartLists.VesselProdPartsList)
                    {
                        if (entry.Value.ValidprtEmergShutDn)
                            entry.Value.PrtEmergShutDnPriority = ESPPriority.LOW;
                    }
                }
                GUILayout.Label("   ", Textures.PartListPartStyle, GUILayout.Width(_eplPartName));
                GUILayout.Label("Total Produced", Textures.PartListPartStyle, GUILayout.Width(_eplPartModuleName));
                //GUILayout.Label(new GUIContent(_totalProdPower.ToString("####0.###"), "Total Production Power"), Textures.PartListPartStyle, GUILayout.Width(_eplec));
                if (AYsettings.showSI)
                {
                    tmpPrtPowerV = Utilities.ConvertECtoSI(_totalProdPower, out Units);
                    tmpPrtPower = tmpPrtPowerV.ToString("####0.###") + Units;
                }
                else
                {
                    tmpPrtPower = _totalProdPower.ToString("####0.###");
                }
                GUILayout.Label(new GUIContent(tmpPrtPower, "Total Production Power"), Textures.PartListPartStyle, GUILayout.Width(_eplec));
                GUILayout.EndHorizontal();
            }
                
            // End the ScrollView
            GUILayout.EndScrollView();
            if (Event.current.type == EventType.Repaint)
                _eplProdlistbox = GUILayoutUtility.GetLastRect();

            GUIContent resizeProdHeightContent = new GUIContent(Textures.BtnResizeHeight, "Resize Producer List Height");
            Rect resizeProdHeightRect = new Rect(_epLwindowPos.width - 20, _eplProdlistbox.y + _eplProdlistbox.height + 4, 16, 16);
            GUI.Label(resizeProdHeightRect, resizeProdHeightContent, Textures.ResizeStyle);
            HandleResizeHeightScrl1Events(resizeProdHeightRect, _eplProdlistbox);

            GUILayout.FlexibleSpace();
            GUILayout.Label("Power Consumer Parts", Textures.PartListStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("", Textures.PartListpartHeadingStyle, GUILayout.Width(5));
            GUILayout.Label(new GUIContent("Calc", "If on, this PartModule will be Included in AmpYear Power Calculations"), Textures.PartListpartHeadingStyle, GUILayout.Width(28));
            GUILayout.Label(new GUIContent("ESP", "If on, this PartModule will be Included in Emergency Shutdown Procedures"), Textures.PartListpartHeadingStyle, GUILayout.Width(24));
            GUILayout.Label(new GUIContent("Priority", "PartModule Priority in Emergency Shutdown Procedures"), Textures.PartListpartHeadingStyle, GUILayout.Width(80));
            GUILayout.Label(new GUIContent("PartTitle", "The Title of the Part"), Textures.PartListpartHeadingStyle, GUILayout.Width(_eplPartName));
            GUILayout.Label(new GUIContent("Module", "The name of the PartModule"), Textures.PartListpartHeadingStyle, GUILayout.Width(_eplPartModuleName));
            if (!AYsettings.showSI)
            {
                GUILayout.Label(new GUIContent("EC", "Electric Charge Used by this PartModule"),Textures.PartListpartHeadingStyle, GUILayout.Width(_eplec));
            }
            else
            {
                GUILayout.Label(new GUIContent("SI", "Electric Charge produced by this PartModule, in Système international Units"), Textures.PartListpartHeadingStyle, GUILayout.Width(_eplec));
            }

            GUILayout.EndHorizontal();
            if (Event.current.type == EventType.Repaint)
                _eplHeaders2 = GUILayoutUtility.GetLastRect();
            
            // Begin the ScrollView
            _plConsscrollViewVector = GUILayout.BeginScrollView(_plConsscrollViewVector, true, true, GUILayout.Height(_eplConsListHeight));
            // Put something inside the ScrollView
            if (AYVesselPartLists.VesselConsPartsList.Count == 0)
                GUILayout.Label("No Power Consuming Parts", Textures.PartListPartStyle);
            _totalConsPower = 0f;
            foreach (var entry in AYVesselPartLists.VesselConsPartsList)
            {
                partModuleName = string.Empty;
                tmpPartKey = AYVesselPartLists.GetPartKeyVals(entry.Key,out partModuleName);
                partModuleName = Utilities.RemoveSubStr(partModuleName, "Module");
                if (entry.Value.PrtEditorInclude)
                    _totalConsPower += entry.Value.PrtPowerF;
                GUILayout.BeginHorizontal();
                tmpPrtEditorInclude = GUILayout.Toggle(entry.Value.PrtEditorInclude, 
                    new GUIContent(Textures.BtnIncInCalcs, "Include this PartModule in AmpYear Calcs"), Textures.PartListbtnStyle, GUILayout.Width(20));
                entry.Value.PrtEditorInclude = tmpPrtEditorInclude;
                if (!entry.Value.ValidprtEmergShutDn)
                    GUI.enabled = false;
                tmpPrtEmergShutDnInclude = GUILayout.Toggle(entry.Value.PrtEmergShutDnInclude, 
                    new GUIContent(Textures.BtnEspInc, GUI.enabled ? "Include this PartModule in Emergency Shutdown Procedures" : "Part not Supported by Emergency Shutdown Procedure"), Textures.PartListbtnStyle, GUILayout.Width(20));
                entry.Value.PrtEmergShutDnInclude = tmpPrtEmergShutDnInclude;
                List<GUIContent> tmpList = new List<GUIContent>();
                tmpList.Add(new GUIContent(Textures.BtnPriority1, entry.Value.ValidprtEmergShutDn ? "Emergency Shutdown Procedures - Priority One" : "Part not Supported by Emergency Shutdown Procedure"));
                tmpList.Add(new GUIContent(Textures.BtnPriority2, entry.Value.ValidprtEmergShutDn ? "Emergency Shutdown Procedures - Priority Two" : "Part not Supported by Emergency Shutdown Procedure"));
                tmpList.Add(new GUIContent(Textures.BtnPriority3, entry.Value.ValidprtEmergShutDn ? "Emergency Shutdown Procedures - Priority Three" : "Part not Supported by Emergency Shutdown Procedure"));
                GUIContent[] tmpToggles = tmpList.ToArray();
                List<GUIStyle> tmpStylesList = new List<GUIStyle>();
                tmpStylesList.Add(Textures.PartListbtnStyle);
                tmpStylesList.Add(Textures.PartListbtnStyle);
                tmpStylesList.Add(Textures.PartListbtnStyle);
                GUIStyle[] tmpStylesToggles = tmpStylesList.ToArray();
                tmpESPPriority = Utilities.ToggleList((int)entry.Value.PrtEmergShutDnPriority - 1, tmpToggles, tmpStylesToggles, 20);
                entry.Value.PrtEmergShutDnPriority = (ESPPriority)(tmpESPPriority + 1);
                GUI.enabled = true;
                GUILayout.Label(entry.Value.PrtTitle, entry.Value.PrtEditorInclude ? Textures.PartListPartStyle : Textures.PartListPartGrayStyle, GUILayout.Width(_eplPartName));
                GUILayout.Label(partModuleName, entry.Value.PrtEditorInclude ? Textures.PartListPartStyle : Textures.PartListPartGrayStyle, GUILayout.Width(_eplPartModuleName));
                if (AYsettings.showSI)
                {
                    tmpPrtPowerV = Utilities.ConvertECtoSI(entry.Value.PrtPowerF, out Units);
                    tmpPrtPower = tmpPrtPowerV.ToString("####0.###") + Units;
                }
                else
                {
                    tmpPrtPower = entry.Value.PrtPower;
                }
                GUILayout.Label(tmpPrtPower, entry.Value.PrtEditorInclude ? Textures.PartListPartStyle : Textures.PartListPartGrayStyle, GUILayout.Width(_eplec));

                GUILayout.EndHorizontal();
            }
            if (AYVesselPartLists.VesselConsPartsList.Count > 0) //This is very clunky but we get there in the end... and KSP 1.1 will probably mean have to re-do this anyway
            {
                GUILayout.BeginHorizontal();
                tmpPrtConsEditorIncludeAll = false;
                tmpPrtConsEditorIncludeAll = GUILayout.Toggle(PrtConsEditorIncludeAll,
                    new GUIContent(Textures.BtnIncInCalcs, "Toggle Include in AmpYear Calcs for all listed part modules above"), Textures.PartListbtnStyle, GUILayout.Width(20));
                if (tmpPrtConsEditorIncludeAll != PrtConsEditorIncludeAll)
                {
                    PrtConsEditorIncludeAll = !PrtConsEditorIncludeAll;
                    foreach (var entry in AYVesselPartLists.VesselConsPartsList)
                    {
                        entry.Value.PrtEditorInclude = PrtConsEditorIncludeAll;
                    }
                }
                tmpPrtConsEmergShutDnIncludeAll = false;
                tmpPrtConsEmergShutDnIncludeAll = GUILayout.Toggle(PrtConsESPIncludeAll,
                    new GUIContent(Textures.BtnEspInc, "Toggle Include in Emergency Shutdown Procedures for all listed part modules above"), Textures.PartListbtnStyle, GUILayout.Width(20));
                if (tmpPrtConsEmergShutDnIncludeAll != PrtConsESPIncludeAll)
                {
                    PrtConsESPIncludeAll = !PrtConsESPIncludeAll;
                    foreach (var entry in AYVesselPartLists.VesselConsPartsList)
                    {
                        if (entry.Value.ValidprtEmergShutDn)
                            entry.Value.PrtEmergShutDnInclude = PrtConsESPIncludeAll;
                    }
                }
                tmpPrtConsOneAll = false;
                tmpPrtConsOneAll = GUILayout.Toggle(tmpPrtConsOneAll,
                    new GUIContent(Textures.BtnPriority1, "Emergency Shutdown Procedures - Set all parts to Priority One"), Textures.PartListbtnStyle, GUILayout.Width(20));
                if (tmpPrtConsOneAll)
                {
                    foreach (var entry in AYVesselPartLists.VesselConsPartsList)
                    {
                        if (entry.Value.ValidprtEmergShutDn)
                            entry.Value.PrtEmergShutDnPriority = ESPPriority.HIGH;
                    }
                }
                tmpPrtConsTwoAll = false;
                tmpPrtConsTwoAll = GUILayout.Toggle(tmpPrtConsTwoAll,
                    new GUIContent(Textures.BtnPriority2, "Emergency Shutdown Procedures - Set all parts to Priority Two"), Textures.PartListbtnStyle, GUILayout.Width(20));
                if (tmpPrtConsTwoAll)
                {
                    foreach (var entry in AYVesselPartLists.VesselConsPartsList)
                    {
                        if (entry.Value.ValidprtEmergShutDn)
                            entry.Value.PrtEmergShutDnPriority = ESPPriority.MEDIUM;
                    }
                }
                tmpPrtConsThreeAll = false;
                tmpPrtConsThreeAll = GUILayout.Toggle(tmpPrtConsThreeAll,
                    new GUIContent(Textures.BtnPriority3, "Emergency Shutdown Procedures - Set all parts to Priority Three"), Textures.PartListbtnStyle, GUILayout.Width(20));
                if (tmpPrtConsThreeAll)
                {
                    foreach (var entry in AYVesselPartLists.VesselConsPartsList)
                    {
                        if (entry.Value.ValidprtEmergShutDn)
                            entry.Value.PrtEmergShutDnPriority = ESPPriority.LOW;
                    }
                }
                GUILayout.Label(" ", Textures.PartListPartStyle, GUILayout.Width(_eplPartName));
                GUILayout.Label("Total Consumed", Textures.PartListPartStyle, GUILayout.Width(_eplPartModuleName));
                if (AYsettings.showSI)
                {
                    tmpPrtPowerV = Utilities.ConvertECtoSI(_totalConsPower, out Units);
                    tmpPrtPower = tmpPrtPowerV.ToString("####0.###") + Units;
                }
                else
                {
                    tmpPrtPower = _totalConsPower.ToString("####0.###");
                }
                GUILayout.Label(new GUIContent(tmpPrtPower, "Total Power Consumption"), Textures.PartListPartStyle, GUILayout.Width(_eplec));
                GUILayout.EndHorizontal();
            }
            // End the ScrollView
            GUILayout.EndScrollView();
            if (Event.current.type == EventType.Repaint)
                _eplConslistbox = GUILayoutUtility.GetLastRect();
            GUILayout.EndVertical();
            GUILayout.Space(14);

            GUIContent resizeConsHeightContent = new GUIContent(Textures.BtnResizeHeight, "Resize Consumer List Height");
            Rect resizeConsHeightRect = new Rect(_epLwindowPos.width - 20, _epLwindowPos.height - 19, 16, 16);
            GUI.Label(resizeConsHeightRect, resizeConsHeightContent, Textures.ResizeStyle);
            HandleResizeHeightScrl2Events(resizeConsHeightRect, _eplConslistbox);

            GUIContent resizeContent = new GUIContent(Textures.BtnResizeWidth, "Resize Window Width");
            Rect resizeRect = new Rect(_epLwindowPos.width - 40, _epLwindowPos.height - 19, 16, 16);
            GUI.Label(resizeRect, resizeContent, Textures.ResizeStyle);
            HandleResizeWidthEvents(resizeRect);

            if (AYsettings.TooltipsOn)
                Utilities.SetTooltipText();
            GUI.DragWindow();
        }
        
        private void WindowD(int windowId)
        {
            GUIContent closeContent = new GUIContent(Textures.BtnRedCross, "Close Window");
            Rect closeRect = new Rect(_dwindowPos.width - 21, 4, 16, 16);
            if (GUI.Button(closeRect, closeContent, Textures.PartListbtnStyle))
            {
                AYVesselPartLists.ResetSolarPartToggles();
                ShowDarkSideWindow = false;
                return;
            }
            AYsettings.showSI = GUI.Toggle(new Rect(_dwindowPos.width - 45, 4, 16, 16), AYsettings.showSI, new GUIContent(Textures.BtnIS, "Toggle the display to use EC or SI units"), Textures.PartListbtnStyle);//, SubsystemButtonOptions);


            GUILayout.BeginVertical();
            GUILayout.Label(new GUIContent("Select Body", "Select the body for darkside period and Solar Panel usage EC calculations"), Textures.SectionTitleStyle, GUILayout.Width(280));
            _dSscrollViewVector = GUILayout.BeginScrollView(_dSscrollViewVector, GUILayout.Height(300), GUILayout.Width(320));
            string[] darkBodiesBtnNames = new string[_darkBodies.Count];
            for (int i = 0; i < _darkBodies.Count; i++)
            {
                darkBodiesBtnNames[i] = _darkBodies[i].theName;
            }
            _darkTargetSelection = _selectedDarkTarget;
            _darkTargetSelection = GUILayout.SelectionGrid(_darkTargetSelection, darkBodiesBtnNames, 1);
            GUILayout.EndScrollView();
            if (_darkTargetSelection != _selectedDarkTarget)
            {
                _selectedDarkTarget = _darkTargetSelection;
                //_selectedSolarSOITarget = _selectedDarkTarget;
                _bodyTarget = FlightGlobals.Bodies[_selectedDarkTarget];
            }
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Enter Orbit height: ", "The orbit height to use in Kilometers"), Textures.StatusStyleLeft, GUILayout.Width(140));
            strOrbit = _showDarkOrbit.ToString();
            Orbit = _showDarkOrbit;
            strOrbit = GUILayout.TextField(strOrbit, GUILayout.Width(40));
            GUILayout.Label("(Km)", GUILayout.Width(30));
            GUILayout.EndHorizontal();
            if (int.TryParse(strOrbit, out Orbit))
                _showDarkOrbit = Orbit;

            if (_bodyTarget != null)
            {
                GUILayout.Label(new GUIContent("Selected Body:" + _darkBodies[_selectedDarkTarget].theName, "Currently selected body for Dark-Side and Solar Panel EC production calculations"), Textures.PartListPartStyle, GUILayout.Width(280));
                //Get the distance and direction to Sun from the currently selected target body
                Utilities.CelestialBodyDistancetoSun(FlightGlobals.Bodies[_selectedDarkTarget], out sun_dir, out sun_dist);
                GUILayout.Label(new GUIContent("Sun Distance:" + sun_dist.ToString("###,###,###,###,###,##0"), _selectedDarkTarget == 0 ? "Assumes you are in orbit at 700,000km from the Sun" : "The Distance to the sun from the selected body"), Textures.PartListPartStyle, GUILayout.Width(280));
                rotPeriod = _darkBodies[_selectedDarkTarget].rotationPeriod;
                if (_darkBodies[_selectedDarkTarget].orbit != null)
                {
                    rotPeriod = _darkBodies[_selectedDarkTarget].orbit.period * rotPeriod / (_darkBodies[_selectedDarkTarget].orbit.period - rotPeriod);
                }
                GUILayout.Label( new GUIContent("Surface Darkness Time:" + KSPUtil.PrintTimeCompact((int)rotPeriod, true), "Darkness Time on the surface"), Textures.StatusStyleLeft, GUILayout.Width(300));
                darkTime = CalculatePeriod(_bodyTarget, Orbit);
                GUILayout.Label(new GUIContent("Dark-Side Transit Period: " + KSPUtil.PrintTimeCompact((int)darkTime, true), "The time it will take to transit the Darkside"), Textures.StatusStyleLeft, GUILayout.Width(300));
                if (TotalPowerDrain > 0)
                {
                    ECreqdfordarkTime = 0;
                    ECreqdfordarkTimeSurface = 0;
                    ECprodfordarkTime = 0;
                    ECprodfordarkTimeSurface = 0;
                    ECreqdfordarkTime = TotalPowerDrain * darkTime;
                    ECreqdfordarkTimeSurface = TotalPowerDrain * rotPeriod;
                    ECprodfordarkTime = TotalPowerProduced * darkTime;
                    ECprodfordarkTimeSurface = TotalPowerProduced * rotPeriod;
                    if (AYsettings.showSI)
                    {
                        tmpPrtPowerV = Utilities.ConvertECtoSI(ECreqdfordarkTimeSurface, out Units);
                        tmpPrtPower = tmpPrtPowerV.ToString("##########0") + Units;
                    }
                    else
                    {
                        tmpPrtPower = ECreqdfordarkTimeSurface.ToString("##########0");
                    }
                    GUILayout.Label(new GUIContent("EC required for Dark-Side Surface: " + tmpPrtPower, "EC required during darkside on the surface based on current EC usage"), Textures.WarningStyleLeft, GUILayout.Width(300));
                    if (AYsettings.showSI)
                    {
                        tmpPrtPowerV = Utilities.ConvertECtoSI(ECprodfordarkTimeSurface, out Units);
                        tmpPrtPower = tmpPrtPowerV.ToString("##########0") + Units;
                    }
                    else
                    {
                        tmpPrtPower = ECprodfordarkTimeSurface.ToString("##########0");
                    }
                    GUILayout.Label(new GUIContent("EC produced for Dark-Side Surface: " + tmpPrtPower, "EC produced during darkside on the surface based on current EC production"), Textures.StatusStyleLeft, GUILayout.Width(300));
                    if (AYsettings.showSI)
                    {
                        tmpPrtPowerV = Utilities.ConvertECtoSI(ECreqdfordarkTime, out Units);
                        tmpPrtPower = tmpPrtPowerV.ToString("##########0") + Units;
                    }
                    else
                    {
                        tmpPrtPower = ECreqdfordarkTime.ToString("##########0");
                    }
                    GUILayout.Label(new GUIContent("EC required for Dark-Side Transit: " + tmpPrtPower, "EC required during darkside period based on current EC usage"), Textures.WarningStyleLeft, GUILayout.Width(300));
                    if (AYsettings.showSI)
                    {
                        tmpPrtPowerV = Utilities.ConvertECtoSI(ECprodfordarkTime, out Units);
                        tmpPrtPower = tmpPrtPowerV.ToString("##########0") + Units;
                    }
                    else
                    {
                        tmpPrtPower = ECprodfordarkTime.ToString("##########0");
                    }
                    GUILayout.Label(new GUIContent("EC produced for Dark-Side Transit: " + tmpPrtPower, "EC produced during darkside period based on current EC production"), Textures.StatusStyleLeft, GUILayout.Width(300));

                    GUILayout.BeginHorizontal();
                    _includeStoredEc = GUILayout.Toggle(_includeStoredEc, new GUIContent(" ", "Toggle to Include Stored MainPower in Dark-Side Calculation"), Textures.SubsystemButtonStyle, SubsystemButtonOptions);
                    
                    if (AYsettings.showSI)
                    {
                        tmpPrtPowerV = Utilities.ConvertECtoSI(TotalElectricCharge, out Units);
                        tmpPrtPower = tmpPrtPowerV.ToString("##########0") + Units;
                    }
                    else
                    {
                        tmpPrtPower = TotalElectricCharge.ToString("##########0");
                    }
                    GUILayout.Label(new GUIContent("Total Stored EC: " + tmpPrtPower, "Total Stored EC on-board."), Textures.StatusStyleLeft, GUILayout.Width(300));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    _includeSoredRp = GUILayout.Toggle(_includeSoredRp, new GUIContent(" ", "Toggle to include Stored ReservePower in Dark-Side Calculation"), Textures.SubsystemButtonStyle, SubsystemButtonOptions);
                    if (AYsettings.showSI)
                    {
                        tmpPrtPowerV = Utilities.ConvertECtoSI(TotalReservePower, out Units);
                        tmpPrtPower = tmpPrtPowerV.ToString("##########0") + Units;
                    }
                    else
                    {
                        tmpPrtPower = TotalReservePower.ToString("##########0");
                    }
                    GUILayout.Label(new GUIContent("Total Stored ReservePower: " + tmpPrtPower, "Total Stored ReservePower on-board."), Textures.StatusStyleLeft, GUILayout.Width(300));
                    GUILayout.EndHorizontal();
                    powerSupply = (_includeStoredEc ? TotalElectricCharge : 0) + (_includeSoredRp ? TotalReservePower : 0) + ECprodfordarkTime;
                    eCdifference = 0;
                    if (ECreqdfordarkTime > powerSupply)
                    {
                        eCdifference = ECreqdfordarkTime - powerSupply;
                    }
                    else
                    {
                        eCdifference = powerSupply - ECreqdfordarkTime;
                    }
                    if (AYsettings.showSI)
                    {
                        tmpPrtPowerV = Utilities.ConvertECtoSI(eCdifference, out Units);
                        tmpPrtPower = tmpPrtPowerV.ToString("##########0") + Units;
                    }
                    else
                    {
                        tmpPrtPower = eCdifference.ToString("##########0");
                    }
                    if (ECreqdfordarkTime > powerSupply)
                    {
                        GUILayout.Label(new GUIContent("EC deficit for Dark-Side Transit: " + tmpPrtPower, "EC required for darkside period"), Textures.AlertStyleLeft, GUILayout.Width(300));
                    }
                    else
                    {
                        GUILayout.Label(new GUIContent("EC surplus for Dark-Side Transit: " + tmpPrtPower, "EC surplus for darkside period"), Textures.StatusStyleLeft, GUILayout.Width(300));
                    }

                    powerSupply = (_includeStoredEc ? TotalElectricCharge : 0) + (_includeSoredRp ? TotalReservePower : 0) + ECprodfordarkTimeSurface;
                    eCdifference = 0;
                    if (ECreqdfordarkTimeSurface > powerSupply)
                    {
                        eCdifference = ECreqdfordarkTimeSurface - powerSupply;
                    }
                    else
                    {
                        eCdifference = powerSupply - ECreqdfordarkTimeSurface;
                    }
                    if (AYsettings.showSI)
                    {
                        tmpPrtPowerV = Utilities.ConvertECtoSI(eCdifference, out Units);
                        tmpPrtPower = tmpPrtPowerV.ToString("##########0") + Units;
                    }
                    else
                    {
                        tmpPrtPower = eCdifference.ToString("##########0");
                    }
                    if (ECreqdfordarkTimeSurface > powerSupply)
                    {
                        GUILayout.Label(new GUIContent("EC deficit for Dark-Side Surface: " + tmpPrtPower, "EC required for darkside at the surface"), Textures.AlertStyleLeft, GUILayout.Width(300));
                    }
                    else
                    {
                        GUILayout.Label(new GUIContent("EC surplus for Dark-Side Surface: " + tmpPrtPower, "EC surplus for darkside at the surface"), Textures.StatusStyleLeft, GUILayout.Width(300));
                    }
                }
            }

            GUILayout.Space(10);
            tmpShowDarkSideWindow = ShowDarkSideWindow;
            ShowDarkSideWindow = !GUILayout.Button("Close");
            if (tmpShowDarkSideWindow != ShowDarkSideWindow)
            {
                AYVesselPartLists.ResetSolarPartToggles();
            }

            GUILayout.EndVertical();
            if (AYsettings.TooltipsOn)
                Utilities.SetTooltipText();
            GUI.DragWindow();
        }


        private bool inputLock;
        private bool mouseOver;

        /// <summary>
        ///     Updates the input lock.
        /// </summary>
        private void UpdateInputLock()
        {
            mouseOver = false; // position.MouseIsOver();
            if (AYMenuAppLToolBar.GuiVisible && !AYMenuAppLToolBar.gamePaused && !AYMenuAppLToolBar.hideUI)
            {
                if (Utilities.GameModeisFlight)
                    mouseOver = _fwindowPos.MouseIsOver() || (_showParts && _epLwindowPos.MouseIsOver());
                if (Utilities.GameModeisEditor)
                    mouseOver = _ewindowPos.MouseIsOver() || (_showParts && _epLwindowPos.MouseIsOver());
                inputLock = InputLock;

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

        //Set the iconAlertstate - DOES NOT PROCESS state = GREEN. If state = Yellow will only change to yellow if state is not already RED.
        //If state = red then it will be changed to RED.
        //If state = gray then it will be changed to GRAY.
        private void SetIconalertstate(IconAlertState state)
        {
            if (state == IconAlertState.YELLOW && _iconAlertState != IconAlertState.RED)
            {
                _iconAlertState = state;
            }
            if (state == IconAlertState.RED)
                _iconAlertState = state;
            if (state == IconAlertState.GRAY)
                _iconAlertState = state;
        }

        private void SetIcon()
        {
            //Time Remaining in Main batteries
            double timeRemainMains = TotalElectricCharge / TotalPowerDrain;

            string icontoSet = AYMenuAppLToolBar.GuiVisible ? "/AYGreenOnTB" : "/AYGreenOffTB";  //Default is Green 
            Texture iconToSet = AYMenuAppLToolBar.GuiVisible ? (Texture)Textures.IconGreenOn : (Texture)Textures.IconGreenOff; //Default is Green

            if (!AYMenuAppLToolBar.GuiVisible)  //We have to set the alert state here if GUI is not visible. Otherwise the GUI display method sets it.                
            {
                _iconAlertState = IconAlertState.GREEN;
                if (Utilities.GameModeisFlight)
                {
                    double powerPercent = TotalElectricCharge / TotalElectricChargeCapacity * 100.0;
                    if (powerPercent < 20.00)
                    {
                        SetIconalertstate(IconAlertState.RED);
                    }
                    else
                    {
                        if (powerPercent < 35)
                            SetIconalertstate(IconAlertState.YELLOW);
                    }
                }

                if (timeRemainMains < 300) //3 mins
                {
                    SetIconalertstate(IconAlertState.RED);
                }
                else
                {
                    if (timeRemainMains < 1800) //30 mins
                    {
                        SetIconalertstate(IconAlertState.YELLOW);
                    }
                }

                if (TotalPowerDrain > TotalPowerProduced) //EC drain > production
                {
                    SetIconalertstate(IconAlertState.RED);
                }
            }

            //if toolbar
            if (AYMenuAppLToolBar.usingToolBar)
            {
                // Set Icon based on the iconAlertState
                if (_iconAlertState == IconAlertState.RED)
                {
                    icontoSet = AYMenuAppLToolBar.GuiVisible ? "/AYRedOnTB" : "/AYRedOffTB";
                }
                else
                {
                    if (_iconAlertState == IconAlertState.YELLOW)
                    {
                        icontoSet = AYMenuAppLToolBar.GuiVisible ? "/AYYellowOnTB" : "/AYYellowOffTB";
                    }
                    else
                    {
                        if (_iconAlertState == IconAlertState.GRAY)
                        {
                            icontoSet = AYMenuAppLToolBar.GuiVisible ? "/AYGrayOnTB" : "/AYGrayOffTB";
                        }
                    }
                }
                if (Utilities.GameModeisFlight && !ManagerIsActive)
                {
                    icontoSet = AYMenuAppLToolBar.GuiVisible ? "/AYGrayOnTB" : "/AYGrayOffTB";
                }
                AYMenuAppLToolBar.setToolBarTexturePath(Textures.PathToolbarIconsPath + icontoSet);
            }
            else  //Stock applauncher
            {
                if (AYMenuAppLToolBar.StockButtonNotNull)
                {
                    // Set Icon
                    if (_iconAlertState == IconAlertState.RED)
                    {
                        iconToSet = AYMenuAppLToolBar.GuiVisible ? (Texture)Textures.IconRedOn : (Texture)Textures.IconRedOff;
                    }
                    else
                    {
                        if (_iconAlertState == IconAlertState.YELLOW)
                        {
                            iconToSet = AYMenuAppLToolBar.GuiVisible ? (Texture)Textures.IconYellowOn : (Texture)Textures.IconYellowOff;
                        }
                        else
                        {
                            if (_iconAlertState == IconAlertState.GRAY)
                            {
                                iconToSet = AYMenuAppLToolBar.GuiVisible ? (Texture)Textures.IconGrayOn : (Texture)Textures.IconGrayOff;
                            }
                        }
                    }
                    if (Utilities.GameModeisFlight && !ManagerIsActive)
                    {
                        iconToSet = AYMenuAppLToolBar.GuiVisible ? (Texture)Textures.IconGrayOn : (Texture)Textures.IconGrayOff;
                    }
                    AYMenuAppLToolBar.setAppLauncherTexture(iconToSet);
                }
            }
        }

        private void HandleResizeWidthEvents(Rect resizeRect)
        {
            var theEvent = Event.current;
            if (theEvent != null)
            {
                if (!_mouseDownResize)
                {
                    if (theEvent.type == EventType.MouseDown && theEvent.button == 0 && resizeRect.Contains(theEvent.mousePosition))
                    {
                        _mouseDownResize = true;
                        theEvent.Use();
                    }
                }
                else if (theEvent.type != EventType.Layout)
                {
                    if (Input.GetMouseButton(0) && _mouseDownResize)
                    {
                        // Flip the mouse Y so that 0 is at the top
                        float mouseY = Screen.height - Input.mousePosition.y;
                        
                        //Do Window Width change processing.
                        _epLwindowPos.width = Mathf.Clamp(Input.mousePosition.x - _epLwindowPos.x + resizeRect.width / 2, 475, Utilities.scaledScreenWidth - _epLwindowPos.x);
                        _eplPartName = Mathf.Round((_epLwindowPos.width - 28f) * .3f);
                        _eplPartModuleName = Mathf.Round((_epLwindowPos.width - 28f) * .4f);
                        _eplec = Mathf.Round((_epLwindowPos.width - 28f) * .17f);
                    }
                    else
                    {
                        _mouseDownResize = false;
                    }
                }
            }
        }

        private void HandleResizeHeightScrl1Events(Rect resizeRect, Rect scrollboxin)
        {
            var theEvent = Event.current;
            if (theEvent != null)
            {
                if (!_mouseDownHorizScl1)
                {
                    if (theEvent.type == EventType.MouseDown && theEvent.button == 0 && resizeRect.Contains(theEvent.mousePosition))
                    {
                        _mouseDownHorizScl1 = true;
                        theEvent.Use();
                    }
                }
                else if (theEvent.type != EventType.Layout)
                {
                    if (Input.GetMouseButton(0) && _mouseDownHorizScl1)
                    {
                        // Flip the mouse Y so that 0 is at the top
                        float mouseY = Screen.height - Input.mousePosition.y;
                        
                        //Save the original first scroll box values.
                        //float originalprodlist = scrollboxin.height;

                        //Calculate the new scroll box value. MouseY position less (top of the window box y position + first scroll box y position + 4?)
                        //Clamp the value to be a minimum of 37 pixels, maximum of the total window box height less 164 + other box (all the headers, footers and other box).
                        _eplProdListHeight = Mathf.Clamp(mouseY - (_epLwindowPos.y + scrollboxin.y + 4), 37, Utilities.scaledScreenHeight - _epLwindowPos.y);

                        _epLwindowPos.height = _eplProdlistbox.y + _eplProdListHeight + _eplHeaders2.height + _eplConsListHeight + 22;
                    }
                    else
                    {
                        _mouseDownHorizScl1 = false;
                    }
                }
            }
        }

        private void HandleResizeHeightScrl2Events(Rect resizeRect, Rect scrollboxin)
        {
            var theEvent = Event.current;
            if (theEvent != null)
            {
                if (!_mouseDownHorizScl2)
                {
                    if (theEvent.type == EventType.MouseDown && theEvent.button == 0 && resizeRect.Contains(theEvent.mousePosition))
                    {
                        _mouseDownHorizScl2 = true;
                        theEvent.Use();
                    }
                }
                else if (theEvent.type != EventType.Layout)
                {
                    if (Input.GetMouseButton(0) && _mouseDownHorizScl2)
                    {
                        // Flip the mouse Y so that 0 is at the top
                        float mouseY = Screen.height - Input.mousePosition.y;

                        //Save the original first scroll box values.
                        //float originalprodlist = scrollboxin.height;

                        //Calculate the new scroll box value. MouseY position less (top of the window box y position + first scroll box y position + 4?)
                        //Clamp the value to be a minimum of 37 pixels, maximum of the total window box height less 164 + other box (all the headers, footers and other box).
                        _eplConsListHeight = Mathf.Clamp(mouseY - (_epLwindowPos.y + scrollboxin.y + 4), 37, Utilities.scaledScreenHeight - _epLwindowPos.y);

                        _epLwindowPos.height = _eplProdlistbox.y + _eplProdListHeight + _eplHeaders2.height + _eplConsListHeight + 22;
                    }
                    else
                    {
                        _mouseDownHorizScl2 = false;
                    }
                }
            }
        }

        private string tmpToolTipSS;
        private void SubsystemButton(Subsystem subsystem)
        {
            tmpToolTipSS = "Enable/Disable " + SubsystemName(subsystem);
            if (subsystem == Subsystem.SAS)
            {
                if (!FlightGlobals.ActiveVessel.Autopilot.CanSetMode(VesselAutopilot.AutopilotMode.StabilityAssist))
                {
                    GUI.enabled = false;
                    tmpToolTipSS = "Vessel has no SAS modules";
                }
                else
                {
                    tmpToolTipSS = "Toggle SAS on or off";
                }
            }
            SetSubsystemEnabled(
                subsystem,
                GUILayout.Toggle(SubsystemEnabled(subsystem), new GUIContent(SubsystemName(subsystem), tmpToolTipSS), Textures.SubsystemButtonStyle, GUILayout.Width(FWINDOW_WIDTH / 2.0f))
                );
            GUI.enabled = true;
        }

        private void SubsystemConsumptionLabel(Subsystem subsystem)
        {
            double drain = _subsystemDrain[(int)subsystem];
            switch (subsystem)
            {
                case Subsystem.RCS:
                    if (currentPoweredRCSDrain > 0.001)
                        drain += drain + currentPoweredRCSDrain;
                    break;

                default:
                    break;
            }
            if (drain == 0.0)
            {
                drain = SubsystemActiveDrain(subsystem);
                ConsumptionLabel(drain, true);
            }
            else
                ConsumptionLabel(drain, false);
        }

        private void ConsumptionLabel(double drain, bool greyed = false)
        {
            if (drain == 0.0 || greyed)
                Textures.SubsystemConsumptionStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
            else if (drain > 0.0)
                Textures.SubsystemConsumptionStyle.normal.textColor = Color.red;
            else
                Textures.SubsystemConsumptionStyle.normal.textColor = Color.green;
            if (AYsettings.showSI)
            {
                tmpPrtPowerV = Utilities.ConvertECtoSI(drain, out Units);
                tmpPrtPower = tmpPrtPowerV.ToString("0.###") + Units;
            }
            else
            {
                tmpPrtPower = drain.ToString("0.###");
            }
            GUILayout.Label(new GUIContent(tmpPrtPower + "/s", "The current EC drain per second if enabled"), Textures.SubsystemConsumptionStyle);
        }

        private static string GuiSectionName(GUISection section)
        {
            switch (section)
            {
                case GUISection.SUBSYSTEM:
                    return "Subsys";

                case GUISection.RESERVE:
                    return "Reserve";

                case GUISection.LUXURY:
                    return "Luxury";

                default:
                    return String.Empty;
            }
        }

        private bool GuiSectionEnabled(GUISection section)
        {
            return _guiSectionEnableFlag[(int)section];
        }

        private static bool SubsystemIsLuxury(Subsystem subsystem)
        {
            switch (subsystem)
            {
                case Subsystem.CLIMATE:
                case Subsystem.MUSIC:
                case Subsystem.MASSAGE:
                    return true;

                default:
                    return false;
            }
        }

        private bool SubsystemVisible(Subsystem subsystem)
        {
            Vessel cv = FlightGlobals.ActiveVessel;
            switch (subsystem)
            {
                case Subsystem.RCS:
                    return HasRcs;

                case Subsystem.CLIMATE:
                case Subsystem.MUSIC:
                    return crewablePartList.Count > 0;

                case Subsystem.MASSAGE:
                    return cv.GetCrewCount() > 0;

                default:
                    return true;
            }
        }

        #endregion GUIFunctions
    }
}
