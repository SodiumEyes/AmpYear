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
        public static Boolean AssemblyExists { get { return (SCANsatType != null); } }

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
        /// <param name="Force">This option will force the Init function to rebind everything</param>
        /// <returns></returns>
        public static Boolean InitSCANsatWrapper()
        {
            //reset the internal objects
            _SSWrapped = false;
            LogFormatted("Attempting to Grab SCANsat Types...");

            //find the SCANsat part module type
            SCANsatType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "SCANsat.SCAN_PartModules.SCANsat");

            if (SCANsatType == null)
            {
                return false;
            }

            LogFormatted("SCANsat Version:{0}", SCANsatType.Assembly.GetName().Version.ToString());

            _SSWrapped = true;
            return true;
        }

        public class SCANsat
        {
            internal SCANsat(Object a)
            {
                actualSCANsat = a;
                powerField = SCANsatType.GetField("power");
                ScanningField = SCANsatType.GetField("scanning", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
            }

            private Object actualSCANsat;

            private FieldInfo powerField;

            /// <summary>
            /// How much power the scanner is using
            /// </summary>
            public float power
            {
                get { return (float)powerField.GetValue(actualSCANsat); }
            }

            private FieldInfo ScanningField;

            /// <summary>
            /// Whether the scanner is on or off
            /// </summary>
            public bool scanning
            {
                get { return (bool)ScanningField.GetValue(actualSCANsat); }
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