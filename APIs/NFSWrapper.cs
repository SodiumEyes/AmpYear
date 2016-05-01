/**
 * DeepFreeze Continued...
 * (C) Copyright 2015, Jamie Leighton
 *
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 *
 *  This file is part of JPLRepo's DeepFreeze (continued...) - a Fork of DeepFreeze. Original Author of DeepFreeze is 'scottpaladin' on the KSP Forums.
 *  This File was not part of the original Deepfreeze but was written by Jamie Leighton based of code and concepts from the Kerbal Alarm Clock Mod. Which was licensed under the MIT license.
 *  (C) Copyright 2015, Jamie Leighton
 *
 * Continues to be licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 4.0)
 * creative commons license. See <https://creativecommons.org/licenses/by-nc-sa/4.0/>
 * for full details.
 *
 */

using System;
using System.Linq;
using System.Reflection;

namespace AY
{
    /// <summary>
    /// The Wrapper class to access Near Future Solar
    /// </summary>
    public class NFSWrapper
    {
        internal static System.Type NFSCurvedsolarPanelType;

        /// <summary>
        /// Whether we found the Near Future Solar assembly in the loadedassemblies.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean AssemblyExists { get { return NFSCurvedsolarPanelType != null; } }

        /// <summary>
        /// Whether we managed to wrap all the methods/functions from the instance.
        ///
        /// SET AFTER INIT
        /// </summary>
        private static Boolean _NFSWrapped;

        /// <summary>
        /// Whether the object has been wrapped
        /// </summary>
        public static Boolean APIReady { get { return _NFSWrapped; } }

        /// <summary>
        /// This method will set up the Near Future Solar object and wrap all the methods/functions
        /// </summary>
        /// <returns></returns>
        public static Boolean InitNFSWrapper()
        {
            //reset the internal objects
            _NFSWrapped = false;
            LogFormatted_DebugOnly("Attempting to Grab Near Future Solar Types...");

            //find the NFSCurvedsolarPanelType type
            NFSCurvedsolarPanelType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "NearFutureSolar.ModuleCurvedSolarPanel");

            if (NFSCurvedsolarPanelType == null)
            {
                return false;
            }

            LogFormatted("Near Future Solar Version:{0}", NFSCurvedsolarPanelType.Assembly.GetName().Version.ToString());

            _NFSWrapped = true;
            return true;
        }

        public class NFSCurvedPanel
        {
            internal NFSCurvedPanel(Object a)
            {
                actualNFSCurvedPanel = a;
                TotalEnergyRateField = NFSCurvedsolarPanelType.GetField("TotalEnergyRate");
                EnergyFlowField = NFSCurvedsolarPanelType.GetField("EnergyFlow");
                StateField = NFSCurvedsolarPanelType.GetField("State");
                DeployMethod = NFSCurvedsolarPanelType.GetMethod("Deploy", BindingFlags.Public | BindingFlags.Instance);
                RetractMethod = NFSCurvedsolarPanelType.GetMethod("Retract", BindingFlags.Public | BindingFlags.Instance);
            }

            private Object actualNFSCurvedPanel;

            private FieldInfo EnergyFlowField;

            /// <summary>
            /// The actual Energy Flow which is a float converted to a string so we convert it back to a float
            /// </summary>
            public float EnergyFlow
            {
                get
                {
                    Object tmpObj = EnergyFlowField.GetValue(actualNFSCurvedPanel);
                    string tmpStr = (string)tmpObj;
                    float tmpFloat = float.Parse(tmpStr);
                    return tmpFloat;
                }
            }

            private FieldInfo TotalEnergyRateField;

            /// <summary>
            /// This is the ideal charge rate, unaffected by LOS / distance to Kerbol. Use it in the Editor
            /// </summary>
            public float TotalEnergyRate
            {
                get { return (float)TotalEnergyRateField.GetValue(actualNFSCurvedPanel); }
            }

            private FieldInfo StateField;

            /// <summary>
            /// The actual State of the panel
            /// </summary>
            public ModuleDeployableSolarPanel.panelStates State
            {
                get
                {
                    Object tmpObj = StateField.GetValue(actualNFSCurvedPanel);
                    ModuleDeployableSolarPanel.panelStates tmpState = (ModuleDeployableSolarPanel.panelStates)tmpObj;
                    return tmpState;
                }
            }

            private MethodInfo DeployMethod;

            /// <summary>
            /// Deploy Panels
            /// </summary>
            /// <returns>Bool indicating success of call</returns>
            public bool Deploy()
            {
                try
                {
                    DeployMethod.Invoke(actualNFSCurvedPanel, null);
                    return true;
                }
                catch (Exception ex)
                {
                    LogFormatted_DebugOnly("Deply Failed: {0}", ex.Message);
                    return false;
                }
            }

            private MethodInfo RetractMethod;

            /// <summary>
            /// Retract Panels
            /// </summary>
            /// <returns>Bool indicating success of call</returns>
            public bool Retract()
            {
                try
                {
                    RetractMethod.Invoke(actualNFSCurvedPanel, null);
                    return true;
                }
                catch (Exception ex)
                {
                    LogFormatted("Retract Failed: {0}", ex.Message);
                    return false;
                }
            }
        }

        #region Logging Stuff

        /// <summary>
        /// Some Structured logging to the debug file - ONLY RUNS WHEN DLL COMPILED IN DEBUG MODE
        /// </summary>
        /// <param name="Message">Text to be printed - can be formatted as per String.format</param>
        /// <param name="strParams">Objects to feed into a String.format</param>
        
        internal static void LogFormatted_DebugOnly(String Message, params Object[] strParams)
        {
            if (RSTUtils.Utilities.debuggingOn)
                LogFormatted(Message, strParams);
        }

        /// <summary>
        /// Some Structured logging to the debug file
        /// </summary>
        /// <param name="Message">Text to be printed - can be formatted as per String.format</param>
        /// <param name="strParams">Objects to feed into a String.format</param>
        internal static void LogFormatted(String Message, params Object[] strParams)
        {
            Message = String.Format(Message, strParams);
            String strMessageLine = String.Format("{0},{2}-{3},{1}",
                DateTime.Now, Message, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            UnityEngine.Debug.Log(strMessageLine);
        }

        #endregion Logging Stuff
    }
}