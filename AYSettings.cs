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
    public class AYSettings
    {
        private const string configNodeName = "AYSettings";
        public float FwindowPosX { get; set; }
        public float FwindowPosY { get; set; }
        public float EwindowPosX { get; set; }
        public float EwindowPosY { get; set; }
        public float SCwindowPosX { get; set; }
        public float SCwindowPosY { get; set; }
        public double CLIMATE_BASE_DRAIN_FACTOR { get; set; }
        public float CLIMATE_TARGET_TEMP { get; set; }        
        public double MASSAGE_BASE_DRAIN_FACTOR { get; set; }
        public double RECHARGE_RESERVE_THRESHOLD { get; set; }
        public bool Craziness_Function { get; set; }
        public double CRAZY_BASE_DRAIN_FACTOR { get; set; }
        public double CRAZY_CLIMATE_UNCOMF_FACTOR { get; set; }
        public double CRAZY_CLIMATE_REDUCE_FACTOR { get; set; }
        public double CRAZY_RADIO_REDUCE_FACTOR { get; set; }
        public double CRAZY_MASSAGE_REDUCE_FACTOR { get; set; }
        public double CRAZY_MINOR_LIMIT { get; set; }
        public double CRAZY_MAJOR_LIMIT { get; set; }
        public bool UseAppLauncher { get; set; }
        public bool debugging { get; set; }

        public AYSettings()
        {
            FwindowPosX = 40;
            FwindowPosY = 50;
            EwindowPosX = 40;
            EwindowPosY = 50;
            SCwindowPosX = 40;
            SCwindowPosY = 50;
            CLIMATE_BASE_DRAIN_FACTOR = 1.0;
            CLIMATE_TARGET_TEMP = 20.0f;            
            MASSAGE_BASE_DRAIN_FACTOR = 3.0;
            RECHARGE_RESERVE_THRESHOLD = 0.95;
            Craziness_Function = true;
            CRAZY_BASE_DRAIN_FACTOR = 0.05;
            CRAZY_CLIMATE_UNCOMF_FACTOR = 0.02;
            CRAZY_CLIMATE_REDUCE_FACTOR = 0.1;
            CRAZY_RADIO_REDUCE_FACTOR = 0.1;
            CRAZY_MASSAGE_REDUCE_FACTOR = 0.2;
            CRAZY_MINOR_LIMIT = 59;
            CRAZY_MAJOR_LIMIT = 89;
            UseAppLauncher = true;
            debugging = false;
        }

        //Settings Functions Follow

        public void Load(ConfigNode node)
        {
            if (node.HasNode(configNodeName))
            {
                ConfigNode AYsettingsNode = node.GetNode(configNodeName);
                FwindowPosX = Utilities.GetNodeValue(AYsettingsNode, "FwindowPosX", FwindowPosX);
                FwindowPosY = Utilities.GetNodeValue(AYsettingsNode, "FwindowPosY", FwindowPosY);
                EwindowPosX = Utilities.GetNodeValue(AYsettingsNode, "EwindowPosX", EwindowPosX);
                EwindowPosY = Utilities.GetNodeValue(AYsettingsNode, "EwindowPosY", EwindowPosY);
                SCwindowPosX = Utilities.GetNodeValue(AYsettingsNode, "SCwindowPosX", SCwindowPosX);
                SCwindowPosY = Utilities.GetNodeValue(AYsettingsNode, "SCwindowPosY", SCwindowPosY);
                CLIMATE_BASE_DRAIN_FACTOR = Utilities.GetNodeValue(AYsettingsNode, "CLIMATE_BASE_DRAIN_FACTOR", CLIMATE_BASE_DRAIN_FACTOR);
                CLIMATE_TARGET_TEMP = Utilities.GetNodeValue(AYsettingsNode, "CLIMATE_TARGET_TEMP", CLIMATE_TARGET_TEMP);
                MASSAGE_BASE_DRAIN_FACTOR = Utilities.GetNodeValue(AYsettingsNode, "MASSAGE_BASE_DRAIN_FACTOR", MASSAGE_BASE_DRAIN_FACTOR);
                RECHARGE_RESERVE_THRESHOLD = Utilities.GetNodeValue(AYsettingsNode, "RECHARGE_RESERVE_THRESHOLD", RECHARGE_RESERVE_THRESHOLD);
                Craziness_Function = Utilities.GetNodeValue(AYsettingsNode, "Craziness_Function", Craziness_Function);
                CRAZY_BASE_DRAIN_FACTOR = Utilities.GetNodeValue(AYsettingsNode, "CRAZY_BASE_DRAIN_FACTOR", CRAZY_BASE_DRAIN_FACTOR);
                CRAZY_CLIMATE_UNCOMF_FACTOR = Utilities.GetNodeValue(AYsettingsNode, "CRAZY_CLIMATE_UNCOMF_FACTOR", CRAZY_CLIMATE_UNCOMF_FACTOR);
                CRAZY_CLIMATE_REDUCE_FACTOR = Utilities.GetNodeValue(AYsettingsNode, "CRAZY_CLIMATE_REDUCE_FACTOR", CRAZY_CLIMATE_REDUCE_FACTOR);
                CRAZY_RADIO_REDUCE_FACTOR = Utilities.GetNodeValue(AYsettingsNode, "CRAZY_RADIO_REDUCE_FACTOR", CRAZY_RADIO_REDUCE_FACTOR);
                CRAZY_MASSAGE_REDUCE_FACTOR = Utilities.GetNodeValue(AYsettingsNode, "CRAZY_MASSAGE_REDUCE_FACTOR", CRAZY_MASSAGE_REDUCE_FACTOR);
                CRAZY_MINOR_LIMIT = Utilities.GetNodeValue(AYsettingsNode, "CRAZY_MINOR_LIMIT", CRAZY_MINOR_LIMIT);
                CRAZY_MAJOR_LIMIT = Utilities.GetNodeValue(AYsettingsNode, "CRAZY_MAJOR_LIMIT", CRAZY_MAJOR_LIMIT);
                UseAppLauncher = Utilities.GetNodeValue(AYsettingsNode, "UseAppLauncher", UseAppLauncher);
                debugging = Utilities.GetNodeValue(AYsettingsNode, "debugging", debugging);
                this.Log_Debug( "AYSettings AYSettings load complete");
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
            settingsNode.AddValue("CLIMATE_BASE_DRAIN_FACTOR", CLIMATE_BASE_DRAIN_FACTOR);
            settingsNode.AddValue("CLIMATE_TARGET_TEMP", CLIMATE_TARGET_TEMP);            
            settingsNode.AddValue("MASSAGE_BASE_DRAIN_FACTOR", MASSAGE_BASE_DRAIN_FACTOR);
            settingsNode.AddValue("Craziness_Function", Craziness_Function);
            settingsNode.AddValue("CRAZY_BASE_DRAIN_FACTOR", CRAZY_BASE_DRAIN_FACTOR);
            settingsNode.AddValue("CRAZY_CLIMATE_UNCOMF_FACTOR", CRAZY_CLIMATE_UNCOMF_FACTOR);
            settingsNode.AddValue("CRAZY_CLIMATE_REDUCE_FACTOR", CRAZY_CLIMATE_REDUCE_FACTOR);
            settingsNode.AddValue("CRAZY_RADIO_REDUCE_FACTOR", CRAZY_RADIO_REDUCE_FACTOR);
            settingsNode.AddValue("CRAZY_MASSAGE_REDUCE_FACTOR", CRAZY_MASSAGE_REDUCE_FACTOR);
            settingsNode.AddValue("CRAZY_MINOR_LIMIT", CRAZY_MINOR_LIMIT);
            settingsNode.AddValue("CRAZY_MAJOR_LIMIT", CRAZY_MAJOR_LIMIT);
            settingsNode.AddValue("UseAppLauncher", UseAppLauncher);
            settingsNode.AddValue("debugging", debugging);
            this.Log_Debug( "AYSettings AYSettings save complete");
        }
    }
}