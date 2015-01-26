/**
 * AYGlobalSettings.cs
 * 
 * AmpYear power management. 
 * (C) Copyright 2015, Jamie Leighton
 * 
 * Parts of this code are based on:-
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP.IO;
using UnityEngine;

namespace AY
{
    public class AYGlobalSettings
    {
        private const string configNodeName = "GlobalSettings";

        public float FwindowPosX { get; set; }
        public float FwindowPosY { get; set; }
        public float EwindowPosX { get; set; }
        public float EwindowPosY { get; set; }
        public float SCwindowPosX { get; set; }
        public float SCwindowPosY { get; set; }
        public double HEATER_BASE_DRAIN_FACTOR { get; set; }
        public float HEATER_TARGET_TEMP { get; set; }
        public float COOLER_TARGET_TEMP { get; set; }
        public double MASSAGE_BASE_DRAIN_FACTOR { get; set; }
        public double RECHARGE_RESERVE_THRESHOLD { get; set; }
        public bool debugging { get; set; }


        public AYGlobalSettings()
        {
            FwindowPosX = 40;
            FwindowPosY = 50;
            EwindowPosX = 40;
            EwindowPosY = 50;
            SCwindowPosX = 40;
            SCwindowPosY = 50;
            HEATER_BASE_DRAIN_FACTOR = 1.0;
            HEATER_TARGET_TEMP = 20.0f;
            COOLER_TARGET_TEMP = 15.0f;
            MASSAGE_BASE_DRAIN_FACTOR = 3.0;
            RECHARGE_RESERVE_THRESHOLD = 0.95;
            debugging = false;
        }

    //Settings Functions Follow

        public void Load(ConfigNode node)
        {
            if (node.HasNode(configNodeName))
            {
                ConfigNode settingsNode = node.GetNode(configNodeName);                
                FwindowPosX = Utilities.GetValue(settingsNode, "FwindowPosX", FwindowPosX);
                FwindowPosY = Utilities.GetValue(settingsNode, "FwindowPosY", FwindowPosY);
                EwindowPosX = Utilities.GetValue(settingsNode, "EwindowPosX", EwindowPosX);
                EwindowPosY = Utilities.GetValue(settingsNode, "EwindowPosY", EwindowPosY);
                SCwindowPosX = Utilities.GetValue(settingsNode, "SCwindowPosX", SCwindowPosX);
                SCwindowPosY = Utilities.GetValue(settingsNode, "SCwindowPosY", SCwindowPosY);
                HEATER_BASE_DRAIN_FACTOR = Utilities.GetValue(settingsNode, "HEATER_BASE_DRAIN_FACTOR", HEATER_BASE_DRAIN_FACTOR);
                HEATER_TARGET_TEMP = Utilities.GetValue(settingsNode, "HEATER_TARGET_TEMP", HEATER_TARGET_TEMP);
                COOLER_TARGET_TEMP = Utilities.GetValue(settingsNode, "COOLER_TARGET_TEMP", COOLER_TARGET_TEMP);
                MASSAGE_BASE_DRAIN_FACTOR = Utilities.GetValue(settingsNode, "MASSAGE_BASE_DRAIN_FACTOR", MASSAGE_BASE_DRAIN_FACTOR);
                RECHARGE_RESERVE_THRESHOLD = Utilities.GetValue(settingsNode, "RECHARGE_RESERVE_THRESHOLD", RECHARGE_RESERVE_THRESHOLD);
                debugging = Utilities.GetValue(settingsNode, "debugging", debugging);
                Utilities.LogFormatted("AYGlobalsettings globalsettings load complete");
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
            settingsNode.AddValue("HEATER_BASE_DRAIN_FACTOR", HEATER_BASE_DRAIN_FACTOR);
            settingsNode.AddValue("HEATER_TARGET_TEMP", HEATER_TARGET_TEMP);
            settingsNode.AddValue("COOLER_TARGET_TEMP", COOLER_TARGET_TEMP);
            settingsNode.AddValue("MASSAGE_BASE_DRAIN_FACTOR", MASSAGE_BASE_DRAIN_FACTOR);
            settingsNode.AddValue("RECHARGE_RESERVE_THRESHOLD", RECHARGE_RESERVE_THRESHOLD);
            settingsNode.AddValue("debugging", debugging);
            Utilities.LogFormatted("AYGlobalsettings globalsettings save complete");
        }       
    }         
}
