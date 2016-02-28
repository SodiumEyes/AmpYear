/**
 * AYMenu.cs
 *  
 * AmpYear power management. 
 * (C) Copyright 2015, Jamie Leighton
 * 
 * This code is based on the original AmpYear module by :- SodiumEyes
 * and parts of Thunder Aerospace Corporation's library for the Kerbal Space Program, by Taranis Elsu * 
 * (C) Copyright 2013, Taranis Elsu 
 * 
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 * 
 * This code is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
 * creative commons license. See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>
 * for full details.
 * 
 * Attribution — You are free to modify this code, so long as you mention that the resulting
 * work is based upon or adapted from this code.
 * 
 * Non-commercial - You may not use this work for commercial purposes.
 * 
 * Share Alike — If you alter, transform, or build upon this work, you may distribute the
 * resulting work only under the same or similar license to the CC BY-NC-SA 3.0 license.
 *  
 */
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;
using System.Runtime.Serialization;

namespace AY
{

   
    public class AYMenu : MonoBehaviour, Savable
    {
        public enum Subsystem
        {            
            POWER_TURN,
            SAS,            
            RCS,
            HEATER,
            COOLER,
            MUSIC,
            MASSAGE
        }

        public enum GUISection
        {
            SUBSYSTEM,
            RESERVE,
            LUXURY
        }


        public class PwrPartList
        {
            public string PrtName { get; set; }
            public string PrtPower { get; set; }

            public PwrPartList(string prtName, string prtPower)
            {
                PrtName = prtName;
                PrtPower = prtPower;                
            }

            public string Serialize()
            {
                return "PrtName=" + PrtName + ",PrtPower=" + PrtPower;
            }

            public static PwrPartList DeserializeResource(string str)
            {
                var arr = str.Split(',');
                var prtName = arr[0].Split('=')[1];
                var prtPower =prtName;
                if (arr.Length == 2)
                {
                    prtPower = arr[1].Split('=')[1];
                }
                return new PwrPartList (prtName,prtPower);
            }
        }
            
        
        //GUI Properties
        private IButton button1; 
        private readonly double[] RESERVE_TRANSFER_INCREMENTS = new double[3] { 0.25, 0.1, 0.01 };
        private bool[] guiSectionEnableFlag = new bool[Enum.GetValues(typeof(GUISection)).Length];
        private const float FWINDOW_WIDTH = 200;
        private const float EWINDOW_WIDTH = 200;
        private const float SCWINDOW_WIDTH = 400;
        private const float WINDOW_BASE_HEIGHT = 140;
        private Vector2 scrollViewVector = Vector2.zero;
        private GUIStyle sectionTitleStyle, subsystemButtonStyle, subsystemConsumptionStyle, statusStyle, warningStyle, powerSinkStyle;
        public GUILayoutOption[] subsystemButtonOptions;              
        private static int windowID = new System.Random().Next();
        private static int mode = -1;  // Display mode, currently  0 for In-Flight, 1 for Editor, -1 to hide   
        private float currThrottle = 1.0F;               
        private static bool[] subsystemToggle = new bool [Enum.GetValues(typeof(Subsystem)).Length];
		private static double[] subsystemDrain = new double [Enum.GetValues(typeof(Subsystem)).Length];
        private bool managerEnabled = true;
        private bool ShowCrew = false;
        private bool ShowParts = false;
		private float powerUpTime = 0.0f;
        private static float sumDeltaTime = 0f;
        private static bool doOneUpdate = false;
        private static bool debugging = false;
        public Rect FwindowPos = new Rect(40, Screen.height / 2 - 100, AYMenu.FWINDOW_WIDTH, 200);
        public Rect EwindowPos = new Rect(40, Screen.height / 2 - 100, AYMenu.EWINDOW_WIDTH, 200);
        public Rect SCwindowPos = new Rect(40, Screen.height / 2 - 100, AYMenu.SCWINDOW_WIDTH, 200);
                
        //AmpYear Properties
		public static List<Part> crewablePartList = new List<Part>();
        public static List<PwrPartList> PowerProdList = new List<PwrPartList>();
        public static List<PwrPartList> PowerConsList = new List<PwrPartList>();
		public static List<CommandPod> commandPods = new List<CommandPod>();
        public static List<ProtoCrewMember> VslRstr = new List<ProtoCrewMember>();       		
		public static float sasTorque = 0.0f;
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
        private bool KarPresent = false;
        private bool BDSMPresent = false;
        private bool reenableRCS = false;
        private bool reenableSAS = false;		
        public Dictionary<String, float> podRotPowerMap = new Dictionary<string, float>();
        public double HEATER_BASE_DRAIN_FACTOR = 1.0;
        public float HEATER_TARGET_TEMP = 20.0f;
        public float COOLER_TARGET_TEMP = 15.0f;
        public double MASSAGE_BASE_DRAIN_FACTOR = 3.0;
        public double RECHARGE_RESERVE_THRESHOLD = 0.95;

        //Constants
        public const double MANAGER_ACTIVE_DRAIN = 1.0 / 60.0;        
        public const double RCS_DRAIN = 1.0 / 60.0;
        public const double TURN_ROT_POWER_DRAIN_FACTOR = 1.0 / 40.0;
        public const float TURN_INACTIVE_ROT_FACTOR = 0.1f;
        public const float POWER_UP_DELAY = 0.5f;
        public const double SAS_BASE_DRAIN = 1.0 / 60.0;
        public const double SAS_TORQUE_DRAIN_FACTOR = 1.0 / 160.0;
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
        private AYGameSettings gameSettings;
        private AYGlobalSettings globalSettings;
        
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
                        
            globalSettings = AmpYear.Instance.globalSettings;
            gameSettings = AmpYear.Instance.gameSettings;

            // create toolbar button
            if (ToolbarManager.ToolbarAvailable)
            {
                
                button1 = ToolbarManager.Instance.add("AmpYear", "button1");
                button1.TexturePath = "AmpYear/Icons/toolbarIcon";
                button1.ToolTip = "AmpYear";
                button1.Visibility = new GameScenesVisibility(GameScenes.SPACECENTER, GameScenes.FLIGHT, GameScenes.EDITOR);                
                button1.OnClick += (e) => GuiVisible = !GuiVisible;                
            }
            
