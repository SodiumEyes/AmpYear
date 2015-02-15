/**
 * AYController.cs
 *
 * AmpYear power management.
 * (C) Copyright 2015, Jamie Leighton
 * The original code and concept of AmpYear rights go to SodiumEyes on the Kerbal Space Program Forums, which was covered by GNU License GPL (no version stated).
 * As such this code continues to be covered by GNU GPL license.
 * Parts of this code were copied from Fusebox by the user ratzap on the Kerbal Space Program Forums, which is covered by GNU License GPLv2.
 * Concepts which are common to the Game Kerbal Space Program for which there are common code interfaces as such some of those concepts used
 * by this program were based on:-checkTACL
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
using System.Linq;
using UnityEngine;

namespace AY
{
    public partial class AYController : MonoBehaviour, Savable
    {
        public class PwrPartList
        {
            public string PrtName { get; set; }

            public string PrtPower { get; set; }

            public float PrtPowerF { get; set; }

            public bool PrtActive { get; set; }

            public PwrPartList(string prtName, string prtPower, float prtPowerF, bool prtActive)
            {
                PrtName = prtName;
                PrtPower = prtPower;
                PrtPowerF = prtPowerF;
                PrtActive = prtActive;
            }

            public string Serialize()
            {
                return "PrtName=" + PrtName + ",PrtPower=" + PrtPower + "PrtPowerF=" + PrtPowerF.ToString("000.00000000") + ",PrtActive=" + PrtActive;
            }

            public static PwrPartList DeserializeResource(string str)
            {
                string[] arr = str.Split(',');
                string prtName = arr[0].Split('=')[1];
                string prtPower = prtName;

                if (arr.Length == 2)
                {
                    prtPower = arr[1].Split('=')[1];
                }

                float prtPowerF = 0;
                if (arr.Length == 3)
                {
                    string prtPowerStr = arr[2].Split('=')[1];
                    bool prse = float.TryParse(prtPowerStr, out prtPowerF);
                    if (!prse) prtPowerF = 0;
                }

                bool prtActive = false;
                if (arr.Length == 4)
                {
                    string prtActStr = arr[3].Split('=')[1];
                    bool prse = bool.TryParse(prtActStr, out prtActive);
                    if (!prse) prtActive = false;
                }
                return new PwrPartList(prtName, prtPower, prtPowerF, prtActive);
            }
        }

        //AmpYear Properties
        public static List<Part> crewablePartList = new List<Part>();                
        public Dictionary<uint, PwrPartList> vesselProdPartsList { get; private set; }
        public Dictionary<uint, PwrPartList> vesselConsPartsList { get; private set; }
        public static List<CommandPod> commandPods = new List<CommandPod>();
        public static List<ProtoCrewMember> VslRstr = new List<ProtoCrewMember>();
        public static float sasAdditionalRotPower = 0.0f;
        public static double turningFactor = 0.0;
        public static double totalElectricCharge = 0.0;
        public static double totalElectricChargeCapacity = 0.0;
        public static double totalReservePower = 0.0;
        public static double totalReservePowerCapacity = 0.0;
        public static double totalPowerDrain = 0.0;
        public static double totalPowerProduced = 0.0;
        public static bool hasPower = true;
        public static bool hasReservePower = true;
        public static bool hasRCS = false;
        public static float currentRCSThrust = 0.0f;
        public static float currentPoweredRCSDrain = 0.0f;
        public static Guid currentvesselid;
        public static int totalHeatedParts = 0;
        public static int totalCooledParts = 0;
        public static int maxCrew = 0;
        private bool ALPresent = false;
        private bool NFEPresent = false;
        private bool NFSPresent = false;
        private bool KASPresent = false;
        private bool RT2Present = false;
        private bool ScSPresent = false;
        private bool TelPresent = false;
        private bool TACLPresent = false;
        private bool KISPPresent = false;
        private bool BioPresent = false;
        private bool AntRPresent = false;
        private bool RegoPresent = false;
        private bool BTSMPresent = false;
        private bool RTKolPresent = false;
        private bool reenableRCS = false;
        private bool reenableSAS = false;
        public Dictionary<String, float> podRotPowerMap = new Dictionary<string, float>();

        //GUI Properties
        private IButton button1;
        private ApplicationLauncherButton stockToolbarButton = null; // Stock Toolbar Button
        private readonly double[] RESERVE_TRANSFER_INCREMENTS = new double[3] { 0.25, 0.1, 0.01 };
        private bool[] guiSectionEnableFlag = new bool[Enum.GetValues(typeof(GUISection)).Length];
        private const float FWINDOW_WIDTH = 200;
        private const float EWINDOW_WIDTH = 200;
        private const float WINDOW_BASE_HEIGHT = 140;
        private Vector2 scrollViewVector = Vector2.zero;
        private GUIStyle sectionTitleStyle, subsystemButtonStyle, subsystemConsumptionStyle, statusStyle, warningStyle, powerSinkStyle;
        public GUILayoutOption[] subsystemButtonOptions;
        private static int windowID = new System.Random().Next();
        private static GameState mode = GameState.EVA;  // Display mode, currently  0 for In-Flight, 1 for Editor, 2 for EVA (Hide)
        private static bool[] subsystemToggle = new bool[Enum.GetValues(typeof(Subsystem)).Length];
        private static double[] subsystemDrain = new double[Enum.GetValues(typeof(Subsystem)).Length];
        private bool managerEnabled = true;
        private bool ShowCrew = false;
        private bool ShowParts = false;
        private float powerUpTime = 0.0f;
        public static float sumDeltaTime = 0f;
        private static bool doOneUpdate = false;
        public bool EmgcyShutActive = false;
        //private static bool debugging = false;        

        public Rect FwindowPos = new Rect(40, Screen.height / 2 - 100, AYController.FWINDOW_WIDTH, 200);
        public Rect EwindowPos = new Rect(40, Screen.height / 2 - 100, AYController.EWINDOW_WIDTH, 200);

        //Constants
        public const double MANAGER_ACTIVE_DRAIN = 1.0 / 60.0;
        public const double RCS_DRAIN = 1.0 / 60.0;        
        public const float POWER_UP_DELAY = 1f;
        public const double SAS_BASE_DRAIN = 1.0 / 60.0;
        public const double POWER_TURN_DRAIN_FACTOR = 1.0 / 5.0;
        public const float SAS_POWER_TURN_TORQUE_FACTOR = 0.25f;
        public const float HEATER_HEAT_RATE = 2.0f;
        public const double HEATER_CAPACITY_DRAIN_FACTOR = 0.5;
        public const int MAX_TRANSFER_ATTEMPTS = 4;
        public const double RECHARGE_RESERVE_RATE = 30.0 / 60.0;
        public const double RECHARGE_OVERFLOW_AVOID_FACTOR = 1.0;
        public const String MAIN_POWER_NAME = "ElectricCharge";
        public const String RESERVE_POWER_NAME = "ReservePower";
        public const String SUBSYS_STATE_LABEL = "Subsys";
        public const String GUI_SECTION_LABEL = "Section";

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
            vesselProdPartsList = new Dictionary<uint, PwrPartList>();
            vesselConsPartsList = new Dictionary<uint, PwrPartList>();

            if (ToolbarManager.ToolbarAvailable && AYsettings.UseAppLauncher == false)
            {
                button1 = ToolbarManager.Instance.add("AmpYear", "button1");
                button1.TexturePath = "AmpYear/Icons/toolbarIcon";
                button1.ToolTip = "AmpYear";
                button1.Visibility = new GameScenesVisibility(GameScenes.FLIGHT, GameScenes.EDITOR);
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

            this.Log_Debug("AYController Awake complete");
        }

        private void OnGUIAppLauncherReady()
        {
            this.Log_Debug("OnGUIAppLauncherReady");
            if (ApplicationLauncher.Ready)
            {
                this.Log_Debug("Adding AppLauncherButton");
                this.stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(onAppLaunchToggleOn, onAppLaunchToggleOff, DummyVoid,
                                          DummyVoid, DummyVoid, DummyVoid, ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH |
                                          ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                                          (Texture)GameDatabase.Instance.GetTexture("AmpYear/Icons/AYIconOff", false));
            }
        }

        private void DummyVoid()
        {
        }

        public void onAppLaunchToggleOn()
        {
            this.stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture("AmpYear/Icons/AYIconOn", false));
            GuiVisible = true;
        }

        public void onAppLaunchToggleOff()
        {
            this.stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture("AmpYear/Icons/AYIconOff", false));
            GuiVisible = false;
        }

        public void Start()
        {
            this.Log_Debug("AYController Start");
            // add callbacks for vessel load and change
            GameEvents.onVesselChange.Add(VesselChange);
            GameEvents.onVesselLoaded.Add(VesselLoad);

            //check if inflight and active vessel set currentvesselid and load config settings for this vessel
            if (FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)
            {
                currentvesselid = FlightGlobals.ActiveVessel.id;
                VesselLoad(FlightGlobals.ActiveVessel);
            }

            // Find out which mods are present
            ALPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "AviationLights");
            NFEPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "NearFutureElectrical");
            NFSPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "NearFutureSolar");
            KASPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "KAS");
            RT2Present = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "RemoteTech");
            ScSPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "SCANsat");
            TelPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "Telemachus");
            TACLPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "TacLifeSupport");
            AntRPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "AntennaRange");
            KISPPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "WarpPlugin");
            BioPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "Biomatic");
            RegoPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "Regolith");
            BTSMPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "BTSM");
            RTKolPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "RTKolonists");

            this.Log_Debug("Checked for mods");
            if (ALPresent)
                this.Log_Debug("Aviation Lights present");
            if (NFEPresent)
                this.Log_Debug("Near Future Electric present");
            if (KASPresent)
                this.Log_Debug("KAS present");
            if (RT2Present)
                this.Log_Debug("RT2 present");
            if (ScSPresent)
                this.Log_Debug("SCANSat present");
            if (TelPresent)
                this.Log_Debug("Telemachus present");
            if (TACLPresent)
                this.Log_Debug("TAC LS present");
            if (AntRPresent)
                this.Log_Debug("AntennaRange present");
            if (KISPPresent)
                this.Log_Debug("Interstellar present");
            if (BioPresent)
                this.Log_Debug("Biomatic present");
            if (RegoPresent)
                this.Log_Debug("Regolith present");
            if (BTSMPresent)
                this.Log_Debug("btsm present");
            if (RTKolPresent)
                this.Log_Debug("RTKolonists present");
            this.Log_Debug("AYController Start complete");
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
            GameEvents.onVesselChange.Remove(VesselChange);
            GameEvents.onVesselLoaded.Remove(VesselLoad);
        }

        //GUI Section

        public static string guiSectionName(GUISection section)
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

        public bool guiSectionEnabled(GUISection section)
        {
            return guiSectionEnableFlag[(int)section];
        }

        public static bool subsystemIsLuxury(Subsystem subsystem)
        {
            switch (subsystem)
            {
                case Subsystem.HEATER:
                case Subsystem.COOLER:
                case Subsystem.MUSIC:
                case Subsystem.MASSAGE:
                    return true;

                default:
                    return false;
            }
        }

        public bool subsystemVisible(Subsystem subsystem)
        {
            Vessel cv = FlightGlobals.ActiveVessel;
            switch (subsystem)
            {
                case Subsystem.POWER_TURN:
                    return sasAdditionalRotPower > 0.0f;

                case Subsystem.RCS:
                    return hasRCS;

                case Subsystem.HEATER:
                case Subsystem.COOLER:
                case Subsystem.MUSIC:
                    return crewablePartList.Count > 0;

                case Subsystem.MASSAGE:
                    return cv.GetCrewCount() > 0;

                default:
                    return true;
            }
        }
       
        private void SetModeFlag()
        {
            //Set the mode flag, 0 = inflight, 1 = editor, 2 on EVA or F2
            if (FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)  // Check if in flight
            {
                if (FlightGlobals.ActiveVessel.isEVA) // EVA kerbal, do nothing
                    mode = GameState.EVA;
                else
                    mode = GameState.FLIGHT;
            }
            else if (EditorLogic.fetch != null) // Check if in editor
                mode = GameState.EDITOR;
            else   // Not in flight, in editor or F2 pressed unset the mode and return
                mode = GameState.EVA;
        }

        private void FixedUpdate()
        {
            if (Time.timeSinceLevelLoad < 2.0f) // Check not loading level
            {
                return;
            }

            //Set the mode flag, 0 = inflight, 1 = editor, 2 on EVA or F2
            SetModeFlag();
            if (mode == GameState.EVA) return;

            if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor) // Only execute Update in Flight or Editor Scene
            {
                sumDeltaTime += TimeWarp.deltaTime;
                if (sumDeltaTime > 2f) // Only update 1 times ever two second max.
                {
                    doOneUpdate = true;
                }
                // execute the next block if in flight, have active vessel and sumdeltatime has elapsed since last update, OR we are in the editor
                if ((FlightGlobals.ready && FlightGlobals.ActiveVessel != null && doOneUpdate) || (HighLogic.LoadedSceneIsEditor))
                {
                    this.Log_Debug("ampYearAYController  FixedUpdate mode == " + mode);
                    doOneUpdate = false;

                    //get current vessel parts list
                    List<Part> parts = new List<Part> { };
                    if (mode == GameState.FLIGHT)
                    {
                        parts = FlightGlobals.ActiveVessel.Parts;
                        CheckVslUpdate();
                    }
                    else
                        try
                        {
                            parts = EditorLogic.SortedShipList;
                            if (parts == null)
                            {
                                this.Log_Debug("In Editor but couldn't get parts list");
                                return;
                            }
                        }
                        catch (Exception Ex)
                        {
                            if (Ex.Message.Contains("Reference"))
                            {
                                this.Log("NullRef occurred getting parts list");
                            }
                            else
                            {
                                this.Log_Debug("Error occurred getting parts list " + Ex.Message);
                            }
                            return;
                        }

                    //Compile information about the vessel and its parts
                    // zero accumulators
                    sasAdditionalRotPower = 0.0f;                    
                    totalElectricCharge = 0.0;
                    totalElectricChargeCapacity = 0.0;
                    totalReservePower = 0.0;
                    totalReservePowerCapacity = 0.0;
                    totalHeatedParts = 0;
                    totalCooledParts = 0;
                    totalPowerDrain = 0;
                    totalPowerProduced = 0;
                    hasRCS = false;
                    currentRCSThrust = 0.0f;
                    currentPoweredRCSDrain = 0.0f;
                    crewablePartList.Clear();
                    commandPods.Clear();
                    maxCrew = 0;
                    PartResourceDefinition definition = PartResourceLibrary.Instance.GetDefinition(MAIN_POWER_NAME);                    
                    vesselProdPartsList.Clear();
                    vesselConsPartsList.Clear();
                    VslRstr.Clear(); //clear the vessel roster

                    //Begin calcs
                    if (mode == GameState.FLIGHT) // if in flight compile the vessel roster
                        VslRstr = FlightGlobals.ActiveVessel.GetVesselCrew();

                    //loop through all parts in the parts list of the vessel
                    foreach (Part current_part in parts)
                    {
                        this.Log_Debug("AYController part = " + current_part.name);
                        bool currentEngActive = false;
                        double alt_rate = 0;
                        string PrtName = current_part.name;
                        string PrtPower = " ";
                        bool PrtActive = false;
                        float tmpPower = 0f;

                        if (current_part is CommandPod)
                        {
                            CommandPod pod = (CommandPod)current_part;
                            String name = pod.partInfo.name;

                            float default_rot_power = pod.rotPower;

                            if (!podRotPowerMap.ContainsKey(name))
                            {
                                //Map the part's default rot power to its name
                                podRotPowerMap.Add(name, pod.rotPower);
                            }
                            else
                                podRotPowerMap.TryGetValue(name, out default_rot_power);

                            commandPods.Add(pod);                            

                            if (TACLPresent)
                                try
                                {
                                    checkTACL(null, current_part, true);
                                }
                                catch
                                {
                                    this.Log("Wrong TAC LS library version - disabled.");
                                    TACLPresent = false;
                                }
                        }

                        if (current_part is SASModule)
                        {
                            SASModule sas_module = (SASModule)current_part;                           
                            sasAdditionalRotPower += sas_module.maxTorque * SAS_POWER_TURN_TORQUE_FACTOR;
                        }

                        bool has_alternator = false;

                        //loop through all the modules in the current part
                        foreach (PartModule module in current_part.Modules)
                        {
                            this.Log_Debug("AYController module = " + module.name);
                            if (module is LaunchClamp)
                                continue; // skip clamps

                            if (module is ModuleAlternator)
                                has_alternator = true;

                            if (module is ModuleRCS)
                            {
                                hasRCS = true;

                                ModuleRCS rcs = (ModuleRCS)module;
                                foreach (float thrust in rcs.thrustForces)
                                    currentRCSThrust += thrust;

                                if (module is ModuleAmpYearPoweredRCS)
                                {                                    
                                    float ElecUse = ((ModuleAmpYearPoweredRCS)module).electricityUse;
                                    totalPowerDrain += ElecUse;           
                                    PrtActive = ElecUse > 0;
                                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, ElecUse, PrtActive);
                                    if (mode == GameState.FLIGHT)
                                        addPart(current_part.flightID, PartAdd, false);
                                    else
                                        addPart(current_part.craftID, PartAdd, false);

                                    currentPoweredRCSDrain += ElecUse;
                                    this.Log_Debug("AYIONRCS ElecUsage = " + ElecUse.ToString("0.00000000"));
                                    if (managerIsActive)
                                    {
                                        ((ModuleAmpYearPoweredRCS)module).isManaged = true; //Inform the thruster that it is being managed
                                        ((ModuleAmpYearPoweredRCS)module).Enable();
                                    }
                                    else
                                    {
                                        ((ModuleAmpYearPoweredRCS)module).isManaged = false; //Inform the thruster that it is not being managed
                                        ((ModuleAmpYearPoweredRCS)module).Disable();
                                    }
                                }

                                if (module is ModuleAmpYearPPTRCS)
                                {                                    
                                    float ElecUse2 = ((ModuleAmpYearPPTRCS)module).electricityUse;
                                    totalPowerDrain += ElecUse2;                                    
                                    PrtActive = ElecUse2 > 0;
                                    PwrPartList PartAdd2 = new PwrPartList(PrtName, PrtPower, ElecUse2, PrtActive);
                                    addPart(current_part.flightID, PartAdd2, false);

                                    currentPoweredRCSDrain += ElecUse2;
                                    this.Log_Debug("AYPPTRCS ElecUsage = " + 2.ToString("0.00000000"));
                                    if (managerIsActive)
                                    {
                                        ((ModuleAmpYearPPTRCS)module).isManaged = true; //Inform the thruster that it is being managed
                                        ((ModuleAmpYearPPTRCS)module).Enable();
                                    }
                                    else
                                    {
                                        ((ModuleAmpYearPPTRCS)module).isManaged = false; //Inform the thruster that it is not being managed
                                        ((ModuleAmpYearPPTRCS)module).Disable();
                                    }
                                }
                                continue;
                            }

                            if (module.moduleName == "ModuleDeployableSolarPanel")
                            {
                                ModuleDeployableSolarPanel tmpSol = (ModuleDeployableSolarPanel)module;                                
                                if (mode == GameState.FLIGHT)
                                {
                                    tmpPower = tmpSol.flowRate;
                                    totalPowerProduced += tmpPower;                                    
                                    if (!hasPower && EmgcyShutActive)
                                    {
                                        if (FlightGlobals.ActiveVessel.atmDensity < 0.2 || FlightGlobals.ActiveVessel.situation != Vessel.Situations.SUB_ORBITAL
                                        || FlightGlobals.ActiveVessel.situation != Vessel.Situations.FLYING)
                                        {
                                            if (tmpSol.panelState == ModuleDeployableSolarPanel.panelStates.RETRACTED)
                                            {
                                                tmpSol.Extend();
                                                ScreenMessages.PostScreenMessage("Electricity Levels Critical! Extending Solar Panels!", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                                                this.Log("Extending solar array");
                                            }
                                        }
                                        else
                                            ScreenMessages.PostScreenMessage("Electricity Levels Critical! In Atmosphere Cannot Extending Solar Panels!", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                                    }
                                    
                                }
                                else
                                {
                                    tmpPower = tmpSol.chargeRate;
                                    totalPowerProduced += (double)tmpPower;                                    
                                }

                                PrtActive = tmpPower > 0f;
                                PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                                if (mode == GameState.FLIGHT)
                                    addPart(current_part.flightID, PartAdd, true);
                                else
                                    addPart(current_part.craftID, PartAdd, true);

                                continue;
                            }

                            if (module.moduleName == "ModuleGenerator")
                            {
                                ModuleGenerator tmpGen = (ModuleGenerator)module;                                
                                foreach (ModuleGenerator.GeneratorResource outp in tmpGen.outputList)
                                {
                                    if (outp.name == MAIN_POWER_NAME)
                                    {
                                        if (mode == GameState.EDITOR)
                                        {
                                            tmpPower = outp.rate;
                                            totalPowerProduced += tmpPower;
                                            PrtActive = true;
                                        }
                                        else
                                        {
                                            if (tmpGen.isAlwaysActive || tmpGen.generatorIsActive)
                                            {
                                                tmpPower = outp.rate;
                                                totalPowerProduced += tmpPower;
                                                PrtActive = true;
                                            }
                                            else
                                            {
                                                tmpPower = outp.rate;
                                                PrtActive = false;
                                            }
                                        }                                       
                                        PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                                        if (mode == GameState.FLIGHT)
                                            addPart(current_part.flightID, PartAdd, true);
                                        else
                                            addPart(current_part.craftID, PartAdd, true);
                                    }
                                }
                                foreach (ModuleGenerator.GeneratorResource inp in tmpGen.inputList)
                                {
                                    if (inp.name == MAIN_POWER_NAME)
                                    {
                                        if (mode == GameState.EDITOR)
                                        {
                                            tmpPower = inp.rate;
                                            totalPowerProduced += tmpPower;
                                            PrtActive = true;
                                        }
                                        else
                                        {
                                            if (tmpGen.isAlwaysActive || tmpGen.generatorIsActive)
                                            {
                                                tmpPower = inp.rate;
                                                totalPowerProduced += inp.rate;
                                                PrtActive = true;
                                            }
                                            else
                                            {
                                                tmpPower = inp.rate;
                                                PrtActive = false;
                                            }
                                        }                                        
                                        PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                                        if (mode == GameState.FLIGHT)
                                            addPart(current_part.flightID, PartAdd, true);
                                        else
                                            addPart(current_part.craftID, PartAdd, true);
                                    }
                                }
                                continue;
                            }

                            if (module.moduleName == "ModuleWheel")
                            {
                                ModuleWheel tmpWheel = (ModuleWheel)module;
                                if (tmpWheel.resourceName == MAIN_POWER_NAME)
                                {
                                    tmpPower = tmpWheel.resourceConsumptionRate;
                                    if (mode == GameState.FLIGHT)
                                    {
                                        if (tmpWheel.throttleInput != 0)
                                        {
                                            totalPowerDrain += tmpWheel.resourceConsumptionRate;
                                            PrtActive = true;
                                            if (!hasPower && EmgcyShutActive)
                                            {
                                                tmpWheel.DisableMotor();
                                                ScreenMessages.PostScreenMessage("Electricity Levels Critical! Disabling Wheel Motors!", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                                                this.Log("Disabling Wheel motors");
                                            }
                                        }
                                        else
                                        {
                                            PrtActive = false;
                                        }
                                    }

                                    if (mode == GameState.EDITOR)
                                    {
                                        totalPowerDrain += tmpWheel.resourceConsumptionRate;
                                        PrtActive = true;
                                    }                                    
                                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                                    if (mode == GameState.FLIGHT)
                                        addPart(current_part.flightID, PartAdd, false);
                                    else
                                        addPart(current_part.craftID, PartAdd, false);
                                }
                                continue;
                            }

                            if (module.moduleName == "ModuleCommand")
                            {
                                ModuleCommand tmpPod = (ModuleCommand)module;
                                foreach (ModuleResource r in tmpPod.inputResources)
                                {
                                    if (r.id == definition.id)
                                    {
                                        totalPowerDrain += r.rate;                                        
                                        PrtActive = r.rate > 0;
                                        tmpPower = (float)r.rate;
                                        PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                                        if (mode == GameState.FLIGHT)
                                            addPart(current_part.flightID, PartAdd, false);
                                        else
                                            addPart(current_part.craftID, PartAdd, false);
                                    }
                                }
                                continue;
                            }

                            if (module.moduleName == "ModuleLight")
                            {
                                ModuleLight tmpLight = (ModuleLight)module;                                
                                if (mode == GameState.EDITOR || (mode == GameState.FLIGHT && tmpLight.isOn))
                                {
                                    PrtActive = true;
                                    totalPowerDrain += tmpLight.resourceAmount;
                                    if (!hasPower && EmgcyShutActive && (mode == GameState.FLIGHT && tmpLight.isOn))
                                    {
                                        tmpLight.LightsOff();
                                        ScreenMessages.PostScreenMessage("Electricity Levels Critical! Turning off Lights!", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                                        this.Log("Turning off lights");
                                    }
                                }                              
                                PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpLight.resourceAmount, PrtActive);
                                if (mode == GameState.FLIGHT)
                                    addPart(current_part.flightID, PartAdd, false);
                                else
                                    addPart(current_part.craftID, PartAdd, false);

                                continue;
                            }

                            if (module.moduleName == "ModuleDataTransmitter")
                            {
                                ModuleDataTransmitter tmpAnt = (ModuleDataTransmitter)module;                                
                                if (mode == GameState.EDITOR || (mode == GameState.FLIGHT && tmpAnt.IsBusy()))
                                {
                                    totalPowerDrain += tmpAnt.DataResourceCost * (1 / tmpAnt.packetInterval);
                                    tmpPower = (float)tmpAnt.DataResourceCost * (1 / tmpAnt.packetInterval);
                                    PrtActive = true;
                                }                              
                                PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                                if (mode == GameState.FLIGHT)
                                    addPart(current_part.flightID, PartAdd, false);
                                else
                                    addPart(current_part.craftID, PartAdd, false);

                                continue;
                            }

                            if (module.moduleName == "ModuleReactionWheel")
                            {
                                ModuleReactionWheel tmpRW = (ModuleReactionWheel)module;
                                sasAdditionalRotPower += tmpRW.RollTorque * SAS_POWER_TURN_TORQUE_FACTOR;                                
                                if (mode == GameState.EDITOR || tmpRW.enabled)
                                    PrtActive = true;

                                foreach (ModuleResource r in tmpRW.inputResources)
                                {
                                    if (r.id == definition.id)
                                    {
                                        if (mode == GameState.EDITOR)
                                        {
                                            totalPowerDrain += r.rate * tmpRW.PitchTorque;  // rough guess for VAB
                                            tmpPower += (float)(r.rate * tmpRW.PitchTorque);
                                        }
                                        else
                                        {
                                            totalPowerDrain += r.currentAmount;
                                            tmpPower += (float)r.currentAmount;                             
                                           
                                        }
                                        
                                    }
                                }                                
                                PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                                if (mode == GameState.FLIGHT)
                                    addPart(current_part.flightID, PartAdd, false);
                                else
                                    addPart(current_part.craftID, PartAdd, false);

                                continue;
                            }

                            if (module.moduleName == "ModuleEngines")
                            {
                                ModuleEngines tmpEng = (ModuleEngines)module;
                                const float grav = 9.81f;
                                bool usesCharge = false;
                                float sumRD = 0;
                                Single ecratio = 0;                                
                                foreach (Propellant prop in tmpEng.propellants)
                                {
                                    if (prop.name == MAIN_POWER_NAME)
                                    {
                                        usesCharge = true;

                                        ecratio = prop.ratio;
                                    }
                                    sumRD += prop.ratio * PartResourceLibrary.Instance.GetDefinition(prop.id).density;
                                }
                                if (usesCharge)
                                {
                                    float massFlowRate = 0;
                                    if (mode == GameState.FLIGHT && tmpEng.isOperational && tmpEng.currentThrottle > 0)
                                    {
                                        PrtActive = true;
                                        massFlowRate = (tmpEng.currentThrottle * tmpEng.maxThrust) / (tmpEng.atmosphereCurve.Evaluate(0) * grav);
                                        totalPowerDrain += (ecratio * massFlowRate) / sumRD;
                                    }
                                    if (mode == GameState.EDITOR)
                                    {
                                        PrtActive = true;
                                        massFlowRate = (1.0f * tmpEng.maxThrust) / (tmpEng.atmosphereCurve.Evaluate(0) * grav);
                                        totalPowerDrain += (ecratio * massFlowRate) / sumRD;
                                    }
                                    tmpPower = ((ecratio * massFlowRate) / sumRD);
                                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                                    if (mode == GameState.FLIGHT)
                                        addPart(current_part.flightID, PartAdd, false);
                                    else
                                        addPart(current_part.craftID, PartAdd, false);
                                }

                                currentEngActive = tmpEng.isOperational && (tmpEng.currentThrottle > 0);
                                if (alt_rate > 0 && currentEngActive)
                                {
                                    totalPowerProduced += alt_rate;
                                    PrtActive = true;
                                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, (float)alt_rate, PrtActive);
                                    if (mode == GameState.FLIGHT)
                                        addPart(current_part.flightID, PartAdd, true);
                                    else
                                        addPart(current_part.craftID, PartAdd, true);
                                }

                                continue;
                            }

                            if (module.moduleName == "ModuleEnginesFX")
                            {
                                ModuleEnginesFX tmpEngFX = (ModuleEnginesFX)module;
                                const float grav = 9.81f;
                                bool usesCharge = false;
                                float sumRD = 0;
                                Single ecratio = 0;
                                foreach (Propellant prop in tmpEngFX.propellants)
                                {
                                    if (prop.name == MAIN_POWER_NAME)
                                    {
                                        usesCharge = true;
                                        ecratio = prop.ratio;
                                    }
                                    sumRD += prop.ratio * PartResourceLibrary.Instance.GetDefinition(prop.id).density;
                                }
                                if (usesCharge)
                                {
                                    float massFlowRate = 0;
                                    
                                    if ((mode == GameState.FLIGHT && tmpEngFX.isOperational && tmpEngFX.currentThrottle > 0) || mode == GameState.EDITOR)
                                    {
                                        if (mode == GameState.EDITOR)

                                            massFlowRate = (tmpEngFX.currentThrottle * tmpEngFX.maxThrust) / (tmpEngFX.atmosphereCurve.Evaluate(0) * grav);
                                        else
                                            massFlowRate = (tmpEngFX.currentThrottle * tmpEngFX.maxThrust) / (tmpEngFX.atmosphereCurve.Evaluate(0) * grav);
                                        totalPowerDrain += (ecratio * massFlowRate) / sumRD;                                        
                                        PrtActive = true;
                                    }

                                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, ((ecratio * massFlowRate) / sumRD), PrtActive);
                                    if (mode == GameState.FLIGHT)
                                        addPart(current_part.flightID, PartAdd, false);
                                    else
                                        addPart(current_part.craftID, PartAdd, false);
                                }
                                currentEngActive = tmpEngFX.isOperational && (tmpEngFX.currentThrottle > 0);
                                if (alt_rate > 0 && currentEngActive)
                                {
                                    totalPowerProduced += alt_rate;
                                    PrtActive = true;
                                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, (float)alt_rate, PrtActive);
                                    if (mode == GameState.FLIGHT)
                                        addPart(current_part.flightID, PartAdd, true);
                                    else
                                        addPart(current_part.craftID, PartAdd, true);
                                }

                                continue;
                            }

                            if (module.moduleName == "ModuleAlternator")
                            {
                                ModuleAlternator tmpAlt = (ModuleAlternator)module;
                                foreach (ModuleResource r in tmpAlt.outputResources)
                                {
                                    if (r.name == MAIN_POWER_NAME)
                                    {                                  
                                        if (mode == GameState.EDITOR || (mode == GameState.FLIGHT && currentEngActive))
                                        {
                                            totalPowerProduced += r.rate;
                                            PrtActive = true;
                                        }
                                        else
                                            alt_rate = r.rate;
                                        PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, (float)r.rate, PrtActive);
                                        if (mode == GameState.FLIGHT)
                                            addPart(current_part.flightID, PartAdd, true);
                                        else
                                            addPart(current_part.craftID, PartAdd, true);
                                    }
                                }
                                continue;
                            }

                            if (module.moduleName == "ModuleScienceLab")
                            {                                
                                double tmpPwr = 0;
                                if (mode == GameState.FLIGHT)
                                {
                                    ModuleScienceLab tmpLab = (ModuleScienceLab)module;

                                    foreach (ModuleResource r in tmpLab.processResources)
                                    {
                                        if (r.name == MAIN_POWER_NAME && tmpLab.IsOperational())
                                        {
                                            PrtActive = true;
                                            totalPowerDrain += r.rate;
                                            tmpPwr += r.rate;
                                        }
                                    }
                                }
                                if (mode == GameState.EDITOR)
                                {
                                    ModuleScienceLab tmpLab = (ModuleScienceLab)module;
                                    PrtActive = true;
                                    foreach (ModuleResource r in tmpLab.processResources)
                                    {
                                        if (r.name == MAIN_POWER_NAME)
                                        {
                                            totalPowerDrain += r.rate;
                                            tmpPwr += r.rate;
                                        }
                                    }
                                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, (float)tmpPwr, PrtActive);
                                    if (mode == GameState.FLIGHT)
                                        addPart(current_part.flightID, PartAdd, false);
                                    else
                                        addPart(current_part.craftID, PartAdd, false);
                                }
                            }

                            if (KASPresent)
                                try
                                {
                                    checkKAS(module, current_part);
                                }
                                catch
                                {
                                    this.Log("Wrong KAS library version - disabled.");
                                    KASPresent = false;
                                }

                            if (RT2Present)
                                try
                                {
                                    checkRT2(module, current_part);
                                }
                                catch
                                {
                                    this.Log("Wrong Remote Tech 2 library version - disabled.");
                                    RT2Present = false;
                                }

                            if (ALPresent)
                                try
                                {
                                    checkAv(module, current_part);
                                }
                                catch
                                {
                                    this.Log_Debug("Wrong Aviation Lights library version - disabled.");
                                    ALPresent = false;
                                }

                            if (NFEPresent)
                                try
                                {
                                    checkNFE(module, current_part);
                                }
                                catch
                                {
                                    this.Log("Wrong Near Future library version - disabled.");
                                    NFEPresent = false;
                                }
                            if (NFSPresent)
                                try
                                {
                                    checkNFS(module, current_part);
                                }
                                catch
                                {
                                    this.Log_Debug("Wrong Near Future solar library version - disabled.");
                                    NFSPresent = false;
                                }

                            if (ScSPresent)
                                try
                                {
                                    checkSCANsat(module, current_part);
                                }
                                catch
                                {
                                    this.Log("Wrong SCANsat library version - disabled.");
                                    ScSPresent = false;
                                }

                            if (TelPresent)
                                try
                                {
                                    checkTel(module, current_part);
                                }
                                catch
                                {
                                    this.Log("Wrong Telemachus library version - disabled.");
                                    TelPresent = false;
                                }

                            if (AntRPresent)
                                try
                                {
                                    checkAntR(module, current_part);
                                }
                                catch
                                {
                                    this.Log("Wrong AntennaRange library version - disabled.");
                                    AntRPresent = false;
                                }

                            if (BioPresent)
                                try
                                {
                                    //checkBio(module);
                                }
                                catch
                                {
                                    this.Log("Wrong Biomatic library version - disabled.");
                                    BioPresent = false;
                                }

                            if (RegoPresent)
                                try
                                {
                                    checkRego(module, current_part);
                                }
                                catch
                                {
                                    this.Log("Wrong Karbonite library version - disabled.");
                                    RegoPresent = false;
                                }

                            if (BTSMPresent)
                                try
                                {
                                    //checkBTSM(module, current_part);
                                }
                                catch
                                {
                                    this.Log("Wrong BTSM library version - disabled.");
                                    BTSMPresent = false;
                                }

                            if (RTKolPresent)
                                try
                                {
                                    //checkRTKol(module, current_part);
                                }
                                catch
                                {
                                    this.Log("Wrong RTKolonists library version - disabled.");
                                    RTKolPresent = false;
                                }
                            if (TACLPresent)
                                try
                                {
                                    checkTACL(module, current_part, false);
                                }
                                catch
                                {
                                    this.Log("Wrong BTSM library version - disabled.");
                                    BTSMPresent = false;
                                }

                            if (module is AYCrewPart)
                            {
                                if (((AYCrewPart)module).CabinTemp >= AYsettings.HEATER_TARGET_TEMP)
                                    totalHeatedParts++;
                                if (((AYCrewPart)module).CabinTemp <= AYsettings.COOLER_TARGET_TEMP)
                                    totalCooledParts++;
                                if (mode == GameState.FLIGHT)
                                    CalcPartCraziness(FlightGlobals.ActiveVessel, current_part, module, sumDeltaTime);
                            }
                        } // end modules loop

                        //Sum up the power resources
                        if (!has_alternator) //Ignore parts with alternators in power-capacity calculate because they don't actually store power
                        {
                            foreach (PartResource resource in current_part.Resources)
                            {
                                if (resource.resourceName == MAIN_POWER_NAME)
                                {
                                    totalElectricCharge += resource.amount;
                                    totalElectricChargeCapacity += resource.maxAmount;
                                }
                                else if (resource.resourceName == RESERVE_POWER_NAME)
                                {
                                    totalReservePower += resource.amount;
                                    totalReservePowerCapacity += resource.maxAmount;
                                }
                            }
                        }

                        if (current_part.CrewCapacity > 0)
                        {
                            crewablePartList.Add(current_part);
                            maxCrew += current_part.CrewCapacity;
                        }
                    } // end part loop

                    subsystemUpdate();
                } // end if active vessel not null
            } // End if Highlogic check
        } // End Function

        private void subsystemUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                Vessel cv = FlightGlobals.ActiveVessel;

                if (cv.ActionGroups[KSPActionGroup.RCS] && !subsystemPowered(Subsystem.RCS))
                {
                    this.Log("RCS - disabled.");
                    //Disable RCS when the subsystem isn't powered
                    cv.ActionGroups.SetGroup(KSPActionGroup.RCS, false);
                    reenableRCS = true;
                }

                if (cv.ActionGroups[KSPActionGroup.SAS] && !subsystemPowered(Subsystem.SAS))
                {
                    this.Log("SAS - disabled.");
                    ScreenMessages.PostScreenMessage(cv.vesselName + " - SAS must be enabled through AmpYear first", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                    //Disable SAS when the subsystem isn't powered
                    cv.ActionGroups.SetGroup(KSPActionGroup.SAS, false);
                    reenableSAS = true;
                }

                if (managerIsActive && hasPower)
                {
                    //Re-enable SAS/RCS if they were shut off by the manager and can be run again
                    if (reenableSAS)
                    {
                        setSubsystemEnabled(Subsystem.SAS, true);
                        reenableSAS = false;
                    }

                    if (reenableRCS)
                    {
                        setSubsystemEnabled(Subsystem.RCS, true);
                        reenableRCS = false;
                    }
                }

                //Update command pod rot powers
                bool power_turn_on = subsystemPowered(Subsystem.POWER_TURN);
                bool first = true;

                foreach (CommandPod pod in commandPods)
                {
                    float default_rot_power = pod.rotPower;
                    podRotPowerMap.TryGetValue(pod.partInfo.name, out default_rot_power);

                    if (power_turn_on && first)
                    {
                        //Apply power turn rotPower
                        pod.rotPower = default_rot_power + sasAdditionalRotPower;
                        first = false;
                    }
                    else
                        pod.rotPower = default_rot_power; //Use default rot power
                }

                if (subsystemPowered(Subsystem.HEATER))
                    changeCrewedPartsTemperature(AYsettings.HEATER_TARGET_TEMP, true);

                if (subsystemPowered(Subsystem.COOLER))
                    changeCrewedPartsTemperature(AYsettings.COOLER_TARGET_TEMP, false);

                //Calculate total drain from subsystems
                double subsystem_drain = 0.0;
                foreach (Subsystem subsystem in Enum.GetValues(typeof(Subsystem)))
                {
                    subsystemDrain[(int)subsystem] = subsystemCurrentDrain(subsystem);
                    subsystem_drain += subsystemDrain[(int)subsystem];
                }

                double manager_drain = managerCurrentDrain;
                subsystemDrain[(int)Subsystem.RCS] -= currentPoweredRCSDrain;
                double total_manager_drain = subsystem_drain + manager_drain;
                totalPowerDrain += total_manager_drain;

                //Recharge reserve power if main power is above a certain threshold
                if (managerIsActive && totalElectricCharge > 0 && totalElectricCharge / totalElectricChargeCapacity > AYsettings.RECHARGE_RESERVE_THRESHOLD
                    && totalReservePower < totalReservePowerCapacity)
                    transferMainToReserve(RECHARGE_RESERVE_RATE * sumDeltaTime);

                //Drain main power
                Part cvp = FlightGlobals.ActiveVessel.rootPart;
                double timestep_drain = total_manager_drain * sumDeltaTime;
                double minimum_sufficient_charge = managerActiveDrain + 4;
                if (totalElectricCharge >= minimum_sufficient_charge)
                {
                    hasPower = (UnityEngine.Time.realtimeSinceStartup > powerUpTime)
                     && (timestep_drain <= 0.0 || requestResource(cvp, MAIN_POWER_NAME, timestep_drain) >= (timestep_drain * 0.99));
                }
                else
                {
                    hasPower = false;
                    if (EmgcyShutActive)
                    {
                        //***** perform emergency shutdown procedure here
                    }
                }
                   

                if (!hasPower && UnityEngine.Time.realtimeSinceStartup > powerUpTime)
                {
                    //Set a delay for powering back up to avoid rapid flickering of the system
                    powerUpTime = UnityEngine.Time.realtimeSinceStartup + POWER_UP_DELAY;
                }

                if (!hasPower && totalReservePower > minimum_sufficient_charge)
                {
                    //If main power is insufficient, drain reserve power for manager
                    double manager_timestep_drain = manager_drain * sumDeltaTime;

                    hasReservePower = manager_drain <= 0.0
                        || requestResource(cvp, RESERVE_POWER_NAME, manager_timestep_drain) >= (manager_timestep_drain * 0.99);
                }
                else
                    hasReservePower = totalReservePower > minimum_sufficient_charge;
            }
            if (HighLogic.LoadedSceneIsEditor)
            {
                //Calculate total drain from subsystems
                double subsystem_drain = 0.0;
                foreach (Subsystem subsystem in Enum.GetValues(typeof(Subsystem)))
                {
                    subsystemDrain[(int)subsystem] = subsystemActiveDrain(subsystem);
                    subsystem_drain += subsystemDrain[(int)subsystem];
                }
                double manager_drain = managerCurrentDrain;
                double total_manager_drain = subsystem_drain + manager_drain;
                totalPowerDrain += total_manager_drain;
                string PrtName = "AmpYear & Subsystems Max";
                string PrtPower = "";                
                PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, (float)total_manager_drain, true);
                addPart(1111, PartAdd, false);
                hasPower = true;
                hasReservePower = true;
            }
            sumDeltaTime = 0;
        }

        private void checkAv(PartModule psdpart, Part current_part)
        {
            //string PrtName = current_part.name;
            //string PrtPower = "";
            //bool PrtActive = false;
            //float tmpPower = 0;
            switch (psdpart.moduleName)
            {
                case "ModuleNavLight":
                    /*        //global::AviationLights.ModuleNavLight tmpLight = (global::AviationLights.ModuleNavLight)psdpart;
                        if ((tmpLight.navLightSwitch != 0) || mode == GameState.EDITOR)
                        {
                            PrtActive = true;
                        }
                            totalPowerDrain += tmpLight.EnergyReq;
                            tmpPower = tmpLight.EnergyReq;                           

                        PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                        if (mode == GameState.FLIGHT)
                            addPart(current_part.flightID, PartAdd, false);
                        else
                            addPart(current_part.craftID, PartAdd, false);

         */
                    break;
            }
        }

        private void checkNFE(PartModule psdpart, Part current_part)
        {
            string PrtName = current_part.name;
            string PrtPower = "";
            bool PrtActive = false;
            float tmpPower = 0;
            switch (psdpart.moduleName)
            {
                case "FissionGenerator":

                    global::NearFutureElectrical.FissionGenerator tmpGen = (global::NearFutureElectrical.FissionGenerator)psdpart;
                    if (mode == GameState.FLIGHT && tmpGen.Enabled)
                    {
                        PrtActive = true;
                        totalPowerProduced += tmpGen.GetCurrentPower();
                        tmpPower = (float)tmpGen.GetCurrentPower();
                    }
                    else if (mode == GameState.EDITOR)
                    {
                        PrtActive = true;
                        totalPowerProduced += tmpGen.PowerGenerationMaximum;
                        tmpPower = (float)tmpGen.PowerGenerationMaximum;
                    }
                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);

                    if (mode == GameState.FLIGHT)
                        addPart(current_part.flightID, PartAdd, true);
                    else
                        addPart(current_part.craftID, PartAdd, true);

                    break;
            }
        }

        private void checkNFS(PartModule psdpart, Part current_part)
        {
            string PrtName = current_part.name;
            string PrtPower = "";
            bool PrtActive = false;
            float tmpPower = 0;
            switch (psdpart.moduleName)
            {
                case "ModuleCurvedSolarPanel":

                    global::NearFutureSolar.ModuleCurvedSolarPanel tmpGen = (global::NearFutureSolar.ModuleCurvedSolarPanel)psdpart;
                    if (mode == GameState.FLIGHT && tmpGen.State == ModuleDeployableSolarPanel.panelStates.EXTENDED)
                    {
                        PrtActive = true;
                        totalPowerProduced += tmpGen.TotalEnergyRate;
                        tmpPower = tmpGen.TotalEnergyRate;
                    }
                    else if (mode == GameState.EDITOR)
                    {
                        PrtActive = true;
                        totalPowerProduced += tmpGen.TotalEnergyRate;
                        tmpPower = tmpGen.TotalEnergyRate;
                    }
                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);

                    if (mode == GameState.FLIGHT)
                        addPart(current_part.flightID, PartAdd, true);
                    else
                        addPart(current_part.craftID, PartAdd, true);

                    break;
            }
        }

        private void checkKAS(PartModule psdpart, Part current_part)
        {
            string PrtName = current_part.name;
            string PrtPower = "";
            bool PrtActive = false;
            float tmpPower = 0;
            switch (psdpart.moduleName)
            {
                case "KASModuleWinch":

                    global::KAS.KASModuleWinch tmpKW = (global::KAS.KASModuleWinch)psdpart;

                    if (mode == GameState.FLIGHT && tmpKW.isActive)
                    {
                        totalPowerDrain += tmpKW.powerDrain * tmpKW.motorSpeed;
                        tmpPower = tmpKW.powerDrain * tmpKW.motorSpeed;
                        PrtActive = true;
                    }
                    if (mode == GameState.EDITOR)
                    {
                        totalPowerDrain += tmpKW.powerDrain;
                        PrtActive = true;
                        tmpPower = tmpKW.powerDrain;
                    }
                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                    if (mode == GameState.FLIGHT)
                        addPart(current_part.flightID, PartAdd, false);
                    else
                        addPart(current_part.craftID, PartAdd, false);

                    break;

                case "KASModuleMagnet":

                    global::KAS.KASModuleMagnet tmpKM = (global::KAS.KASModuleMagnet)psdpart;
                    if (mode == GameState.EDITOR || (mode == GameState.FLIGHT && tmpKM.MagnetActive))
                    {
                        totalPowerDrain += tmpKM.powerDrain;
                        PrtActive = true;
                        tmpPower = tmpKM.powerDrain;
                    }
                    PwrPartList PartAdd2 = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                    addPart(current_part.flightID, PartAdd2, false);
                    break;
            }
        }

        private void checkRT2(PartModule psdpart, Part current_part)
        {
            switch (psdpart.moduleName)
            {
                case "ModuleRTAntenna":
                    string PrtName = current_part.name;
                    string PrtPower = "";
                    bool PrtActive = false;
                    float tmpPower = 0;
                    global::RemoteTech.Modules.ModuleRTAntenna tmpAnt = (global::RemoteTech.Modules.ModuleRTAntenna)psdpart;
                    if (mode == GameState.FLIGHT && tmpAnt.Activated)
                    {
                        totalPowerDrain += tmpAnt.Consumption;
                        PrtActive = true;
                        tmpPower = tmpAnt.Consumption;
                    }
                    if (mode == GameState.EDITOR)
                    {
                        totalPowerDrain += tmpAnt.EnergyCost;
                        PrtActive = true;
                        tmpPower = tmpAnt.EnergyCost;
                    }
                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                    if (mode == GameState.FLIGHT)
                        addPart(current_part.flightID, PartAdd, false);
                    else
                        addPart(current_part.craftID, PartAdd, false);

                    break;
            }
        }

        private void checkSCANsat(PartModule psdpart, Part current_part)
        {
            switch (psdpart.moduleName)
            {
                case "SCANsat":
                    string PrtName = current_part.name;
                    string PrtPower = "";
                    bool PrtActive = false;
                    float tmpPower = 0;
                    global::SCANsat.SCANsat tmpSS = (global::SCANsat.SCANsat)psdpart;
                    if ((mode == GameState.EDITOR) || (mode == GameState.FLIGHT && (tmpSS.power > 0.0 && tmpSS.scanningNow())))
                    {
                        totalPowerDrain += tmpSS.power;
                        PrtActive = true;
                        tmpPower = tmpSS.power;
                    }
                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                    if (mode == GameState.FLIGHT)
                        addPart(current_part.flightID, PartAdd, false);
                    else
                        addPart(current_part.craftID, PartAdd, false);

                    break;
            }
        }

        private void checkTel(PartModule psdpart, Part current_part)
        {
            switch (psdpart.moduleName)
            {
                case "TelemachusPowerDrain":
                    string PrtName = current_part.name;
                    string PrtPower = "";
                    bool PrtActive = false;
                    float tmpPower = 0;
                    if (mode == GameState.FLIGHT && global::Telemachus.TelemachusPowerDrain.isActive)
                    {
                        totalPowerDrain += global::Telemachus.TelemachusPowerDrain.powerConsumption;
                        PrtPower = global::Telemachus.TelemachusPowerDrain.powerConsumption.ToString("000.00000");
                        PrtActive = true;
                        tmpPower = global::Telemachus.TelemachusPowerDrain.powerConsumption;
                    }
                    if (mode == GameState.EDITOR)
                    {
                        totalPowerDrain += 0.01;
                        PrtPower = ("0.010");
                        PrtActive = true;
                        tmpPower = 0.01f;
                    }
                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                    if (mode == GameState.FLIGHT)
                        addPart(current_part.flightID, PartAdd, false);
                    else
                        addPart(current_part.craftID, PartAdd, false);

                    break;
            }
        }

        private void checkTACL(PartModule psdpart, Part current_part, bool CmdPod)
        {
            global::Tac.TacLifeSupport tmpTLS = Tac.TacLifeSupport.Instance;
            if (tmpTLS.gameSettings.Enabled)
            {
                if (CmdPod)
                {
                    string PrtName = "TACL Life Support";
                    string PrtPower = "";
                    bool PrtActive = false;
                    if (mode == GameState.FLIGHT) //if in flight set maxCrew to actual crew on board. Set earlier based on maximum crew capacity of each part
                    {
                        maxCrew = FlightGlobals.ActiveVessel.GetCrewCount();
                    }

                    double CalcDrain = 0;
                    CalcDrain = tmpTLS.globalSettings.BaseElectricityConsumptionRate * crewablePartList.Count;
                    CalcDrain += tmpTLS.globalSettings.ElectricityConsumptionRate * maxCrew;
                    totalPowerDrain += CalcDrain;
                    PrtActive = maxCrew > 0;
                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, (float)CalcDrain, PrtActive);
                    if (mode == GameState.FLIGHT)
                        addPart(current_part.flightID, PartAdd, false);
                    else
                        addPart(current_part.craftID, PartAdd, false);
                }
                else 
                {
                    string PrtName = current_part.name;
                    string PrtPower = "";
                    bool PrtActive = false;
                    float tmpPower = 0;
                    switch (psdpart.moduleName)
                    {
                        case "TACGenericConverter":

                            global::Tac.TacGenericConverter tacGC = (global::Tac.TacGenericConverter)psdpart;
                            PrtActive = tacGC.converterEnabled;
                            string[] arr = tacGC.inputResources.Split(',');
                            for (int i = 0; i < arr.Length; i += 2)
                            {
                                string ResName = arr[i];
                                if (ResName == MAIN_POWER_NAME)
                                {
                                    double ResAmt = 0;
                                    bool prse = double.TryParse(arr[i + 1], out ResAmt);
                                    if (!prse) ResAmt = 0;
                                    tmpPower = (float)ResAmt;
                                    break;
                                }
                            }                    
                            PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                            if (mode == GameState.FLIGHT)
                                addPart(current_part.flightID, PartAdd, false);
                            else
                                addPart(current_part.craftID, PartAdd, false);


                            arr = tacGC.outputResources.Split(',');
                            for (int i = 0; i < arr.Length; i += 2)
                            {
                                string ResName = arr[i];
                                if (ResName == MAIN_POWER_NAME)
                                {
                                    double ResAmt = 0;
                                    bool prse = double.TryParse(arr[i + 1], out ResAmt);
                                    if (!prse) ResAmt = 0;
                                    tmpPower = (float)ResAmt;
                                    break;
                                }
                            }                    
                            PwrPartList PartAdd2 = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                            if (mode == GameState.FLIGHT)
                                addPart(current_part.flightID, PartAdd2, true);
                            else
                                addPart(current_part.craftID, PartAdd2, true);


                            break;
                    }
                }
                
            }
        }

        private void checkAntR(PartModule psdpart, Part current_part)
        {
            switch (psdpart.moduleName)
            {
                case "ModuleLimitedDataTransmitter":
                    string PrtName = current_part.name;
                    string PrtPower = "";
                    bool PrtActive = false;
                    float tmpPower = 0;
                    ModuleDataTransmitter tmpAnt = (ModuleDataTransmitter)psdpart;
                    if (mode == GameState.EDITOR || (mode == GameState.FLIGHT && tmpAnt.IsBusy()))
                    {
                        totalPowerDrain += tmpAnt.DataResourceCost * (1 / tmpAnt.packetInterval);
                        PrtActive = true;
                        tmpPower = (float)tmpAnt.DataResourceCost * (1 / tmpAnt.packetInterval);
                    }
                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                    if (mode == GameState.FLIGHT)
                        addPart(current_part.flightID, PartAdd, false);
                    else
                        addPart(current_part.craftID, PartAdd, false);

                    break;
            }
        }

        /*
        private void checkBio(PartModule psdpart, Part current_part)
        {
            switch (psdpart.moduleName)
            {
                case "Biomatic":
                    string PrtName = current_part.name;
                    string PrtPower = "";
                    bool PrtActive = false;
                    const double biouse = 0.04;
                    biouse = global::   Telemachus.TelemachusPowerDrain.powerConsumption;
                    totalPowerDrain += biouse;
                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                    PowerConsList.Add(PartAdd);
                    break;
            }
        }
*/

        private void checkRego(PartModule psdpart, Part current_part)
        {
            //string PrtName = " ";
            bool PrtActive = false;
            //string PrtPower = " ";
            float tmpPower = 0f;

            switch (psdpart.moduleName)
            {
                case "REGO_ModuleResourceConverter":
                    global::Regolith.Common.REGO_ModuleResourceConverter tmpRegRC = (global::Regolith.Common.REGO_ModuleResourceConverter)psdpart;
                    string PrtName = tmpRegRC.name;
                    string PrtPower = "";
                    PrtActive = tmpRegRC.ModuleIsActive();
                    string[] arr = tmpRegRC.RecipeInputs.Split(',');
                    for (int i = 0; i < arr.Length; i += 2)
                    {
                        string ResName = arr[i];
                        if (ResName == MAIN_POWER_NAME)
                        {
                            double ResAmt = 0;
                            bool prse = double.TryParse(arr[i + 1], out ResAmt);
                            if (!prse) ResAmt = 0;
                            tmpPower = (float)ResAmt;
                            break;
                        }
                    }
                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                    if (mode == GameState.FLIGHT)
                        addPart(current_part.flightID, PartAdd, false);
                    else
                        addPart(current_part.craftID, PartAdd, false);

                    PrtPower = "";
                    tmpPower = 0f;
                    arr = tmpRegRC.RecipeOutputs.Split(',');
                    for (int i = 0; i < arr.Length; i += 2)
                    {
                        string ResName = arr[i];
                        if (ResName == MAIN_POWER_NAME)
                        {
                            double ResAmt = 0;
                            bool prse = double.TryParse(arr[i + 1], out ResAmt);
                            if (!prse) ResAmt = 0;
                            tmpPower = (float)ResAmt;
                            break;
                        }
                    }
                    PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                    if (mode == GameState.FLIGHT)
                        addPart(current_part.flightID, PartAdd, true);
                    else
                        addPart(current_part.craftID, PartAdd, true);

                    break;

                case "REGO_ModuleResourceHarvester":
                    global::Regolith.Common.REGO_ModuleResourceHarvester tmpRegRH = (global::Regolith.Common.REGO_ModuleResourceHarvester)psdpart;
                    PrtName = tmpRegRH.name;
                    PrtPower = "";
                    PrtActive = tmpRegRH.ModuleIsActive();
                    string[] arr2 = tmpRegRH.RecipeInputs.Split(',');
                    for (int i = 0; i < arr2.Length; i += 2)
                    {
                        string ResName = arr2[i];
                        if (ResName == MAIN_POWER_NAME)
                        {
                            double ResAmt = 0;
                            bool prse = double.TryParse(arr2[i + 1], out ResAmt);
                            if (!prse) ResAmt = 0;
                            tmpPower = (float)ResAmt;
                            break;
                        }
                    }
                    PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                    if (mode == GameState.FLIGHT)
                        addPart(current_part.flightID, PartAdd, false);
                    else
                        addPart(current_part.craftID, PartAdd, false);

                    break;
            }
        }

        /*
         * private void checkRTKol(PartModule psdpart, Part current_part)
        {
            switch (psdpart.moduleName)
            {
                case "ModuleInVitroNursery":
                    string PrtName = current_part.name;
                    string PrtPower = "";
                    bool PrtActive = false;
                    float tmpPower = 0;
                    global::RTKolonists.ModuleInVitroNursery tmpIN = (global::RTKolonists.ModuleInVitroNursery) psdpart;
                    if (mode == GameState.FLIGHT)
                    {
                        if (tmpIN.isEnabled)
                        {
                            PrtActive = true;
                            totalPowerDrain += tmpIN.resourceCost;
                            tmpPower = tmpIN.resourceCost;
                        }                        
                    }
                    if (mode == GameState.EDITOR)
                    {
                        PrtActive = true;
                        totalPowerDrain += tmpIN.resourceCost;
                        tmpPower = tmpIN.resourceCost;
                    }
                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                    if (mode == GameState.FLIGHT)
                        addPart(current_part.flightID, PartAdd, false);
                    else
                        addPart(current_part.craftID, PartAdd, false);

                    break;
            }
        }

        /*
        private void checkBTSM(PartModule psdpart, Part current_part)
        {
            string PrtName = current_part.name;
            string PrtPower = "";
            bool PrtActive = false;
            switch (psdpart.moduleName)
            {
                case "BTSMModuleProbePower":

                    //					global::BTSM.BTSMModuleProbePower tmpBP = (global::BTSM.BTSMModuleProbePower) tmpPM;
                    //					am_use += tmpBP.energyConsumedRate;
                    switch (psdpart.part.name)
                    {
                        case "probeCoreSphere":

                            totalPowerDrain += 0.16666668;
                            PrtPower = "0.166";
                            PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                            PowerConsList.Add(PartAdd);
                            break;

                        case "probeCoreCube":
                            totalPowerDrain += 0.08333334;
                            PrtPower = "0.083";
                            PwrPartList PartAdd2 = new PwrPartList(PrtName, PrtPower);
                            PowerConsList.Add(PartAdd2);
                            break;

                        case "probeCoreHex":
                            totalPowerDrain += 0.04166667;
                            PrtPower = "0.041";
                            PwrPartList PartAdd3 = new PwrPartList(PrtName, PrtPower);
                            PowerConsList.Add(PartAdd3);
                            break;

                        case "probeCoreOcto":
                            totalPowerDrain += 0.033333336;
                            PrtPower = "0.033";
                            PwrPartList PartAdd4 = new PwrPartList(PrtName, PrtPower);
                            PowerConsList.Add(PartAdd4);
                            break;

                        default:
                            totalPowerDrain += 0.02777778;
                            PrtPower = "0.027";
                            PwrPartList PartAdd5 = new PwrPartList(PrtName, PrtPower);
                            PowerConsList.Add(PartAdd5);
                            break;
                    }

                    break;

                case "BTSMModuleLifeSupport":

                    switch (psdpart.part.name)
                    {
                        case "Mark1-2Pod":
                            totalPowerDrain += 0.20833335;
                            PrtPower = "0.208";
                            PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                            PowerConsList.Add(PartAdd);
                            break;

                        case "landerCabinSmall":
                            totalPowerDrain += 0.25000002;
                            PrtPower = "0.250";
                            PwrPartList PartAdd2 = new PwrPartList(PrtName, PrtPower);
                            PowerConsList.Add(PartAdd2);
                            break;

                        case "mark3Cockpit":
                            totalPowerDrain += 0.19444446;
                            PrtPower = "0.194";
                            PwrPartList PartAdd3 = new PwrPartList(PrtName, PrtPower);
                            PowerConsList.Add(PartAdd3);
                            break;

                        default:
                            totalPowerDrain += 0.2777778;
                            PrtPower = "0.277";
                            PwrPartList PartAdd4 = new PwrPartList(PrtName, PrtPower);
                            PowerConsList.Add(PartAdd4);
                            break;
                    }

                    break;
            }
        }
*/
        //Current Vessel Parts list maintenance - used primarily in editor for parts list, also used for emergency shutdown procedure

        private void addPart(uint Pkey, PwrPartList PartAdd, bool ProdPrt)
        {
            PwrPartList PartFnd;

            this.Log_Debug("AYController Partadd " + Pkey + " " + PartAdd.PrtName);
            this.Log_Debug("AYController Partadd S" + PartAdd.PrtPower + " F" + PartAdd.PrtPowerF.ToString("00.00000"));

            if (ProdPrt) // Producer part list
            {
                if (vesselProdPartsList.TryGetValue(Pkey, out PartFnd))
                {
                    this.Log_Debug("AYController Partfnd " + Pkey + " " + PartFnd.PrtName);
                    this.Log_Debug("AYController Partfnd S" + PartFnd.PrtPower + " F" + PartFnd.PrtPowerF.ToString("00.00000"));

                    PartAdd.PrtPowerF += PartFnd.PrtPowerF;
                    PartAdd.PrtPower = PartAdd.PrtPowerF.ToString("000.00000");
                    vesselProdPartsList[Pkey] = PartAdd;
                    this.Log_Debug("AYController Partadded " + Pkey + " " + PartAdd.PrtName);
                    this.Log_Debug("AYController Partadded S" + PartAdd.PrtPower + " F" + PartAdd.PrtPowerF.ToString("00.00000"));
                }
                else
                {
                    PartAdd.PrtPower = PartAdd.PrtPowerF.ToString("000.00000");
                    vesselProdPartsList.Add(Pkey, PartAdd);
                    this.Log_Debug("AYController new Partadded");
                }
            }
            else // consumer part list
            {
                if (vesselConsPartsList.TryGetValue(Pkey, out PartFnd))
                {
                    this.Log_Debug("AYController Partfnd " + Pkey + " " + PartFnd.PrtName);
                    this.Log_Debug("AYController Partfnd S" + PartFnd.PrtPower + " F" + PartFnd.PrtPowerF.ToString("00.00000"));
                    PartAdd.PrtPowerF += PartFnd.PrtPowerF;
                    PartAdd.PrtPower = PartAdd.PrtPowerF.ToString("000.00000");
                    vesselConsPartsList[Pkey] = PartAdd;
                    this.Log_Debug("AYController Partadded " + Pkey + " " + PartAdd.PrtName);
                    this.Log_Debug("AYController Partadded S" + PartAdd.PrtPower + " F" + PartAdd.PrtPowerF.ToString("00.00000"));
                }
                else
                {
                    PartAdd.PrtPower = PartAdd.PrtPowerF.ToString("000.00000");
                    vesselConsPartsList.Add(Pkey, PartAdd);
                    this.Log_Debug("AYController new Partadded");
                }
            }
        }

        //Vessel Functions Follow - to store list of vessels and store/retrieve AmpYear settings for each vessel

        private void CheckVslUpdate()
        {
            // Called every fixed update from fixedupdate - Check for vessels that have been deleted and remove from Dictionary
            double currentTime = Planetarium.GetUniversalTime();
            List <Vessel> allVessels = FlightGlobals.Vessels;
            var knownVessels = AYgameSettings.knownVessels;
            var vesselsToDelete = new List<Guid>();

            this.Log_Debug("AYController CheckVslUpdate");
            this.Log_Debug("AYController allvessels count = " + allVessels.Count);            
            this.Log_Debug("AYController knownvessels count = " + knownVessels.Count);
            foreach (var entry in knownVessels)
            {
                this.Log_Debug("AYController knownvessels guid = " + entry.Key);
                Guid vesselId = entry.Key;
                VesselInfo vesselInfo = entry.Value;
                Vessel vessel = allVessels.Find(v => v.id == vesselId);

                if (vessel == null)
                {
                    this.Log_Debug("Deleting vessel " + vesselInfo.vesselName + " - vessel does not exist anymore");
                    vesselsToDelete.Add(vesselId);
                    continue;
                }

                if (vessel.loaded)
                {
                    int crewCapacity = UpdateVesselCounts(vesselInfo, vessel);

                    if (vesselInfo.numCrew == 0)
                    {
                        this.Log_Debug("Deleting vessel " + vesselInfo.vesselName + " - no crew parts anymore");
                        vesselsToDelete.Add(vesselId);
                        continue;
                    }
                }

                if (vessel.loaded)
                {
                    ConsumeResources(currentTime, vessel, vesselInfo);
                }

                if (vesselId == currentvesselid)
                {
                    this.Log_Debug("updating current vessel");
                    UpdateVesselInfo(vesselInfo, vessel);
                    vesselInfo.vesselType = FlightGlobals.ActiveVessel.vesselType;
                }
            }

            vesselsToDelete.ForEach(id => knownVessels.Remove(id));

            foreach (Vessel vessel in allVessels.Where(v => v.loaded))
            {                
                if (!knownVessels.ContainsKey(vessel.id) && (vessel.FindPartModulesImplementing<ModuleCommand>().First() != null ))
                {
                    this.Log_Debug("New vessel: " + vessel.vesselName + " (" + vessel.id + ")");
                    VesselInfo vesselInfo = new VesselInfo(vessel.vesselName, currentTime);
                    knownVessels[vessel.id] = vesselInfo;
                    vesselInfo.vesselType = vessel.vesselType;
                    UpdateVesselInfo(vesselInfo, vessel);
                }
            }

            this.Log_Debug("AYController CheckVslUpdate complete");
        }

        private void ConsumeResources(double currentTime, Vessel vessel, VesselInfo vesselInfo)
        {
            return;
        }


        private void UpdateVesselInfo(VesselInfo vesselInfo, Vessel vessel)
        {
            // save current toggles to current vesselinfo
            this.Log_Debug("AYController UpdateVesselInfo " + vessel.id);
            vesselInfo.managerEnabled = managerEnabled;
            vesselInfo.ShowCrew = ShowCrew;
            vesselInfo.ShowParts = ShowParts;
            vesselInfo.subsystemDrain = subsystemDrain;
            vesselInfo.subsystemToggle = subsystemToggle;
            vesselInfo.guiSectionEnableFlag = guiSectionEnableFlag;
            vesselInfo.EmgcyShutActive = EmgcyShutActive;
            int crewCapacity = UpdateVesselCounts(vesselInfo, vessel);
        }

        private int UpdateVesselCounts(VesselInfo vesselInfo, Vessel vessel)
        {
            // save current toggles to current vesselinfo
            this.Log_Debug("AYController UpdateVesselCounts " + vessel.id);

            int crewCapacity = 0;
            vesselInfo.ClearAmounts(); // numCrew = 0; numOccupiedParts = 0;
            foreach (Part part in vessel.parts)
            {
                crewCapacity += part.CrewCapacity;
                if (part.protoModuleCrew.Count > 0)
                {
                    vesselInfo.numCrew += part.protoModuleCrew.Count;
                    ++vesselInfo.numOccupiedParts;
                }
            }
            return crewCapacity;
        }

        private void VesselLoad(Vessel newvessel)
        {
            double currentTime = Planetarium.GetUniversalTime();
            var knownVessels = AYgameSettings.knownVessels;
            VesselInfo info = new VesselInfo(newvessel.name, currentTime);
            this.Log_Debug("AYController VesselLoad " + newvessel.id);
            //if newvessel is not the active vessel then do nothing.
            if (newvessel.id != FlightGlobals.ActiveVessel.id)
            {
                this.Log_Debug("AYController newvessel is not active vessel");
                return;
            }
            currentvesselid = newvessel.id;
            loadVesselSettings(newvessel, info);
        }

        private void VesselChange(Vessel newvessel)
        {
            this.Log_Debug("AYController VesselChange New " + newvessel.id + " Old " + currentvesselid);
            this.Log_Debug("AYController active vessel " + FlightGlobals.ActiveVessel.id);
            double currentTime = Planetarium.GetUniversalTime();
            var knownVessels = AYgameSettings.knownVessels;
            VesselInfo info = new VesselInfo(newvessel.name, currentTime);

            if (currentvesselid == newvessel.id)
                return;

            // Update Old Vessel settings into Dictionary
            if (knownVessels.ContainsKey(currentvesselid))
            {
                info = knownVessels[currentvesselid];
                UpdateVesselInfo(info, FlightGlobals.ActiveVessel);
                this.Log_Debug("Updated old vessel dict " + knownVessels[currentvesselid].ToString());
            }
            loadVesselSettings(newvessel, info);
            currentvesselid = newvessel.id;
        }

        private void loadVesselSettings(Vessel newvessel, VesselInfo info)
        {
            var knownVessels = AYgameSettings.knownVessels;
            // Load New Vessel settings from Dictionary
            if (knownVessels.ContainsKey(newvessel.id))
            {
                this.Log_Debug("AYController Vessel Loading Settings");
                info = knownVessels[newvessel.id];
                this.Log_Debug(info.ToString());
                for (int i = 0; i < Enum.GetValues(typeof(Subsystem)).Length; i++)
                    subsystemToggle[i] = info.subsystemToggle[i];
                for (int i = 0; i < Enum.GetValues(typeof(GUISection)).Length; i++)
                    guiSectionEnableFlag[i] = info.guiSectionEnableFlag[i];
                managerEnabled = info.managerEnabled;
                ShowCrew = info.ShowCrew;
                EmgcyShutActive = info.EmgcyShutActive;
            }
            else //New Vessel not found in Dictionary so set default
            {
                this.Log_Debug("AYController Vessel Setting Default Settings");
                for (int i = 0; i < Enum.GetValues(typeof(Subsystem)).Length; i++)
                    subsystemToggle[i] = false;
                for (int i = 0; i < Enum.GetValues(typeof(GUISection)).Length; i++)
                    guiSectionEnableFlag[i] = false;
                managerEnabled = false;
                ShowCrew = false;
                EmgcyShutActive = false;
            }
        }

        //Subsystem Functions Follow

        private bool timewarpIsValid
        {
            get
            {
                return TimeWarp.CurrentRate <= 4;
            }
        }

        private double managerActiveDrain
        {
            get
            {
                return MANAGER_ACTIVE_DRAIN;
            }
        }

        public bool managerIsActive
        {
            get
            {
                return timewarpIsValid && managerEnabled && (hasPower || hasReservePower);
            }
        }

        private double managerCurrentDrain
        {
            get
            {
                if (managerIsActive)
                    return managerActiveDrain;
                else
                    return 0.0;
            }
        }

        private static string subsystemName(Subsystem subsystem)
        {
            switch (subsystem)
            {
                case Subsystem.POWER_TURN:
                    return "PowerTurn";

                case Subsystem.SAS:
                    return "SAS";

                case Subsystem.RCS:
                    return "RCS";

                case Subsystem.HEATER:
                    return "Heater";

                case Subsystem.COOLER:
                    return "Air Cond.";

                case Subsystem.MUSIC:
                    return "Smooth Jazz";

                case Subsystem.MASSAGE:
                    return "Massage Chair";

                default:
                    return String.Empty;
            }
        }

        public bool subsystemEnabled(Subsystem subsystem)
        {
            Vessel cv = FlightGlobals.ActiveVessel;
            switch (subsystem)
            {
                case Subsystem.SAS:
                    return cv.ActionGroups[KSPActionGroup.SAS];

                case Subsystem.RCS:
                    return cv.ActionGroups[KSPActionGroup.RCS];

                default:
                    return subsystemToggle[(int)subsystem];
            }
        }

        public void setSubsystemEnabled(Subsystem subsystem, bool enabled)
        {
            Vessel cv = FlightGlobals.ActiveVessel;
            switch (subsystem)
            {
                case Subsystem.SAS:
                    if (cv.Autopilot.CanSetMode(VesselAutopilot.AutopilotMode.StabilityAssist))
                        cv.ActionGroups.SetGroup(KSPActionGroup.SAS, enabled);
                    else
                        ScreenMessages.PostScreenMessage(cv.vesselName + " - Cannot Engage SAS - Autopilot function not available", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                    break;

                case Subsystem.RCS:
                    cv.ActionGroups.SetGroup(KSPActionGroup.RCS, enabled);
                    break;

                default:
                    subsystemToggle[(int)subsystem] = enabled;
                    break;
            }
        }

        public bool subsystemActive(Subsystem subsystem)
        {
            if (!subsystemEnabled(subsystem))
                return false;

            switch (subsystem)
            {
                case Subsystem.HEATER:
                    return totalHeatedParts < crewablePartList.Count;

                case Subsystem.COOLER:
                    return totalCooledParts < crewablePartList.Count;

                default:
                    return true;
            }
        }

        public double subsystemCurrentDrain(Subsystem subsystem)
        {
            if (!subsystemActive(subsystem) || !managerIsActive || !hasPower)
                return 0.0;

            switch (subsystem)
            {
                case Subsystem.RCS:
                    if (currentRCSThrust > 0.0f)
                        return subsystemActiveDrain(subsystem);
                    else
                        return 0.0;

                //case Subsystem.POWER_TURN:
                //    return turningFactor * subsystemActiveDrain(subsystem);

                default:
                    return subsystemActiveDrain(subsystem);
            }
        }

        public double subsystemActiveDrain(Subsystem subsystem)
        {
            switch (subsystem)
            {
                case Subsystem.SAS:                    
                    return SAS_BASE_DRAIN;

                case Subsystem.RCS:
                    return RCS_DRAIN + currentPoweredRCSDrain;
                    //return RCS_DRAIN;

                case Subsystem.POWER_TURN:
                    return sasAdditionalRotPower * POWER_TURN_DRAIN_FACTOR;

                case Subsystem.HEATER:
                case Subsystem.COOLER:
                    if (mode == GameState.FLIGHT)
                        return HEATER_HEAT_RATE
                               * (crewablePartList.Count * AYsettings.HEATER_BASE_DRAIN_FACTOR + HEATER_CAPACITY_DRAIN_FACTOR * FlightGlobals.ActiveVessel.GetCrewCapacity());
                    else return HEATER_HEAT_RATE
                               * (crewablePartList.Count * AYsettings.HEATER_BASE_DRAIN_FACTOR + HEATER_CAPACITY_DRAIN_FACTOR);

                case Subsystem.MUSIC:
                    return 1.0 * crewablePartList.Count;

                case Subsystem.MASSAGE:
                    if (mode == GameState.FLIGHT)
                        return AYsettings.MASSAGE_BASE_DRAIN_FACTOR * FlightGlobals.ActiveVessel.GetCrewCount();
                    else return AYsettings.MASSAGE_BASE_DRAIN_FACTOR;

                default:
                    return 0.0;
            }
        }

        public bool subsystemPowered(Subsystem subsystem)
        {
            return hasPower && managerIsActive && subsystemActive(subsystem);
        }

        private void changeCrewedPartsTemperature(double target_temp, bool heat)
        {
            foreach (Part crewed_part in crewablePartList)
            {
                if (heat)
                {
                    foreach (PartModule module in crewed_part.Modules)
                    {
                        if (module.moduleName == "AYCrewPart")
                        {
                            if (((AYCrewPart)module).CabinTemp < target_temp)
                            {
                                ((AYCrewPart)module).CabinTemp += AYController.HEATER_HEAT_RATE * sumDeltaTime;
                            }
                        }
                    }
                }
                else
                {
                    foreach (PartModule module in crewed_part.Modules)
                    {
                        if (module.moduleName == "AYCrewPart")
                        {
                            if (((AYCrewPart)module).CabinTemp > target_temp)
                            {
                                ((AYCrewPart)module).CabinTemp -= AYController.HEATER_HEAT_RATE * sumDeltaTime;
                            }
                        }
                    }
                }
            }
        }

        //Resources Functions Follow

        private double requestResource(Part cvp, String name, double amount)
        {
            //Part cvp = FlightGlobals.ActiveVessel.rootPart;
            //return requestResource(name, amount, cvp);
            if (amount <= 0.0)
                return 0.0;
            double total_received = 0.0;
            double request_amount = amount;
            for (int attempts = 0; ((attempts < MAX_TRANSFER_ATTEMPTS) && (amount > 0.0)); attempts++)
            {
                double received = cvp.RequestResource(name, request_amount, ResourceFlowMode.ALL_VESSEL);
                this.Log_Debug("requestResource attempt " + attempts);
                this.Log_Debug("requested power = " + request_amount.ToString("0.000000000000"));
                this.Log_Debug("received power = " + received.ToString("0.000000000000"));
                total_received += received;
                amount -= received;
                this.Log_Debug("amount = " + amount.ToString("0.000000000000"));

                if (received <= 0.0)
                    request_amount = amount * 0.5;
                else
                    request_amount = amount;
            }
            return total_received;
        }

        public void transferReserveToMain(double amount)
        {
            Part cvp = FlightGlobals.ActiveVessel.rootPart;

            if (amount > totalReservePower * RECHARGE_OVERFLOW_AVOID_FACTOR)
                amount = totalReservePower * RECHARGE_OVERFLOW_AVOID_FACTOR;

            if (amount > (totalElectricChargeCapacity - totalElectricCharge))
                amount = (totalElectricChargeCapacity - totalElectricCharge);

            double received = requestResource(cvp, RESERVE_POWER_NAME, amount);

            int transfer_attempts = 0;
            while (received > 0.0 && transfer_attempts < MAX_TRANSFER_ATTEMPTS)
            {
                received += cvp.RequestResource(MAIN_POWER_NAME, -received);
                transfer_attempts++;
            }
        }

        public void transferMainToReserve(double amount)
        {
            Part cvp = FlightGlobals.ActiveVessel.rootPart;
            if (amount > totalElectricCharge * RECHARGE_OVERFLOW_AVOID_FACTOR)
                amount = totalElectricCharge * RECHARGE_OVERFLOW_AVOID_FACTOR;

            if (amount > (totalReservePowerCapacity - totalReservePower))
                amount = (totalReservePowerCapacity - totalReservePower);

            double received = requestResource(cvp, MAIN_POWER_NAME, amount);

            int transfer_attempts = 0;
            while (received > 0.0 && transfer_attempts < MAX_TRANSFER_ATTEMPTS)
            {
                received += cvp.RequestResource(RESERVE_POWER_NAME, -received);
                transfer_attempts++;
            }
        }

        //GUI Functions Follow

        private void onDraw()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)  // Check if in flight
                {
                    if (FlightGlobals.ActiveVessel.isEVA) // EVA kerbal, do nothing
                    {
                        mode = GameState.EVA;
                        return;
                    }
                    mode = 0;
                }
                else if (EditorLogic.fetch != null) // Check if in editor
                    mode = GameState.EDITOR;
                else   // Not in flight, in editor or F2 pressed unset the mode and return
                {
                    mode = GameState.EVA;
                    return;
                }

                if (mode == GameState.FLIGHT)
                {
                    GUI.skin = HighLogic.Skin;
                    FwindowPos = GUILayout.Window(windowID, FwindowPos, windowF, "AmpYear Power Manager",
                        GUILayout.Width(FWINDOW_WIDTH), GUILayout.Height(WINDOW_BASE_HEIGHT));                   
                }
            }

            if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                GUI.skin = HighLogic.Skin;
                EwindowPos = GUILayout.Window(windowID, EwindowPos, windowE, "AmpYear Power Manager",
                    GUILayout.Width(EWINDOW_WIDTH), GUILayout.Height(WINDOW_BASE_HEIGHT));
                if (ShowParts) ScrollParts();
            }
        }

        private void windowF(int id)
        {
            //Init styles
            sectionTitleStyle = new GUIStyle(GUI.skin.label);
            sectionTitleStyle.alignment = TextAnchor.MiddleCenter;
            sectionTitleStyle.stretchWidth = true;
            sectionTitleStyle.fontStyle = FontStyle.Bold;

            subsystemConsumptionStyle = new GUIStyle(GUI.skin.label);
            subsystemConsumptionStyle.alignment = TextAnchor.LowerRight;
            subsystemConsumptionStyle.stretchWidth = true;
            //subsystemConsumptionStyle.margin.top = 4;

            powerSinkStyle = new GUIStyle(GUI.skin.label);
            powerSinkStyle.alignment = TextAnchor.LowerLeft;
            powerSinkStyle.stretchWidth = true;

            statusStyle = new GUIStyle(GUI.skin.label);
            statusStyle.alignment = TextAnchor.MiddleCenter;
            statusStyle.stretchWidth = true;
            statusStyle.normal.textColor = Color.white;

            warningStyle = new GUIStyle(GUI.skin.label);
            warningStyle.alignment = TextAnchor.MiddleCenter;
            warningStyle.stretchWidth = true;
            warningStyle.fontStyle = FontStyle.Bold;
            warningStyle.normal.textColor = Color.red;

            subsystemButtonStyle = new GUIStyle(GUI.skin.toggle);
            subsystemButtonStyle.margin.top = 0;
            subsystemButtonStyle.margin.bottom = 0;
            subsystemButtonStyle.padding.top = 0;
            subsystemButtonStyle.padding.bottom = 0;

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            foreach (GUISection section in Enum.GetValues(typeof(GUISection)))
            {
                guiSectionEnableFlag[(int)section]
                    = GUILayout.Toggle(guiSectionEnableFlag[(int)section], guiSectionName(section), GUI.skin.button);
            }
            GUILayout.EndHorizontal();

            //Manager status+drain
            if (timewarpIsValid)
            {
                GUILayout.BeginHorizontal();
                managerEnabled = GUILayout.Toggle(managerEnabled, "Manager", subsystemButtonStyle, subsystemButtonOptions);
                if (managerIsActive)
                    consumptionLabel(managerCurrentDrain, false);
                else
                    consumptionLabel(managerActiveDrain, true);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                ShowCrew = GUILayout.Toggle(ShowCrew, "ShowCrew", subsystemButtonStyle, subsystemButtonOptions);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EmgcyShutActive = GUILayout.Toggle(EmgcyShutActive, "EmgcyShutActive", subsystemButtonStyle, subsystemButtonOptions);
                GUILayout.EndHorizontal();
            }

            //Manager status label
            if (hasPower || hasReservePower)
            {
                if (managerIsActive)
                {
                    if (hasPower)
                    {
                        if (totalElectricChargeCapacity > 0.0)
                        {
                            double power_percent = (totalElectricCharge / totalElectricChargeCapacity) * 100.0;
                            if (power_percent < 20.00)
                                GUILayout.Label("Power: " + power_percent.ToString("0.00") + '%', warningStyle);
                            else
                                GUILayout.Label("Power: " + power_percent.ToString("0.00") + '%', statusStyle);
                            if (totalPowerDrain > totalPowerProduced)
                                GUILayout.Label("Power Drain : " + totalPowerDrain.ToString("0.000"), warningStyle);
                            else
                                GUILayout.Label("Power Drain : " + totalPowerDrain.ToString("0.000"), statusStyle);
                            if (totalPowerProduced > 0)
                                GUILayout.Label("Power Prod : " + totalPowerProduced.ToString("0.000"), statusStyle);
                            else
                                GUILayout.Label("Power Prod : " + totalPowerProduced.ToString("0.000"), warningStyle);
                        }
                    }
                    else
                        GUILayout.Label("Running on Reserve Power!", warningStyle);
                }
                else if (timewarpIsValid)
                    GUILayout.Label("Manager Disabled", warningStyle);
                else
                    GUILayout.Label("Auto-Hibernation", statusStyle);
            }
            else
                GUILayout.Label("Insufficient Power", warningStyle);

            if (managerIsActive)
            {
                //Subsystems
                if (guiSectionEnabled(GUISection.SUBSYSTEM))
                {
                    GUILayout.Label("Subsystems", sectionTitleStyle);
                    foreach (Subsystem subsystem in Enum.GetValues(typeof(Subsystem)))
                    {
                        if (!subsystemIsLuxury(subsystem) && subsystemVisible(subsystem))
                        {
                            GUILayout.BeginHorizontal();
                            subsystemButton(subsystem);
                            subsystemConsumptionLabel(subsystem);
                            GUILayout.EndHorizontal();
                        }
                    }
                }

                //Luxury
                if (guiSectionEnabled(GUISection.LUXURY))
                {
                    GUILayout.Label("Luxury", sectionTitleStyle);
                    foreach (Subsystem subsystem in Enum.GetValues(typeof(Subsystem)))
                    {
                        if (subsystemIsLuxury(subsystem) && subsystemVisible(subsystem))
                        {
                            GUILayout.BeginHorizontal();
                            subsystemButton(subsystem);
                            subsystemConsumptionLabel(subsystem);
                            GUILayout.EndHorizontal();
                        }
                    }
                }

                //Reserve
                if (guiSectionEnabled(GUISection.RESERVE))
                {
                    GUILayout.Label("Reserve Power", sectionTitleStyle);

                    //Reserve status label
                    if (totalReservePowerCapacity > 0.0)
                    {
                        if (hasReservePower)
                        {
                            double reserve_percent = (totalReservePower / totalReservePowerCapacity) * 100.0;
                            if (reserve_percent < 30.0)
                                GUILayout.Label("Reserve Power: " + reserve_percent.ToString("0.00") + '%', warningStyle);
                            else
                                GUILayout.Label("Reserve Power: " + reserve_percent.ToString("0.00") + '%', statusStyle);
                        }
                        else
                            GUILayout.Label("Reserve Power Depleted", warningStyle);
                    }
                    else
                        GUILayout.Label("Reserve Power not Found!", warningStyle);

                    //Reserve transfer
                    String[] increment_percent_string = new String[RESERVE_TRANSFER_INCREMENTS.Length];

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Reserve to Main");
                    for (int i = 0; i < RESERVE_TRANSFER_INCREMENTS.Length; i++)
                    {
                        increment_percent_string[i] = (RESERVE_TRANSFER_INCREMENTS[i] * 100).ToString("F0") + '%';
                        if (GUILayout.Button(increment_percent_string[i]))
                            transferReserveToMain(totalReservePowerCapacity * RESERVE_TRANSFER_INCREMENTS[i]);
                    }

                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Main to Reserve");
                    for (int i = 0; i < RESERVE_TRANSFER_INCREMENTS.Length; i++)
                    {
                        if (GUILayout.Button(increment_percent_string[i]))
                            transferMainToReserve(totalReservePowerCapacity * RESERVE_TRANSFER_INCREMENTS[i]);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            //ShowCrew
            if (ShowCrew)
            {
                GUILayout.Label("Crew", sectionTitleStyle);
                if (VslRstr.Count > 0)
                {
                    foreach (ProtoCrewMember CrewMbr in VslRstr)
                    {
                        GUILayout.Label(CrewMbr.name + " " + CrewMbr.experienceTrait.Title, statusStyle);
                    }
                }
                else if (timewarpIsValid)
                    GUILayout.Label("No Crew OnBoard", warningStyle);
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void windowE(int id)
        {
            //Init styles
            sectionTitleStyle = new GUIStyle(GUI.skin.label);
            sectionTitleStyle.alignment = TextAnchor.MiddleCenter;
            sectionTitleStyle.stretchWidth = true;
            sectionTitleStyle.fontStyle = FontStyle.Bold;

            subsystemConsumptionStyle = new GUIStyle(GUI.skin.label);
            subsystemConsumptionStyle.alignment = TextAnchor.LowerRight;
            subsystemConsumptionStyle.stretchWidth = true;
            //subsystemConsumptionStyle.margin.top = 4;

            powerSinkStyle = new GUIStyle(GUI.skin.label);
            powerSinkStyle.alignment = TextAnchor.MiddleCenter;
            powerSinkStyle.stretchWidth = true;
            powerSinkStyle.normal.textColor = Color.white;

            statusStyle = new GUIStyle(GUI.skin.label);
            statusStyle.alignment = TextAnchor.MiddleCenter;
            statusStyle.stretchWidth = true;
            statusStyle.normal.textColor = Color.white;

            warningStyle = new GUIStyle(GUI.skin.label);
            warningStyle.alignment = TextAnchor.MiddleCenter;
            warningStyle.stretchWidth = true;
            warningStyle.fontStyle = FontStyle.Bold;
            warningStyle.normal.textColor = Color.red;

            subsystemButtonStyle = new GUIStyle(GUI.skin.toggle);
            subsystemButtonStyle.margin.top = 0;
            subsystemButtonStyle.margin.bottom = 0;
            subsystemButtonStyle.padding.top = 0;
            subsystemButtonStyle.padding.bottom = 0;

            GUILayout.BeginVertical();

            //Manager status+drain
            GUILayout.BeginHorizontal();
            managerEnabled = GUILayout.Toggle(managerEnabled, "Manager", subsystemButtonStyle, subsystemButtonOptions);
            if (managerIsActive)
                consumptionLabel(managerCurrentDrain, false);
            else
                consumptionLabel(managerActiveDrain, true);
            GUILayout.EndHorizontal();
            ShowParts = GUILayout.Toggle(ShowParts, "ShowParts", subsystemButtonStyle, subsystemButtonOptions);
            //Power Capacity
            GUILayout.Label("Power Capacity: " + totalElectricChargeCapacity.ToString("0.000"), statusStyle);
            if (totalPowerDrain > totalPowerProduced)
                GUILayout.Label("Power Drain : " + totalPowerDrain.ToString("0.000"), warningStyle);
            else
                GUILayout.Label("Power Drain : " + totalPowerDrain.ToString("0.000"), statusStyle);
            if (totalPowerProduced > 0)
                GUILayout.Label("Power Prod : " + totalPowerProduced.ToString("0.000"), statusStyle);
            else
                GUILayout.Label("Power Prod : " + totalPowerProduced.ToString("0.000"), warningStyle);

            //Reserve
            GUILayout.Label("Reserve Power", sectionTitleStyle);
            //Reserve status label
            GUILayout.Label("Reserve Power Capacity: " + totalReservePowerCapacity.ToString("0.000"), statusStyle);
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void ScrollParts()
        {
            GUI.skin = HighLogic.Skin;
            Rect EPLwindowPos = new Rect(EwindowPos.x + EWINDOW_WIDTH + 10, EwindowPos.y, AYController.EWINDOW_WIDTH, Screen.height / 2 - 100);
            // Begin the ScrollView
            scrollViewVector = GUI.BeginScrollView(EPLwindowPos, scrollViewVector, new Rect(0, 0, EWINDOW_WIDTH - 20, 1700));
            // Put something inside the ScrollView
            GUILayout.Label("Power Production Parts", sectionTitleStyle);
            foreach (var entry in vesselProdPartsList)
            {
                GUILayout.Label(entry.Value.PrtName + " " + entry.Value.PrtPower, powerSinkStyle);
            }
            GUILayout.Label("Power Consumer Parts", sectionTitleStyle);
            foreach (var entry in vesselConsPartsList)
            {
                GUILayout.Label(entry.Value.PrtName + " " + entry.Value.PrtPower, powerSinkStyle);
            }

            
            // End the ScrollView
            GUI.EndScrollView();
        }
               
        private void subsystemButton(Subsystem subsystem)
        {
            setSubsystemEnabled(
                subsystem,
                GUILayout.Toggle(subsystemEnabled(subsystem), subsystemName(subsystem), subsystemButtonStyle, GUILayout.Width(FWINDOW_WIDTH / 2.0f))
                );
        }

        private void subsystemConsumptionLabel(Subsystem subsystem)
        {
            double drain = subsystemDrain[(int)subsystem];
            switch (subsystem)
            {
                case Subsystem.RCS:
                    drain += drain + currentPoweredRCSDrain;
                    break;

                default:
                    break;
            }
            if (drain == 0.0)
            {
                drain = subsystemActiveDrain(subsystem);
                consumptionLabel(drain, true);
            }
            else
                consumptionLabel(drain, false);
        }

        private void consumptionLabel(double drain, bool greyed = false)
        {
            if (drain == 0.0 || greyed)
                subsystemConsumptionStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
            else if (drain > 0.0)
                subsystemConsumptionStyle.normal.textColor = Color.red;
            else
                subsystemConsumptionStyle.normal.textColor = Color.green;

            GUILayout.Label(drain.ToString("0.000") + "/s", subsystemConsumptionStyle);
        }

        //Class Load and Save of global settings
        public void Load(ConfigNode globalNode)
        {
            this.Log_Debug("AYController Load");
            FwindowPos.x = AYsettings.FwindowPosX;
            FwindowPos.y = AYsettings.FwindowPosY;
            EwindowPos.x = AYsettings.EwindowPosX;
            EwindowPos.y = AYsettings.EwindowPosY;
            this.Log_Debug("AYController Load end");
        }

        public void Save(ConfigNode globalNode)
        {
            this.Log_Debug("AYController Save");
            AYsettings.FwindowPosX = FwindowPos.x;
            AYsettings.FwindowPosY = FwindowPos.y;
            AYsettings.EwindowPosX = EwindowPos.x;
            AYsettings.EwindowPosY = EwindowPos.y;
            this.Log_Debug("AYController Save end");
        }
    }
}