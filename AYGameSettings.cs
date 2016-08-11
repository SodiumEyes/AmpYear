/**
 * AYVesselSettings.cs
 *
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

using System;
using System.Collections.Generic;

namespace AY
{
    public class AYGameSettings
    {
        private const string configNodeName = "AYGameSettings";

        public bool Enabled;

        public Dictionary<Guid, VesselInfo> KnownVessels { get; set; }

        public AYGameSettings()
        {
            Enabled = true;
            KnownVessels = new Dictionary<Guid, VesselInfo>();
        }

        public void Load(ConfigNode node)
        {
            KnownVessels.Clear();
            if (node.HasNode(configNodeName))
            {
                ConfigNode AYsettingsNode = node.GetNode(configNodeName);

                node.TryGetValue("Enabled", ref Enabled);

                KnownVessels.Clear();
                var vesselNodes = AYsettingsNode.GetNodes(VesselInfo.ConfigNodeName);
                foreach (ConfigNode vesselNode in vesselNodes)
                {
                    if (vesselNode.HasValue("Guid"))
                    {
                        //String id = vesselNode.GetValue("Guid");
                        Guid id = new Guid(vesselNode.GetValue("Guid"));
                        RSTUtils.Utilities.Log_Debug("AYGameSettings Loading Guid = {0}" , id.ToString());
                        VesselInfo vesselInfo = VesselInfo.Load(vesselNode);
                        KnownVessels[id] = vesselInfo;
                    }
                }
            }
            RSTUtils.Utilities.Log_Debug("AYGameSettings Loading Complete");
        }

        public void Save(ConfigNode node)
        {
            var settingsNode = node.HasNode(configNodeName) ? node.GetNode(configNodeName) : node.AddNode(configNodeName);

            settingsNode.AddValue("Enabled", Enabled);

            foreach (var entry in KnownVessels)
            {
                ConfigNode vesselNode = entry.Value.Save(settingsNode);
                RSTUtils.Utilities.Log_Debug("AYGameSettings Saving Guid = {0}" , entry.Key.ToString());
                vesselNode.AddValue("Guid", entry.Key);
            }
            RSTUtils.Utilities.Log_Debug("AYGameSettings Saving Complete");
        }
    }
}