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
using System.Text.RegularExpressions;
using UnityEngine;

namespace AY
{
    internal class AYSCController : MonoBehaviour, Savable
    {
        //GUI Properties
        private IButton button1;

        private ApplicationLauncherButton stockToolbarButton = null; // Stock Toolbar Button
        private const float SCWINDOW_WIDTH = 400;
        private const float WINDOW_BASE_HEIGHT = 140;
        public Rect SCwindowPos = new Rect(40, Screen.height / 2 - 100, SCWINDOW_WIDTH, 200);
        private static int windowID = new System.Random().Next();
        private GUIStyle statusStyle, sectionTitleStyle;

        private static bool debugging = false;
        private String InputSMBDF = "";
        private double InputVMBDF = 0f;
        private String InputSRRT = "";
        private double InputVRRT = 0f;
        private bool InputSCF = true;
        private bool InputVCF = true;
        private string InputSCCBDF = "";
        private double InputVCCBDF = 0f;
        private string InputSCBDF = "";
        private double InputVCBDF = 0f;
        private string InputSCTT = "";
        private float InputVCTT = 0f;
        private string InputSCCUF = "";
        private double InputVCCUF = 0f;
        private string InputSCCRF = "";
        private double InputVCCRF = 0f;
        private string InputSCRRF = "";
        private double InputVCRRF = 0f;
        private string InputSCMRF = "";
        private double InputVCMRF = 0f;
        private string InputSCMinL = "";
        private double InputVCMinL = 0f;
        private string InputSCMajL = "";
        private double InputVCMajL = 0f;
        private string InputSPLW = "";
        private double InputVPLW = 0f;
        private bool InputAppL = false;
        private bool InputSdebug = debugging;
        private bool InputVdebug = debugging;
        private bool LoadSettingsSC = true;
        public double CLIMATE_BASE_DRAIN_FACTOR = 1.0;
        public float CLIMATE_TARGET_TEMP = 20.0f;
        public double MASSAGE_BASE_DRAIN_FACTOR = 3.0;
        public double RECHARGE_RESERVE_THRESHOLD = 0.95;
        public double POWER_LOW_WARNING_AMT = 5;
        public bool Craziness_Function = true;
        public double CRAZY_BASE_DRAIN_FACTOR = 0.05;
        public double CRAZY_CLIMATE_UNCOMF_FACTOR = 0.02;
        public double CRAZY_CLIMATE_REDUCE_FACTOR = 0.1;
        public double CRAZY_RADIO_REDUCE_FACTOR = 0.1;
        public double CRAZY_MASSAGE_REDUCE_FACTOR = 0.2;
        public double CRAZY_MINOR_LIMIT = 59;
        public double CRAZY_MAJOR_LIMIT = 89;
        public bool Useapplauncher = false;

        //AmpYear Savable settings
        private AYSettings AYsettings;

        private AYGameSettings AYgameSettings;

        //GuiVisibility
        private bool _Visible = false;

        public Boolean GuiVisible
        {
            get { return _Visible; }
            set
            {
                _Visible = value;      //Set the private variable
                if (_Visible)
                {
                    RenderingManager.AddToPostDrawQueue(5, this.onDraw);
                }
                else
                {
                    RenderingManager.RemoveFromPostDrawQueue(5, this.onDraw);
                }
            }
        }

        public void Awake()
        {
            AYsettings = AmpYear.Instance.AYsettings;
            AYgameSettings = AmpYear.Instance.AYgameSettings;

            // create toolbar button
            if (ToolbarManager.ToolbarAvailable && AYsettings.UseAppLauncher == false)
            {
                button1 = ToolbarManager.Instance.add("AmpYear", "button1");
                button1.TexturePath = "AmpYear/Icons/toolbarIcon";
                button1.ToolTip = "AmpYear";
                button1.Visibility = new GameScenesVisibility(GameScenes.SPACECENTER);
                button1.OnClick += (e) => GuiVisible = !GuiVisible;
            }
            else
            {
                // Set up the stock toolbar
                this.Log_Debug("Adding onGUIAppLauncher callbacks");
                if (ApplicationLauncher.Ready)
                {
                    OnGUIAppLauncherReady();
                }
                else
                    GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            }
            this.Log_Debug("AYSCController Awake complete");
        }

