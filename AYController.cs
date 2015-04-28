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
using System.Linq;
using UnityEngine;

namespace AY
{
    public partial class AYController : MonoBehaviour, Savable, Iayaddon
    {
        #region Iayaddon

        public List<Part> CrewablePartList
        {
            get
            {
                return this.crewablePartList;
            }
        }

        public bool[] SubsystemToggle
        {
            get
            {
                return this.subsystemToggle;
            }
        }

        public bool HasPower
        {
            get
            {
                return this.hasPower;
            }
        }

        public bool ManagerisActive
        {
            get
            {
                return this.managerIsActive;
            }
        }

        public bool DeBugging
        {
            get
            {
                return this.AYsettings.debugging;
            }
        }

        #endregion Iayaddon

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

        public static AYController Instance { get; private set; }

        public AYController()
        {
            Utilities.Log("AYController", "Constructor");
            Instance = this;
        }

        //AmpYear Properties
        public List<Part> crewablePartList = new List<Part>();

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
        public bool hasPower = true;
        public bool hasReservePower = true;
        public bool hasRCS = false;
        public float currentRCSThrust = 0.0f;
        public float currentPoweredRCSDrain = 0.0f;
        public static Guid currentvesselid;
        public int totalClimateParts = 0;
        public int maxCrew = 0;
        private bool KKPresent = false;
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
        private GUIStyle sectionTitleStyle, subsystemButtonStyle, subsystemConsumptionStyle, statusStyle, warningStyle, powerSinkStyle, PartListStyle, PartListPartStyle;
        public GUILayoutOption[] subsystemButtonOptions;
        private static int FwindowID = new System.Random().Next();
        private static int EwindowID = new System.Random().Next();
        private static int WwindowID = new System.Random().Next();
        private static GameState mode = GameState.EVA;  // Display mode, currently  0 for In-Flight, 1 for Editor, 2 for EVA (Hide)
        private bool[] subsystemToggle = new bool[Enum.GetValues(typeof(Subsystem)).Length];
        private double[] subsystemDrain = new double[Enum.GetValues(typeof(Subsystem)).Length];
        private bool managerEnabled = true;
        private bool ShowCrew = false;
        private bool ShowParts = false;
        private double timeLastElectricity;
        private double lastUpdate;
        private float powerUpTime = 0.0f;
        public bool EmgcyShutActive = false;
        private static bool ShowWarn = true;
        private static bool WarnWinOn = false;
        private Rect FwindowPos = new Rect(40, Screen.height / 2 - 100, AYController.FWINDOW_WIDTH, 200); // Flight Window position and size
        private Rect EwindowPos = new Rect(40, Screen.height / 2 - 100, AYController.EWINDOW_WIDTH, 200); // Editor Window position and size
        private Rect WarnWinPos = new Rect(Screen.width / 2 - 250, Screen.height / 4, 500, 150);  // Warp warning window position and size

        //Constants
        public const double MANAGER_ACTIVE_DRAIN = 1.0 / 60.0;

