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
    /// The Wrapper class to access KSP Interstellar Extended
    /// </summary>
    public class KSPIEWrapper
    {
        protected static System.Type KSPIEType;
        protected static System.Type FNModuleCryostatType;

        /// <summary>
        /// Whether we found the WarpPlugin assembly in the loadedassemblies.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean AssemblyExists { get { return KSPIEType != null; } }

        /// <summary>
        /// Whether we managed to wrap all the methods/functions from the instance.
        ///
        /// SET AFTER INIT
        /// </summary>
        private static Boolean _KSPIEWrapped;

        /// <summary>
        /// Whether the object has been wrapped
        /// </summary>
        public static Boolean APIReady { get { return _KSPIEWrapped; } }

        /// <summary>
        /// This method will set up the WarpPlugin object and wrap all the methods/functions
        /// </summary>
        /// <returns></returns>
        public static Boolean InitKSPIEWrapper()
        {
            //reset the internal objects
            _KSPIEWrapped = false;
            LogFormatted_DebugOnly("Attempting to Grab FNPlugin Types...");

            //find the FNPlugin type
            var asm = AssemblyLoader.loadedAssemblies;
            var types = AssemblyLoader.loadedTypes;
            var types2 = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetExportedTypes()).ToList();

            KSPIEType = getType("FNPlugin.PluginHelper"); 

            if (KSPIEType == null)
            {
                return false;
            }

            LogFormatted("WarpPlugin Version:{0}", KSPIEType.Assembly.GetName().Version.ToString());
            _KSPIEWrapped = true;
            return true;
        }

        internal static Type getType(string name)
        {
            Type type = null;
            AssemblyLoader.loadedAssemblies.TypeOperation(t =>

            {
                if (t.FullName == name)
                    type = t;
            }
            );

            if (type != null)
            {
                return type;
            }
            return null;
        }

        public class FNModuleCryostat
        {
            internal FNModuleCryostat(Object a)
            {
                actualFNModuleCryostat = a;
                Type objType = actualFNModuleCryostat.GetType();
                LogFormatted_DebugOnly("Getting isDisabledField Field");
                isDisabledField = objType.GetField("isDisabled", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
                LogFormatted_DebugOnly("Success: " + (isDisabledField != null).ToString());
                LogFormatted_DebugOnly("Getting recievedPowerKWField Field");
                recievedPowerKWField = objType.GetField("recievedPowerKW", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
                LogFormatted_DebugOnly("Success: " + (recievedPowerKWField != null).ToString());
                LogFormatted_DebugOnly("Getting isActiveField Field");
                powerReqKWField = objType.GetField("powerReqKWField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
                LogFormatted_DebugOnly("Success: " + (powerReqKWField != null).ToString());
            }

            private Object actualFNModuleCryostat;

            private FieldInfo isDisabledField;

            /// <summary>
            /// If the Cryostat is Disabled or not(=active)
            /// </summary>
            public bool isDisabled
            {
                get
                {
                    return (bool)isDisabledField.GetValue(actualFNModuleCryostat);
                }
            }

            private FieldInfo recievedPowerKWField;

            /// <summary>
            /// current power usage/received
            /// </summary>
            public float recievedPowerKW
            {
                get
                {
                    return (float)recievedPowerKWField.GetValue(actualFNModuleCryostat);
                }
            }

            private FieldInfo powerReqKWField;

            /// <summary>
            /// current power usage/received
            /// </summary>
            public float powerReqKW
            {
                get
                {
                    return (float)powerReqKWField.GetValue(actualFNModuleCryostat);
                }
            }
        }

        public class FNGenerator
        {
            internal FNGenerator(Object a)
            {
                actualFNGenerator = a;
                Type objType = actualFNGenerator.GetType();
                LogFormatted_DebugOnly("Getting IsEnabledField Field");
                IsEnabledField = objType.GetField("IsEnabled", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
                LogFormatted_DebugOnly("Success: " + (IsEnabledField != null).ToString());
                LogFormatted_DebugOnly("Getting outputPowerField Field");
                outputPowerField = objType.GetField("outputPower", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
                LogFormatted_DebugOnly("Success: " + (outputPowerField != null).ToString());
                LogFormatted_DebugOnly("Getting maxThermalPowerField Field");
                maxThermalPowerField = objType.GetField("maxThermalPower", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
                LogFormatted_DebugOnly("Success: " + (maxThermalPowerField != null).ToString());
            }

            private Object actualFNGenerator;

            private FieldInfo IsEnabledField;

            /// <summary>
            /// If the Generator is active or not
            /// </summary>
            public bool IsEnabled
            {
                get
                {
                    return (bool)IsEnabledField.GetValue(actualFNGenerator);
                }
            }

            private FieldInfo outputPowerField;

            /// <summary>
            /// current power usage/received
            /// </summary>
            public float outputPower
            {
                get
                {
                    return (float)outputPowerField.GetValue(actualFNGenerator);
                }
            }

            private FieldInfo maxThermalPowerField;

            /// <summary>
            /// Max power usage/received
            /// </summary>
            public float maxThermalPower
            {
                get
                {
                    return (float)maxThermalPowerField.GetValue(actualFNGenerator);
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
