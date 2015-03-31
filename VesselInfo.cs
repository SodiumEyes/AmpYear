/**
 * * AmpYear power management.
 * (C) Copyright 2015, Jamie Leighton
 * The original code and concept of AmpYear rights go to SodiumEyes on the Kerbal Space Program Forums, which was covered by GNU License GPL (no version stated).
 * As such this code continues to be covered by GNU GPL license.
 * Based On:-
 * Thunder Aerospace Corporation's Life Support for Kerbal Space Program.
 * Written by Taranis Elsu. *
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
 * Note that Thunder Aerospace Corporation is a ficticious entity created for entertainment
 * purposes. It is in no way meant to represent a real entity. Any similarity to a real entity
 * is purely coincidental.
 */

using System;

namespace AY
{
    public class VesselInfo
    {
        public const string ConfigNodeName = "VesselInfo";

        public string vesselName;
        public VesselType vesselType = VesselType.Unknown;
        public int numCrew;
        public int numOccupiedParts;
        public bool[] subsystemToggle = new bool[Enum.GetValues(typeof(Subsystem)).Length];
        public double[] subsystemDrain = new double[Enum.GetValues(typeof(Subsystem)).Length];
        public bool[] guiSectionEnableFlag = new bool[Enum.GetValues(typeof(GUISection)).Length];
        public bool managerEnabled = true;
        public bool ShowCrew = false;
        public bool ShowParts = false;
        public bool hibernating;
        public bool EmgcyShutActive = false;
        public bool AutoPilotDisabled = false;
        public double AutoPilotDisTime = 0f;
        public double AutoPilotDisCounter = 0f;

        public VesselInfo(string vesselName, double currentTime)
        {
            this.vesselName = vesselName;
            hibernating = false;
        }

        public static VesselInfo Load(ConfigNode node)
        {
            string vesselName = Utilities.GetNodeValue(node, "vesselName", "Unknown");
            double lastUpdate = Utilities.GetNodeValue(node, "lastUpdate", 0.0);

            VesselInfo info = new VesselInfo(vesselName, lastUpdate);
            info.vesselType = Utilities.GetNodeValue(node, "vesselType", VesselType.Unknown);
            info.numCrew = Utilities.GetNodeValue(node, "numCrew", 0);
            info.numOccupiedParts = Utilities.GetNodeValue(node, "numOccupiedParts", 0);
            for (int i = 0; i < Enum.GetValues(typeof(Subsystem)).Length; i++)
            {
                string Nme = ((Subsystem)i).ToString();
                info.subsystemToggle[i] = Utilities.GetNodeValue(node, Nme + "Toggle", false);
                info.subsystemDrain[i] = Utilities.GetNodeValue(node, Nme + "Drain", 0);
            }
            for (int i = 0; i < Enum.GetValues(typeof(GUISection)).Length; i++)
            {
                string Nme = ((GUISection)i).ToString();
                info.guiSectionEnableFlag[i] = Utilities.GetNodeValue(node, Nme + "Flag", false);
            }
            info.managerEnabled = Utilities.GetNodeValue(node, "managerEnabled", true);
            info.ShowCrew = Utilities.GetNodeValue(node, "ShowCrew", false);
            info.ShowParts = Utilities.GetNodeValue(node, "ShowParts", false);
            info.hibernating = Utilities.GetNodeValue(node, "hibernating", false);
            info.EmgcyShutActive = Utilities.GetNodeValue(node, "EmgcyShutActive", false);
            info.AutoPilotDisabled = Utilities.GetNodeValue(node, "AutoPilotDisabled", false);
            info.AutoPilotDisTime = Utilities.GetNodeValue(node, "AutoPilotDisTime", 0f);
            info.AutoPilotDisCounter = Utilities.GetNodeValue(node, "AutoPilotDisCounter", 0f);

            return info;
        }

        public ConfigNode Save(ConfigNode config)
        {
            ConfigNode node = config.AddNode(ConfigNodeName);
            node.AddValue("vesselName", vesselName);
            node.AddValue("vesselType", vesselType.ToString());
            node.AddValue("numCrew", numCrew);
            node.AddValue("numOccupiedParts", numOccupiedParts);

            for (int i = 0; i < Enum.GetValues(typeof(Subsystem)).Length; i++)
            {
                string Nme = ((Subsystem)i).ToString();
                node.AddValue(Nme + "Toggle", subsystemToggle[i]);
                node.AddValue(Nme + "Drain", subsystemDrain[i]);
            }
            for (int i = 0; i < Enum.GetValues(typeof(GUISection)).Length; i++)
            {
                string Nme = ((GUISection)i).ToString();
                node.AddValue(Nme + "Flag", guiSectionEnableFlag[i]);
            }

            node.AddValue("managerEnabled", managerEnabled);
            node.AddValue("ShowCrew", ShowCrew);
            node.AddValue("ShowParts", ShowParts);
            node.AddValue("hibernating", hibernating);
            node.AddValue("EmgcyShutActive", EmgcyShutActive);
            node.AddValue("AutoPilotDisabled", AutoPilotDisabled);
            node.AddValue("AutoPilotDisTime", AutoPilotDisTime);
            node.AddValue("AutoPilotDisCounter", AutoPilotDisCounter);
            return node;
        }

        public void ClearAmounts()
        {
            numCrew = 0;
            numOccupiedParts = 0;
        }
    }
}