        private void OnGUIAppLauncherReady()
        {
            this.Log_Debug("OnGUIAppLauncherReady");
            if (ApplicationLauncher.Ready)
            {
                this.Log_Debug("Adding AppLauncherButton");
                this.stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(onAppLaunchToggleOn, onAppLaunchToggleOff, DummyVoid,
                                          DummyVoid, DummyVoid, DummyVoid, ApplicationLauncher.AppScenes.SPACECENTER,
                                          (Texture)GameDatabase.Instance.GetTexture("AmpYear/Icons/AYIconOff", false));
            }
        }

        private void DummyVoid()
        {
        }

        private void onAppLaunchToggleOn()
        {
            this.stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture("AmpYear/Icons/AYIconOn", false));
            GuiVisible = true;
        }

        private void onAppLaunchToggleOff()
        {
            this.stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture("AmpYear/Icons/AYIconOff", false));
            GuiVisible = false;
        }

        public void OnDestroy()
        {
            if (ToolbarManager.ToolbarAvailable && AYsettings.UseAppLauncher == false)
            {
                button1.Destroy();
            }
            else
            {
                // Set up the stock toolbar
                this.Log_Debug("Removing onGUIAppLauncher callbacks");
                GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
                if (this.stockToolbarButton != null)
                {
                    ApplicationLauncher.Instance.RemoveModApplication(this.stockToolbarButton);
                    this.stockToolbarButton = null;
                }
            }

            if (GuiVisible) GuiVisible = !GuiVisible;
        }

        //GUI Functions Follow

        private void onDraw()
        {
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                if (LoadSettingsSC)
                {
                    InputSCCBDF = CLIMATE_BASE_DRAIN_FACTOR.ToString();
                    InputVCCBDF = CLIMATE_BASE_DRAIN_FACTOR;
                    InputSCTT = CLIMATE_TARGET_TEMP.ToString();
                    InputVCTT = CLIMATE_TARGET_TEMP;
                    InputSMBDF = MASSAGE_BASE_DRAIN_FACTOR.ToString();
                    InputVMBDF = MASSAGE_BASE_DRAIN_FACTOR;
                    InputSRRT = (100 * RECHARGE_RESERVE_THRESHOLD).ToString();
                    InputVRRT = RECHARGE_RESERVE_THRESHOLD;
                    InputSCBDF = CRAZY_BASE_DRAIN_FACTOR.ToString();
                    InputVCBDF = CRAZY_BASE_DRAIN_FACTOR;
                    InputSCCUF = CRAZY_CLIMATE_UNCOMF_FACTOR.ToString();
                    InputVCCUF = CRAZY_CLIMATE_UNCOMF_FACTOR;
                    InputSCCRF = CRAZY_CLIMATE_REDUCE_FACTOR.ToString();
                    InputVCCRF = CRAZY_CLIMATE_REDUCE_FACTOR;
                    InputSCRRF = CRAZY_RADIO_REDUCE_FACTOR.ToString();
                    InputVCRRF = CRAZY_RADIO_REDUCE_FACTOR;
                    InputSCMRF = CRAZY_MASSAGE_REDUCE_FACTOR.ToString();
                    InputVCMRF = CRAZY_MASSAGE_REDUCE_FACTOR;
                    InputSCMinL = CRAZY_MINOR_LIMIT.ToString();
                    InputVCMinL = CRAZY_MINOR_LIMIT;
                    InputSCMajL = CRAZY_MAJOR_LIMIT.ToString();
                    InputVCMajL = CRAZY_MAJOR_LIMIT;
                    InputSPLW = POWER_LOW_WARNING_AMT.ToString();
                    InputVPLW = POWER_LOW_WARNING_AMT;
                    InputAppL = Useapplauncher;
                    InputSdebug = debugging;
                    InputVdebug = debugging;
                    InputSCF = Craziness_Function;
                    InputVCF = Craziness_Function;
                    LoadSettingsSC = false;
                }
                GUI.skin = HighLogic.Skin;
                if (!Utilities.WindowVisibile(SCwindowPos))
                    Utilities.MakeWindowVisible(SCwindowPos);
                SCwindowPos = GUILayout.Window(windowID, SCwindowPos, windowSC, "AmpYear Power Manager Settings",
                    GUILayout.Width(SCWINDOW_WIDTH), GUILayout.Height(WINDOW_BASE_HEIGHT));
            }
        }

        private void windowSC(int id)
        {
            //Init styles
            sectionTitleStyle = new GUIStyle(GUI.skin.label);
            sectionTitleStyle.alignment = TextAnchor.MiddleCenter;
            sectionTitleStyle.stretchWidth = true;
            sectionTitleStyle.fontStyle = FontStyle.Bold;

            statusStyle = new GUIStyle(GUI.skin.label);
            statusStyle.alignment = TextAnchor.MiddleCenter;
            statusStyle.stretchWidth = true;
            statusStyle.normal.textColor = Color.white;

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Box("Climate Control Elec. Drain", statusStyle, GUILayout.Width(300));
            InputSCCBDF = Regex.Replace(GUILayout.TextField(InputSCCBDF, 2, GUILayout.MinWidth(10.0F)), "[^.0-9]", ""); //you can play with the width of the text box

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Box("Climate Control Target Temp.", statusStyle, GUILayout.Width(300));
            InputSCTT = Regex.Replace(GUILayout.TextField(InputSCTT, 2, GUILayout.MinWidth(30.0F)), "[^.0-9]", ""); //you can play with the width of the text box

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Box("Massage Electrical Drain", statusStyle, GUILayout.Width(300));
            InputSMBDF = Regex.Replace(GUILayout.TextField(InputSMBDF, 2, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Box("Reserve Battery Recharge Percentage", statusStyle, GUILayout.Width(300));
            InputSRRT = Regex.Replace(GUILayout.TextField(InputSRRT, 3, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Box("Power Low Warning Percentage", statusStyle, GUILayout.Width(300));
            InputSPLW = Regex.Replace(GUILayout.TextField(InputSPLW, 3, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Box("Kraziness Base Growth Amount", statusStyle, GUILayout.Width(300));
            InputSCBDF = Regex.Replace(GUILayout.TextField(InputSCBDF, 5, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Box("Temp Uncomfortable Craziness", statusStyle, GUILayout.Width(300));
            InputSCCUF = Regex.Replace(GUILayout.TextField(InputSCCUF, 5, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Box("Climate Kraziness Reduction", statusStyle, GUILayout.Width(300));
            InputSCCRF = Regex.Replace(GUILayout.TextField(InputSCCRF, 5, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Box("Music Kraziness Reduction", statusStyle, GUILayout.Width(300));
            InputSCRRF = Regex.Replace(GUILayout.TextField(InputSCRRF, 5, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Box("Massage Kraziness Reduction", statusStyle, GUILayout.Width(300));
            InputSCMRF = Regex.Replace(GUILayout.TextField(InputSCMRF, 5, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Box("Kraziness Minor Problem Limit", statusStyle, GUILayout.Width(300));
            InputSCMinL = Regex.Replace(GUILayout.TextField(InputSCMinL, 5, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Box("Craziness Major Problem Limit", statusStyle, GUILayout.Width(300));
            InputSCMajL = Regex.Replace(GUILayout.TextField(InputSCMajL, 5, GUILayout.MinWidth(30.0F)), "[^.0-9]", "");  //you can play with the width of the text box

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Box("KabinKraziness Function", statusStyle, GUILayout.Width(300));
            InputVCF = GUILayout.Toggle(InputVCF, "", GUILayout.MinWidth(30.0F)); //you can play with the width of the text box

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Box("Use Application Launcher", statusStyle, GUILayout.Width(300));
            InputAppL = GUILayout.Toggle(InputAppL, "", GUILayout.MinWidth(30.0F)); //you can play with the width of the text box

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Box("Debug Mode", statusStyle, GUILayout.Width(300));
            InputSdebug = GUILayout.Toggle(InputSdebug, "", GUILayout.MinWidth(30.0F)); //you can play with the width of the text box
            InputVdebug = InputSdebug;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            if (!Double.TryParse(InputSCCBDF, out InputVCCBDF))
            {
                InputVCCBDF = CLIMATE_BASE_DRAIN_FACTOR;
            }
            if (!float.TryParse(InputSCTT, out InputVCTT))
            {
                InputVCTT = CLIMATE_TARGET_TEMP;
            }
            if (!Double.TryParse(InputSMBDF, out InputVMBDF))
            {
                InputVMBDF = MASSAGE_BASE_DRAIN_FACTOR;
            }
            if (!Double.TryParse(InputSRRT, out InputVRRT))
            {
                InputVRRT = RECHARGE_RESERVE_THRESHOLD;
            }
            if (!Double.TryParse(InputSPLW, out InputVPLW))
            {
                InputVPLW = POWER_LOW_WARNING_AMT;
            }
            if (!Double.TryParse(InputSCBDF, out InputVCBDF))
            {
                InputVCBDF = CRAZY_BASE_DRAIN_FACTOR;
            }
            if (!Double.TryParse(InputSCCUF, out InputVCCUF))
            {
                InputVCCUF = CRAZY_CLIMATE_UNCOMF_FACTOR;
            }
            if (!Double.TryParse(InputSCCRF, out InputVCCRF))
            {
                InputVCCRF = CRAZY_CLIMATE_REDUCE_FACTOR;
            }
            if (!Double.TryParse(InputSCRRF, out InputVCRRF))
            {
                InputVCRRF = CRAZY_RADIO_REDUCE_FACTOR;
            }
            if (!Double.TryParse(InputSCMRF, out InputVCMRF))
            {
                InputVCMRF = CRAZY_MASSAGE_REDUCE_FACTOR;
            }
            if (!Double.TryParse(InputSCMinL, out InputVCMinL))
            {
                InputVCMinL = CRAZY_MINOR_LIMIT;
            }
            if (!Double.TryParse(InputSCMajL, out InputVCMajL))
            {
                InputVCMajL = CRAZY_MAJOR_LIMIT;
            }
            if (GUILayout.Button("Save Settings"))
            {
                CLIMATE_BASE_DRAIN_FACTOR = InputVCCBDF;
                CLIMATE_TARGET_TEMP = InputVCTT;
                MASSAGE_BASE_DRAIN_FACTOR = InputVMBDF;
                RECHARGE_RESERVE_THRESHOLD = InputVRRT / 100;
                POWER_LOW_WARNING_AMT = InputVPLW;
                CRAZY_BASE_DRAIN_FACTOR = InputVCBDF;
                CRAZY_CLIMATE_UNCOMF_FACTOR = InputVCCUF;
                CRAZY_CLIMATE_REDUCE_FACTOR = InputVCCRF;
                CRAZY_RADIO_REDUCE_FACTOR = InputVCRRF;
                CRAZY_MASSAGE_REDUCE_FACTOR = InputVCMRF;
                CRAZY_MINOR_LIMIT = InputVCMinL;
                CRAZY_MAJOR_LIMIT = InputVCMajL;
                Craziness_Function = InputVCF;
                Useapplauncher = InputAppL;
                debugging = InputVdebug;
                //onAppLaunchToggleOff();
                LoadSettingsSC = true;
            }
            if (GUILayout.Button("Reset Settings"))
            {
                //onAppLaunchToggleOff();
                LoadSettingsSC = true;
            }
            if (!Input.GetMouseButtonDown(1))
            {
                GUI.DragWindow();
            }
        }

        //Class Load and Save of global settings
        public void Load(ConfigNode globalNode)
        {
            this.Log_Debug("AYSCController Load");
            SCwindowPos.x = AYsettings.SCwindowPosX;
            SCwindowPos.y = AYsettings.SCwindowPosY;
            CLIMATE_BASE_DRAIN_FACTOR = AYsettings.CLIMATE_BASE_DRAIN_FACTOR;
            CLIMATE_TARGET_TEMP = AYsettings.CLIMATE_TARGET_TEMP;
            MASSAGE_BASE_DRAIN_FACTOR = AYsettings.MASSAGE_BASE_DRAIN_FACTOR;
            RECHARGE_RESERVE_THRESHOLD = AYsettings.RECHARGE_RESERVE_THRESHOLD;
            POWER_LOW_WARNING_AMT = AYsettings.POWER_LOW_WARNING_AMT;
            Craziness_Function = AYsettings.Craziness_Function;
            CRAZY_BASE_DRAIN_FACTOR = AYsettings.CRAZY_BASE_DRAIN_FACTOR;
            CRAZY_CLIMATE_UNCOMF_FACTOR = AYsettings.CRAZY_CLIMATE_UNCOMF_FACTOR;
            CRAZY_CLIMATE_REDUCE_FACTOR = AYsettings.CRAZY_CLIMATE_REDUCE_FACTOR;
            CRAZY_RADIO_REDUCE_FACTOR = AYsettings.CRAZY_RADIO_REDUCE_FACTOR;
            CRAZY_MASSAGE_REDUCE_FACTOR = AYsettings.CRAZY_MASSAGE_REDUCE_FACTOR;
            CRAZY_MINOR_LIMIT = AYsettings.CRAZY_MINOR_LIMIT;
            CRAZY_MAJOR_LIMIT = AYsettings.CRAZY_MAJOR_LIMIT;
            Useapplauncher = AYsettings.UseAppLauncher;
            debugging = AYsettings.debugging;
            this.Log_Debug("AYController Load end");
        }

        public void Save(ConfigNode globalNode)
        {
            this.Log_Debug("AYSCController Save");
            AYsettings.SCwindowPosX = SCwindowPos.x;
            AYsettings.SCwindowPosY = SCwindowPos.y;
            AYsettings.CLIMATE_BASE_DRAIN_FACTOR = CLIMATE_BASE_DRAIN_FACTOR;
            AYsettings.CLIMATE_TARGET_TEMP = CLIMATE_TARGET_TEMP;
            AYsettings.MASSAGE_BASE_DRAIN_FACTOR = MASSAGE_BASE_DRAIN_FACTOR;
            AYsettings.RECHARGE_RESERVE_THRESHOLD = RECHARGE_RESERVE_THRESHOLD;
            AYsettings.POWER_LOW_WARNING_AMT = POWER_LOW_WARNING_AMT;
            AYsettings.Craziness_Function = Craziness_Function;
            AYsettings.CRAZY_BASE_DRAIN_FACTOR = CRAZY_BASE_DRAIN_FACTOR;
            AYsettings.CRAZY_CLIMATE_UNCOMF_FACTOR = CRAZY_CLIMATE_UNCOMF_FACTOR;
            AYsettings.CRAZY_CLIMATE_REDUCE_FACTOR = CRAZY_CLIMATE_REDUCE_FACTOR;
            AYsettings.CRAZY_RADIO_REDUCE_FACTOR = CRAZY_RADIO_REDUCE_FACTOR;
            AYsettings.CRAZY_MASSAGE_REDUCE_FACTOR = CRAZY_MASSAGE_REDUCE_FACTOR;
            AYsettings.CRAZY_MINOR_LIMIT = CRAZY_MINOR_LIMIT;
            AYsettings.CRAZY_MAJOR_LIMIT = CRAZY_MAJOR_LIMIT;
            AYsettings.UseAppLauncher = Useapplauncher;
            AYsettings.debugging = debugging;
            this.Log_Debug("AYController Save end");
        }
    }
}