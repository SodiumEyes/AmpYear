

using System.Collections.Generic;
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
    
    public class AYSettings
    {

        public static string[] ValidPartModuleEmergShutDn = new string[]
        {
            "RCS",
            "SAS",
            "AYSubsystems",
            "Climate Control",
            "Smooth Jazz",
            "Massage Chair",
            "ModuleIONPoweredRCS",
            "ModulePPTPoweredRCS",
            "ModuleDeployableSolarPanel",
            "ModuleWheel",
            "ModuleLight",
            "ModuleDataTransmitter",
            "ModuleReactionWheel",
        //   "ModuleScienceLab",
        //   "ModuleScienceConverter",
            "ModuleResourceHarvester",
            "ModuleResourceConverter",
            "ModuleNavLight", //AV Lights
            "Curved Solar Panel", //NFS
        //   "KASModuleWinch", //KAS
        //   "KASModuleMagnet", //KAS
           "ModuleRTAntenna", //Remote Tech
           "SCANsat",  //SCANsat
           "TacGenericConverter", //TAC-LS
           "ModuleLimitedDataTransmitter", //Antenna Range
           "Scrubber",  //Kerbalism
           "Greenhouse", //Kerbalism
           "GravityRing" //Kerbalism
        };

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

        public List<KeyValuePair<string, ESPValues>> PartModuleEmergShutDnDflt  ;

        public double ESPHighThreshold ;

        public double ESPMediumThreshold ;

        public double ESPLowThreshold ;

        public double EmgcyShutOverrideCooldown;

        public bool AYMonitoringUseEC;

        public bool showSI;

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
            PartModuleEmergShutDnDflt = new List<KeyValuePair<string, ESPValues>>();
            ESPHighThreshold = 5;
            ESPMediumThreshold = 10;
            ESPLowThreshold = 20;
            EmgcyShutOverrideCooldown = 300;
            showSI = false;
            AYMonitoringUseEC = true;
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
                foreach (string validentry in ValidPartModuleEmergShutDn)
                {
                    ESPValues tmpESPVals = new ESPValues(true, ESPPriority.MEDIUM);
                    string tmpStr = "";
                    if (AYsettingsNode.TryGetValue(validentry, ref tmpStr))
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
                    PartModuleEmergShutDnDflt.Add(new KeyValuePair<string, ESPValues>(validentry, tmpESPVals));
                }
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
            foreach (KeyValuePair<string, ESPValues> validentry in PartModuleEmergShutDnDflt)
            {
                string tmpString = validentry.Value.EmergShutDnDflt.ToString() + ',' +
                                   (int)validentry.Value.EmergShutPriority;
                settingsNode.AddValue(validentry.Key, tmpString);
            }
            RSTUtils.Utilities.Log_Debug("AYSettings save complete");
        }
    }
}