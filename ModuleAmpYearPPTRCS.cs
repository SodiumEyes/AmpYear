/**
 * ModuleAmpYearPPTRCS.cs
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

using UnityEngine;

namespace AY
{
    public class ModuleAmpYearPPTRCS : ModuleRCS
    {
        //private Propellant definition;
        private string ElecChge = "ElectricCharge";

        private string Teflon = "Teflon";

        // New context menu info
        [KSPField(isPersistant = true, guiName = "AmpYear Managed", guiActive = true)]
        public bool isManaged = false;

        [KSPField(isPersistant = true, guiName = "Teflon Ratio", guiUnits = "U/s", guiFormat = "F3", guiActive = true)]
        public float teflonRatio = 0f;

        [KSPField(isPersistant = true, guiName = "Power Ratio", guiUnits = "U/s", guiFormat = "F3", guiActive = true)]
        public float powerRatio = 0f;

        public float electricityUse
        {
            get
            {
                float ElecUsedTmp = 0f;
                foreach (Propellant propellant in this.propellants)
                {
                    if (propellant.name == ElecChge)
                    {
                        ElecUsedTmp = (float)propellant.currentRequirement;
                    }
                }
                return ElecUsedTmp;
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            if (!node.HasNode("PROPELLANT") && node.HasValue("resourceName") && (propellants == null || propellants.Count == 0))
            {
                ConfigNode c = new ConfigNode("PROPELLANT");
                c.SetValue("name", node.GetValue("resourceName"));
                if (node.HasValue("ratio"))
                    c.SetValue("ratio", node.GetValue("ratio"));
                else
                    c.SetValue("ratio", "1.0");
                if (node.HasValue("resourceFlowMode"))
                    c.SetValue("resourceFlowMode", node.GetValue("resourceFlowMode"));
                else
                    c.SetValue("resourceFlowMode", "ALL_VESSEL");
                node.AddNode(c);
            }
            base.OnLoad(node);

            foreach (Propellant propellant in propellants)
            {                
                if (propellant.name == ElecChge)
                {
                    powerRatio = propellant.ratio;
                }
                if (propellant.name == Teflon)
                {
                    teflonRatio = propellant.ratio;
                }
            }
            G = 9.80665f;
        }

        public override string GetInfo()
        {
            string text = base.GetInfo();
            return text;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
        }

        public override void OnUpdate()
        {
            if (isManaged) base.OnUpdate();
        }

        public override void OnFixedUpdate()
        {
            if (isManaged) base.OnFixedUpdate();
        }

        public static void Log_Debug(string context, string message)
        {
            Debug.Log(context + "[][" + Time.time.ToString("0.00") + "]: " + message);
        }
    }
}