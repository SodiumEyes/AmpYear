

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/**
* AYSettings.cs
* (C) Copyright 2015, Jamie Leighton
* AmpYear power management.
* The original code and concept of AmpYear rights go to SodiumEyes on the Kerbal Space Program Forums, which was covered by GNU License GPL (no version stated).
* As such this code continues to be covered by GNU GPL license.
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
namespace AY
{
    public class ESPValues
    {
        public bool EmergShutDnDflt ;

        public ESPPriority EmergShutPriority ;

        public ESPValues(bool emergShutDnDflt, ESPPriority emergShutPriority)
        {
            EmergShutDnDflt = emergShutDnDflt;
            EmergShutPriority = emergShutPriority;
        }
    }

    public class ValidEmergencyPartModule : IEquatable<ValidEmergencyPartModule>, IEquatable<string>
    {
        public string Name;
        public string displayName;

        public ValidEmergencyPartModule(string nme, string dispnme)
        {
            Name = nme;
            displayName = dispnme;
        }

        public bool Equals(ValidEmergencyPartModule other)
        {
            if (other != null)
            {
                return Name == other.Name;
            }
            return false;
        }

        public bool Equals(string other)
        {
            if (other != null)
            {
                return Name == other;
            }
            return false;
        }
    }
    
    public class AYSettings
    {
        private const string configNodeName = "AYSettings";

        public float FwindowPosX ;

        public float FwindowPosY ;

        public float EwindowPosX ;

        public float EwindowPosY ;

        public float SCwindowPosX ;

        public float SCwindowPosY ;

        public float EPLwindowPosX ;

        public float EPLwindowPosY ;

        public double RECHARGE_RESERVE_THRESHOLD ;

        public double POWER_LOW_WARNING_AMT ;

        public bool UseAppLauncher ;

        public bool debugging ;

        public bool TooltipsOn ;

        public List<ValidEmergencyPartModule> ValidPartModuleEmergShutDn;

        public List<KeyValuePair<ValidEmergencyPartModule, ESPValues>> PartModuleEmergShutDnDflt  ;

        public double ESPHighThreshold ;

        public double ESPMediumThreshold ;

        public double ESPLowThreshold ;

        public double EmgcyShutOverrideCooldown;

        public bool AYMonitoringUseEC;

        public bool showSI;

        public Color ProdPartHighlightColor;

        public Color ConsPartHighlightColor;

        public AYSettings()
        {
            FwindowPosX = 40;
            FwindowPosY = 50;
            EwindowPosX = 40;
            EwindowPosY = 50;
            SCwindowPosX = 40;
            SCwindowPosY = 50;
            EPLwindowPosX = 270;
            EPLwindowPosY = 50;
            RECHARGE_RESERVE_THRESHOLD = 0.95;
            POWER_LOW_WARNING_AMT = 5;
            UseAppLauncher = true;
            debugging = true;
            TooltipsOn = true;
            PartModuleEmergShutDnDflt = new List<KeyValuePair<ValidEmergencyPartModule, ESPValues>>();
            ValidPartModuleEmergShutDn = new List<ValidEmergencyPartModule>();
            SetupValidEmergencyParts();
            ESPHighThreshold = 5;
            ESPMediumThreshold = 10;
            ESPLowThreshold = 20;
            EmgcyShutOverrideCooldown = 300;
            showSI = false;
            AYMonitoringUseEC = true;
            ProdPartHighlightColor = Color.green;
            ConsPartHighlightColor = Color.red;
        }

        private void SetupValidEmergencyParts()
        {
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("RCS", "#autoLOC_AmpYear_1000160"));
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("SAS", "#autoLOC_AmpYear_1000159"));
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("AYSubsystems", "#autoLOC_AmpYear_1000269"));
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("ClimateControl", "#autoLOC_AmpYear_1000161"));
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("SmoothJazz", "#autoLOC_AmpYear_1000162"));
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("MassageChair", "#autoLOC_AmpYear_1000163"));
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("ModuleIONPoweredRCS", "#autoLOC_AmpYear_1000270"));
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("ModulePPTPoweredRCS", "#autoLOC_AmpYear_1000271"));
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("ModuleDeployableSolarPanel", "#autoLOC_AmpYear_1000272"));
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("ModuleWheel", "#autoLOC_AmpYear_1000273"));
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("ModuleLight", "#autoLOC_AmpYear_1000274"));
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("ModuleDataTransmitter", "#autoLOC_AmpYear_1000275"));
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("ModuleReactionWheel", "#autoLOC_AmpYear_1000276"));
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("ModuleResourceHarvester", "#autoLOC_AmpYear_1000277"));
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("ModuleResourceConverter", "#autoLOC_AmpYear_1000278"));
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("ModuleNavLight", "#autoLOC_AmpYear_1000279")); //AV Lights
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("CurvedSolarPanel", "#autoLOC_AmpYear_1000280")); //NFS
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("ModuleRTAntenna", "#autoLOC_AmpYear_1000281")); //Remote Tech
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("SCANsat", "#autoLOC_AmpYear_1000282")); //SCANsat
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("TacGenericConverter", "#autoLOC_AmpYear_1000283")); //TAC-LS
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("ModuleLimitedDataTransmitter","#autoLOC_AmpYear_1000284")); //Antenna Range
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("Scrubber", "#autoLOC_AmpYear_1000285")); //Kerbalism
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("Greenhouse", "#autoLOC_AmpYear_1000286")); //Kerbalism
            ValidPartModuleEmergShutDn.Add(new ValidEmergencyPartModule("GravityRing", "#autoLOC_AmpYear_1000287")); //Kerbalism
        }

        //Settings Functions Follow

        public void Load(ConfigNode node)
        {
            if (node.HasNode(configNodeName))
            {
                ConfigNode AYsettingsNode = new ConfigNode();
                if (!node.TryGetNode(configNodeName, ref AYsettingsNode)) return;
                AYsettingsNode.TryGetValue("FwindowPosX", ref this.FwindowPosX);
                AYsettingsNode.TryGetValue("FwindowPosY", ref FwindowPosY);
                AYsettingsNode.TryGetValue("EwindowPosX", ref EwindowPosX);
                AYsettingsNode.TryGetValue("EwindowPosY", ref EwindowPosY);
                AYsettingsNode.TryGetValue("SCwindowPosX", ref SCwindowPosX);
                AYsettingsNode.TryGetValue("SCwindowPosY", ref SCwindowPosY);
                AYsettingsNode.TryGetValue("EPLwindowPosX", ref EPLwindowPosX);
                AYsettingsNode.TryGetValue("EPLwindowPosY", ref EPLwindowPosY);
                AYsettingsNode.TryGetValue("RECHARGE_RESERVE_THRESHOLD", ref RECHARGE_RESERVE_THRESHOLD);
                AYsettingsNode.TryGetValue("POWER_LOW_WARNING_AMT", ref POWER_LOW_WARNING_AMT);
                AYsettingsNode.TryGetValue("UseAppLauncher", ref UseAppLauncher);
                AYsettingsNode.TryGetValue("debugging", ref debugging);
                RSTUtils.Utilities.debuggingOn = debugging;
                AYsettingsNode.TryGetValue("TooltipsOn", ref TooltipsOn);
                AYsettingsNode.TryGetValue("ShowSI", ref showSI);
                AYsettingsNode.TryGetValue("ESPHighThreshold", ref ESPHighThreshold);
                AYsettingsNode.TryGetValue("ESPMediumThreshold", ref ESPMediumThreshold);
                AYsettingsNode.TryGetValue("ESPLowThreshold", ref ESPLowThreshold);
                AYsettingsNode.TryGetValue("EmgcyShutOverrideCooldown", ref EmgcyShutOverrideCooldown);
                AYsettingsNode.TryGetValue("AYMonitoringUseEC", ref AYMonitoringUseEC);
                AYsettingsNode.TryGetValue("ProdPartHighlightColor", ref ProdPartHighlightColor);
                AYsettingsNode.TryGetValue("ConsPartHighlightColor", ref ConsPartHighlightColor);
                foreach (ValidEmergencyPartModule validentry in ValidPartModuleEmergShutDn)
                {
                    ESPValues tmpESPVals = new ESPValues(true, ESPPriority.MEDIUM);
                    string tmpStr = "";
                    if (AYsettingsNode.TryGetValue(validentry.Name, ref tmpStr))
                    {
                        string[] tmpStrStrings = tmpStr.Split(',');
                        if (tmpStrStrings.Length == 2)
                        {
                            bool tmpBool = false;
                            if (bool.TryParse(tmpStrStrings[0], out tmpBool))
                                tmpESPVals.EmergShutDnDflt = tmpBool;

                            int tmpInt = 2;
                            if (int.TryParse(tmpStrStrings[1], out tmpInt))
                                tmpESPVals.EmergShutPriority = (ESPPriority)tmpInt;
                        }
                    }
                    PartModuleEmergShutDnDflt.Add(new KeyValuePair<ValidEmergencyPartModule, ESPValues>(validentry, tmpESPVals));
                }
                Textures.SetupHighLightStyles(ProdPartHighlightColor, ConsPartHighlightColor);
                RSTUtils.Utilities.Log_Debug("AYSettings load complete");
            }
        }

        public void Save(ConfigNode node)
        {
            ConfigNode settingsNode;
            if (node.HasNode(configNodeName))
            {
                settingsNode = node.GetNode(configNodeName);
                settingsNode.ClearData();
            }
            else
            {
                settingsNode = node.AddNode(configNodeName);
            }
            settingsNode.AddValue("FwindowPosX", FwindowPosX);
            settingsNode.AddValue("FwindowPosY", FwindowPosY);
            settingsNode.AddValue("EwindowPosX", EwindowPosX);
            settingsNode.AddValue("EwindowPosY", EwindowPosY);
            settingsNode.AddValue("SCwindowPosX", SCwindowPosX);
            settingsNode.AddValue("SCwindowPosY", SCwindowPosY);
            settingsNode.AddValue("EPLwindowPosX", EPLwindowPosX);
            settingsNode.AddValue("EPLwindowPosY", EPLwindowPosY);
            settingsNode.AddValue("RECHARGE_RESERVE_THRESHOLD", RECHARGE_RESERVE_THRESHOLD);
            settingsNode.AddValue("POWER_LOW_WARNING_AMT", POWER_LOW_WARNING_AMT);
            settingsNode.AddValue("UseAppLauncher", UseAppLauncher);
            settingsNode.AddValue("debugging", debugging);
            settingsNode.AddValue("TooltipsOn", TooltipsOn);
            settingsNode.AddValue("ShowSI", showSI);
            settingsNode.AddValue("ESPHighThreshold", ESPHighThreshold);
            settingsNode.AddValue("ESPMediumThreshold", ESPMediumThreshold);
            settingsNode.AddValue("ESPLowThreshold", ESPLowThreshold);
            settingsNode.AddValue("EmgcyShutOverrideCooldown", EmgcyShutOverrideCooldown);
            settingsNode.AddValue("AYMonitoringUseEC", AYMonitoringUseEC);
            settingsNode.AddValue("ProdPartHighlightColor", ProdPartHighlightColor);
            settingsNode.AddValue("ConsPartHighlightColor", ConsPartHighlightColor);
            foreach (KeyValuePair<ValidEmergencyPartModule, ESPValues> validentry in PartModuleEmergShutDnDflt)
            {
                string tmpString = validentry.Value.EmergShutDnDflt.ToString() + ',' +
                                   (int)validentry.Value.EmergShutPriority;
                settingsNode.AddValue(validentry.Key.Name, tmpString);
            }
            RSTUtils.Utilities.Log_Debug("AYSettings save complete");
        }
    }
}