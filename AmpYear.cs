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
using System.IO;
using System.Linq;
using System.Reflection;
using RSTUtils;
using UnityEngine;

namespace AY
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class LoadGlobals : MonoBehaviour
    {
        internal static Subsystem[] SubsystemArrayCache;
        internal static GUISection[] GuiSectionArrayCache;

        public static LoadGlobals Instance;
        //Awake Event - when the DLL is loaded
        public void Awake()
        {
            if (Instance != null)
                return;
            Instance = this;
            Textures.LoadIconAssets();
            AYVesselPartLists.InitDictionaries();
            DontDestroyOnLoad(this);
            SubsystemArrayCache = Enum.GetValues(typeof(Subsystem)).Cast<Subsystem>().ToArray();
            GuiSectionArrayCache = Enum.GetValues(typeof(GUISection)).Cast<GUISection>().ToArray();
            Utilities.Log("AmpYear LoadGlobals Awake Complete");
        }

        public void Start()
        {
            GameEvents.onGameSceneSwitchRequested.Add(onGameSceneSwitchRequested);
        }

        public void OnDestroy()
        {
            GameEvents.onGameSceneSwitchRequested.Remove(onGameSceneSwitchRequested);
        }

        private void onGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> data)
        {
            if (data.to == GameScenes.FLIGHT || data.to == GameScenes.EDITOR || data.to == GameScenes.TRACKSTATION)
            {
                AYVesselPartLists.ResetDictionaries();
                Utilities.Log("AmpYear Reset Parts Dictionaries Completed");
            }
        }
        
    }

    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class AddScenarioModules : MonoBehaviour
    {
        private void Start()
        {
            var currentgame = HighLogic.CurrentGame;
            Utilities.Log("AmpYear  AddScenarioModules ScenarioModules Start");
            ProtoScenarioModule protoscenmod = currentgame.scenarios.Find(s => s.moduleName == typeof(AmpYear).Name);

            if (protoscenmod == null)
            {
                Utilities.Log("AmpYear  AddScenarioModules Adding the scenario module.");
                protoscenmod = currentgame.AddProtoScenarioModule(typeof(AmpYear), GameScenes.SPACECENTER, GameScenes.FLIGHT, GameScenes.EDITOR);
            }
            else
            {
                if (protoscenmod.targetScenes.All(s => s != GameScenes.SPACECENTER))
                {
                    Utilities.Log("AmpYear  AddScenarioModules Adding the SpaceCenter scenario module.");
                    protoscenmod.targetScenes.Add(GameScenes.SPACECENTER);
                }
                if (protoscenmod.targetScenes.All(s => s != GameScenes.FLIGHT))
                {
                    Utilities.Log("AmpYear  AddScenarioModules Adding the flight scenario module.");
                    protoscenmod.targetScenes.Add(GameScenes.FLIGHT);
                }
                if (protoscenmod.targetScenes.All(s => s != GameScenes.EDITOR))
                {
                    Utilities.Log("AmpYear  AddScenarioModules Adding the Editor scenario module.");
                    protoscenmod.targetScenes.Add(GameScenes.EDITOR);
                }
            }
        }
    }

    public class AmpYear : ScenarioModule
    {
        public static AmpYear Instance { get; private set; }

        public AYGameSettings AYgameSettings { get;  }

        public AYSettings AYsettings { get;  }

        private readonly string _globalConfigFilename;

        //private readonly string FilePath;
        private ConfigNode _globalNode = new ConfigNode();

        private readonly List<Component> _children = new List<Component>();

        public AmpYear()
        {
            Utilities.Log("AmpYear Constructor");
            Instance = this;
            AYsettings = new AYSettings();
            AYgameSettings = new AYGameSettings();
            _globalConfigFilename = Path.Combine(AssemblyFolder, "PluginData/Config.cfg").Replace("\\", "/");
            Utilities.Log("globalConfigFilename = " + _globalConfigFilename);
        }

        public override void OnAwake()
        {
            Utilities.Log("OnAwake in " + HighLogic.LoadedScene);
            base.OnAwake();

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                Utilities.Log("Adding SpaceCenterManager");
                var child = gameObject.AddComponent<AYSCController>();
                _children.Add(child);
            }
            else if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                Utilities.Log("Adding FlightManager");
                var child = gameObject.AddComponent<AYController>();
                _children.Add(child);
            }
            else if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                Utilities.Log("Adding EditorController");
                var child = gameObject.AddComponent<AYController>();
                _children.Add(child);
            }
        }

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);
            // Load the global settings
            if (File.Exists(_globalConfigFilename))
            {
                _globalNode = ConfigNode.Load(_globalConfigFilename);
                AYsettings.Load(_globalNode);
                foreach (var component in _children.Where(c => c is ISavable))
                {
                    var s = (ISavable) component;
                    s.Load(_globalNode);
                }
            }
            AYgameSettings.Load(gameNode);
            if (Utilities.debuggingOn)
                Debug.Log("OnLoad: " + gameNode + "\n" + _globalNode);
            else
            {
                Debug.Log("AmpYear Scenario OnLoad completed.");
            }
        }

        public override void OnSave(ConfigNode gameNode)
        {
            base.OnSave(gameNode);
            AYgameSettings.Save(gameNode);
            foreach (var component in _children.Where(c => c is ISavable))
            {
                var s = (ISavable) component;
                s.Save(_globalNode);
            }
            AYsettings.Save(_globalNode);
            _globalNode.Save(_globalConfigFilename);

            if (Utilities.debuggingOn)
                Debug.Log("OnSave: " + gameNode + "\n" + _globalNode);
            else
            {
                Debug.Log("AmpYear Scenario OnSave completed.");
            }
        }

        private void OnDestroy()
        {
            Utilities.Log("OnDestroy");
            foreach (Component child in _children)
            {
                Utilities.Log("AmpYear Child Destroy for " + child.name);
                Destroy(child);
            }
            _children.Clear();
        }

        #region Assembly/Class Information

        /// <summary>
        /// Name of the Assembly that is running this MonoBehaviour
        /// </summary>
        internal static String AssemblyName
        { get { return Assembly.GetExecutingAssembly().GetName().Name; } }

        /// <summary>
        /// Full Path of the executing Assembly
        /// </summary>
        internal static String AssemblyLocation
        { get { return Assembly.GetExecutingAssembly().Location.Replace("\\", "/"); } }

        /// <summary>
        /// Folder containing the executing Assembly
        /// </summary>
        internal static String AssemblyFolder
        { get { return Path.GetDirectoryName(AssemblyLocation).Replace("\\", "/"); } }

        #endregion Assembly/Class Information
    }

    internal interface ISavable
    {
        void Load(ConfigNode globalNode);

        void Save(ConfigNode globalNode);
    }
}