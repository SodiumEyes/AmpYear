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
    /// The Wrapper class to access KPBS
    /// </summary>
    public class KPBSWrapper
    {
        protected static System.Type KPBSGHType;
        protected static System.Type KPBSCnvType;

        /// <summary>
        /// Whether we found the KPBS assembly in the loadedassemblies.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean AssemblyExists { get { return KPBSGHType != null; } }

        /// <summary>
        /// Whether we managed to wrap all the methods/functions from the instance.
        ///
        /// SET AFTER INIT
        /// </summary>
        private static Boolean _KPBSWrapped;

        /// <summary>
        /// Whether the object has been wrapped
        /// </summary>
        public static Boolean APIReady { get { return _KPBSWrapped; } }

        /// <summary>
        /// This method will set up the KPBS object and wrap all the methods/functions
        /// </summary>
        /// <returns></returns>
        public static Boolean InitKPBSWrapper()
        {
            //reset the internal objects
            _KPBSWrapped = false;
            LogFormatted_DebugOnly("Attempting to Grab KPBS Types...");

            //find the KPBS Greenhouse part module type
            KPBSGHType = getType("PlanetarySurfaceStructures.PlanetaryGreenhouse"); 

            if (KPBSGHType == null)
            {
                return false;
            }

            //find the KPBS Converter part module type
            KPBSCnvType = getType("PlanetarySurfaceStructures.ModuleKPBSConverter"); 

            if (KPBSCnvType == null)
            {
                return false;
            }

            LogFormatted("KPBS Version:{0}", KPBSGHType.Assembly.GetName().Version.ToString());

            _KPBSWrapped = true;
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

        public class PlanetaryGreenhouse
        {
            internal PlanetaryGreenhouse(Object a)
            {
                actualPlanetaryGreenhouse = a;
                currentRateField = KPBSGHType.GetField("currentRate", BindingFlags.Public | BindingFlags.Instance);
            }

            private Object actualPlanetaryGreenhouse;

            private FieldInfo currentRateField;

            /// <summary>
            /// Get the Current Rate Multiplier for the ModuleResourceConverter
            /// </summary>
            /// <returns>Float CurrentRate</returns>
            public float currentRate
            {
                get { return (float)currentRateField.GetValue(actualPlanetaryGreenhouse); }
            }
        }

        public class ModuleKPBSConverter
        {
            internal ModuleKPBSConverter(Object a)
            {
                actualModuleKPBSConverter = a;
                currentRateField = KPBSCnvType.GetField("currentRate", BindingFlags.Public | BindingFlags.Instance);
            }

            private Object actualModuleKPBSConverter;

            private FieldInfo currentRateField;

            /// <summary>
            /// Get the Current Rate Multiplier for the ModuleResourceConverter
            /// </summary>
            /// <returns>Float CurrentRate</returns>
            public float currentRate
            {
                get { return (float)currentRateField.GetValue(actualModuleKPBSConverter); }
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