            //toggle debugging
            if (Input.GetKeyDown(KeyCode.Y) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.A))
            {
                debugging = !debugging;
            }
            Utilities.LogFormatted("AYMenu Awake complete");
        }

        public void Start()
        {
            Utilities.LogFormatted("AYMenu Start");
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
                //			KISPPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "WarpPlugin");
                //			BioPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "Biomatic");
                //			KarPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "Karbonite");
            BDSMPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "BTSM");

            Utilities.LogFormatted_DebugOnly("Checked for mods");
            if (ALPresent)
                Utilities.LogFormatted_DebugOnly("Aviation Lights present");                    
            if (NFEPresent)
                Utilities.LogFormatted_DebugOnly("Near Future Electric present");
            if (KASPresent)
                Utilities.LogFormatted_DebugOnly("KAS present");
            if (RT2Present)
                Utilities.LogFormatted_DebugOnly("RT2 present");
            if (ScSPresent)
                Utilities.LogFormatted_DebugOnly("SCANSat present");
            if (TelPresent)
                Utilities.LogFormatted_DebugOnly("Telemachus present");
            if (TACLPresent)
                Utilities.LogFormatted_DebugOnly("TAC LS present");
            if (AntRPresent)
                Utilities.LogFormatted_DebugOnly("AntennaRange present");
            if (KISPPresent)
                Utilities.LogFormatted_DebugOnly("Interstellar present");
            if (BioPresent)
                Utilities.LogFormatted_DebugOnly("Biomatic present");
            if (KarPresent)
                Utilities.LogFormatted_DebugOnly("Karbonite present");
            if (BDSMPresent)
                Utilities.LogFormatted_DebugOnly("btsm present");
            Utilities.LogFormatted("AYMenu Start complete");
        }

        public void OnDestroy()
        {
            if (ToolbarManager.ToolbarAvailable)
            {
                button1.Destroy();
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

        private int guiSectionStateToInt()
        {
            int val = 0;
            for (int i = 0; i < Enum.GetValues(typeof(GUISection)).Length; i++)
            {
                if (guiSectionEnableFlag[i])
                    val |= (1 << i);
            }
            return val;
        }

        private void setGuiSectionStateFromInt(int state)
        {
            for (int i = 0; i < Enum.GetValues(typeof(GUISection)).Length; i++)
            {
                guiSectionEnableFlag[i] = (state & (1 << i)) != 0;
            }
        }

        private void SetModeFlag()
        {
            //Set the mode flag, 0 = inflight, 1 = editor, -1 on EVA or F2
            if (FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)  // Check if in flight
            {

                if (FlightGlobals.ActiveVessel.isEVA) // EVA kerbal, do nothing
                    mode = -1;                    
                else 
                    mode = 0;
            }
            else if (EditorLogic.fetch != null) // Check if in editor
                mode = 1;
            else   // Not in flight, in editor or F2 pressed unset the mode and return
                mode = -1;            
        }

        private void FixedUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Y) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.A)) // Toggle Debug Mode
            {
                debugging = !debugging;
            }

            if (Time.timeSinceLevelLoad < 2.0f) // Check not loading level
            {
                return;
            }

            //Set the mode flag, 0 = inflight, 1 = editor, -1 on EVA or F2
            SetModeFlag();
            if (mode == -1) return;
                        
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
                    Utilities.LogFormatted_DebugOnly("ampYear AYMenu FixedUpdate mode == " + mode);
                    doOneUpdate = false;
                    
                    //get current vessel parts list
                    List<Part> parts = new List<Part> { };
                    if (mode == 0)
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
                                Utilities.LogFormatted("In Editor but couldn't get parts list");
                                return;
                            }
                        }
                        catch (Exception Ex)
                        {
                            if (Ex.Message.Contains("Reference"))
                            {
                                Utilities.LogFormatted("NullRef occurred getting parts list");
                            }
                            else
                            {
                                Utilities.LogFormatted_DebugOnly("Error occurred getting parts list " + Ex.Message);
                            }
                            return;
                        }
                    
                    //Compile information about the vessel and its parts    
                    // zero accumulators               
                    sasAdditionalRotPower = 0.0f;
                    sasTorque = 0.0f;
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
                    if (mode == 1) // if in editor clear the power producing and consuming part lists (we gunna rebuild them)
                    {
                        PowerProdList.Clear();
                        PowerConsList.Clear();
                    }
                    VslRstr.Clear(); //clear the vessel roster

                    //Begin calcs
                    if (mode == 0) // if in flight compile the vessel roster
                    VslRstr = FlightGlobals.ActiveVessel.GetVesselCrew();
                                                    
                    //loop through all parts in the parts list of the vessel
                    foreach (Part current_part in parts)
                    {
                        Utilities.LogFormatted_DebugOnly("AYMenu part = " + current_part.name);                        
                        bool currentEngActive = false;
                        double alt_rate = 0;

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
                            sasTorque += pod.maxTorque;

                        }

                        if (current_part is SASModule)
                        {                            
                            SASModule sas_module = (SASModule)current_part;
                            sasTorque += sas_module.maxTorque;
                            sasAdditionalRotPower += sas_module.maxTorque * SAS_POWER_TURN_TORQUE_FACTOR;                            
                        }

                        bool has_alternator = false;     
                   
                        //loop through all the modules in the current part
                        foreach (PartModule module in current_part.Modules)
                        {
                            Utilities.LogFormatted_DebugOnly("AYMenu module = " + module.name);
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
                                    float ElecUse = 0f;
                                    ElecUse = ((ModuleAmpYearPoweredRCS)module).electricityUse;
                                    totalPowerDrain += ElecUse;
                                    if (mode == 1)
                                    {                                                                               
                                        var PrtName = current_part.name;
                                        var PrtPower = ElecUse.ToString("0.000");
                                        PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower); 
                                        PowerConsList.Add(PartAdd);
                                    }                                    
                                    currentPoweredRCSDrain += ElecUse;
                                    Utilities.LogFormatted_DebugOnly("AYRCS ElecUsage = " + ElecUse.ToString("0.00000000"));
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
                                continue;
                            }

                            if (module.moduleName == "ModuleDeployableSolarPanel")
                            {                               
                                ModuleDeployableSolarPanel tmpSol = (ModuleDeployableSolarPanel)module;
                                if (mode == 0)
                                    totalPowerProduced += tmpSol.flowRate;
                                else
                                    totalPowerProduced += tmpSol.chargeRate;
                                if (mode == 1)
                                {
                                    var PrtName = current_part.name;
                                    var PrtPower = tmpSol.chargeRate.ToString("0.000");
                                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                                    PowerProdList.Add(PartAdd);
                                }                                         
                                continue;
                            }

                            if (module.moduleName == "ModuleGenerator")
                            {                                
                                ModuleGenerator tmpGen = (ModuleGenerator)module;
                                foreach (ModuleGenerator.GeneratorResource outp in tmpGen.outputList)
                                {
                                    if (outp.name == "ElectricCharge")
                                        if (mode == 1)
                                            totalPowerProduced += outp.rate;
                                        else
                                            if (tmpGen.isAlwaysActive || tmpGen.generatorIsActive)
                                                totalPowerProduced += outp.rate;
                                }
                                foreach (ModuleGenerator.GeneratorResource inp in tmpGen.inputList)
                                {
                                    if (inp.name == "ElectricCharge")
                                        if (mode == 1)
                                           totalPowerDrain += inp.rate;
                                        else
                                            if (tmpGen.isAlwaysActive || tmpGen.generatorIsActive)
                                                totalPowerDrain += inp.rate;
                                }                                
                                continue;
                            }

                            if (module.moduleName == "ModuleWheel")
                            {                                
                                ModuleWheel tmpWheel = (ModuleWheel)module;
                                if (tmpWheel.resourceName == "ElectricCharge")
                                {
                                    if (mode == 0 && tmpWheel.throttleInput != 0)
                                        totalPowerDrain += tmpWheel.resourceConsumptionRate;
                                    if (mode == 1)
                                    {
                                        totalPowerDrain += tmpWheel.resourceConsumptionRate;
                                        var PrtName = current_part.name;
                                        var PrtPower = tmpWheel.resourceConsumptionRate.ToString("0.000");
                                        PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                                        PowerConsList.Add(PartAdd);                                                     
                                    }
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
                                        if (mode == 1)
                                        {                                                
                                            var PrtName = current_part.name;
                                            var PrtPower = r.rate.ToString("0.000");
                                            PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                                            PowerConsList.Add(PartAdd);
                                        }
                                    }
                                }
                                continue;
                            }

                            if (module.moduleName == "ModuleLight")
                            {                                
                                ModuleLight tmpLight = (ModuleLight)module;
                                if (mode == 1 || (mode == 0 && tmpLight.isOn))
                                    totalPowerDrain += tmpLight.resourceAmount;
                                if (mode == 1)
                                {
                                    var PrtName = current_part.name;
                                    var PrtPower = tmpLight.resourceAmount.ToString("0.000");
                                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                                    PowerConsList.Add(PartAdd);
                                }                                
                                continue;
                            }

                            if (module.moduleName == "ModuleDataTransmitter")
                            {                                
                                ModuleDataTransmitter tmpAnt = (ModuleDataTransmitter)module;
                                if (mode == 1 || (mode == 0 && tmpAnt.IsBusy()))
                                    totalPowerDrain += tmpAnt.DataResourceCost * (1 / tmpAnt.packetInterval);
                                if (mode == 1)
                                {
                                    var PrtName = current_part.name;
                                    var PrtPower = (tmpAnt.DataResourceCost * (1 / tmpAnt.packetInterval)).ToString("0.000");
                                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                                    PowerConsList.Add(PartAdd);
                                }                                
                                continue;
                            }

                            if (module.moduleName == "ModuleReactionWheel")
                            {                                                      
                                ModuleReactionWheel tmpRW = (ModuleReactionWheel)module;
                                sasAdditionalRotPower += tmpRW.RollTorque * SAS_POWER_TURN_TORQUE_FACTOR; 
                                foreach (ModuleResource r in tmpRW.inputResources)
                                {
                                    if (r.id == definition.id)
                                    {
                                        if (mode == 1)
                                        {
                                            totalPowerDrain += r.rate * tmpRW.PitchTorque;  // rough guess for VAB
                                            var PrtName = current_part.name;
                                            var PrtPower = (r.rate * tmpRW.PitchTorque).ToString("0.000");
                                            PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                                            PowerConsList.Add(PartAdd);
                                        } 
                                    }
                                }                                
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
                                    if (prop.name == "ElectricCharge")
                                    {
                                        usesCharge = true;
                                        ecratio = prop.ratio;
                                    }
                                    sumRD += prop.ratio * PartResourceLibrary.Instance.GetDefinition(prop.id).density;
                                }
                                if (usesCharge)
                                {
                                    float massFlowRate;
                                    if (mode == 0 && tmpEng.isOperational && tmpEng.currentThrottle > 0)
                                    {
                                        massFlowRate = (tmpEng.currentThrottle * tmpEng.maxThrust) / (tmpEng.atmosphereCurve.Evaluate(0) * grav);
                                        totalPowerDrain += (ecratio * massFlowRate) / sumRD;
                                    }
                                    if (mode == 1 && currThrottle > 0.0)
                                    {
                                        massFlowRate = (currThrottle * tmpEng.maxThrust) / (tmpEng.atmosphereCurve.Evaluate(0) * grav);
                                        totalPowerDrain += (ecratio * massFlowRate) / sumRD;
                                        var PrtName = current_part.name;
                                        var PrtPower = ((ecratio * massFlowRate) / sumRD).ToString("0.000");
                                        PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                                        PowerConsList.Add(PartAdd);
                                    }
                                }
                                
                                currentEngActive = tmpEng.isOperational && (tmpEng.currentThrottle > 0);
                                if (alt_rate > 0 && currentEngActive)
                                    totalPowerProduced += alt_rate;
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
                                    if (prop.name == "ElectricCharge")
                                    {
                                        usesCharge = true;
                                        ecratio = prop.ratio;
                                    }
                                    sumRD += prop.ratio * PartResourceLibrary.Instance.GetDefinition(prop.id).density;
                                }
                                if (usesCharge)
                                {
                                    float massFlowRate;
                                    if (mode == 0 && tmpEngFX.isOperational && tmpEngFX.currentThrottle > 0)
                                    {
                                        massFlowRate = (tmpEngFX.currentThrottle * tmpEngFX.maxThrust) / (tmpEngFX.atmosphereCurve.Evaluate(0) * grav);
                                        totalPowerDrain += (ecratio * massFlowRate) / sumRD;
                                    }
                                    if (mode == 1 && currThrottle > 0.0)
                                    {
                                        massFlowRate = (currThrottle * tmpEngFX.maxThrust) / (tmpEngFX.atmosphereCurve.Evaluate(0) * grav);
                                        totalPowerDrain += (ecratio * massFlowRate) / sumRD;
                                        var PrtName = current_part.name;
                                        var PrtPower = ((ecratio * massFlowRate) / sumRD).ToString("0.000");
                                        PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                                        PowerConsList.Add(PartAdd);
                                    }
                                }                                
                                currentEngActive = tmpEngFX.isOperational && (tmpEngFX.currentThrottle > 0);
                                if (alt_rate > 0 && currentEngActive)
                                    totalPowerProduced += alt_rate;
                                continue;
                            }

                            if (module.moduleName == "ModuleAlternator")
                            {                                
                                ModuleAlternator tmpAlt = (ModuleAlternator)module;
                                foreach (ModuleResource r in tmpAlt.outputResources)
                                {
                                    if (r.name == "ElectricCharge")
                                    {
                                        if (mode == 1 || (mode == 0 && currentEngActive))
                                        {
                                            totalPowerProduced += r.rate;
                                            if (mode == 1)
                                            {
                                                var PrtName = current_part.name;
                                                var PrtPower = r.rate.ToString("0.000");
                                                PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                                                PowerProdList.Add(PartAdd);
                                            }
                                        }
                                        else
                                            alt_rate = r.rate;
                                    }
                                }                                
                                continue;
                            }

                            if (module.moduleName == "ModuleScienceLab")
                            {                                
                                if (mode == 0)
                                {
                                    ModuleScienceLab tmpLab = (ModuleScienceLab)module;
                                    foreach (ModuleResource r in tmpLab.processResources)
                                    {
                                        if (r.name == "ElectricCharge" && tmpLab.IsOperational())
                                            totalPowerProduced += r.rate;
                                    }
                                }
                                if (mode == 1)
                                {
                                    var PrtName = current_part.name;
                                    double tmpPwr = 0;
                                    ModuleScienceLab tmpLab = (ModuleScienceLab)module;
                                    foreach (ModuleResource r in tmpLab.processResources)
                                    {
                                        if (r.name == "ElectricCharge")
                                            totalPowerProduced += r.rate;
                                            tmpPwr += r.rate;
                                    }
                                    var PrtPower = tmpPwr.ToString("0.000");
                                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                                    PowerConsList.Add(PartAdd);
                                }
                            }

                            if (KASPresent)
                                try
                                {
                                    checkKAS(module);
                                }
                                catch
                                {
                                    Utilities.LogFormatted("Wrong KAS library version - disabled.");
                                    KASPresent = false;
                                }

                            if (RT2Present)
                                try
                                {
                                    checkRT2(module);
                                }
                                catch
                                {
                                    Utilities.LogFormatted("Wrong Remote Tech 2 library version - disabled.");
                                    RT2Present = false;
                                }
                            /*
                                                        if (ALPresent)
                                                            try
                                                            {
                                                                checkAv(module);
                                                            }
                                                            catch
                                                            {
                                                                Utilities.LogFormatted_DebugOnly("Wrong Aviation Lights library version - disabled.");
                                                                ALPresent = false;
                                                            }		
                            */
                            if (NFEPresent)
							    try
							    {
									checkNFE(module);
							    }
							    catch
							    {
                                    Utilities.LogFormatted("Wrong Near Future library version - disabled.");
								    NFEPresent = false;
							    }
                            /*
                                                        if (NFSPresent)
                                                            try
                                                            {
                                                                checkNFS(module);
                                                            }
                                                            catch
                                                            {
                                                                Utilities.LogFormatted_DebugOnly("Wrong Near Future solar library version - disabled.");
                                                                NFSPresent = false;
                                                            }
                            */
                            if (ScSPresent)
							    try
							    {
									checkSCANsat(module);
							    }
							    catch
							    {
                                    Utilities.LogFormatted("Wrong SCANsat library version - disabled.");
								    ScSPresent = false;
							    }
						
						    if (TelPresent)
							    try
							    {
									checkTel(module);
							    }
							    catch
							    {
                                    Utilities.LogFormatted("Wrong Telemachus library version - disabled.");
								    TelPresent = false;
							    }

						    if (AntRPresent)
							    try
						        {
							        checkAntR(module);
						        }
						        catch
						        {
                                    Utilities.LogFormatted("Wrong AntennaRange library version - disabled.");
							        AntRPresent = false;
						        }

						    if (BioPresent)
							    try
						        {
							        checkBio(module);
						        }
						        catch
						        {
                                    Utilities.LogFormatted("Wrong Biomatic library version - disabled.");
							        BioPresent = false;
						        }

    					    if (KarPresent)
							    try
						        {
							        checkKar(module);
						        }
						        catch
						        {
                                    Utilities.LogFormatted("Wrong Karbonite library version - disabled.");
							        KarPresent = false;
						        } 

    					    if (BDSMPresent)
							    try
						        {
							        checkBDSM(module);
						        }
						        catch
						        {
                                    Utilities.LogFormatted("Wrong BTSM library version - disabled.");
							        BDSMPresent = false;
						        }

                            if (module is AYCrewPart)
                            {
                                if (((AYCrewPart)module).CabinTemp >= HEATER_TARGET_TEMP)
                                    totalHeatedParts++;
                                if (((AYCrewPart)module).CabinTemp <= COOLER_TARGET_TEMP)
                                    totalCooledParts++;
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
                    
                    if (TACLPresent)
                        try
                        {
                            checkTACL();
                        }
                        catch
                        {
                            Utilities.LogFormatted("Wrong TAC LS library version - disabled.");
                            TACLPresent = false;
                        }  
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
                    //Disable RCS when the subsystem isn't powered
                    cv.ActionGroups.SetGroup(KSPActionGroup.RCS, false);
                    reenableRCS = true;
                }

                if (cv.ActionGroups[KSPActionGroup.SAS] && !subsystemPowered(Subsystem.SAS))
                {                   
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
                    changeCrewedPartsTemperature(HEATER_TARGET_TEMP, true);

                if (subsystemPowered(Subsystem.COOLER))
                    changeCrewedPartsTemperature(COOLER_TARGET_TEMP, false);
                
                //Calculate total drain from subsystems
                double subsystem_drain = 0.0;
                foreach (Subsystem subsystem in Enum.GetValues(typeof(Subsystem)))
                {
                    subsystemDrain[(int)subsystem] = subsystemCurrentDrain(subsystem);
                    subsystem_drain += subsystemDrain[(int)subsystem];
                }

                double manager_drain = managerCurrentDrain;
                double total_manager_drain = subsystem_drain + manager_drain;
                totalPowerDrain += total_manager_drain;


                //Recharge reserve power if main power is above a certain threshold
                if (managerIsActive && totalElectricCharge > 0 && totalElectricCharge / totalElectricChargeCapacity > RECHARGE_RESERVE_THRESHOLD
                    && totalReservePower < totalReservePowerCapacity)
                    transferMainToReserve(RECHARGE_RESERVE_RATE * sumDeltaTime);
                                   
                //Drain main power
                double timestep_drain = total_manager_drain * sumDeltaTime;
                double minimum_sufficient_charge = managerActiveDrain;
                if (totalElectricCharge >= minimum_sufficient_charge)
                {
                    
                    hasPower = (UnityEngine.Time.realtimeSinceStartup > powerUpTime)
                     && (timestep_drain <= 0.0 || requestResource(MAIN_POWER_NAME, timestep_drain) >= (timestep_drain * 0.99));
                }
                else
                    hasPower = false;

                if (!hasPower && UnityEngine.Time.realtimeSinceStartup > powerUpTime)
                {
                    //Set a delay for powering back up to avoid rapid flickering of the system
                    powerUpTime = UnityEngine.Time.realtimeSinceStartup + POWER_UP_DELAY;
                }

                if (!hasPower && totalReservePower > minimum_sufficient_charge)
                {
                    //If main power is insufficient, drain reserve power for manager
                    double manager_timestep_drain = manager_drain * sumDeltaTime;
                    //double manager_timestep_drain = manager_drain * Time.fixedDeltaTime;
                    
                    hasReservePower = manager_drain <= 0.0
                        || requestResource(RESERVE_POWER_NAME, manager_timestep_drain) >= (manager_timestep_drain * 0.99);
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
                hasPower = true;
                hasReservePower = true;
            }
            sumDeltaTime = 0;            
        }
        
