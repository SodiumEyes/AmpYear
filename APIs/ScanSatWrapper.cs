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
using System.Collections.Generic;
using System.Reflection;

namespace AY
{
    /// <summary>
    /// The Wrapper class to access ScanSat
    /// </summary>
    public class ScanSatWrapper
    {
        protected static System.Type SCANsatType;

        /// <summary>
        /// Whether we found the ScanSat assembly in the loadedassemblies.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean AssemblyExists { get { return SCANsatType != null; } }

        /// <summary>
        /// Whether we managed to wrap all the methods/functions from the instance.
        ///
        /// SET AFTER INIT
        /// </summary>
        private static Boolean _SSWrapped;

        /// <summary>
        /// Whether the object has been wrapped
        /// </summary>
        public static Boolean APIReady { get { return _SSWrapped; } }

        /// <summary>
        /// This method will set up the SCANsat object and wrap all the methods/functions
        /// </summary>
        /// <returns></returns>
        public static Boolean InitSCANsatWrapper()
        {
            //reset the internal objects
            _SSWrapped = false;
            LogFormatted_DebugOnly("Attempting to Grab SCANsat Types...");

            //find the SCANsat part module type
            SCANsatType = getType("SCANsat.SCAN_PartModules.SCANsat"); 

            if (SCANsatType == null)
            {
                return false;
            }

            LogFormatted("SCANsat Version:{0}", SCANsatType.Assembly.GetName().Version.ToString());

            _SSWrapped = true;
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

        public class SCANsat
        {
            internal SCANsat(Object a)
            {
                actualSCANsat = a;
                resourceInputsField = SCANsatType.GetField("resourceInputs", BindingFlags.Public | BindingFlags.Instance);
                ScanningField = SCANsatType.GetField("scanning", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
                startScanMethod = SCANsatType.GetMethod("startScan", BindingFlags.Public | BindingFlags.Instance);
                stopScanMethod = SCANsatType.GetMethod("stopScan", BindingFlags.Public | BindingFlags.Instance);
            }

            private Object actualSCANsat;

            private FieldInfo resourceInputsField;

            /// <summary>
            /// How much power the scanner is using
            /// </summary>
            public List<ModuleResource> resourceInputs
            {
                get { return (List<ModuleResource>)resourceInputsField.GetValue(actualSCANsat); }
            }

            private FieldInfo ScanningField;

            /// <summary>
            /// Whether the scanner is on or off
            /// </summary>
            public bool scanning
            {
                get { return (bool)ScanningField.GetValue(actualSCANsat); }
            }

            private MethodInfo startScanMethod;

            /// <summary>
            /// Start Scanning
            /// </summary>
            /// <returns>Bool indicating success of call</returns>
            public bool startScan()
            {
                try
                {
                    startScanMethod.Invoke(actualSCANsat, null);
                    return true;
                }
                catch (Exception ex)
                {
                    LogFormatted("Invoke startScan Method failed: {0}", ex.Message);
                    return false;
                }
            }

            private MethodInfo stopScanMethod;

            /// <summary>
            /// Stop Scanning
            /// </summary>
            /// <returns>Bool indicating success of call</returns>
            public bool stopScan()
            {
                try
                {
                    stopScanMethod.Invoke(actualSCANsat, null);
                    return true;
                }
                catch (Exception ex)
                {
                    LogFormatted("Invoke stopScan Method failed: {0}", ex.Message);
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