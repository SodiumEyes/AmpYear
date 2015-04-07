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

        public double RECHARGE_RESERVE_THRESHOLD { get; set; }

        public double POWER_LOW_WARNING_AMT { get; set; }

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
            RECHARGE_RESERVE_THRESHOLD = 0.95;
            POWER_LOW_WARNING_AMT = 5;
            UseAppLauncher = true;
            debugging = true;
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
                RECHARGE_RESERVE_THRESHOLD = Utilities.GetNodeValue(AYsettingsNode, "RECHARGE_RESERVE_THRESHOLD", RECHARGE_RESERVE_THRESHOLD);
                POWER_LOW_WARNING_AMT = Utilities.GetNodeValue(AYsettingsNode, "POWER_LOW_WARNING_AMT", POWER_LOW_WARNING_AMT);
                UseAppLauncher = Utilities.GetNodeValue(AYsettingsNode, "UseAppLauncher", UseAppLauncher);
                debugging = Utilities.GetNodeValue(AYsettingsNode, "debugging", debugging);
                this.Log_Debug("AYSettings load complete");
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
            settingsNode.AddValue("RECHARGE_RESERVE_THRESHOLD", RECHARGE_RESERVE_THRESHOLD);
            settingsNode.AddValue("POWER_LOW_WARNING_AMT", POWER_LOW_WARNING_AMT);
            settingsNode.AddValue("UseAppLauncher", UseAppLauncher);
            settingsNode.AddValue("debugging", debugging);
            this.Log_Debug("AYSettings save complete");
        }
    }
}