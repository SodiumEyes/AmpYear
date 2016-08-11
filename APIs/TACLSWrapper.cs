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
    /// The Wrapper class to access TAC LS
    /// </summary>
    public class TACLSWrapper
    {
        public static System.Type TACLSType;
        public static System.Type TACLSgameSettingsType;
        public static System.Type TACLSglobalSettingsType;
        public static System.Type TACLSGenericConverterType;
        protected static Object actualTACLS;

        /// <summary>
        /// This is the Remote Tech API object
        ///
        /// SET AFTER INIT
        /// </summary>
        public static TACLSAPI TACactualAPI;

        /// <summary>
        /// Whether we found the TAC LS assembly in the loadedassemblies.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean AssemblyExists { get { return TACLSType != null; } }

        /// <summary>
        /// Whether we managed to hook the running Instance from the assembly.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean InstanceExists { get { return actualTACLS != null; } }

        /// <summary>
        /// Whether we managed to wrap all the methods/functions from the instance.
        ///
        /// SET AFTER INIT
        /// </summary>
        private static Boolean _TACLSWrapped;

        /// <summary>
        /// Whether the object has been wrapped
        /// </summary>
        public static Boolean APIReady { get { return _TACLSWrapped; } }

        /// <summary>
        /// This method will set up the TAC LS object and wrap all the methods/functions
        /// </summary>
        /// <returns></returns>
        public static Boolean InitTACLSWrapper()
        {
            //reset the internal objects
            _TACLSWrapped = false;
            actualTACLS = null;
            LogFormatted_DebugOnly("Attempting to Grab TAC LS Types...");

            //find the base type
            TACLSType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "Tac.TacLifeSupport");

            if (TACLSType == null)
            {
                return false;
            }

            LogFormatted("TAC LS Version:{0}", TACLSType.Assembly.GetName().Version.ToString());

            TACLSgameSettingsType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "Tac.TacGameSettings");

            if (TACLSgameSettingsType == null)
            {
                return false;
            }

            TACLSglobalSettingsType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "Tac.GlobalSettings");

            if (TACLSgameSettingsType == null)
            {
                return false;
            }

            TACLSGenericConverterType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "Tac.TacGenericConverter");

            if (TACLSGenericConverterType == null)
            {
                return false;
            }

            //now grab the running instance
            LogFormatted_DebugOnly("Got Assembly Types, grabbing Instances");
            try
            {
                actualTACLS = TACLSType.GetMember("get_Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            }
            catch (Exception)
            {
                LogFormatted("No TAC LS Instance found");
                //throw;
            }

            if (actualTACLS == null)
            {
                LogFormatted("Failed grabbing Instance");
                return false;
            }
            //If we get this far we can set up the local object and its methods/functions
            LogFormatted_DebugOnly("Got Instance, Creating Wrapper Objects");

            TACactualAPI = new TACLSAPI(actualTACLS);

            _TACLSWrapped = true;
            return true;
        }

        public class TACLSAPI
        {
            public static TACLSAPI Instance { get; private set; }

            internal TACLSAPI(Object a)
            {
                Instance = this;
                actualTACLSAPI = a;
                getInstanceMethod = TACLSType.GetMethod("get_Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                TAClifeSupport = getInstance();

                getgblsettingsInstanceMethod = TACLSType.GetMethod("get_globalSettings", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                actualTACLSglobalSettings = getgblsettingsInstance();

                BaseElectricityConsumptionRateMethod = TACLSglobalSettingsType.GetMethod("get_BaseElectricityConsumptionRate", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                ElectricityConsumptionRateMethod = TACLSglobalSettingsType.GetMethod("get_ElectricityConsumptionRate", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

                getgameSettingsInstanceMethod = TACLSType.GetMethod("get_gameSettings", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                actualTACLSgameSettings = getgameSettingsInstance();
                getEnabledMethod = TACLSgameSettingsType.GetMethod("get_Enabled", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            }

            private Object actualTACLSAPI;
            private Object TAClifeSupport;
            private Object actualTACLSglobalSettings;
            private Object actualTACLSgameSettings;

            #region Methods

            private MethodInfo getInstanceMethod;

            /// <summary>
            /// Get the instance of TacLifeSupport class from Tac
            /// </summary>
            /// <returns>TacLifeSupport object</returns>
            internal Object getInstance()
            {
                try
                {
                    return (Object)getInstanceMethod.Invoke(actualTACLSAPI, null);
                }
                catch (Exception ex)
                {
                    LogFormatted("Unable to invoke TAC LS get_Instance Method");
                    LogFormatted("Exception: {0}", ex);
                    return false;
                    //throw;
                }
            }

            private MethodInfo getgblsettingsInstanceMethod;

            /// <summary>
            /// Get the instance of TacLifeSupport class from Tac
            /// </summary>
            /// <returns>TacLifeSupport object</returns>
            private Object getgblsettingsInstance()
            {
                try
                {
                    return (Object)getgblsettingsInstanceMethod.Invoke(TAClifeSupport, null);
                }
                catch (Exception ex)
                {
                    LogFormatted("Unable to invoke TAC LS get_Instance globalSettings Method");
                    LogFormatted("Exception: {0}", ex);
                    return false;
                    //throw;
                }
            }

            private MethodInfo BaseElectricityConsumptionRateMethod;

            /// <summary>
            /// How much power the scanner is using
            /// </summary>
            public double BaseElectricityConsumptionRate
            {
                get { return (double)BaseElectricityConsumptionRateMethod.Invoke(actualTACLSglobalSettings, null); }
            }

            private MethodInfo ElectricityConsumptionRateMethod;

            /// <summary>
            /// How much power the scanner is using
            /// </summary>
            public double ElectricityConsumptionRate
            {
                get { return (double)ElectricityConsumptionRateMethod.Invoke(actualTACLSglobalSettings, null); }
            }

            private MethodInfo getgameSettingsInstanceMethod;

            /// <summary>
            /// Get the instance of TacLifeSupport class from Tac
            /// </summary>
            /// <returns>TacLifeSupport object</returns>
            private Object getgameSettingsInstance()
            {
                try
                {
                    return (Object)getgameSettingsInstanceMethod.Invoke(TAClifeSupport, null);
                }
                catch (Exception ex)
                {
                    LogFormatted("Unable to invoke TAC LS get_Instance gameSettings Method");
                    LogFormatted("Exception: {0}", ex);
                    return false;
                    //throw;
                }
            }

            private MethodInfo getEnabledMethod;

            /// <summary>
            /// Get the Enabled flag of TacLifeSupport gameSettings class from Tac
            /// </summary>
            /// <returns>TacLifeSupport object</returns>
            internal bool getEnabled()
            {
                try
                {
                    return (bool)getEnabledMethod.Invoke(actualTACLSgameSettings, null);
                }
                catch (Exception ex)
                {
                    LogFormatted("Unable to invoke TAC LS gameSettings get_Enabled Method");
                    LogFormatted("Exception: {0}", ex);
                    return false;
                    //throw;
                }
            }

            #endregion Methods

        }

        /// <summary>
        /// The Type that is an analogue of the real TAC LS. This lets you access all the API-able properties and Methods of TAC LS
        /// </summary>
        public class TACLSGenericConverter
        {
            internal TACLSGenericConverter(Object a)
            {
                //store the actual object
                actualTACLSGenericConverter = a;
                converterEnabledField = TACLSGenericConverterType.GetField("converterEnabled");
                inputResourcesField = TACLSGenericConverterType.GetField("inputResources");
                outputResourcesField = TACLSGenericConverterType.GetField("outputResources");
                conversionRateField = TACLSGenericConverterType.GetField("conversionRate");
                ActivateConverterMethod = TACLSGenericConverterType.GetMethod("ActivateConverter", BindingFlags.Public | BindingFlags.Instance);
                DeactivateConverterMethod = TACLSGenericConverterType.GetMethod("DeactivateConverter", BindingFlags.Public | BindingFlags.Instance);
            }

            private Object actualTACLSGenericConverter;

            private FieldInfo converterEnabledField;

            /// <summary>
            /// How much power the scanner is using
            /// </summary>
            public bool converterEnabled
            {
                get { return (bool)converterEnabledField.GetValue(actualTACLSGenericConverter); }
            }

            private FieldInfo inputResourcesField;

            /// <summary>
            /// How much power the scanner is using
            /// </summary>
            public string inputResources
            {
                get { return (string)inputResourcesField.GetValue(actualTACLSGenericConverter); }
            }

            private FieldInfo conversionRateField;

            /// <summary>
            /// The rate to multiple the input and output resources by
            /// </summary>
            public float conversionRate
            {
                get { return (float)conversionRateField.GetValue(actualTACLSGenericConverter); }
            }

            private FieldInfo outputResourcesField;

            /// <summary>
            /// How much power the scanner is using
            /// </summary>
            public string outputResources
            {
                get { return (string)outputResourcesField.GetValue(actualTACLSGenericConverter); }
            }

            private MethodInfo ActivateConverterMethod;

            /// <summary>
            /// Activate Converter
            /// </summary>
            /// <returns>Bool indicating success of call</returns>
            public bool ActivateConverter()
            {
                try
                {
                    ActivateConverterMethod.Invoke(actualTACLSGenericConverter, null);
                    return true;
                }
                catch (Exception ex)
                {
                    LogFormatted("Unable to invoke ActivateConverter Method: {0}", ex.Message);
                    return false;
                }
            }

            private MethodInfo DeactivateConverterMethod;

            /// <summary>
            /// Deactivate Converter
            /// </summary>
            /// <returns>Bool indicating success of call</returns>
            public bool DeactivateConverter()
            {
                try
                {
                    DeactivateConverterMethod.Invoke(actualTACLSGenericConverter, null);
                    return true;
                }
                catch (Exception ex)
                {
                    LogFormatted("Unable to invoke DeactivateConverter Method: {0}", ex.Message);
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