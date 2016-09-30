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
    /// The Wrapper class to access Near Future Propulsion
    /// </summary>
    public class NFPWrapper
    {
        internal static System.Type NFPVariableISPEngineType;

        /// <summary>
        /// Whether we found the Near Future Propulsion assembly in the loadedassemblies.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean AssemblyExists { get { return NFPVariableISPEngineType != null; } }

        /// <summary>
        /// Whether we managed to wrap all the methods/functions from the instance.
        ///
        /// SET AFTER INIT
        /// </summary>
        private static Boolean _NFPWrapped;

        /// <summary>
        /// Whether the object has been wrapped
        /// </summary>
        public static Boolean APIReady { get { return _NFPWrapped; } }

        /// <summary>
        /// This method will set up the Near Future Propulsion object and wrap all the methods/functions
        /// </summary>
        /// <returns></returns>
        public static Boolean InitNFPWrapper()
        {
            //reset the internal objects
            _NFPWrapped = false;
            LogFormatted_DebugOnly("Attempting to Grab Near Future Propulsion Types...");

            //find the NFSCurvedsolarPanelType type
            NFPVariableISPEngineType = getType("NearFuturePropulsion.VariableISPEngine"); 

            if (NFPVariableISPEngineType == null)
            {
                return false;
            }

            LogFormatted("Near Future Propulsion Version:{0}", NFPVariableISPEngineType.Assembly.GetName().Version.ToString());

            _NFPWrapped = true;
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

        public class VariableISPEngine
        {
            internal VariableISPEngine(Object a)
            {
                actualVariableISPEngine = a;
                ecPropellantField = NFPVariableISPEngineType.GetField("ecPropellant", BindingFlags.NonPublic | BindingFlags.Instance);
                
            }

            private Object actualVariableISPEngine;

            private FieldInfo ecPropellantField;

            /// <summary>
            /// The actual EC usage of the engine
            /// </summary>
            public float ECUsage
            {
                get
                {
                    Object tmpObj = ecPropellantField.GetValue(actualVariableISPEngine);
                    Propellant tmpProp = (Propellant) tmpObj;
                    float tmpFloat = tmpProp.ratio;
                    return tmpFloat;
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