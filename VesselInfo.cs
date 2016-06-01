

using RSTUtils;
/**
* * AmpYear power management.
* (C) Copyright 2015, Jamie Leighton
* The original code and concept of AmpYear rights go to SodiumEyes on the Kerbal Space Program Forums, which was covered by GNU License GPL (no version stated).
* As such this code continues to be covered by GNU GPL license.
* Parts of this code is Based On:-
* Thunder Aerospace Corporation's Life Support for Kerbal Space Program.
* Written by Taranis Elsu. *
* (C) Copyright 2013, Taranis Elsu
* This code is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
* creative commons license. See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>
* for full details.
*
* Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
* project is in no way associated with nor endorsed by Squad.
*
*/
using System;

namespace AY
{
    public class VesselInfo
    {
        public const string ConfigNodeName = "VesselInfo";

        public string VesselName;
        public VesselType VesselType = VesselType.Unknown;
        public int NumCrew;
        public int NumOccupiedParts;
        public bool[] SubsystemToggle = new bool[LoadGlobals.SubsystemArrayCache.Length]; //Enum.GetValues(typeof(Subsystem)).Length];
        public double[] SubsystemDrain = new double[LoadGlobals.SubsystemArrayCache.Length]; //Enum.GetValues(typeof(Subsystem)).Length];
        public bool[] GuiSectionEnableFlag = new bool[LoadGlobals.GuiSectionArrayCache.Length]; //Enum.GetValues(typeof(GUISection)).Length];
        public bool ManagerEnabled = true;
        public bool ShowCrew = false;
        public bool ShowParts = false;
        public bool Hibernating;
        public bool EmgcyShutActive = false;
        public bool AutoPilotDisabled = false;
        public double AutoPilotDisTime = 0f;
        public double AutoPilotDisCounter = 0f;
        public double TimeLastElectricity = 0f;
        public double LastUpdate = 0f;
        public bool EmgcyShutOverride = false;
        public bool EmgcyShutOverrideStarted = false;
        public bool Emergencypowerdownactivated = false;
        public bool Emergencypowerdownreset = false;
        public ESPPriority EspPriority = ESPPriority.LOW;
        public bool Emergencypowerdownprevactivated = false;
        public bool Emergencypowerdownresetprevactivated = false;
        public bool ESPPriorityHighProcessed = false;  
        public bool ESPPriorityMediumProcessed = false; 
        public bool ESPPriorityLowProcessed = false;
        public bool ESPPriorityHighResetProcessed = false;
        public bool ESPPriorityMediumResetProcessed = false;
        public bool ESPPriorityLowResetProcessed = false;
        public bool IsolateReservePower = false;


        public bool ReenableRcs = false;
        public bool ReenableSas = false;

        public VesselInfo(string vesselName, double currentTime)
        {
            VesselName = vesselName;
            Hibernating = false;
            LastUpdate = currentTime;
            TimeLastElectricity = currentTime;
        }

        public static VesselInfo Load(ConfigNode node)
        {
            string vesselName = "";
            node.TryGetValue("vesselName", ref vesselName);
            double lastUpdate = 0;
            node.TryGetValue("lastUpdate", ref lastUpdate);

            VesselInfo info = new VesselInfo(vesselName, lastUpdate);

            info.VesselType = Utilities.GetNodeValue(node, "vesselType", VesselType.Unknown);
            node.TryGetValue("numCrew", ref info.NumCrew);
            node.TryGetValue("numOccupiedParts", ref info.NumOccupiedParts);
            for (int i = 0; i < LoadGlobals.SubsystemArrayCache.Length; i++) // Enum.GetValues(typeof(Subsystem)).Length; i++)
            {
                string nme = ((Subsystem)i).ToString();
                node.TryGetValue(nme + "Toggle", ref info.SubsystemToggle[i]);
                node.TryGetValue(nme + "Drain", ref info.SubsystemDrain[i]);
            }
            for (int i = 0; i < LoadGlobals.GuiSectionArrayCache.Length; i++) // Enum.GetValues(typeof(GUISection)).Length; i++)
            {
                string nme = ((GUISection)i).ToString();
                node.TryGetValue(nme + "Flag", ref info.GuiSectionEnableFlag[i]);
            }
            node.TryGetValue("managerEnabled", ref info.ManagerEnabled);
            node.TryGetValue("ShowCrew", ref info.ShowCrew);
            node.TryGetValue("ShowParts", ref info.ShowParts);
            node.TryGetValue("hibernating", ref info.Hibernating);
            node.TryGetValue("EmgcyShutActive", ref info.EmgcyShutActive);
            node.TryGetValue("AutoPilotDisabled", ref info.AutoPilotDisabled);
            node.TryGetValue("AutoPilotDisTime", ref info.AutoPilotDisTime);
            node.TryGetValue("AutoPilotDisCounter", ref info.AutoPilotDisCounter);
            node.TryGetValue("timeLastElectricity", ref info.TimeLastElectricity);
            node.TryGetValue("EmgcyShutOverride", ref info.EmgcyShutOverride);
            node.TryGetValue("EmgcyShutOverrideStarted", ref info.EmgcyShutOverrideStarted);
            node.TryGetValue("Emergencypowerdownreset", ref info.Emergencypowerdownreset);
            node.TryGetValue("Emergencypowerdownresetprevactivated", ref info.Emergencypowerdownresetprevactivated);
            node.TryGetValue("Emergencypowerdownactivated", ref info.Emergencypowerdownactivated);
            node.TryGetValue("Emergencypowerdownprevactivated", ref info.Emergencypowerdownprevactivated);
            node.TryGetValue("ESPPriorityHighProcessed", ref info.ESPPriorityHighProcessed);
            node.TryGetValue("ESPPriorityMediumProcessed", ref info.ESPPriorityMediumProcessed);
            node.TryGetValue("ESPPriorityLowProcessed", ref info.ESPPriorityLowProcessed);
            node.TryGetValue("ESPPriorityHighResetProcessed", ref info.ESPPriorityHighResetProcessed);
            node.TryGetValue("ESPPriorityMediumResetProcessed", ref info.ESPPriorityMediumResetProcessed);
            node.TryGetValue("ESPPriorityLowResetProcessed", ref info.ESPPriorityLowResetProcessed);
            int tmpEspPriority = 1;
            node.TryGetValue("ESPPriority", ref tmpEspPriority);
            info.EspPriority = (ESPPriority)tmpEspPriority;
            node.TryGetValue("ReenableRcs", ref info.ReenableRcs);
            node.TryGetValue("ReenableSas", ref info.ReenableSas);
            node.TryGetValue("IsolateReservePower", ref info.IsolateReservePower);
            
            return info;
        }

