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
    /// The Wrapper class to access Remote Tech
    /// </summary>
    public class RTWrapper
    {
        protected static System.Type RTAPIType;
        protected static Object actualRTAPI;
        protected static System.Type actualRTAntennaType;
        protected static System.Type RTSettingsType;
        protected static System.Type SettingsType;
        protected static Object actualRTsettings;

        /// <summary>
        /// This is the Remote Tech API object
        ///
        /// SET AFTER INIT
        /// </summary>
        public static RTAPI RTactualAPI;

        /// <summary>
        /// Whether we found the Remote Tech API assembly in the loadedassemblies.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean AssemblyExists { get { return (RTAPIType != null); } }

        /// <summary>
        /// Whether we managed to hook the running Instance from the assembly.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean InstanceExists { get { return (actualRTAPI != null); } }

        /// <summary>
        /// Whether we managed to wrap all the methods/functions from the instance.
        ///
        /// SET AFTER INIT
        /// </summary>
        private static Boolean _RTWrapped;

        /// <summary>
        /// Whether the object has been wrapped
        /// </summary>
        public static Boolean APIReady { get { return _RTWrapped; } }

        /// <summary>
        /// This method will set up the Remote Tech object and wrap all the methods/functions
        /// </summary>
        /// <param name="Force">This option will force the Init function to rebind everything</param>
        /// <returns></returns>
        public static Boolean InitTRWrapper()
        {
            //reset the internal objects
            _RTWrapped = false;
            actualRTAPI = null;
            LogFormatted("Attempting to Grab Remote Tech Types...");

            //find the base type
            RTAPIType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "RemoteTech.API.API");

            if (RTAPIType == null)
            {
                return false;
            }

            LogFormatted("Remote Tech Version:{0}", RTAPIType.Assembly.GetName().Version.ToString());

            //find the RTSettings type
            RTSettingsType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "RemoteTech.RTSettings");

            if (RTSettingsType == null)
            {
                return false;
            }

            //find the Settings type
            SettingsType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "RemoteTech.Settings");

            if (SettingsType == null)
            {
                return false;
            }

            //now the RTAntenna Type
            actualRTAntennaType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "RemoteTech.Modules.ModuleRTAntenna");

            if (actualRTAntennaType == null)
            {
                return false;
            }

            //now grab the running instance
            LogFormatted("Got Assembly Types, grabbing Instances");
            try
            {
                actualRTAPI = RTAPIType.GetMember("HasLocalControl", BindingFlags.Public | BindingFlags.Static);
            }
            catch (Exception)
            {
                LogFormatted("No Remote Tech isInitialised found");
                //throw;
            }

            try
            {
                actualRTsettings = RTSettingsType.GetField("mInstance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            }
            catch (Exception)
            {
                LogFormatted("No Remote Tech RTSettings Instance found");
                //throw;
            }

            if (actualRTAPI == null || actualRTsettings == null)
            {
                LogFormatted("Failed grabbing Instance");
                return false;
            }

            //If we get this far we can set up the local object and its methods/functions
            LogFormatted("Got Instance, Creating Wrapper Objects");
            RTactualAPI = new RTAPI(actualRTAPI);

            _RTWrapped = true;
            return true;
        }

        /// <summary>
        /// The Type that is an analogue of the real Remote Tech. This lets you access all the API-able properties and Methods of Remote Tech
        /// </summary>
        public class RTAPI
        {
            internal RTAPI(Object actualRT)
            {
                //store the actual object
                APIactualRT = actualRT;

                //these sections get and store the reflection info and actual objects where required. Later in the properties we then read the values from the actual objects
                //for events we also add a handler

                //WORK OUT THE STUFF WE NEED TO HOOK FOR PEOPLE HERE
                //Methods
                LogFormatted("Getting HasLocalControl Method");
                HasLocalControlMethod = RTAPIType.GetMethod("HasLocalControl", BindingFlags.Public | BindingFlags.Static);
                LogFormatted_DebugOnly("Success: " + (HasLocalControlMethod != null).ToString());

                LogFormatted("Getting HasAnyConnection Method");
                HasAnyConnectionMethod = RTAPIType.GetMethod("HasAnyConnection", BindingFlags.Public | BindingFlags.Static);
                LogFormatted_DebugOnly("Success: " + (HasAnyConnectionMethod != null).ToString());

                LogFormatted("Getting GetShortestSignalDelay Method");
                GetShortestSignalDelayMethod = RTAPIType.GetMethod("GetShortestSignalDelay", BindingFlags.Public | BindingFlags.Static);
                LogFormatted_DebugOnly("Success: " + (GetShortestSignalDelayMethod != null).ToString());
            }

            private Object APIactualRT;

            #region Methods

            private MethodInfo HasLocalControlMethod;

            /// <summary>
            /// Whether the current vessel HasLocalControl
            /// </summary>
            /// <param name="id">The vessel id reference</param>
            /// <returns>Success of call</returns>
            internal bool HasLocalControl(Guid id)
            {
                try
                {
                    return (bool)HasLocalControlMethod.Invoke(APIactualRT, new System.Object[] { id });
                }
                catch (Exception ex)
                {
                    LogFormatted("Unable to invoke Remote Tech HasLocalControl Method");
                    LogFormatted("Exception: {0}", ex);
                    return false;
                    //throw;
                }
            }

            private MethodInfo HasAnyConnectionMethod;

            /// <summary>
            /// Whether the current vessel HasAnyConnection
            /// </summary>
            /// <param name="id">The vessel id reference</param>
            /// <returns>Success of call</returns>
            internal bool HasAnyConnection(Guid id)
            {
                try
                {
                    return (bool)HasAnyConnectionMethod.Invoke(APIactualRT, new System.Object[] { id });
                }
                catch (Exception ex)
                {
                    LogFormatted("Unable to invoke Remote Tech HasAnyConnection Method");
                    LogFormatted("Exception: {0}", ex);
                    return false;
                    //throw;
                }
            }

            private MethodInfo GetShortestSignalDelayMethod;

            /// <summary>
            /// Gets the signal delay
            /// </summary>
            /// <param name="id">The vessel id reference</param>
            /// <returns>A double indicating the signaldelay time</returns>
            internal double GetShortestSignalDelay(Guid id)
            {
                try
                {
                    return (double)GetShortestSignalDelayMethod.Invoke(APIactualRT, new System.Object[] { id });
                }
                catch (Exception ex)
                {
                    LogFormatted("Unable to invoke Remote Tech GetShortestSignalDelay Method");
                    LogFormatted("Exception: {0}", ex);
                    return 0;
                    //throw;
                }
            }

            #endregion Methods
        }

        public class RTAntenna
        {
            internal RTAntenna(Object a)
            {
                actualRTAntenna = a;
                IsBrokenField = actualRTAntennaType.GetField("IsRTBroken");
                ActivatedField = actualRTAntennaType.GetField("IsRTActive");
                ConsumptionMultiplierField = SettingsType.GetField("ConsumptionMultiplier");
                EnergyCostField = actualRTAntennaType.GetField("EnergyCost");
            }

            private Object actualRTAntenna;

            private FieldInfo IsBrokenField;

            /// <summary>
            /// Whether the antenna is broken in flight or not
            /// </summary>
            public bool IsBroken
            {
                get { return (bool)IsBrokenField.GetValue(actualRTAntenna); }
            }

            private FieldInfo ActivatedField;

            /// <summary>
            /// Whether the antenna is activated in flight or not
            /// </summary>
            public bool Activated
            {
                get { return (bool)ActivatedField.GetValue(actualRTAntenna); }
            }

            private FieldInfo ConsumptionMultiplierField;

            /// <summary>
            /// Current consumption multiplier of EC in flight
            /// </summary>
            public float ConsumptionMultiplier
            {
                get { return (float)ConsumptionMultiplierField.GetValue(actualRTsettings); }
            }

            /// <summary>
            /// Current consumption of EC in flight
            /// </summary>
            public float Consumption
            {
                get
                {
                    if (IsBroken) return 0.0f;
                    if (Activated)
                    {
                        return EnergyCost * ConsumptionMultiplier;
                    }
                    else
                    {
                        return 0.0f;
                    }
                }
            }

            private FieldInfo EnergyCostField;

            /// <summary>
            /// The energy cost of using the Antenna
            /// </summary>
            public float EnergyCost
            {
                get { return (float)EnergyCostField.GetValue(actualRTAntenna); }
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