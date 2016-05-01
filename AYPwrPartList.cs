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

using System.Collections.Generic;
using System.Linq;

namespace AY
{
    public class PwrPartList
    {

        public const string ConfigNodeName = "AYPwrPartList";

        public string PrtName { get; set; }
        public string PrtModuleName { get; set; }
        public bool PrtSubsystem { get; set; }
        public string PrtPower { get; set; }
        public float PrtPowerF { get; set; }
        public bool PrtActive { get; set; }
        private bool _prtEditorInclude;

        public bool PrtEditorInclude
        {
            get { return _prtEditorInclude; }
            set { _prtEditorInclude = value; }
        }

        private bool _prtUserEditorInclude;

        public bool PrtUserEditorInclude
        {
            get { return _prtUserEditorInclude; }
            set { _prtUserEditorInclude = value; }
        }

        private bool _prtSolarDependant;

        public bool PrtSolarDependant
        {
            get { return _prtSolarDependant; }
            set { _prtSolarDependant = value; }
        }

        private bool _validprtEmergShutDn;

        public bool ValidprtEmergShutDn
        {
            get { return _validprtEmergShutDn; }
            set { _validprtEmergShutDn = value; }
        }

        private bool _prtEmergShutDnInclude;

        public bool PrtEmergShutDnInclude
        {
            get { return _prtEmergShutDnInclude; }
            set { _prtEmergShutDnInclude = value; }
        }

        private bool _prtPreEmergShutDnStateActive;

        public bool PrtPreEmergShutDnStateActive
        {
            get { return _prtPreEmergShutDnStateActive; }
            set { _prtPreEmergShutDnStateActive = value; }
        }

        private ESPPriority _prtEmergShutDnPriority;

        public ESPPriority PrtEmergShutDnPriority
        {
            get { return _prtEmergShutDnPriority; }
            set { _prtEmergShutDnPriority = value; }
        }

        public PwrPartList(string prtName, string prtModuleName, bool prtSubsystem, string prtPower, float prtPowerF, bool prtActive,
            bool prtSolarDependant)
        {
            PrtName = prtName;
            PrtModuleName = prtModuleName;
            PrtSubsystem = prtSubsystem;
            PrtPower = prtPower;
            PrtPowerF = prtPowerF;
            PrtActive = prtActive;
            PrtSolarDependant = prtSolarDependant;
            PrtEditorInclude = true;
            PrtUserEditorInclude = true;
            ValidprtEmergShutDn = AYSettings.ValidPartModuleEmergShutDn.Contains(prtModuleName);
            PrtEmergShutDnInclude = ValidprtEmergShutDn;
            PrtPreEmergShutDnStateActive = prtActive;
            KeyValuePair<string, ESPValues> tmpEspPair = AmpYear.Instance.AYsettings.PartModuleEmergShutDnDflt
                .FirstOrDefault(
                    a => a.Key == prtModuleName);
            PrtEmergShutDnPriority = tmpEspPair.Key != null ? tmpEspPair.Value.EmergShutPriority : ESPPriority.MEDIUM;
        }

        public static PwrPartList Load(ConfigNode node)
        {
            string prtName = "", prtModuleName = "", prtPower = "";
            bool prtSubsystem = false, prtActive = false, prtSolarDependant = false;
            float prtPowerF = 0f;
            node.TryGetValue("PrtName", ref prtName);
            node.TryGetValue("PrtModuleName", ref prtModuleName);
            node.TryGetValue("PrtPower", ref prtPower);
            node.TryGetValue("PrtPowerF", ref prtPowerF);
            node.TryGetValue("PrtActive", ref prtActive);
            node.TryGetValue("PrtSolarDependant", ref prtSolarDependant);
            
            PwrPartList info = new PwrPartList(prtName, prtModuleName, prtSubsystem, prtPower, prtPowerF, prtActive, prtSolarDependant);

            node.TryGetValue("PrtEditorInclude", ref info._prtEditorInclude);
            node.TryGetValue("PrtUserEditorInclude", ref info._prtUserEditorInclude);
            node.TryGetValue("PrtValidprtEmergShutDn", ref info._validprtEmergShutDn);
            node.TryGetValue("PrtEmergShutDnInclude", ref info._prtEmergShutDnInclude);
            node.TryGetValue("PrtPreEmergShutDnStateActive", ref info._prtPreEmergShutDnStateActive);
            int tmpEspPriority = 1;
            node.TryGetValue("PrtEmergShutDnPriority", ref tmpEspPriority);
            info._prtEmergShutDnPriority = (ESPPriority) tmpEspPriority;

            return info;
            
        }

        internal ConfigNode Save(ConfigNode config)
        {
            ConfigNode node = config.AddNode(ConfigNodeName);
            node.AddValue("PrtName", PrtName);
            node.AddValue("PrtModuleName", PrtModuleName);
            node.AddValue("PrtSubsystem", PrtSubsystem);
            node.AddValue("PrtPower", PrtPower);
            node.AddValue("PrtPowerF", PrtPowerF);
            node.AddValue("PrtActive", PrtActive);
            node.AddValue("PrtSolarDependant", _prtSolarDependant);
            node.AddValue("PrtEditorInclude", _prtEditorInclude);
            node.AddValue("PrtUserEditorInclude", _prtUserEditorInclude);
            node.AddValue("PrtValidprtEmergShutDn", _validprtEmergShutDn);
            node.AddValue("PrtEmergShutDnInclude", _prtEmergShutDnInclude);
            node.AddValue("PrtPreEmergShutDnStateActive", _prtPreEmergShutDnStateActive);
            node.AddValue("PrtEmergShutDnPriority", (int)_prtEmergShutDnPriority);

            return node;
        }
    }
}
