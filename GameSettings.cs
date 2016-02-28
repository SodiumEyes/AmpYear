/**
 * GameSettings.cs
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
using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AY
{
    public class AYGameSettings
    {
        private const string configNodeName = "SavedGameSettings";
                
        public bool Enabled { get; set; }        

        public Dictionary<Guid, VesselInfo> knownVessels { get; private set; }

        public AYGameSettings()
        {
            Enabled = true;                 
            knownVessels = new Dictionary<Guid, VesselInfo>();
        }

        public void Load(ConfigNode node)
        {
            knownVessels.Clear();
            if (node.HasNode(configNodeName))
            {
                ConfigNode settingsNode = node.GetNode(configNodeName);
                                
                Enabled = Utilities.GetValue(settingsNode, "Enabled", Enabled);               
                                
                knownVessels.Clear();
                var vesselNodes = settingsNode.GetNodes(VesselInfo.ConfigNodeName);
                foreach (ConfigNode vesselNode in vesselNodes)
                {
                    if (vesselNode.HasValue("Guid"))
                    {
                        Guid id = new Guid(vesselNode.GetValue("Guid"));
                        Utilities.LogFormatted("AYGameSettings Loading Guid = " + id);
                        VesselInfo vesselInfo = VesselInfo.Load(vesselNode);
                        knownVessels[id] = vesselInfo;
                    }
                }
            }
            Utilities.LogFormatted("AYGameSettings Loading Complete");
        }

        public void Save(ConfigNode node)
        {
            ConfigNode settingsNode;
            if (node.HasNode(configNodeName))
            {
                settingsNode = node.GetNode(configNodeName);
            }
            else
            {
                settingsNode = node.AddNode(configNodeName);
            }
                        
            settingsNode.AddValue("Enabled", Enabled);
            
            foreach (var entry in knownVessels)
            {
                ConfigNode vesselNode = entry.Value.Save(settingsNode);
                Utilities.LogFormatted("AYGameSettings Saviong Guid = " + entry.Key);
                vesselNode.AddValue("Guid", entry.Key);
                
            }
            Utilities.LogFormatted("AYGameSettings Saving Complete");
        }
    }
}