        public const double RCS_DRAIN = 1.0 / 60.0;
        public const float POWER_UP_DELAY = 10f;
        public const double SAS_BASE_DRAIN = 1.0 / 60.0;
        public const double POWER_TURN_DRAIN_FACTOR = 1.0 / 5.0;
        public const float SAS_POWER_TURN_TORQUE_FACTOR = 0.25f;
        public const float CLIMATE_HEAT_RATE = 1f;
        public const double CLIMATE_CAPACITY_DRAIN_FACTOR = 0.5;
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
            }
        }

        public void Awake()
        {
            AYsettings = AmpYear.Instance.AYsettings;
            AYgameSettings = AmpYear.Instance.AYgameSettings;
            vesselProdPartsList = new Dictionary<uint, PwrPartList>();
            vesselConsPartsList = new Dictionary<uint, PwrPartList>();
            /*
            if (ToolbarManager.ToolbarAvailable && AYsettings.UseAppLauncher == false)
            {
                button1 = ToolbarManager.Instance.add("AmpYear", "button1");
                button1.TexturePath = "REPOSoftTech/AmpYear/Icons/toolbarIcon";
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
             * */
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
                                          (Texture)GameDatabase.Instance.GetTexture("REPOSoftTech/AmpYear/Icons/AYIconOff", false));
            }
        }

        private void DummyVoid()
        {
        }

        public void onAppLaunchToggleOn()
        {
            this.stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture("REPOSoftTech/AmpYear/Icons/AYIconOn", false));
            GuiVisible = true;
        }

        public void onAppLaunchToggleOff()
        {
            this.stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture("REPOSoftTech/AmpYear/Icons/AYIconOff", false));
            GuiVisible = false;
        }

        public void Start()
        {
            this.Log_Debug("AYController Start");

            if (ToolbarManager.ToolbarAvailable && AYsettings.UseAppLauncher == false)
            {
                button1 = ToolbarManager.Instance.add("AmpYear", "button1");
                button1.TexturePath = "REPOSoftTech/AmpYear/Icons/toolbarIcon";
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
            KKPresent = KKClient.KKInstalled;

            this.Log_Debug("Checked for mods");
            if (KKPresent)
                this.Log_Debug("KabinKraziness present");
            else
                this.Log_Debug("KabinKraziness NOT present");
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
            //check if inflight and active vessel set currentvesselid and load config settings for this vessel
            if (FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)
            {
                currentvesselid = FlightGlobals.ActiveVessel.id;
                onVesselLoad(FlightGlobals.ActiveVessel);
            }

            // add callbacks for vessel load and change
            GameEvents.onVesselChange.Add(onVesselChange);
            GameEvents.onVesselLoaded.Add(onVesselLoad);
            GameEvents.onCrewBoardVessel.Add(onCrewBoardVessel);

            RenderingManager.AddToPostDrawQueue(5, this.onDraw);

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
            GameEvents.onVesselChange.Remove(onVesselChange);
            GameEvents.onVesselLoaded.Remove(onVesselLoad);
            GameEvents.onCrewBoardVessel.Remove(onCrewBoardVessel);
            RenderingManager.RemoveFromPostDrawQueue(5, this.onDraw);
        }

        private void FixedUpdate()
        {
            if (Time.timeSinceLevelLoad < 2.0f) // Check not loading level
            {
                return;
            }
            //Set the mode flag, 0 = inflight, 1 = editor, 2 on EVA or F2
            mode = Utilities.SetModeFlag();
            if (mode == GameState.EVA) return;

            if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor) // Only execute Update in Flight or Editor Scene
            {
                if ((FlightGlobals.ready && FlightGlobals.ActiveVessel != null) || (HighLogic.LoadedSceneIsEditor))
                {
                    this.Log_Debug("ampYearAYController  FixedUpdate mode == " + mode);
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
                    totalClimateParts = 0;
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

                            if (mode == GameState.FLIGHT)
                            {
                                float default_rot_power = pod.rotPower;
                                if (!podRotPowerMap.ContainsKey(name))
                                {
                                    //Map the part's default rot power to its name
                                    podRotPowerMap.Add(name, pod.rotPower);
                                }
                                else
                                    podRotPowerMap.TryGetValue(name, out default_rot_power);
                                commandPods.Add(pod);
                            }

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

                        if (current_part.CrewCapacity > 0)
                        {
                            crewablePartList.Add(current_part);
                            maxCrew += current_part.CrewCapacity;
                        }

                        bool has_alternator = false;

                        //loop through all the modules in the current part
                        foreach (PartModule module in current_part.Modules)
                        {
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
                                    this.Log_Debug("totalPowerProduced SolarPanel Power = " + tmpPower + " Part = " + current_part.name);
                                    totalPowerProduced += tmpPower;
                                    if (!hasPower && EmgcyShutActive)
                                    {
                                        if (FlightGlobals.ActiveVessel.atmDensity < 0.2 || FlightGlobals.ActiveVessel.situation != Vessel.Situations.SUB_ORBITAL
                                        || FlightGlobals.ActiveVessel.situation != Vessel.Situations.FLYING)
                                        {
                                            if (tmpSol.panelState == ModuleDeployableSolarPanel.panelStates.RETRACTED)
                                            {
                                                tmpSol.Extend();
                                                ScreenMessages.PostScreenMessage("Electricity Levels Critical! Extending Solar Panels!", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                                                this.Log("Extending solar array");
                                            }
                                        }
                                        else
                                            ScreenMessages.PostScreenMessage("Electricity Levels Critical! In Atmosphere can not Extend Solar Panels!", 5.0f, ScreenMessageStyle.UPPER_CENTER);
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
                                                this.Log_Debug("totalPowerProduced Generator Output Active Power = " + tmpPower + " Part = " + current_part.name);
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
                                            totalPowerDrain += tmpPower;
                                            PrtActive = true;
                                        }
                                        else
                                        {
                                            if (tmpGen.isAlwaysActive || tmpGen.generatorIsActive)
                                            {
                                                tmpPower = inp.rate;
                                                totalPowerDrain += inp.rate;
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
                                                ScreenMessages.PostScreenMessage("Electricity Levels Critical! Disabling Wheel Motors!", 5.0f, ScreenMessageStyle.UPPER_CENTER);
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
                                        ScreenMessages.PostScreenMessage("Electricity Levels Critical! Turning off Lights!", 5.0f, ScreenMessageStyle.UPPER_CENTER);
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
                                if (mode == GameState.FLIGHT)
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

                                    this.Log_Debug("totalPowerProduced ModEngine Active Power = " + alt_rate + " Part = " + current_part.name);
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
                                    this.Log_Debug("totalPowerProduced ModEngine Active Power = " + alt_rate + " Part = " + current_part.name);
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
                                            this.Log_Debug("totalPowerProduced ModAlt Active Power = " + r.rate + " Part = " + current_part.name);
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

                            if (module.moduleName == "ModuleResourceHarvester")
                            {
                                double tmpPwr = 0;
                                this.Log_Debug("Resource Harvester " + current_part.name);
                                    ModuleResourceHarvester tmpHvstr = (ModuleResourceHarvester)module;
                                    List<PartResourceDefinition> Rscse = tmpHvstr.GetConsumedResources();

                                    if (mode == GameState.FLIGHT)
                                    {
                                        PrtActive = tmpHvstr.ModuleIsActive();
                                        this.Log_Debug("Inflight andIsactive = " + PrtActive);
                                    }                                        
                                    if (mode == GameState.EDITOR)
                                    {
                                        PrtActive = true;
                                        this.Log_Debug("In VAB so part active");
                                    }

                                    foreach (PartResourceDefinition r in tmpHvstr.GetConsumedResources())
                                    {
                                        this.Log_Debug("Harvester resource = " + r.name + " cost = " + r.unitCost);                                        
                                        if (r.name == MAIN_POWER_NAME && PrtActive)
                                        {                                            
                                            //Appears to be NO way to get to the input resources.... set to 15 for current value in distro files
                                            //totalPowerDrain += r.unitCost;
                                            //tmpPwr += r.unitCost;
                                            totalPowerDrain += 15.0;
                                            tmpPwr += 15.0;
                                        }
                                    }
                                    tmpPower = (float)tmpPwr;
                                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                                    if (mode == GameState.FLIGHT)
                                        addPart(current_part.flightID, PartAdd, false);
                                    else
                                        addPart(current_part.craftID, PartAdd, false);                                 
                                

                                
                            }

                            if (module.moduleName == "ModuleResourceConverter")
                            {
                                
                                
                                ModuleResourceConverter tmpRegRC = (ModuleResourceConverter)module;
                                //PrtName = current_part.name + " " + tmpRegRC.ConverterName;
                                this.Log_Debug("Resource Converter " + PrtName);

                                if (mode == GameState.FLIGHT)
                                {
                                    PrtActive = tmpRegRC.ModuleIsActive();
                                    this.Log_Debug("Inflight andIsactive = " + PrtActive);
                                    this.Log_Debug("Info : " + tmpRegRC.GetInfo());
                                    this.Log_Debug("TakeAmount " + tmpRegRC.TakeAmount.ToString("00.00000"));
                                    this.Log_Debug("Status :" + tmpRegRC.status);
                                } 
                                if (mode == GameState.EDITOR)
                                {
                                    PrtActive = true;
                                    this.Log_Debug("In VAB so part active");
                                }

                                PrtPower = "";
                                tmpPower = 0f;
                                List<ResourceRatio> RecInputs = tmpRegRC.Recipe.Inputs;

                                foreach (ResourceRatio r in RecInputs)
                                {
                                    this.Log_Debug("Converter Input resource = " + r.ResourceName + " ratio = " + r.Ratio);
                                    if (r.ResourceName == MAIN_POWER_NAME && PrtActive)
                                    {                                        
                                        tmpPower = (float)r.Ratio;
                                        totalPowerDrain += r.Ratio;
                                        PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                                        if (mode == GameState.FLIGHT)
                                            addPart(current_part.flightID, PartAdd, false);
                                        else
                                            addPart(current_part.craftID, PartAdd, false);
                                    }
                                }

                                

                                PrtPower = "";
                                tmpPower = 0f;
                                List<ResourceRatio> RecOutputs = tmpRegRC.Recipe.Outputs;

                                foreach (ResourceRatio r in RecOutputs)
                                {
                                    this.Log_Debug("Converter Output resource = " + r.ResourceName + " ratio = " + r.Ratio);
                                    if (r.ResourceName == MAIN_POWER_NAME && PrtActive)
                                    {
                                        tmpPower = (float)r.Ratio;
                                        totalPowerProduced += r.Ratio;
                                        PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, tmpPower, PrtActive);
                                        if (mode == GameState.FLIGHT)
                                            addPart(current_part.flightID, PartAdd, true);
                                        else
                                            addPart(current_part.craftID, PartAdd, true);
                                    }
                                }         
                      
                                List<ResourceRatio> RecreqList = tmpRegRC.reqList;

                                foreach (ResourceRatio r in RecOutputs)
                                {
                                    this.Log_Debug("Converter reqList resource = " + r.ResourceName + " ratio= " + r.Ratio);
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
                                    //checkRego(module, current_part);
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

                            /*if (KKPresent)
                            {
                                checkKK(module, current_part);
                            }*/
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
                    } // end part loop

                    subsystemUpdate();
                } // end if active vessel not null
            } // End if Highlogic check
        }

        private void subsystemUpdate()
        {
            //This is the Logic that Executes in Flight
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

                if (KKPresent)
                {
                    KKAutopilotChk(cv);
                }
                else
                {
                    if ((cv.ActionGroups[KSPActionGroup.SAS] && !subsystemPowered(Subsystem.SAS)))
                    {
                        this.Log("SAS - disabled.");
                        //ScreenMessages.PostScreenMessage(cv.vesselName + " - SAS must be enabled through AmpYear first", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                        //Disable SAS when the subsystem isn't powered
                        cv.ActionGroups.SetGroup(KSPActionGroup.SAS, false);
                        reenableSAS = true;
                    }
                }

                if (managerIsActive && hasPower)
                {
                    //Re-enable SAS/RCS if they were shut off by the manager and can be run again
                    if (KKPresent)
                    {
                        KKSASChk();
                    }
                    else
                    {
                        if (reenableSAS)
                        {
                            setSubsystemEnabled(Subsystem.SAS, true);
                            reenableSAS = false;
                            this.Log("SAS - enabled.");
                        }
                    }

                    if (reenableRCS)
                    {
                        setSubsystemEnabled(Subsystem.RCS, true);
                        reenableRCS = false;
                        this.Log("RCS - enabled.");
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

                //Calculate total drain from subsystems
                double subsystem_drain = 0.0;
                subsystemDrain[(int)Subsystem.RCS] -= currentPoweredRCSDrain;
                foreach (Subsystem subsystem in Enum.GetValues(typeof(Subsystem)))
                {
                    subsystemDrain[(int)subsystem] = subsystemCurrentDrain(subsystem);
                    subsystem_drain += subsystemDrain[(int)subsystem];
                }

                double manager_drain = managerCurrentDrain;
                
                double total_manager_drain = subsystem_drain + manager_drain;
                totalPowerDrain += total_manager_drain;

                //Recharge reserve power if main power is above a certain threshold
                if (managerIsActive && (totalElectricCharge > 0) && (totalElectricCharge / totalElectricChargeCapacity > AYsettings.RECHARGE_RESERVE_THRESHOLD)
                    && (totalReservePower < totalReservePowerCapacity))
                    transferMainToReserve(RECHARGE_RESERVE_RATE * TimeWarp.fixedDeltaTime);

                //Drain main power
                Part cvp = FlightGlobals.ActiveVessel.rootPart;
                double currentTime = Planetarium.GetUniversalTime();
                double timestep_drain = total_manager_drain * TimeWarp.fixedDeltaTime;
                double minimum_sufficient_charge = managerActiveDrain + 4;
                double deltaTime = Math.Min(currentTime - timeLastElectricity, Math.Max(1, TimeWarp.fixedDeltaTime));
                double desiredElectricity = total_manager_drain * deltaTime;

                this.Log_Debug("Timewarp CurrentRate = " + TimeWarp.CurrentRate + " FixedDeltaTime = " + TimeWarp.fixedDeltaTime);
                this.Log_Debug("totalelectriccharge = " + totalElectricCharge + " total_manager_drain = " + total_manager_drain);
                this.Log_Debug("totalPowerDrain = " + totalPowerDrain + " totalPowerProduced = " + totalPowerProduced);
                this.Log_Debug("timestep_drain = " + timestep_drain + "deltaTime = " + deltaTime + " desiredElectricity = " + desiredElectricity);
                this.Log_Debug("currentTime = " + currentTime + " timeLastElectricity = " + timeLastElectricity);
                this.Log_Debug("timewarp CurrentRateIndex = " + TimeWarp.CurrentRateIndex);
                this.Log_Debug("hasReservePower = " + hasReservePower);
                this.Log_Debug("hasPower = " + hasPower);

                if (desiredElectricity > 0.0 && timewarpIsValid) // if power required > 0 and time warp is valid
                {
                    //if (totalElectricCharge >= minimum_sufficient_charge)
                    if (totalElectricCharge >= desiredElectricity) // if main power >= power required
                    {
                        this.Log_Debug("drawing main power");
                        double totalElecreceived = requestResource(cvp, MAIN_POWER_NAME, desiredElectricity);  //get power
                        timeLastElectricity = currentTime - ((desiredElectricity - totalElecreceived) / total_manager_drain); //set time last power received
                        hasPower = (UnityEngine.Time.realtimeSinceStartup > powerUpTime)
                        && (desiredElectricity <= 0.0 || totalElecreceived >= (desiredElectricity * 0.99)); //set hasPower > power up delay and we received power requested
                        this.Log_Debug("reatime = " + UnityEngine.Time.realtimeSinceStartup + " poweruptime = " + powerUpTime);
                        this.Log_Debug("desiredelec = " + desiredElectricity + " totalElecreceived = " + totalElecreceived);
                        this.Log_Debug("hasPower = " + hasPower);

                        //hasPower = (UnityEngine.Time.realtimeSinceStartup > powerUpTime)
                        // && (timestep_drain <= 0.0 || requestResource(cvp, MAIN_POWER_NAME, timestep_drain) >= (timestep_drain * 0.99));
                    }
                    else //not enough main power try reserve power
                    {
                        if (TimeWarp.CurrentRateIndex > 3)
                        {
                            TimeWarp.SetRate(0, false);
                            ScreenMessages.PostScreenMessage(FlightGlobals.ActiveVessel.vesselName + " - Not Enough Power to run TimeWarp - Deactivated.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                        }
                        hasPower = totalElectricCharge >= minimum_sufficient_charge; //set hasPower
                        this.Log_Debug("drawing reserve power");
                        if (totalReservePower > minimum_sufficient_charge) // if reserve power available > minimum charge required
                        {
                            //If main power is insufficient, drain reserve power for manager
                            //EmergencyPowerDown(); //***** perform emergency shutdown procedure here
                            double manager_timestep_drain = manager_drain * TimeWarp.fixedDeltaTime; //we only drain manager function
                            double deltaTime2 = Math.Min(currentTime - timeLastElectricity, Math.Max(1, TimeWarp.fixedDeltaTime));
                            double desiredElectricity2 = manager_drain * deltaTime;
                            double totalElecreceived2 = requestResource(cvp, RESERVE_POWER_NAME, desiredElectricity2); // get power
                            timeLastElectricity = currentTime - ((desiredElectricity2 - totalElecreceived2) / manager_drain); // set time last power received
                            hasReservePower = (UnityEngine.Time.realtimeSinceStartup > powerUpTime)
                            && (desiredElectricity2 <= 0.0 || totalElecreceived2 >= (desiredElectricity2 * 0.99)); // set hasReservePower > power up delay and we received power

                            //hasReservePower = manager_drain <= 0.0
                            //    || requestResource(cvp, RESERVE_POWER_NAME, manager_timestep_drain) >= (manager_timestep_drain * 0.99);
                        }
                        else  // not enough reservepower
                        {
                            this.Log_Debug("not enough reserve power");
                            hasReservePower = totalReservePower > minimum_sufficient_charge; //set hasReservePower
                            //timeLastElectricity += UnityEngine.Random.Range(60, 600);
                            timeLastElectricity += currentTime - lastUpdate; //set time we last received electricity to current time - last update
                        }                        
                    }
                }
                else  // no electricity required OR time warp is too high (so we hibernate)
                {
                    this.Log_Debug("Timewarp not valid or elec < 0");
                    timeLastElectricity += currentTime - lastUpdate;
                }

                if (!hasPower) //some processing if we are out of power
                {
                    this.Log_Debug("no main power processing");
                    if (EmgcyShutActive) //emergency shutdown activate
                    {
                        this.Log_Debug("emergency shutdown");
                        EmergencyPowerDown();
                    }
                    if (UnityEngine.Time.realtimeSinceStartup > powerUpTime) //reset the power up delay - for powering back up to avoid rapid flickering of the system
                    {
                        this.Log_Debug("reset power up delay");
                        powerUpTime = UnityEngine.Time.realtimeSinceStartup + POWER_UP_DELAY;
                        this.Log_Debug("powerup time = " + powerUpTime);
                    }
                    hasReservePower = totalReservePower > minimum_sufficient_charge; //set hasReservePower
                    hasPower = totalElectricCharge >= minimum_sufficient_charge; //set hasPower
                    this.Log_Debug("hasReservePower = " + hasReservePower);
                    this.Log_Debug("hasPower = " + hasPower);
                }                                                     
                lastUpdate = currentTime;
            }

            //This is the Logic that Executes in the Editor (VAB/SPH)
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
                string PrtName = "AmpYear&Subsystems-Max";
                string PrtPower = "";
                PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower, (float)total_manager_drain, true);
                addPart(1111, PartAdd, false);
                hasPower = true;
                hasReservePower = true;
            }
        }

        #region OtherMods

        
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
        /*
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
        */
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

        #endregion OtherMods

        //Current Vessel Parts list maintenance - used primarily in editor for parts list, also used for emergency shutdown procedure

        private void addPart(uint Pkey, PwrPartList PartAdd, bool ProdPrt)
        {
            PwrPartList PartFnd;
            if (ProdPrt) // Producer part list
            {
                if (vesselProdPartsList.TryGetValue(Pkey, out PartFnd))
                {
                    PartAdd.PrtPowerF += PartFnd.PrtPowerF;
                    PartAdd.PrtPower = PartAdd.PrtPowerF.ToString("00.000");
                    vesselProdPartsList[Pkey] = PartAdd;
                }
                else
                {
                    PartAdd.PrtPower = PartAdd.PrtPowerF.ToString("00.000");
                    vesselProdPartsList.Add(Pkey, PartAdd);
                }
            }
            else // consumer part list
            {
                if (vesselConsPartsList.TryGetValue(Pkey, out PartFnd))
                {
                    PartAdd.PrtPowerF += PartFnd.PrtPowerF;
                    PartAdd.PrtPower = PartAdd.PrtPowerF.ToString("00.000");
                    vesselConsPartsList[Pkey] = PartAdd;
                }
                else
                {
                    PartAdd.PrtPower = PartAdd.PrtPowerF.ToString("00.000");
                    vesselConsPartsList.Add(Pkey, PartAdd);
                }
            }
        }

        #region VesselFunctions

        //Vessel Functions Follow - to store list of vessels and store/retrieve AmpYear settings for each vessel

        private void CheckVslUpdate()
        {
            // Called every fixed update from fixedupdate - Check for vessels that have been deleted and remove from Dictionary
            // also updates current active vessel details/settings
            // adds new vessel if current active vessel is not known and updates it's details/settings
            double currentTime = Planetarium.GetUniversalTime();
            List<Vessel> allVessels = FlightGlobals.Vessels;
            var vesselsToDelete = new List<Guid>();
            this.Log_Debug("AYController CheckVslUpdate");
            this.Log_Debug("AYController allvessels count = " + allVessels.Count);
            this.Log_Debug("AYController knownvessels count = " + AYgameSettings.knownVessels.Count);
            //* Delete vessels that do not exist any more or have no crew
            foreach (var entry in AYgameSettings.knownVessels)
            {
                this.Log_Debug("AYController knownvessels id = " + entry.Key + " Name = " + entry.Value.vesselName);
                Guid vesselId = entry.Key;
                VesselInfo vesselInfo = new VesselInfo(entry.Value.vesselName, currentTime);
                vesselInfo = entry.Value;
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
            }
            vesselsToDelete.ForEach(id => AYgameSettings.knownVessels.Remove(id));

            //* Add all new vessels
            foreach (Vessel vessel in allVessels.Where(v => v.loaded))
            {
                Guid VesselID = vessel.id;
                this.Log_Debug("Add new vessels check? " + VesselID + " " + vessel.vesselType);
                if (!AYgameSettings.knownVessels.ContainsKey(VesselID) && ValidVslType(vessel))
                {
                    if (vessel.FindPartModulesImplementing<ModuleCommand>().First() != null)
                    {
                        this.Log_Debug("New vessel: " + vessel.vesselName + " (" + vessel.id + ")");
                        VesselInfo vesselInfo = new VesselInfo(vessel.vesselName, currentTime);
                        vesselInfo.vesselType = vessel.vesselType;
                        UpdateVesselInfo(vesselInfo);
                        int crewCapacity = UpdateVesselCounts(vesselInfo, vessel);
                        AYgameSettings.knownVessels.Add(VesselID, vesselInfo);
                    }
                }
            }

            //*Update the current vessel
            VesselInfo currvesselInfo = new VesselInfo(FlightGlobals.ActiveVessel.vesselName, currentTime);
            if (AYgameSettings.knownVessels.TryGetValue(currentvesselid, out currvesselInfo))
            {
                this.Log_Debug("updating current vessel");
                UpdateVesselInfo(currvesselInfo);
                int crewCapacity = UpdateVesselCounts(currvesselInfo, FlightGlobals.ActiveVessel);
                currvesselInfo.vesselType = FlightGlobals.ActiveVessel.vesselType;
                AYgameSettings.knownVessels[currentvesselid] = currvesselInfo;
            }

            this.Log_Debug("AYController CheckVslUpdate complete");
        }

        private void UpdateVesselInfo(VesselInfo vesselInfo)
        {
            // save current toggles to current vesselinfo
            this.Log_Debug("AYController UpdateVesselInfo " + vesselInfo.vesselName);
            vesselInfo.managerEnabled = managerEnabled;
            vesselInfo.ShowCrew = ShowCrew;
            vesselInfo.ShowParts = ShowParts;
            vesselInfo.timeLastElectricity = timeLastElectricity;
            vesselInfo.lastUpdate = lastUpdate;
            for (int i = 0; i < Enum.GetValues(typeof(Subsystem)).Length; i++)
            {
                vesselInfo.subsystemToggle[i] = subsystemToggle[i];
                vesselInfo.subsystemDrain[i] = subsystemDrain[i];
            }
            for (int i = 0; i < Enum.GetValues(typeof(GUISection)).Length; i++)
                vesselInfo.guiSectionEnableFlag[i] = guiSectionEnableFlag[i];
            vesselInfo.EmgcyShutActive = EmgcyShutActive;
            if (KKPresent)
            {
                KKUpdateVslInfo(vesselInfo);
            }
            else
            {
                vesselInfo.AutoPilotDisabled = false;
                vesselInfo.AutoPilotDisCounter = 0;
                vesselInfo.AutoPilotDisTime = 0;
            }
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

        private void onVesselLoad(Vessel newvessel)
        {
            this.Log_Debug("AYController onVesselLoad " + newvessel.name + " (" + newvessel.id + ")");
            if (newvessel.id != FlightGlobals.ActiveVessel.id)
            {
                this.Log_Debug("AYController newvessel is not active vessel");
                return;
            }
            // otherwise we load the vessel settings
            currentvesselid = newvessel.id;
            loadVesselSettings(newvessel);
        }

        private void onVesselChange(Vessel newvessel)
        {
            this.Log_Debug("AYController onVesselChange New " + newvessel.name + " (" + newvessel.id + ") Old " + " (" + currentvesselid + ")");
            this.Log_Debug("AYController active vessel " + FlightGlobals.ActiveVessel.id);
            if (currentvesselid == newvessel.id) // which would be the case if it's an EVA kerbal re-joining ship
                return;
            double currentTime = Planetarium.GetUniversalTime();
            if (KKPresent)
            {
                KKonVslChng();
            }

            // Update Old Vessel settings into Dictionary
            VesselInfo oldvslinfo = new VesselInfo(newvessel.name, currentTime);
            if (AYgameSettings.knownVessels.TryGetValue(currentvesselid, out oldvslinfo))
            {
                UpdateVesselInfo(oldvslinfo);
                AYgameSettings.knownVessels[currentvesselid] = oldvslinfo;
                this.Log_Debug("Updated old vessel " + AYgameSettings.knownVessels[currentvesselid].vesselName + "(" + currentvesselid + ")");
                DbgListVesselInfo(oldvslinfo);
            }
            // load the settings for the newvessel
            currentvesselid = newvessel.id;
            loadVesselSettings(newvessel);
        }

        private void onCrewBoardVessel(GameEvents.FromToAction<Part, Part> action)
        {
            this.Log_Debug("AYController onCrewBoardVessel " + action.to.vessel.name + " (" + action.to.vessel.id + ") Old " + action.from.vessel.name + " (" + action.from.vessel.id + ")");
            this.Log_Debug("AYController active vessel " + FlightGlobals.ActiveVessel.id);
            this.Log_Debug("newvessel UniqueID = " + action.to.vessel.id);
            this.Log_Debug("oldvessel UniqueID = " + action.from.vessel.id);
            currentvesselid = action.to.vessel.id;
            loadVesselSettings(action.to.vessel);
        }

        private void loadVesselSettings(Vessel newvessel)
        {
            double currentTime = Planetarium.GetUniversalTime();
            VesselInfo info = new VesselInfo(newvessel.name, currentTime);
            // Load New Vessel settings from Dictionary
            if (AYgameSettings.knownVessels.TryGetValue(newvessel.id, out info))
            {
                this.Log_Debug("AYController Vessel Loading Settings " + newvessel.name + " (" + newvessel.id + ")");
                for (int i = 0; i < Enum.GetValues(typeof(Subsystem)).Length; i++)
                {
                    subsystemToggle[i] = info.subsystemToggle[i];
                    subsystemDrain[i] = info.subsystemDrain[i];
                }                    
                for (int i = 0; i < Enum.GetValues(typeof(GUISection)).Length; i++)
                    guiSectionEnableFlag[i] = info.guiSectionEnableFlag[i];
                managerEnabled = info.managerEnabled;
                ShowCrew = info.ShowCrew;
                EmgcyShutActive = info.EmgcyShutActive;
                timeLastElectricity = info.timeLastElectricity;
                lastUpdate = info.lastUpdate;
                
                if (KKPresent)
                {
                    KKLoadVesselSettings(info, false);
                }
                DbgListVesselInfo(info);
            }
            else //New Vessel not found in Dictionary so set default
            {
                this.Log_Debug("AYController Vessel Setting Default Settings");
                for (int i = 0; i < Enum.GetValues(typeof(Subsystem)).Length; i++)
                {
                    subsystemToggle[i] = false;
                    subsystemDrain[i] = 0.0;
                }
                    
                for (int i = 0; i < Enum.GetValues(typeof(GUISection)).Length; i++)
                    guiSectionEnableFlag[i] = false;
                managerEnabled = true;
                ShowCrew = false;
                EmgcyShutActive = false;
                timeLastElectricity = 0f;
                if (KKPresent)
                {
                    KKLoadVesselSettings(info, true);
                }
            }
            if (!KKPresent) //KabinKraziness not present turn off settings for KabinKraziness on vessel
            {
                subsystemToggle[3] = false;
                subsystemToggle[4] = false;
                subsystemToggle[5] = false;
                subsystemDrain[3] = 0.0;
                subsystemDrain[4] = 0.0;
                subsystemDrain[5] = 0.0;
                guiSectionEnableFlag[2] = false;                   
            }
        }

        private void DbgListVesselInfo(VesselInfo info)
        {
            this.Log_Debug("VesselInfo " + info.vesselName);
            this.Log_Debug("ManagerEnabled = " + info.managerEnabled + " ShowCrew = " + info.ShowCrew + " ShowParts = " + info.ShowParts);
            this.Log_Debug("subsystemToggle Power Turn     = " + info.subsystemToggle[0]);
            this.Log_Debug("subsystemToggle SAS            = " + info.subsystemToggle[1]);
            this.Log_Debug("subsystemToggle RCS            = " + info.subsystemToggle[2]);
            this.Log_Debug("subsystemToggle ClimateCtl     = " + info.subsystemToggle[3]);
            this.Log_Debug("subsystemToggle Music          = " + info.subsystemToggle[4]);
            this.Log_Debug("subsystemToggle Massage        = " + info.subsystemToggle[5]);
            this.Log_Debug("guiSectionEnableFlag Subsystem = " + info.guiSectionEnableFlag[0]);
            this.Log_Debug("guiSectionEnableFlag Reserve   = " + info.guiSectionEnableFlag[1]);
            this.Log_Debug("guiSectionEnableFlag Luxury    = " + info.guiSectionEnableFlag[2]);
            this.Log_Debug("timeLastElectricity = " + info.timeLastElectricity + " lastUpdate = " + info.lastUpdate);
            this.Log_Debug("EmgcyShutActive = " + info.EmgcyShutActive + " AutoPilotDisabled = " + info.AutoPilotDisabled + " AutoPilotDisCounter = " + info.AutoPilotDisCounter);
            this.Log_Debug("AutoPilotDisTime = " + info.AutoPilotDisTime + " numCrew = " + info.numCrew + " numOccupiedParts = " + info.numOccupiedParts);
        }

        private bool ValidVslType(Vessel v)
        {
            switch (v.vesselType)
            {
                case VesselType.Base:
                case VesselType.Lander:
                case VesselType.Probe:
                case VesselType.Rover:
                case VesselType.Ship:
                case VesselType.Station:
                    return true;

                default:
                    return false;
            }
        }

        #endregion VesselFunctions

        #region SubSystemFunctions

        //Subsystem Functions Follow

        private bool timewarpIsValid
        {
            get
            {
                return TimeWarp.CurrentRateIndex < 7;
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
                //return managerEnabled && (hasPower || hasReservePower);
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
                    return "Turn Booster";

                case Subsystem.SAS:
                    return "SAS";

                case Subsystem.RCS:
                    return "RCS";

                case Subsystem.CLIMATE:
                    return "Climate Control";

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
                        ScreenMessages.PostScreenMessage(cv.vesselName + " - Cannot Engage SAS - Autopilot function not available", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                    break;

                case Subsystem.RCS:
                    cv.ActionGroups.SetGroup(KSPActionGroup.RCS, enabled);
                    break;

                default:
                    subsystemToggle[(int)subsystem] = enabled;
                    break;
            }
        }

        public void EmergencyPowerDown()
        {
            ScreenMessages.PostScreenMessage(FlightGlobals.ActiveVessel.vesselName + " - Emergency Power Procedures Activated. Shutdown Subsystems.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
            setSubsystemEnabled(Subsystem.CLIMATE, false);
            setSubsystemEnabled(Subsystem.MASSAGE, false);
            setSubsystemEnabled(Subsystem.MUSIC, false);
            setSubsystemEnabled(Subsystem.POWER_TURN, false);
            setSubsystemEnabled(Subsystem.RCS, false);
            setSubsystemEnabled(Subsystem.SAS, false);
            TimeWarp.SetRate(0, false);
            reenableRCS = true;
            reenableSAS = true;
        }

        public bool subsystemActive(Subsystem subsystem)
        {
            if (!subsystemEnabled(subsystem))
                return false;

            switch (subsystem)
            {
                //case Subsystem.CLIMATE:
                //    return totalClimateParts < crewablePartList.Count;

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

                case Subsystem.CLIMATE:
                    if (KKPresent)
                    {
                        double clmtrate = KKClimateActDrain();
                        return clmtrate;
                    }
                    else
                        return 0.0;

                case Subsystem.MUSIC:
                    return 1.0 * crewablePartList.Count;

                case Subsystem.MASSAGE:
                    if (KKPresent)
                    {
                        double msgrate = KKMassActDrain();
                        return msgrate;
                    }
                    else
                        return 0.0;

                default:
                    return 0.0;
            }
        }

        public bool subsystemPowered(Subsystem subsystem)
        {
            return hasPower && managerIsActive && subsystemActive(subsystem);
        }

        #endregion SubSystemFunctions

        #region ResourceFunctions

        //Resources Functions Follow

        private double requestResource(Part cvp, String name, double amount)
        {
            if (amount <= 0.0)
                return 0.0;
            double total_received = 0.0;
            double request_amount = amount;
            for (int attempts = 0; ((attempts < MAX_TRANSFER_ATTEMPTS) && (amount > 0.000000000001)); attempts++)
            {
                double received = cvp.RequestResource(name, request_amount, ResourceFlowMode.ALL_VESSEL);
                this.Log_Debug("requestResource attempt " + attempts);
                this.Log_Debug("requested power = " + request_amount.ToString("0.0000000000000000000000"));
                this.Log_Debug("received power = " + received.ToString("0.0000000000000000000000"));
                total_received += received;
                amount -= received;
                this.Log_Debug("amount = " + amount.ToString("0.0000000000000000000000"));
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

        #endregion ResourceFunctions

        #region GUIFunctions

        //GUI Functions Follow

        private void onDraw()
        {
            if (!GuiVisible) return;

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
                    if (!Utilities.WindowVisibile(FwindowPos)) Utilities.MakeWindowVisible(FwindowPos);
                    FwindowPos = GUILayout.Window(FwindowID, FwindowPos, windowF, "AmpYear Power Manager", GUILayout.Width(FWINDOW_WIDTH), GUILayout.Height(WINDOW_BASE_HEIGHT));

                    CheckPowerLowWarning();
                    if (WarnWinOn)
                    {
                        WarnWinPos = GUILayout.Window(WwindowID, WarnWinPos, stopAndWarn, "WARNING!", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    }
                }
            }

            if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                GUI.skin = HighLogic.Skin;
                if (!Utilities.WindowVisibile(EwindowPos)) Utilities.MakeWindowVisible(EwindowPos);
                EwindowPos = GUILayout.Window(EwindowID, EwindowPos, windowE, "AmpYear Power Manager", GUILayout.Width(EWINDOW_WIDTH), GUILayout.Height(WINDOW_BASE_HEIGHT));
                if (ShowParts) ScrollParts();
            }
        }

        private void CheckPowerLowWarning()
        {
            double chrgpct = ((totalElectricCharge / totalElectricChargeCapacity) * 100);
            if (ShowWarn && !WarnWinOn) // If Warning is on check if it's triggered
            {
                if ((TimeWarp.CurrentRate > 0) && (chrgpct < AYsettings.POWER_LOW_WARNING_AMT)) // We have hit the warning stop timewarp and show warning
                {
                    this.Log_Debug("cutting timewarp power warning limit reached");
                    TimeWarp.SetRate(0, false);
                    ShowWarn = false;
                    WarnWinOn = true;
                }
            }
            if ((chrgpct > AYsettings.POWER_LOW_WARNING_AMT) && !ShowWarn) // Reset the Warning indicator
            {
                this.Log_Debug("Reset power warning");
                ShowWarn = true;
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
                if (section == GUISection.LUXURY)
                {
                    if (KKPresent)
                    {
                        guiSectionEnableFlag[(int)section]
                        = GUILayout.Toggle(guiSectionEnableFlag[(int)section], guiSectionName(section), GUI.skin.button);
                    }
                    else
                    {
                        guiSectionEnableFlag[(int)section] = false;
                    }
                }
                else
                {
                    guiSectionEnableFlag[(int)section]
                    = GUILayout.Toggle(guiSectionEnableFlag[(int)section], guiSectionName(section), GUI.skin.button);
                }
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
                else
                {
                    if (timewarpIsValid)
                    {
                        GUILayout.Label("Manager Disabled", warningStyle);
                    }
                    else
                    {
                        GUILayout.Label("Auto-Hibernation", statusStyle);
                    }
                }   
            }
            else
                GUILayout.Label("Insufficient Power", warningStyle);

            //if (AYsettings.Craziness_Function)
            if (KKPresent)
            {
                KKKrazyWrngs();
            }

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
                    GUILayout.BeginHorizontal();
                    ShowCrew = GUILayout.Toggle(ShowCrew, "ShowCrew", subsystemButtonStyle, subsystemButtonOptions);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EmgcyShutActive = GUILayout.Toggle(EmgcyShutActive, "EmergencyProc.Active", subsystemButtonStyle, subsystemButtonOptions);
                    GUILayout.EndHorizontal();
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
                        GUILayout.Label(CrewMbr.name + " - " + CrewMbr.experienceTrait.Title, statusStyle);
                    }
                }
                else //if (timewarpIsValid)
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
            GUILayout.Label("Power Capacity: " + totalElectricChargeCapacity.ToString("0.00"), statusStyle);
            if (totalPowerDrain > totalPowerProduced)
                GUILayout.Label("Power Drain : " + totalPowerDrain.ToString("0.00"), warningStyle);
            else
                GUILayout.Label("Power Drain : " + totalPowerDrain.ToString("0.00"), statusStyle);
            if (totalPowerProduced > 0)
                GUILayout.Label("Power Prod : " + totalPowerProduced.ToString("0.00"), statusStyle);
            else
                GUILayout.Label("Power Prod : " + totalPowerProduced.ToString("0.00"), warningStyle);

            //Reserve
            GUILayout.Label("Reserve Power", sectionTitleStyle);
            //Reserve status label
            GUILayout.Label("Reserve Power Capacity: " + totalReservePowerCapacity.ToString("0.00"), statusStyle);
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void ScrollParts()
        {
            //GUI.skin = HighLogic.Skin;

            PartListStyle = new GUIStyle(GUI.skin.label);
            PartListStyle.alignment = TextAnchor.LowerLeft;
            PartListStyle.stretchWidth = false;
            PartListStyle.normal.textColor = Color.yellow;

            PartListPartStyle = new GUIStyle(GUI.skin.label);
            PartListPartStyle.alignment = TextAnchor.LowerLeft;
            PartListPartStyle.stretchWidth = false;
            PartListPartStyle.normal.textColor = Color.white;

            Rect EPLwindowPos = new Rect(EwindowPos.x + EWINDOW_WIDTH + 10, EwindowPos.y, AYController.EWINDOW_WIDTH + 30, Screen.height / 2 - 100);
            // Begin the ScrollView
            scrollViewVector = GUI.BeginScrollView(EPLwindowPos, scrollViewVector, new Rect(0, 0, EWINDOW_WIDTH + 100, 1700));
            // Put something inside the ScrollView
            GUILayout.Label("Power Production Parts", PartListStyle);
            if (vesselProdPartsList.Count == 0)
                GUILayout.Label("No Power Producing Parts", PartListPartStyle);
            foreach (var entry in vesselProdPartsList)
            {
                GUILayout.Label(entry.Value.PrtName + " " + entry.Value.PrtPower, PartListPartStyle);
            }
            GUILayout.Label("Power Consumer Parts", PartListStyle);
            if (vesselConsPartsList.Count == 0)
                GUILayout.Label("No Power Consuming Parts", PartListPartStyle);
            foreach (var entry in vesselConsPartsList)
            {
                GUILayout.Label(entry.Value.PrtName + " " + entry.Value.PrtPower, PartListPartStyle);
            }
            // End the ScrollView
            GUI.EndScrollView();
        }

        private void stopAndWarn(int id)
        {
            GUIStyle WarnStyle = new GUIStyle(GUI.skin.label);
            WarnStyle.fontStyle = FontStyle.Bold;
            WarnStyle.alignment = TextAnchor.UpperLeft;
            WarnStyle.normal.textColor = Color.white;

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Ship Electric charge has dropped below the Warp Warning Percentage.", WarnStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("This will not trigger again until Electric charge > Warning Percentage again.", WarnStyle);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            if (GUILayout.Button("OK"))
            {
                this.Log_Debug("ElectricCharge Warning Threshold Triggered Alert Msg");
                WarnWinOn = false;
            }
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
                    if (currentPoweredRCSDrain > 0.001)
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
                case Subsystem.CLIMATE:
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

        #region Savable

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

        #endregion Savable

        #region KabinKrazinessInterfaces

        private double KKClimateActDrain()
        {
            //KabinKraziness Interface to Calculate the Climate Control Electrical Drain amount
            KabinKraziness.Ikkaddon _KK = KKClient.GetKK();
            if (mode == GameState.FLIGHT)
                return CLIMATE_HEAT_RATE
                       * (crewablePartList.Count * _KK.CLMT_BSE_DRN_FTR + CLIMATE_CAPACITY_DRAIN_FACTOR * FlightGlobals.ActiveVessel.GetCrewCapacity());
            else return CLIMATE_HEAT_RATE
                       * (crewablePartList.Count * _KK.CLMT_BSE_DRN_FTR + CLIMATE_CAPACITY_DRAIN_FACTOR);
        }

        private double KKMassActDrain()
        {
            //KabinKraziness Interface to Calculate the Massage Chairs Electrical Drain amount
            KabinKraziness.Ikkaddon _KK = KKClient.GetKK();
            if (mode == GameState.FLIGHT)
                return _KK.MSG_BSE_DRN_FTR * FlightGlobals.ActiveVessel.GetCrewCount();
            else return _KK.MSG_BSE_DRN_FTR;
        }

        private void KKAutopilotChk(Vessel vessel)
        {
            //KabinKraziness Interface to check if the crew have Disabled the SAS
            KabinKraziness.Ikkaddon _KK = KKClient.GetKK();
            if ((vessel.ActionGroups[KSPActionGroup.SAS] && !subsystemPowered(Subsystem.SAS)) || _KK.AutoPilotDisabled)
            {
                this.Log("KKSASChk SAS - disabled.");
                //ScreenMessages.PostScreenMessage(cv.vesselName + " - SAS must be enabled through AmpYear first", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                //Disable SAS when the subsystem isn't powered
                vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, false);
                reenableSAS = true;
            }
        }

        private void KKSASChk()
        {
            //KabinKraziness Interface to check if the crew haven't disabled the SAS before re-enabling it
            KabinKraziness.Ikkaddon _KK = KKClient.GetKK();
            if (reenableSAS && !_KK.AutoPilotDisabled)
            {
                this.Log("KKSASChk SAS - enabled.");
                setSubsystemEnabled(Subsystem.SAS, true);
                reenableSAS = false;
            }
        }

        private void KKLoadVesselSettings(VesselInfo info, bool isnew)
        {
            //KabinKraziness Interface to Load Vessel values for KabinKraziness, as they are stored in the vessel settings for AmpYear rather than separately.
            KabinKraziness.Ikkaddon _KK = KKClient.GetKK();
            if (isnew)
            {
                _KK.AutoPilotDisabled = false;
                _KK.AutoPilotDisTime = 0f;
                _KK.AutoPilotDisCounter = 0f;
            }
            else
            {
                _KK.AutoPilotDisabled = info.AutoPilotDisabled;
                _KK.AutoPilotDisCounter = info.AutoPilotDisCounter;
                _KK.AutoPilotDisTime = info.AutoPilotDisTime;
            }
        }

        private void KKonVslChng()
        {
            //KabinKraziness Interface on Vessel change, to remove autopilot disabled countdown if it is active
            KabinKraziness.Ikkaddon _KK = KKClient.GetKK();
            if (_KK.AutoPilotDisabled) ScreenMessages.RemoveMessage(_KK.AutoTimer);
        }

        private void KKUpdateVslInfo(VesselInfo vesselInfo)
        {
            //KabinKraziness Interface to Save Vessel values for KabinKraziness, as they are stored in the vessel settings for AmpYear rather than separately.
            KabinKraziness.Ikkaddon _KK = KKClient.GetKK();
            vesselInfo.AutoPilotDisabled = _KK.AutoPilotDisabled;
            vesselInfo.AutoPilotDisCounter = _KK.AutoPilotDisCounter;
            vesselInfo.AutoPilotDisTime = _KK.AutoPilotDisTime;
        }

        private void KKKrazyWrngs()
        {
            //KabinKraziness Interface to display Kraziness Warnings in the AmpYear GUI
            KabinKraziness.Ikkaddon _KK = KKClient.GetKK();
            if (_KK.FirstMajCrazyWarning)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Craziness Major Alert!", warningStyle);
                GUILayout.EndHorizontal();
            }
            else if (_KK.FirstMinCrazyWarning)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Craziness Minor Alert!", sectionTitleStyle);
                GUILayout.EndHorizontal();
            }
        }

        #endregion KabinKrazinessInterfaces
    }
}