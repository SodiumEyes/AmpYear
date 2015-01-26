/**
 * ModuleAmpYearPoweredRCS.cs
 * 
 * * AmpYear power management. 
 * (C) Copyright 2015, Jamie Leighton
 * 
 * This code is based on the original AmpYear module by :- SodiumEyes
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
using KSP;
using UnityEngine;

namespace AY
{

    public class ModuleAmpYearPoweredRCS : ModuleRCS
    {       
        //private Propellant definition;
        private string ElecChge = "ElectricCharge";
        private string Xenon = "XenonGas"; 
        
        // New context menu info
        [KSPField(isPersistant = true, guiName = "AmpYear Managed", guiActive = true)]
        public bool isManaged = false;
        [KSPField(isPersistant = true, guiName = "Xenon Ratio", guiUnits = "U/s", guiFormat = "F3", guiActive = true)]
        public float xenonRatio = 0f;
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
            LogFormatted_DebugOnly("AmpYearRCSModule onLoad");
            LogFormatted_DebugOnly("hasnodes.count " + node.CountNodes);
            for (int i = 0; i < node.CountNodes ; ++i)
            
            {
                LogFormatted_DebugOnly("RCSModule Node =  " + node.nodes[i].name);                                        
            }

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
                LogFormatted_DebugOnly("AmpYearRCSModule Adding Prop " + c.name);                
                node.AddNode(c);                
            }
            base.OnLoad(node);
            
            foreach (Propellant propellant in propellants)
            {
                LogFormatted_DebugOnly("AmpYearRCSModule Props List = " + propellant.name);
                LogFormatted_DebugOnly("AmpYearRCSModule flowmode = " + propellant.GetFlowMode());
                LogFormatted_DebugOnly("AmpYearRCSModule ratio = " + propellant.ratio);                
                if (propellant.name == ElecChge)
                {
                    powerRatio = propellant.ratio;
                }
                if (propellant.name == Xenon)
                {
                    xenonRatio = propellant.ratio;
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
              
        [KSPEvent(guiActive = true, guiName = "Toggle AY GUI")]
        public void toggleGUI()
        {
            AYMenu AYM;
            AYM = FindObjectOfType<AYMenu>();
            AYM.GuiVisible = !AYM.GuiVisible;
        }       

        #region Logging
        /// <summary>
        /// Some Structured logging to the debug file - ONLY RUNS WHEN DLL COMPILED IN DEBUG MODE
        /// </summary>
        /// <param name="Message">Text to be printed - can be formatted as per String.format</param>
        /// <param name="strParams">Objects to feed into a String.format</param>
        [System.Diagnostics.Conditional("DEBUG")]
        internal static void LogFormatted_DebugOnly(String Message, params object[] strParams)
        {
            LogFormatted("DEBUG: " + Message, strParams);
        }

        /// <summary>
        /// Some Structured logging to the debug file
        /// </summary>
        /// <param name="Message">Text to be printed - can be formatted as per String.format</param>
        /// <param name="strParams">Objects to feed into a String.format</param>
        internal static void LogFormatted(String Message, params object[] strParams)
        {
            Message = String.Format(Message, strParams);                  // This fills the params into the message
            String strMessageLine = String.Format("{0},{2},{1}",
                DateTime.Now, Message,
                "ModuleAmpYearPoweredRCS");                                           // This adds our standardised wrapper to each line
            UnityEngine.Debug.Log(strMessageLine);                        // And this puts it in the log
        }

        #endregion
    }

}