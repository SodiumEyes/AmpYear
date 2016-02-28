/**
 * AmpYear.cs
 *
 * AmpYear power management.
 * (C) Copyright 2015, Jamie Leighton
 * The original code and concept of AmpYear rights go to SodiumEyes on the Kerbal Space Program Forums, which was covered by GNU License GPL (no version stated).
 * As such this code continues to be covered by GNU GPL license.
 * Concepts which are common to the Game Kerbal Space Program for which there are common code interfaces as such some of those concepts used
 * by this program were based on:-
 * Thunder Aerospace Corporation's Life Support for Kerbal Space Program.
 * Written by Taranis Elsu.
 * (C) Copyright 2013, Taranis Elsu
 * Which is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
 * creative commons license. See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>
 * for full details.
 *
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
using System.Linq;
using UnityEngine;

namespace AY
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class LoadGUI : MonoBehaviour
    {
        //Awake Event - when the DLL is loaded
        public void Awake()
        {            
            Textures.loadIconAssets();
            Utilities.Log("AmpYear LoadGUI", " Awake Complete");
        }
    }

    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class AddScenarioModules : MonoBehaviour
    {
        private void Start()
        {
            var Currentgame = HighLogic.CurrentGame;
            Utilities.Log("AmpYear  AddScenarioModules", " ScenarioModules Start");
            ProtoScenarioModule protoscenmod = Currentgame.scenarios.Find(s => s.moduleName == typeof(AmpYear).Name);

            if (protoscenmod == null)
            {
                Utilities.Log("AmpYear AddScenarioModules", " Adding the scenario module.");
                protoscenmod = Currentgame.AddProtoScenarioModule(typeof(AmpYear), GameScenes.SPACECENTER, GameScenes.FLIGHT, GameScenes.EDITOR);
            }
            else
            {
                if (!protoscenmod.targetScenes.Any(s => s == GameScenes.SPACECENTER))
                {
                    Utilities.Log("AmpYear  AddScenarioModules", " Adding the SpaceCenter scenario module.");
                    protoscenmod.targetScenes.Add(GameScenes.SPACECENTER);
                }
                if (!protoscenmod.targetScenes.Any(s => s == GameScenes.FLIGHT))
                {
                    Utilities.Log("AmpYear  AddScenarioModules", " Adding the flight scenario module.");
                    protoscenmod.targetScenes.Add(GameScenes.FLIGHT);
                }
                if (!protoscenmod.targetScenes.Any(s => s == GameScenes.EDITOR))
                {
                    Utilities.Log("AmpYear  AddScenarioModules", " Adding the Editor scenario module.");
                    protoscenmod.targetScenes.Add(GameScenes.EDITOR);
                }
            }
        }
    }

    public class AmpYear : ScenarioModule
    {
        public static AmpYear Instance { get; private set; }

        public AYGameSettings AYgameSettings { get; private set; }

        public AYSettings AYsettings { get; private set; }

        private readonly string globalConfigFilename;

        //private readonly string FilePath;
        private ConfigNode globalNode = new ConfigNode();

        private readonly List<Component> children = new List<Component>();

        public AmpYear()
        {
            Utilities.Log("AmpYear", "Constructor");
            Instance = this;
            AYsettings = new AYSettings();
            AYgameSettings = new AYGameSettings();
            globalConfigFilename = System.IO.Path.Combine(_AssemblyFolder, "Config.cfg").Replace("\\", "/");
            this.Log("globalConfigFilename = " + globalConfigFilename);
        }

        public override void OnAwake()
        {
            this.Log("OnAwake in " + HighLogic.LoadedScene);
            base.OnAwake();

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                this.Log("Adding SpaceCenterManager");
                var child = gameObject.AddComponent<AYSCController>();
                children.Add(child);
            }
            else if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                this.Log("Adding FlightManager");
                var child = gameObject.AddComponent<AYController>();
                children.Add(child);
            }
            else if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                this.Log("Adding EditorController");
                var child = gameObject.AddComponent<AYController>();
                children.Add(child);
            }
        }

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);
            AYgameSettings.Load(gameNode);
            // Load the global settings
            if (System.IO.File.Exists(globalConfigFilename))
            {
                globalNode = ConfigNode.Load(globalConfigFilename);
                AYsettings.Load(globalNode);
                foreach (Savable s in children.Where(c => c is Savable))
                {
                    this.Log("AmpYear Child Load Call for " + s.ToString());
                    s.Load(globalNode);
                }
            }
            this.Log("OnLoad: \n " + gameNode + "\n" + globalNode);
        }

        public override void OnSave(ConfigNode gameNode)
        {
            base.OnSave(gameNode);
            AYgameSettings.Save(gameNode);
            foreach (Savable s in children.Where(c => c is Savable))
            {
                this.Log("AmpYear Child Save Call for " + s.ToString());
                s.Save(globalNode);
            }
            AYsettings.Save(globalNode);
            globalNode.Save(globalConfigFilename);

            this.Log("OnSave: " + gameNode + "\n" + globalNode);
        }

        private void OnDestroy()
        {
            this.Log("OnDestroy");
            foreach (Component child in children)
            {
                this.Log("AmpYear Child Destroy for " + child.name);
                Destroy(child);
            }
            children.Clear();
        }

        #region Assembly/Class Information

        /// <summary>
        /// Name of the Assembly that is running this MonoBehaviour
        /// </summary>
        internal static String _AssemblyName
        { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name; } }

        /// <summary>
        /// Full Path of the executing Assembly
        /// </summary>
        internal static String _AssemblyLocation
        { get { return System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("\\", "/"); } }

        /// <summary>
        /// Folder containing the executing Assembly
        /// </summary>
        internal static String _AssemblyFolder
        { get { return System.IO.Path.GetDirectoryName(_AssemblyLocation).Replace("\\", "/"); } }

        #endregion Assembly/Class Information
    }

    internal interface Savable
    {
        void Load(ConfigNode globalNode);

        void Save(ConfigNode globalNode);
    }
}