        public ConfigNode Save(ConfigNode config)
        {
            ConfigNode node = config.AddNode(ConfigNodeName);
            node.AddValue("vesselName", VesselName);
            node.AddValue("vesselType", VesselType.ToString());
            node.AddValue("numCrew", NumCrew);
            node.AddValue("numOccupiedParts", NumOccupiedParts);

            for (int i = 0; i < LoadGlobals.SubsystemArrayCache.Length; i++) // Enum.GetValues(typeof(Subsystem)).Length; i++)
            {
                string nme = ((Subsystem)i).ToString();
                node.AddValue(nme + "Toggle", SubsystemToggle[i]);
                node.AddValue(nme + "Drain", SubsystemDrain[i]);
            }
            for (int i = 0; i < LoadGlobals.GuiSectionArrayCache.Length; i++) // Enum.GetValues(typeof(GUISection)).Length; i++)
            {
                string nme = ((GUISection)i).ToString();
                node.AddValue(nme + "Flag", GuiSectionEnableFlag[i]);
            }

            node.AddValue("managerEnabled", ManagerEnabled);
            node.AddValue("ShowCrew", ShowCrew);
            node.AddValue("ShowParts", ShowParts);
            node.AddValue("hibernating", Hibernating);
            node.AddValue("EmgcyShutActive", EmgcyShutActive);
            node.AddValue("AutoPilotDisabled", AutoPilotDisabled);
            node.AddValue("AutoPilotDisTime", AutoPilotDisTime);
            node.AddValue("AutoPilotDisCounter", AutoPilotDisCounter);
            node.AddValue("timeLastElectricity", TimeLastElectricity);
            node.AddValue("lastUpdate", LastUpdate);
            node.AddValue("EmgcyShutOverride", EmgcyShutOverride);
            node.AddValue("EmgcyShutOverrideStarted", EmgcyShutOverrideStarted);
            node.AddValue("Emergencypowerdownactivated", Emergencypowerdownactivated);
            node.AddValue("Emergencypowerdownreset", Emergencypowerdownreset);
            node.AddValue("Emergencypowerdownprevactivated", Emergencypowerdownprevactivated);
            node.AddValue("Emergencypowerdownresetprevactivated", Emergencypowerdownresetprevactivated);
            node.AddValue("ESPPriorityHighProcessed", ESPPriorityHighProcessed);
            node.AddValue("ESPPriorityMediumProcessed", ESPPriorityMediumProcessed);
            node.AddValue("ESPPriorityLowProcessed", ESPPriorityLowProcessed);
            node.AddValue("ESPPriorityHighResetProcessed", ESPPriorityHighResetProcessed);
            node.AddValue("ESPPriorityMediumResetProcessed", ESPPriorityMediumResetProcessed);
            node.AddValue("ESPPriorityLowResetProcessed", ESPPriorityLowResetProcessed);
            node.AddValue("EspPriority", (int)EspPriority);
            node.AddValue("ReenableRcs", ReenableRcs);
            node.AddValue("ReenableSas", ReenableSas);
            node.AddValue("IsolateReservePower", IsolateReservePower);
            
            return node;
        }

        public void ClearAmounts()
        {
            NumCrew = 0;
            NumOccupiedParts = 0;
        }
    }
}