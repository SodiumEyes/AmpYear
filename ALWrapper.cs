/**
 * AmpYear power management.
 * (C) Copyright 2015, Jamie Leighton
 * The original code and concept of AmpYear rights go to SodiumEyes on the Kerbal Space Program Forums, which was covered by GNU License GPL (no version stated).
 * As such this code continues to be covered by GNU GPL license.
 * (C) Copyright 2015, Jamie Leighton
 *
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 *
 *
 */

using System;
using System.Linq;
using System.Reflection;

namespace AY
{
    /// <summary>
    /// The Wrapper class to access Aviation Lights
    /// </summary>
    public class ALWrapper
    {
        protected static System.Type ALNavLightType;

        /// <summary>
        /// Whether we found the Aviation Lights assembly in the loadedassemblies.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean AssemblyExists { get { return (ALNavLightType != null); } }

        /// <summary>
        /// Whether we managed to wrap all the methods/functions from the instance.
        ///
        /// SET AFTER INIT
        /// </summary>
        private static Boolean _ALWrapped;

        /// <summary>
        /// Whether the object has been wrapped
        /// </summary>
        public static Boolean APIReady { get { return _ALWrapped; } }

        /// <summary>
        /// This method will set up the Aviation Lights object and wrap all the methods/functions
        /// </summary>
        /// <param name="Force">This option will force the Init function to rebind everything</param>
        /// <returns></returns>
        public static Boolean InitTALWrapper()
        {
            //reset the internal objects
            _ALWrapped = false;
            LogFormatted("Attempting to Grab Aviation Lights Types...");

            //find the ModuleNavLight type
            ALNavLightType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "AviationLights.ModuleNavLight");

            if (ALNavLightType == null)
            {
                return false;
            }

            LogFormatted("Aviation Lights Version:{0}", ALNavLightType.Assembly.GetName().Version.ToString());

            _ALWrapped = true;
            return true;
        }

        public class ALNavLight
        {
            internal ALNavLight(Object a)
            {
                actualALNavLight = a;
                navLightSwitchField = ALNavLightType.GetField("navLightSwitch");
                EnergyRequiredField = ALNavLightType.GetField("EnergyReq");
            }

            private Object actualALNavLight;

            private FieldInfo navLightSwitchField;

            /// <summary>
            /// Whether the navlight swithc is on or not
            /// </summary>
            public int navLightSwitch
            {
                get { return (int)navLightSwitchField.GetValue(actualALNavLight); }
            }

            private FieldInfo EnergyRequiredField;

            /// <summary>
            /// How much energy the light is using
            /// </summary>
            public float EnergyReq
            {
                get { return (float)EnergyRequiredField.GetValue(actualALNavLight); }
            }
        }

        #region Logging Stuff

        /// <summary>
        /// Some Structured logging to the debug file - ONLY RUNS WHEN DLL COMPILED IN DEBUG MODE
        /// </summary>
        /// <param name="Message">Text to be printed - can be formatted as per String.format</param>
        /// <param name="strParams">Objects to feed into a String.format</param>
        [System.Diagnostics.Conditional("DEBUG")]
        internal static void LogFormatted_DebugOnly(String Message, params Object[] strParams)
        {
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