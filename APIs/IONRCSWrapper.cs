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
using System.Reflection;

namespace AY
{
    /// <summary>
    /// The Wrapper class to access IONRCS
    /// </summary>
    public class IONRCSWrapper
    {
        protected static System.Type IONRCSType;
        protected static System.Type PPTRCSType;

        /// <summary>
        /// Whether we found the IONRCS assembly in the loadedassemblies.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean AssemblyExists { get { return (IONRCSType != null && PPTRCSType != null); } }

        /// <summary>
        /// Whether we managed to wrap all the methods/functions from the instance.
        ///
        /// SET AFTER INIT
        /// </summary>
        private static Boolean _IONRCSWrapped;

        /// <summary>
        /// Whether the object has been wrapped
        /// </summary>
        public static Boolean APIReady { get { return _IONRCSWrapped; } }

        /// <summary>
        /// This method will set up the IONRCS object and wrap all the methods/functions
        /// </summary>
        /// <returns></returns>
        public static Boolean InitIONRCSWrapper()
        {
            //reset the internal objects
            _IONRCSWrapped = false;
            LogFormatted_DebugOnly("Attempting to Grab IONRCS Types...");

            //find the IONRCSTypeoduleIONPoweredRCS type
            IONRCSType = getType("IONRCS.ModuleIONPoweredRCS"); 

            if (IONRCSType == null)
            {
                return false;
            }

            //find the ModulePPTPoweredRCS type
            PPTRCSType = getType("IONRCS.ModulePPTPoweredRCS"); 

            if (PPTRCSType == null)
            {
                return false;
            }

            LogFormatted("IONRCS Version:{0}", IONRCSType.Assembly.GetName().Version.ToString());

            _IONRCSWrapped = true;
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

        public class ModuleIONPoweredRCS
        {
            internal ModuleIONPoweredRCS(Object a)
            {
                actualModuleIONPoweredRCS = a;
                LogFormatted_DebugOnly("Getting powerRatioField Field");
                powerRatioField = IONRCSType.GetField("powerRatio");
                LogFormatted_DebugOnly("Success: " + (powerRatioField != null));
                LogFormatted_DebugOnly("Getting ElecUsedField Field");
                ElecUsedField = IONRCSType.GetField("ElecUsed");
                LogFormatted_DebugOnly("Success: " + (ElecUsedField != null));
            }

            private Object actualModuleIONPoweredRCS;

            private FieldInfo powerRatioField;

            /// <summary>
            /// The current EC ration usage
            /// </summary>
            /// <returns>float value</returns>
            public float powerRatio
            {
                get
                {
                    try
                    {
                        return (float)powerRatioField.GetValue(actualModuleIONPoweredRCS);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to get powerRatio field");
                        LogFormatted("Exception: {0}", ex);
                        return 0;
                    }

                }
            }

            private FieldInfo ElecUsedField;

            /// <summary>
            /// The current EC ratio usage
            /// </summary>
            /// <returns>float value</returns>
            public float ElecUsed
            {
                get
                {
                    try
                    {
                        return (float)ElecUsedField.GetValue(actualModuleIONPoweredRCS);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to get ElecUsed field");
                        LogFormatted("Exception: {0}", ex);
                        return 0;
                    }

                }
            }
        }

        public class ModulePPTPoweredRCS
        {
            internal ModulePPTPoweredRCS(Object a)
            {
                actualModulePPTPoweredRCS = a;
                LogFormatted_DebugOnly("Getting powerRatioField Field");
                powerRatioField = PPTRCSType.GetField("powerRatio");
                LogFormatted_DebugOnly("Success: " + (powerRatioField != null));
                LogFormatted_DebugOnly("Getting ElecUsedField Field");
                ElecUsedField = PPTRCSType.GetField("ElecUsed");
                LogFormatted_DebugOnly("Success: " + (ElecUsedField != null));
            }

            private Object actualModulePPTPoweredRCS;

            private FieldInfo powerRatioField;

            /// <summary>
            /// The current EC ration usage
            /// </summary>
            /// <returns>float value</returns>
            public float powerRatio
            {
                get
                {
                    try
                    {
                        return (float)powerRatioField.GetValue(actualModulePPTPoweredRCS);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to get powerRatio field");
                        LogFormatted("Exception: {0}", ex);
                        return 0;
                    }

                }
            }

            private FieldInfo ElecUsedField;

            /// <summary>
            /// The current EC ratio usage
            /// </summary>
            /// <returns>float value</returns>
            public float ElecUsed
            {
                get
                {
                    try
                    {
                        return (float)ElecUsedField.GetValue(actualModulePPTPoweredRCS);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to get ElecUsed field");
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
