/**
 * AmpYear.cs
 * 
 * AmpYear power management. 
 * (C) Copyright 2015, Jamie Leighton
 * 
 * Parts of this code are based on:-
 * Thunder Aerospace Corporation's Life Support for Kerbal Space Program.
 * Written by Taranis Elsu.  
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
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP.IO;
using UnityEngine;

namespace AY
{

    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class AddScenarioModules : MonoBehaviour
    {
        void Start()
        {
            var game = HighLogic.CurrentGame;

            ProtoScenarioModule psm = game.scenarios.Find(s => s.moduleName == typeof(AmpYear).Name);
            if (psm == null)
            {
                Utilities.LogFormatted("Adding the scenario module.");
                psm = game.AddProtoScenarioModule(typeof(AmpYear), GameScenes.SPACECENTER,
                    GameScenes.FLIGHT, GameScenes.EDITOR);
            }
            else
            {
                if (!psm.targetScenes.Any(s => s == GameScenes.SPACECENTER))
                {
                    psm.targetScenes.Add(GameScenes.SPACECENTER);
                }
                if (!psm.targetScenes.Any(s => s == GameScenes.FLIGHT))
                {
                    psm.targetScenes.Add(GameScenes.FLIGHT);
                }
                if (!psm.targetScenes.Any(s => s == GameScenes.EDITOR))
                {
                    psm.targetScenes.Add(GameScenes.EDITOR);
                }                
            }
        }
    }

    public class AmpYear : ScenarioModule
    {
        public static AmpYear Instance { get; private set; }

        public AYGameSettings gameSettings { get; private set; }
        public AYGlobalSettings globalSettings { get; private set; }

        private readonly string globalConfigFilename;
        //private readonly string FilePath;
        private ConfigNode globalNode = new ConfigNode();

        private readonly List<Component> children = new List<Component>();

        public AmpYear()
        {
            Utilities.LogFormatted("Constructor");
            Instance = this;
            gameSettings = new AYGameSettings();
            globalSettings = new  AYGlobalSettings();
            globalConfigFilename = System.IO.Path.Combine(_AssemblyFolder, "Config.cfg").Replace("\\", "/");
            Utilities.LogFormatted("globalConfigFilename = " + globalConfigFilename);            
        }

        public override void OnAwake()
        {
            Utilities.LogFormatted("OnAwake in " + HighLogic.LoadedScene);
            base.OnAwake();

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                Utilities.LogFormatted("Adding SpaceCenterManager");
                var c = gameObject.AddComponent<AYMenu>();
                children.Add(c);
            }
            else if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                Utilities.LogFormatted("Adding FlightManager");
                var c = gameObject.AddComponent<AYMenu>();
                children.Add(c);
            }
            else if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                Utilities.LogFormatted("Adding EditorController");
                var c = gameObject.AddComponent<AYMenu>();
                children.Add(c);
            }
        }

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);            
            gameSettings.Load(gameNode);
            // Load the global settings
            if (System.IO.File.Exists(globalConfigFilename))
            {
                
                globalNode = ConfigNode.Load(globalConfigFilename);                              
                globalSettings.Load(globalNode);               
                foreach (Savable s in children.Where(c => c is Savable))
                {
                    Utilities.LogFormatted("AmpYear Child Load Call for " + s.ToString());
                    s.Load(globalNode);
                }
            }
            this.Log("OnLoad: \n " + gameNode + "\n" + globalNode);
        }

        public override void OnSave(ConfigNode gameNode)
        {
            base.OnSave(gameNode);
            gameSettings.Save(gameNode);
            // Save the global settings
            globalSettings.Save(globalNode);
            foreach (Savable s in children.Where(c => c is Savable))
            {
                Utilities.LogFormatted("AmpYear Child Save Call for " + s.ToString());
                s.Save(globalNode);
            }
            globalNode.Save(globalConfigFilename);

            this.Log("OnSave: " + gameNode + "\n" + globalNode);
        }

        void OnDestroy()
        {
            Utilities.LogFormatted("OnDestroy");
            foreach (Component c in children)
            {
                Utilities.LogFormatted("AmpYear Child Destroy for " + c.name);
                Destroy(c);
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
        { get { return System.Reflection.Assembly.GetExecutingAssembly().Location; } }

        /// <summary>
        /// Folder containing the executing Assembly
        /// </summary>
        internal static String _AssemblyFolder
        { get { return System.IO.Path.GetDirectoryName(_AssemblyLocation); } }

        #endregion
    }

    interface Savable
    {
        void Load(ConfigNode globalNode);
        void Save(ConfigNode globalNode);
    }

    
}
