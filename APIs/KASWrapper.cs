/*
 * AmpYear power management.
 * (C) Copyright 2015, Jamie Leighton
 * The original code and concept of AmpYear rights go to SodiumEyes on the Kerbal Space Program Forums, which was covered by GNU License GPL(no version stated).
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
    /// The Wrapper class to access KAS
    /// </summary>
    public class KASWrapper
    {
        protected static System.Type KASType;
        protected static System.Type KASModuleWinchType;
        protected static System.Type KASModuleMagnetType;

        /// <summary>
        /// Whether we found the KAS assembly in the loadedassemblies.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean AssemblyExists { get { return KASType != null; } }

        /// <summary>
        /// Whether we managed to wrap all the methods/functions from the instance.
        ///
        /// SET AFTER INIT
        /// </summary>
        private static Boolean _KASWrapped;

        /// <summary>
        /// Whether the object has been wrapped
        /// </summary>
        public static Boolean APIReady { get { return _KASWrapped; } }

        /// <summary>
        /// This method will set up the KAS object and wrap all the methods/functions
        /// </summary>
        /// <returns></returns>
        public static Boolean InitKASWrapper()
        {
            //reset the internal objects
            _KASWrapped = false;
            LogFormatted_DebugOnly("Attempting to Grab KAS Types...");

            //find the base type
            KASType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName.Contains("KAS"));

            if (KASType == null)
            {
                return false;
            }

            LogFormatted("KAS Version:{0}", KASType.Assembly.GetName().Version.ToString());

            //find the ModuleNavLight type
            KASModuleWinchType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "KAS.KASModuleWinch");

            if (KASModuleWinchType == null)
            {
                return false;
            }

            //find the ModuleNavLight type
            KASModuleMagnetType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "KAS.KASModuleMagnet");

            if (KASModuleMagnetType == null)
            {
                return false;
            }

            _KASWrapped = true;
            return true;
        }

        public class KASModuleWinch
        {
            internal KASModuleWinch(Object a)
            {
                actualKASModuleWinch = a;
                LogFormatted_DebugOnly("Getting isActiveField Field");
                isActiveField = KASModuleWinchType.GetField("isActive");
                LogFormatted_DebugOnly("Success: " + (isActiveField != null).ToString());
                LogFormatted_DebugOnly("Getting powerDrainField Field");
                powerDrainField = KASModuleWinchType.GetField("powerDrain");
                LogFormatted_DebugOnly("Success: " + (powerDrainField != null).ToString());
                LogFormatted_DebugOnly("Getting motorSpeedField Field");
                motorSpeedField = KASModuleWinchType.GetField("motorSpeed");
                LogFormatted_DebugOnly("Success: " + (motorSpeedField != null).ToString());
            }

            private Object actualKASModuleWinch;

            private FieldInfo isActiveField;

            /// <summary>
            /// Whether the winch is active or not
            /// </summary>
            public bool isActive
            {
                get
                {
                    try
                    {
                        return (bool)isActiveField.GetValue(actualKASModuleWinch);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to get isActive field");
                        LogFormatted("Exception: {0}", ex);
                        return false;
                    }
                }
            }

            private FieldInfo powerDrainField;

            /// <summary>
            /// How much energy the winch is using
            /// </summary>
            public float powerDrain
            {
                get
                {
                    try
                    {
                        return (float)powerDrainField.GetValue(actualKASModuleWinch);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to get powerDrain field");
                        LogFormatted("Exception: {0}", ex);
                        return 0;
                    }
                }
            }

            private FieldInfo motorSpeedField;

            /// <summary>
            /// The speed of the winch motor
            /// </summary>
            public float motorSpeed
            {
                get
                {
                    try
                    {
                        return (float)motorSpeedField.GetValue(actualKASModuleWinch);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to get motorSpeed field");
                        LogFormatted("Exception: {0}", ex);
                        return 0;
                    }
                    
                }
            }
        }

        public class KASModuleMagnet
        {
            internal KASModuleMagnet(Object a)
            {
                actualKASModuleMagnet = a;
                LogFormatted_DebugOnly("Getting isActiveField Field");
                isActiveField = KASModuleMagnetType.GetField("_magnetActive", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                LogFormatted_DebugOnly("Success: " + (isActiveField != null).ToString());
                LogFormatted_DebugOnly("Getting powerDrainField Field");
                powerDrainField = KASModuleMagnetType.GetField("powerDrain");
                LogFormatted_DebugOnly("Success: " + (powerDrainField != null).ToString());
            }

            private Object actualKASModuleMagnet;

            private FieldInfo isActiveField;

            /// <summary>
            /// Whether the winch is active or not
            /// </summary>
            public bool MagnetActive
            {
                get
                {
                    try
                    {
                        return (bool)isActiveField.GetValue(actualKASModuleMagnet);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to get MagnetActive field");
                        LogFormatted("Exception: {0}", ex);
                        return false;
                    }
                }
            }

            private FieldInfo powerDrainField;

            /// <summary>
            /// How much energy the winch is using
            /// </summary>
            public float powerDrain
            {
                get
                {
                    try
                    {
                        return (float)powerDrainField.GetValue(actualKASModuleMagnet);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to get powerDrain field");
                        LogFormatted("Exception: {0}", ex);
                        return 0;
                    }
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