/* 
              private void checkAv(PartModule psdpart)
              {
                  switch (psdpart.moduleName)
                  {
                      case "ModuleNavLight":                          
                              global::AviationLights.ModuleNavLight tmpLight = (global::AviationLights.ModuleNavLight)psdpart;
                              totalPowerDrain += tmpLight.EnergyReq;                          
                          break; 
                  }
              }
*/
        private void checkNFE(PartModule psdpart)
        {
            var PrtName = psdpart.name;
            var PrtPower = "";      
            switch (psdpart.moduleName)
                  {
                      case "FissionGenerator":
                          
                              global::NearFutureElectrical.FissionGenerator tmpGen = (global::NearFutureElectrical.FissionGenerator)psdpart;
                              if (mode == 0 && tmpGen.Enabled)
                              {
                                  totalPowerProduced += tmpGen.GetCurrentPower();
                                  PrtPower = tmpGen.GetCurrentPower().ToString("0.000");
                              }
                              else if (mode == 1)
                              {
                                  totalPowerProduced += tmpGen.PowerGenerationMaximum;
                                  PrtPower = tmpGen.PowerGenerationMaximum.ToString("0.000");
                              }
                          PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                          PowerProdList.Add(PartAdd);
                          break;   
                  }
        }
       /*
              private void checkNFS(PartModule psdpart)
              {
                  switch (psdpart.moduleName)
                  {
                      case "ModuleCurvedSolarPanel":
                         
                              global::NearFutureSolar.ModuleCurvedSolarPanel tmpGen = (global::NearFutureSolar.ModuleCurvedSolarPanel)psdpart;
                              if (mode == 0 && tmpGen.State == ModuleDeployableSolarPanel.panelStates.EXTENDED)
                                  totalPowerProduced += tmpGen.TotalEnergyRate;
                              else if (mode == 1)
                                  totalPowerProduced += tmpGen.TotalEnergyRate;
                          
                          break;
                  }
              }
        */     
        private void checkKAS(PartModule psdpart)
        {
            var PrtName = psdpart.name;
            var PrtPower = "";
            switch (psdpart.moduleName)
            {
                case "KASModuleWinch":
                    
                        global::KAS.KASModuleWinch tmpKW = (global::KAS.KASModuleWinch)psdpart;
                        
                        if (mode == 0 && tmpKW.isActive)
                        {
                            totalPowerDrain += tmpKW.powerDrain * tmpKW.motorSpeed;
                            PrtPower = (tmpKW.powerDrain * tmpKW.motorSpeed).ToString("0.000");
                        }
                        if (mode == 1)
                        {
                            totalPowerDrain += tmpKW.powerDrain;
                            PrtPower = tmpKW.powerDrain.ToString("0.000");
                        }             
                        PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                        PowerConsList.Add(PartAdd);
                    break;
                case "KASModuleMagnet":
                    
                        global::KAS.KASModuleMagnet tmpKM = (global::KAS.KASModuleMagnet)psdpart;
                        if (mode == 1 || (mode == 0 && tmpKM.MagnetActive))
                        {
                            totalPowerDrain += tmpKM.powerDrain;
                            PrtPower = tmpKM.powerDrain.ToString("0.000");
                        }
                        PwrPartList PartAdd2 = new PwrPartList(PrtName, PrtPower);
                        PowerConsList.Add(PartAdd2);
                    break;
            }
        }
       
        private void checkRT2(PartModule psdpart)
        {
            switch (psdpart.moduleName)
            {
                case "ModuleRTAntenna":
                        var PrtName = psdpart.name;
                        var PrtPower = "";
                        global::RemoteTech.ModuleRTAntenna tmpAnt = (global::RemoteTech.ModuleRTAntenna)psdpart;
                        if (mode == 0 && tmpAnt.Activated)
                        {
                            totalPowerDrain += tmpAnt.Consumption;
                            PrtPower = tmpAnt.Consumption.ToString("0.000");
                        }
                        if (mode == 1)
                        {
                            totalPowerDrain += tmpAnt.EnergyCost;
                            PrtPower = tmpAnt.EnergyCost.ToString("0.000");
                        }
                        PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                        PowerConsList.Add(PartAdd);
                    break;
            }
        }
        
        private void checkSCANsat(PartModule psdpart)
         {
             switch (psdpart.moduleName)
             {
                 case "SCANsat":
                        var PrtName = psdpart.name;
                        var PrtPower = "";
                        global::SCANsat.SCANsat tmpSS = (global::SCANsat.SCANsat)psdpart;
                        if ((mode == 1) || (mode == 0 && (tmpSS.power > 0.0 && tmpSS.scanningNow())))
                        {
                            totalPowerDrain += tmpSS.power;
                            PrtPower = tmpSS.power.ToString("0.000");
                        }
                        PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                        PowerConsList.Add(PartAdd);                     
                     break; 
             }
         }

        private void checkTel(PartModule psdpart)
         {
             switch (psdpart.moduleName)
             {
                 case "TelemachusPowerDrain":
                        var PrtName = psdpart.name;
                        var PrtPower = "";
                        if (mode == 0 && global::Telemachus.TelemachusPowerDrain.isActive)
                        {
                            totalPowerDrain += global::Telemachus.TelemachusPowerDrain.powerConsumption;
                            PrtPower = global::Telemachus.TelemachusPowerDrain.powerConsumption.ToString("0.000");
                        }
                        if (mode == 1)
                        {
                            totalPowerDrain += 0.01;
                            PrtPower = ("0.010");
                        }
                        PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                        PowerConsList.Add(PartAdd); 
                     break;
             }
         }
      
        private void checkTACL()
        {           
                global::Tac.TacLifeSupport tmpTLS = Tac.TacLifeSupport.Instance;
                if (tmpTLS.gameSettings.Enabled)
                {
                    var PrtName = "TACL Life Support";
                    var PrtPower = "";
                    if (mode == 0) //if in flight set maxCrew to actual crew on board. Set earlier based on maximum crew capacity of each part
                        maxCrew = FlightGlobals.ActiveVessel.GetCrewCount();                 
                    double CalcDrain = 0;
                    CalcDrain = tmpTLS.globalSettings.BaseElectricityConsumptionRate * crewablePartList.Count;
                    CalcDrain += tmpTLS.globalSettings.ElectricityConsumptionRate * maxCrew;
                    totalPowerDrain += CalcDrain;
                    PrtPower = CalcDrain.ToString("0.000");
                    PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                    PowerConsList.Add(PartAdd); 
                }
            
        }
       
        private void checkAntR(PartModule psdpart)
         {
             switch (psdpart.moduleName)
             {
                 case "ModuleLimitedDataTransmitter":
                        var PrtName = psdpart.name;
                        var PrtPower = "";
                         ModuleDataTransmitter tmpAnt = (ModuleDataTransmitter)psdpart;
                         if (mode == 1 || (mode == 0 && tmpAnt.IsBusy()))
                         {
                             totalPowerDrain += tmpAnt.DataResourceCost * (1 / tmpAnt.packetInterval);
                             PrtPower = (tmpAnt.DataResourceCost * (1 / tmpAnt.packetInterval)).ToString("0.000");

                         }
                         PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                         PowerConsList.Add(PartAdd);
                     break; 
             }
         }

        private void checkBio(PartModule psdpart)
         {
             switch (psdpart.moduleName)
             {
                 case "Biomatic":
                        var PrtName = psdpart.name;
                        var PrtPower = "";
                         const double biouse = 0.04;
                         totalPowerDrain += biouse;
                         PrtPower = biouse.ToString("0.000");
                         PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                         PowerConsList.Add(PartAdd);
                     break;
             }
         }

        private void checkKar(PartModule psdpart)
         {
             switch (psdpart.moduleName)
             {
                 case "USI_ResourceConverter":
                     var PrtName = psdpart.name;
                     var PrtPower = "";
                     totalPowerDrain += 1;
                     PrtPower = ("1.000");
                     PwrPartList PartAdd = new PwrPartList(PrtName, PrtPower);
                     PowerConsList.Add(PartAdd);
                     break;
             }
         }

        private void checkBDSM(PartModule psdpart)
         {
             var PrtName = psdpart.name;
             var PrtPower = ""; 
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
        private void CheckVslUpdate()
        {
            // Called every fixed update from fixedupdate - Check for vessels that have been deleted and remove from Dictionary
            double currentTime = Planetarium.GetUniversalTime();
            var allVessels = FlightGlobals.Vessels;
            var knownVessels = gameSettings.knownVessels;
            var vesselsToDelete = new List<Guid>();

            Utilities.LogFormatted("AYMenu CheckVslUpdate");
            Utilities.LogFormatted("AYMenu knownvessels count = " + knownVessels.Count);
            foreach (var entry in knownVessels)
            {
                Utilities.LogFormatted("AYMenu knownvessels guid = " + entry.Key);
                Guid vesselId = entry.Key;
                VesselInfo vesselInfo = entry.Value;
                Vessel vessel = allVessels.Find(v => v.id == vesselId);

                if (vessel == null)
                {
                    Utilities.LogFormatted_DebugOnly("Deleting vessel " + vesselInfo.vesselName + " - vessel does not exist anymore");
                    vesselsToDelete.Add(vesselId);
                    continue;
                }

                if (vessel.loaded)
                {
                    int crewCapacity = UpdateVesselCounts(vesselInfo, vessel);

                    if (vesselInfo.numCrew == 0)
                    {
                        Utilities.LogFormatted_DebugOnly("Deleting vessel " + vesselInfo.vesselName + " - no crew parts anymore");
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
                    Utilities.LogFormatted("updating current vessel"); 
                    UpdateVesselInfo(vesselInfo, vessel);
                    vesselInfo.vesselType = FlightGlobals.ActiveVessel.vesselType;
                }
            }

            vesselsToDelete.ForEach(id => knownVessels.Remove(id));

            foreach (Vessel vessel in allVessels.Where(v => v.loaded))
            {
                if (!knownVessels.ContainsKey(vessel.id) && vessel.parts.Any(p => p.protoModuleCrew.Count > 0))
                {
                    Utilities.LogFormatted_DebugOnly("New vessel: " + vessel.vesselName + " (" + vessel.id + ")");                                                        
                    VesselInfo vesselInfo = new VesselInfo(vessel.vesselName, currentTime);
                    knownVessels[vessel.id] = vesselInfo;
                    vesselInfo.vesselType = vessel.vesselType;
                    UpdateVesselInfo(vesselInfo, vessel);
                                        
                }
            }

            Utilities.LogFormatted("AYMenu CheckVslUpdate complete");
        }
        private void ConsumeResources(double currentTime, Vessel vessel, VesselInfo vesselInfo)
        {
            return;
        }

        private void UpdateVesselInfo(VesselInfo vesselInfo, Vessel vessel)
        {            
            // save current toggles to current vesselinfo
            Utilities.LogFormatted("AYMenu UpdateVesselInfo " + vessel.id);
            vesselInfo.managerEnabled = managerEnabled;
            vesselInfo.ShowCrew = ShowCrew;
            vesselInfo.ShowParts = ShowParts;
            vesselInfo.subsystemDrain = subsystemDrain;
            vesselInfo.subsystemToggle = subsystemToggle;
            vesselInfo.guiSectionEnableFlag = guiSectionEnableFlag;
            int crewCapacity = UpdateVesselCounts(vesselInfo, vessel);            
        }

        private int UpdateVesselCounts(VesselInfo vesselInfo, Vessel vessel)
        {
            // save current toggles to current vesselinfo
            Utilities.LogFormatted("AYMenu UpdateVesselCounts " + vessel.id);           

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
            var knownVessels = gameSettings.knownVessels;            
            VesselInfo info = new VesselInfo(newvessel.name, currentTime);
            Utilities.LogFormatted("AYMenu VesselLoad " + newvessel.id);
            //if newvessel is not the active vessel then do nothing.
            if (newvessel.id != FlightGlobals.ActiveVessel.id)
            {
                Utilities.LogFormatted("AYMenu newvessel is not active vessel");
                return;
            }
            currentvesselid = newvessel.id;
            loadVesselSettings(newvessel, info);            
        }

        private void VesselChange(Vessel newvessel)
        {
            Utilities.LogFormatted("AYMenu VesselChange New " + newvessel.id + " Old " + currentvesselid);
            Utilities.LogFormatted("AYMenu active vessel " + FlightGlobals.ActiveVessel.id);
            double currentTime = Planetarium.GetUniversalTime();
            var knownVessels = gameSettings.knownVessels;            
            VesselInfo info = new VesselInfo(newvessel.name, currentTime);
           
            if (currentvesselid == newvessel.id)
                return;
            GuiVisible = !GuiVisible;
            // Update Old Vessel settings into Dictionary
            if (knownVessels.ContainsKey(currentvesselid))
            {
                info = knownVessels[currentvesselid];
                UpdateVesselInfo(info, FlightGlobals.ActiveVessel);
                this.Log("Updated old vessel dict " + knownVessels[currentvesselid].ToString());
            }           
            loadVesselSettings(newvessel, info);
            currentvesselid = newvessel.id;
            GuiVisible = !GuiVisible; 
        }

        private void loadVesselSettings(Vessel newvessel, VesselInfo info)
        {
            var knownVessels = gameSettings.knownVessels;            
            // Load New Vessel settings from Dictionary
            if (knownVessels.ContainsKey(newvessel.id))
            {
                Utilities.LogFormatted("AYMenu Vessel Loading Settings");
                info = knownVessels[newvessel.id];
                this.Log(info.ToString());
                for (int i = 0; i < Enum.GetValues(typeof(Subsystem)).Length; i++)
                    subsystemToggle[i] = info.subsystemToggle[i];
                for (int i = 0; i < Enum.GetValues(typeof(GUISection)).Length; i++)
                    guiSectionEnableFlag[i] = info.guiSectionEnableFlag[i];
                managerEnabled = info.managerEnabled;
                ShowCrew = info.ShowCrew;                
            }
            else //New Vessel not found in Dictionary so set default
            {
                Utilities.LogFormatted("AYMenu Vessel Setting Default Settings");
                for (int i = 0; i < Enum.GetValues(typeof(Subsystem)).Length; i++)
                    subsystemToggle[i] = false;
                for (int i = 0; i < Enum.GetValues(typeof(GUISection)).Length; i++)
                    guiSectionEnableFlag[i] = false;
                managerEnabled = false;
                ShowCrew = false;
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
                    cv.ActionGroups.SetGroup(KSPActionGroup.SAS, enabled);
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
                    //return SAS_BASE_DRAIN + (sasTorque * SAS_TORQUE_DRAIN_FACTOR);
                    return SAS_BASE_DRAIN;

                case Subsystem.RCS:
                    //return RCS_DRAIN + currentPoweredRCSDrain;
                    return RCS_DRAIN;
                                    
                case Subsystem.POWER_TURN:
                    return sasAdditionalRotPower * POWER_TURN_DRAIN_FACTOR;

                case Subsystem.HEATER:
                case Subsystem.COOLER:
                    if (mode == 0)                    
                        return HEATER_HEAT_RATE
                                * (crewablePartList.Count * HEATER_BASE_DRAIN_FACTOR + HEATER_CAPACITY_DRAIN_FACTOR * FlightGlobals.ActiveVessel.GetCrewCapacity());                    
                    else return HEATER_HEAT_RATE
                               * (crewablePartList.Count * HEATER_BASE_DRAIN_FACTOR + HEATER_CAPACITY_DRAIN_FACTOR);
                    
                case Subsystem.MUSIC:
                    return 1.0 * crewablePartList.Count;

                case Subsystem.MASSAGE:
                    if (mode == 0)                    
                        return MASSAGE_BASE_DRAIN_FACTOR * FlightGlobals.ActiveVessel.GetCrewCount();                    
                    else return MASSAGE_BASE_DRAIN_FACTOR;

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
                            Utilities.LogFormatted_DebugOnly("Subsys update crewpart temp = " + crewed_part.name + " " + crewed_part.flightID + " Temp = " + ((AYCrewPart)module).CabinTemp);
                            if (((AYCrewPart)module).CabinTemp < target_temp)
                            {
                                ((AYCrewPart)module).CabinTemp += AYMenu.HEATER_HEAT_RATE * sumDeltaTime;
                                Utilities.LogFormatted_DebugOnly("crewpart temp changed = " + crewed_part.name + " " + crewed_part.flightID + " Temp = " + ((AYCrewPart)module).CabinTemp);
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
                                ((AYCrewPart)module).CabinTemp -= AYMenu.HEATER_HEAT_RATE * sumDeltaTime;     
                            }

                        }
                    }

                    
                }        
            }
        }
        
        //Resources Functions Follow

        private double requestResource(String name, double amount)
        {
            Part cvp = FlightGlobals.ActiveVessel.rootPart;            
            //return requestResource(name, amount, cvp);
            if (amount <= 0.0)
                return 0.0;            
            double total_received = 0.0;
            double request_amount = amount;
            for (int attempts = 0; ((attempts < MAX_TRANSFER_ATTEMPTS) && (amount > 0.0)); attempts++)
            {
               
                double received = cvp.RequestResource(name, request_amount, ResourceFlowMode.ALL_VESSEL);
                Utilities.LogFormatted_DebugOnly("requestResource attempt " + attempts); 
                Utilities.LogFormatted_DebugOnly("requested power = " + request_amount.ToString("0.000000000000"));
                Utilities.LogFormatted_DebugOnly("received power = " + received.ToString("0.000000000000"));
                total_received += received;
                amount -= received;
                Utilities.LogFormatted_DebugOnly("amount = " + amount.ToString("0.000000000000"));
                
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

            

            double received = requestResource(RESERVE_POWER_NAME, amount);
            

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

            double received = requestResource(MAIN_POWER_NAME, amount);
            

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
                        mode = -1;
                        return;
                    }
                    mode = 0;
                }
                else if (EditorLogic.fetch != null) // Check if in editor
                    mode = 1;
                else   // Not in flight, in editor or F2 pressed unset the mode and return
                {
                    mode = -1;
                    return;
                }
                
                if (mode == 0)
                {              
                    GUI.skin = HighLogic.Skin;
                    FwindowPos = GUILayout.Window(
                        windowID,
                        FwindowPos,
                        windowF,
                        "AmpYear Power Manager",
                        GUILayout.Width(FWINDOW_WIDTH),
                        GUILayout.Height(WINDOW_BASE_HEIGHT)
                        );
                    //globalSettings.FwindowPosX = FwindowPos.x;
                    //globalSettings.FwindowPosY = FwindowPos.y;                    
                }
            }
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                GUI.skin = HighLogic.Skin;
                SCwindowPos = GUILayout.Window(
                    windowID,
                    SCwindowPos,
                    windowSC,
                    "AmpYear Power Manager Settings",
                    GUILayout.Width(SCWINDOW_WIDTH),
                    GUILayout.Height(WINDOW_BASE_HEIGHT)
                    );
                //globalSettings.SCwindowPosX = SCwindowPos.x;
                //globalSettings.SCwindowPosY = SCwindowPos.y;  
            }
            if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                GUI.skin = HighLogic.Skin;
                EwindowPos = GUILayout.Window(
                    windowID,
                    EwindowPos,
                    windowE,
                    "AmpYear Power Manager",
                    GUILayout.Width(EWINDOW_WIDTH),
                    GUILayout.Height(WINDOW_BASE_HEIGHT)
                    );
                if (ShowParts)  ScrollParts();
                //globalSettings.EwindowPosX = EwindowPos.x;
                //globalSettings.EwindowPosY = EwindowPos.y;  
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
            Rect EPLwindowPos = new Rect(EwindowPos.x + EWINDOW_WIDTH + 10, EwindowPos.y, AYMenu.EWINDOW_WIDTH, Screen.height / 2 - 100);
            // Begin the ScrollView
            scrollViewVector = GUI.BeginScrollView(EPLwindowPos, scrollViewVector, new Rect(0, 0, EWINDOW_WIDTH-20, 1700));
            // Put something inside the ScrollView            
            GUILayout.Label("Power Production Parts", sectionTitleStyle);
            foreach (PwrPartList PwrPart in PowerProdList)
            {
                GUILayout.Label(PwrPart.PrtName + " " + PwrPart.PrtPower, statusStyle);
            }
            GUILayout.Label("Power Consumer Parts", sectionTitleStyle);
            foreach (PwrPartList PwrPart in PowerConsList)
            {
                GUILayout.Label(PwrPart.PrtName + " " + PwrPart.PrtPower, statusStyle);
            }            
            // End the ScrollView
            GUI.EndScrollView();            
        }
           
        private void windowSC(int id)
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
            String Inputtext1 = HEATER_BASE_DRAIN_FACTOR.ToString();
            double InputValue1 = HEATER_BASE_DRAIN_FACTOR;
            string Inputtext2 = HEATER_TARGET_TEMP.ToString();
            float InputValue2 = HEATER_TARGET_TEMP;
            string Inputtext3 = COOLER_TARGET_TEMP.ToString();
            float InputValue3 = COOLER_TARGET_TEMP;
            String Inputtext4 = MASSAGE_BASE_DRAIN_FACTOR.ToString();
            double InputValue4 = MASSAGE_BASE_DRAIN_FACTOR;
            String Inputtext5 = (100 * RECHARGE_RESERVE_THRESHOLD).ToString();
            double InputValue5 = RECHARGE_RESERVE_THRESHOLD;
           
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Box("Heater/Cooler Drain factor", statusStyle, GUILayout.Width(300));            
            Inputtext1 = GUILayout.TextField(Inputtext1, GUILayout.MinWidth(30.0F)); //you can play with the width of the text box
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Box("Heater Target Temp.", statusStyle, GUILayout.Width(300));   
            Inputtext2 = GUILayout.TextField(Inputtext2, GUILayout.MinWidth(30.0F)); //you can play with the width of the text box
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Box("Cooler Target Temp.", statusStyle, GUILayout.Width(300)); 
            Inputtext3 = GUILayout.TextField(Inputtext3, GUILayout.MinWidth(30.0F)); //you can play with the width of the text box
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Box("Massage Drain Factor", statusStyle, GUILayout.Width(300));  
            Inputtext4 = GUILayout.TextField(Inputtext4, GUILayout.MinWidth(30.0F)); //you can play with the width of the text box
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Box("Reserve Battery Recharge Percentage", statusStyle, GUILayout.Width(300));
            Inputtext5 = GUILayout.TextField(Inputtext5, GUILayout.MinWidth(30.0F)); //you can play with the width of the text box
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            if (!Double.TryParse(Inputtext1, out InputValue1))
            {
                //the text couldn't be parsed; you'll have to decide what you want to do here
                InputValue1 = HEATER_BASE_DRAIN_FACTOR;
            }
            else HEATER_BASE_DRAIN_FACTOR = InputValue1;
            if (!float.TryParse(Inputtext2, out InputValue2))
            {
                //the text couldn't be parsed; you'll have to decide what you want to do here
                InputValue2 = HEATER_TARGET_TEMP;
            }
            else HEATER_TARGET_TEMP = InputValue2;
            if (!float.TryParse(Inputtext3, out InputValue3))
            {
                //the text couldn't be parsed; you'll have to decide what you want to do here
                InputValue3 = COOLER_TARGET_TEMP;
            }
            else COOLER_TARGET_TEMP = InputValue3;
            if (!Double.TryParse(Inputtext4, out InputValue4))
            {
                //the text couldn't be parsed; you'll have to decide what you want to do here
                InputValue4 = MASSAGE_BASE_DRAIN_FACTOR;
            }
            else MASSAGE_BASE_DRAIN_FACTOR = InputValue4;
            if (!Double.TryParse(Inputtext5, out InputValue5))
            {
                //the text couldn't be parsed; you'll have to decide what you want to do here
                InputValue5 = RECHARGE_RESERVE_THRESHOLD;
            }
            else RECHARGE_RESERVE_THRESHOLD = InputValue5 / 100;

            if (GUILayout.Button("Exit Save Settings"))
            {                
                GuiVisible = !GuiVisible;
            }

            if (!Input.GetMouseButtonDown(1))
            {
                GUI.DragWindow();
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

        public void Load(ConfigNode globalNode)
        {           
            FwindowPos.x = globalSettings.FwindowPosX;
            FwindowPos.y = globalSettings.FwindowPosY;
            EwindowPos.x = globalSettings.EwindowPosX;
            EwindowPos.y = globalSettings.EwindowPosY;
            SCwindowPos.x = globalSettings.SCwindowPosX;
            SCwindowPos.y = globalSettings.SCwindowPosY;
            HEATER_BASE_DRAIN_FACTOR = globalSettings.HEATER_BASE_DRAIN_FACTOR;
            HEATER_TARGET_TEMP = globalSettings.HEATER_TARGET_TEMP;
            MASSAGE_BASE_DRAIN_FACTOR = globalSettings.MASSAGE_BASE_DRAIN_FACTOR;
            RECHARGE_RESERVE_THRESHOLD = globalSettings.RECHARGE_RESERVE_THRESHOLD;
            debugging = globalSettings.debugging;
            Utilities.LogFormatted("AYMenu Load");
        }

        public void Save(ConfigNode globalNode)
        {
            globalSettings.FwindowPosX = FwindowPos.x;
            globalSettings.FwindowPosY = FwindowPos.y;
            globalSettings.EwindowPosX = EwindowPos.x;
            globalSettings.EwindowPosY = EwindowPos.y;
            globalSettings.SCwindowPosX = SCwindowPos.x;
            globalSettings.SCwindowPosY = SCwindowPos.y;
            globalSettings.HEATER_BASE_DRAIN_FACTOR = HEATER_BASE_DRAIN_FACTOR;
            globalSettings.HEATER_TARGET_TEMP = HEATER_TARGET_TEMP;
            globalSettings.MASSAGE_BASE_DRAIN_FACTOR = MASSAGE_BASE_DRAIN_FACTOR;
            globalSettings.RECHARGE_RESERVE_THRESHOLD = RECHARGE_RESERVE_THRESHOLD;
            globalSettings.debugging = debugging;     
            Utilities.LogFormatted("AYMenu Save");
        }
    }   
}
