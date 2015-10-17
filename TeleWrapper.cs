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
    /// The Wrapper class to access Telemachus
    /// </summary>
    public class TeleWrapper
    {
        protected static System.Type TMPowerDrainType;

        /// <summary>
        /// Whether we found the Telemachus assembly in the loadedassemblies.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean AssemblyExists { get { return (TMPowerDrainType != null); } }

        /// <summary>
        /// Whether we managed to wrap all the methods/functions from the instance.
        ///
        /// SET AFTER INIT
        /// </summary>
        private static Boolean _TMWrapped = false;

        /// <summary>
        /// Whether the object has been wrapped
        /// </summary>
        public static Boolean APIReady { get { return _TMWrapped; } }

        /// <summary>
        /// This method will set up the Telemachus object and wrap all the methods/functions
        /// </summary>
        public static Boolean InitTALWrapper()
        {
            //reset the internal objects
            _TMWrapped = false;
            LogFormatted("Attempting to Grab Telemachus Types...");

            //find the TMPowerDrain type
            TMPowerDrainType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "Telemachus.TelemachusPowerDrain");

            if (TMPowerDrainType == null)
            {
                return false;
            }

            LogFormatted("Telemachus Version:{0}", TMPowerDrainType.Assembly.GetName().Version.ToString());

            _TMWrapped = true;
            return true;
        }

        public class TMPowerDrain
        {
            internal TMPowerDrain(Object a)
            {
                actualTMPowerDrain = a;
                isActiveField = TMPowerDrainType.GetField("isActive", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                powerConsumptionField = TMPowerDrainType.GetField("powerConsumption", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            }

            private Object actualTMPowerDrain;

            private FieldInfo isActiveField;

            /// <summary>
            /// Whether Telemachus is on or not
            /// </summary>
            public bool isActive
            {
                get { return (bool)isActiveField.GetValue(actualTMPowerDrain); }
            }

            private FieldInfo powerConsumptionField;

            /// <summary>
            /// How much energy Telemachus is using
            /// </summary>
            public float powerConsumption
            {
                get { return (float)powerConsumptionField.GetValue(actualTMPowerDrain); }
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