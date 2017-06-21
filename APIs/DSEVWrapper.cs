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
    /// The Wrapper class to access DSEV
    /// </summary>
    public class DSEVWrapper
    {
        internal static System.Type DSEVSupernovaControllerType;

        /// <summary>
        /// Whether we found the DSEV assembly in the loadedassemblies.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean AssemblyExists { get { return DSEVSupernovaControllerType != null; } }

        /// <summary>
        /// Whether we managed to wrap all the methods/functions from the instance.
        ///
        /// SET AFTER INIT
        /// </summary>
        private static Boolean _DSEVWrapped;

        /// <summary>
        /// Whether the object has been wrapped
        /// </summary>
        public static Boolean APIReady { get { return _DSEVWrapped; } }

        /// <summary>
        /// This method will set up the DSEV object and wrap all the methods/functions
        /// </summary>
        /// <returns></returns>
        public static Boolean InitDSEVWrapper()
        {
            //reset the internal objects
            _DSEVWrapped = false;
            LogFormatted_DebugOnly("Attempting to Grab DSEV SupernovaController Types...");

            //find the NFSCurvedsolarPanelType type
            DSEVSupernovaControllerType = getType("WildBlueIndustries.SupernovaController");

            if (DSEVSupernovaControllerType == null)
            {
                return false;
            }

            LogFormatted("DSEV Version:{0}", DSEVSupernovaControllerType.Assembly.GetName().Version.ToString());

            _DSEVWrapped = true;
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

        
        public class SupernovaController
        {
            public enum EReactorStates
            {
                None,
                Off,
                Charging,
                Ready,
                Idling,
                Running,
                Overheated
            }

            internal SupernovaController(Object a)
            {
                actualSupernovaController = a;
                reactorStateField = DSEVSupernovaControllerType.GetField("reactorState");
                requiresECToStartField = DSEVSupernovaControllerType.GetField("requiresECToStart");
                ecChargePerSecField = DSEVSupernovaControllerType.GetField("ecChargePerSec");
                ecNeededToStartField = DSEVSupernovaControllerType.GetField("ecNeededToStart");
                currentElectricChargeField = DSEVSupernovaControllerType.GetField("currentElectricCharge");
            }

            private Object actualSupernovaController;

            private FieldInfo reactorStateField;

            /// <summary>
            /// The actual Reactor State
            /// </summary>
            public EReactorStates reactorState
            {
                get
                {
                    Object tmpObj = reactorStateField.GetValue(actualSupernovaController);
                    try
                    {
                        EReactorStates tmpState = (EReactorStates)Enum.Parse(typeof(EReactorStates), tmpObj.ToString(), true);
                        return tmpState;
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("get reactor state failed: {0}", ex.Message);
                        return EReactorStates.None;
                    }
                }
            }

            private FieldInfo requiresECToStartField;

            /// <summary>
            /// This is a bool idicating if EC is required to start
            /// </summary>
            public bool requiresECToStart
            {
                get { return (bool)requiresECToStartField.GetValue(actualSupernovaController); }
            }

            private FieldInfo ecChargePerSecField;

            /// <summary>
            /// The actual EC required per second while starting
            /// </summary>
            public float ecChargePerSec
            {
                get
                {
                    return (float)ecChargePerSecField.GetValue(actualSupernovaController);
                }
            }

            private FieldInfo ecNeededToStartField;

            /// <summary>
            /// The actual amount of EC required to start
            /// </summary>
            public float ecNeededToStart
            {
                get
                {
                    return (float)ecNeededToStartField.GetValue(actualSupernovaController);
                }
            }

            private FieldInfo currentElectricChargeField;

            /// <summary>
            /// The actual EC stored while charging
            /// </summary>
            public double currentElectricCharge
            {
                get
                {
                    return (double)currentElectricChargeField.GetValue(actualSupernovaController);
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
