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
using System.Reflection;

namespace AY
{
    /// <summary>
    /// The Wrapper class to access KKKabinKraziness
    /// </summary>
    public class KKWrapper
    {
        protected static System.Type KKType;
        protected static Object actualKK;
        protected static System.Type KKControllerType;
        protected static Object actualKKController;

        /// <summary>
        /// This is the Remote Tech API object
        ///
        /// SET AFTER INIT
        /// </summary>
        public static KKAPI KKactualAPI;

        /// <summary>
        /// Whether we found the KabinKraziness assembly in the loadedassemblies.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean AssemblyExists { get { return KKType != null; } }

        /// <summary>
        /// Whether we managed to wrap all the methods/functions from the instance.
        ///
        /// SET AFTER INIT
        /// </summary>
        private static Boolean _KKWrapped;

        /// <summary>
        /// Whether the object has been wrapped
        /// </summary>
        public static Boolean APIReady { get { return _KKWrapped; } }

        /// <summary>
        /// This method will set up the KK object and wrap all the methods/functions
        /// </summary>
        /// <returns></returns>
        public static Boolean InitKKWrapper()
        {
            //reset the internal objects
            _KKWrapped = false;
            LogFormatted_DebugOnly("Attempting to Grab KabinKraziness Types...");

            //find the Controller base type
            KKType = getType("KabinKraziness.KabinKraziness"); 

            if (KKType == null)
            {
                return false;
            }

            LogFormatted("KabinKraziness Version:{0}", KKType.Assembly.GetName().Version.ToString());

            //now grab the running instance
            LogFormatted_DebugOnly("Got Assembly Types, grabbing Instances");
            try
            {
                actualKK = KKType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
            }
            catch (Exception ex)
            {
                LogFormatted("No KabinKraziness Instance found");
                LogFormatted(ex.Message);
                //throw;
            }

            if (actualKK == null)
            {
                LogFormatted("Failed grabbing Instance");
                return false;
            }
            

            //find the Controller base type
            KKControllerType = getType("KabinKraziness.KKController"); 

            if (KKControllerType == null)
            {
                return false;
            }
            
            //now grab the running instance
            LogFormatted_DebugOnly("Got Assembly Types, grabbing Instances");
            try
            {
                actualKKController = KKControllerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
            }
            catch (Exception)
            {
                LogFormatted("No KabinKraziness Controller Instance found");
                //throw;
            }

            if (actualKKController == null)
            {
                LogFormatted("Failed grabbing Instance");
                return false;
            }

            //If we get this far we can set up the local object and its methods/functions
            LogFormatted_DebugOnly("Got Instance, Creating Wrapper Objects");

            KKactualAPI = new KKAPI(actualKK, actualKKController);

            _KKWrapped = true;
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

        public class KKAPI
        {
            public static KKAPI Instance { get; private set; }

            internal KKAPI(Object a, object b)
            {
                Instance = this;
                actualKKAPI = a;
                actualKKControllerAPI = b;
                
                AutoPilotDisabledField = KKControllerType.GetField("autoPilotDisabled");
                autoPilotDisTimeField = KKControllerType.GetField("autoPilotDisTime");
                autoPilotDisCounterField = KKControllerType.GetField("autoPilotDisCounter");
                firstMajCrazyWarningField = KKControllerType.GetField("firstMajCrazyWarning");
                firstMinCrazyWarningField = KKControllerType.GetField("firstMinCrazyWarning");

                getCLMT_BSE_DRN_FTRMethod = KKControllerType.GetMethod("get_CLMT_BSE_DRN_FTR", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                setCLMT_BSE_DRN_FTRMethod = KKControllerType.GetMethod("set_CLMT_BSE_DRN_FTR", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                getCLMT_TGT_TMPMethod = KKControllerType.GetMethod("get_CLMT_TGT_TMP", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                setCLMT_TGT_TMPMethod = KKControllerType.GetMethod("set_CLMT_TGT_TMP", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                getMSG_BSE_DRN_FTRMethod = KKControllerType.GetMethod("get_MSG_BSE_DRN_FTR", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                setMSG_BSE_DRN_FTRMethod = KKControllerType.GetMethod("set_MSG_BSE_DRN_FTR", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                getMSG_BSE_DRN_FTRMethod = KKControllerType.GetMethod("get_MSG_BSE_DRN_FTR", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                setMSG_BSE_DRN_FTRMethod = KKControllerType.GetMethod("set_MSG_BSE_DRN_FTR", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                getCRZ_BSE_DRN_FTRMethod = KKControllerType.GetMethod("get_CRZ_BSE_DRN_FTR", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                setCRZ_BSE_DRN_FTRMethod = KKControllerType.GetMethod("set_CRZ_BSE_DRN_FTR", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                getCRZ_CTE_UNC_FTRMethod = KKControllerType.GetMethod("get_CRZ_CTE_UNC_FTR", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                setCRZ_CTE_UNC_FTRMethod = KKControllerType.GetMethod("set_CRZ_CTE_UNC_FTR", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                getCRZ_CTE_RED_FTRMethod = KKControllerType.GetMethod("get_CRZ_CTE_RED_FTR", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                setCRZ_CTE_RED_FTRMethod = KKControllerType.GetMethod("set_CRZ_CTE_RED_FTR", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                getCRZ_RDO_RED_FTRMethod = KKControllerType.GetMethod("get_CRZ_RDO_RED_FTR", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                setCRZ_RDO_RED_FTRMethod = KKControllerType.GetMethod("set_CRZ_RDO_RED_FTR", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                getCRZ_MSG_RED_FTRMethod = KKControllerType.GetMethod("get_CRZ_MSG_RED_FTR", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                setCRZ_MSG_RED_FTRMethod = KKControllerType.GetMethod("set_CRZ_MSG_RED_FTR", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                getCRZ_MINOR_LMTMethod = KKControllerType.GetMethod("get_CRZ_MINOR_LMT", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                setCRZ_MINOR_LMTMethod = KKControllerType.GetMethod("set_CRZ_MINOR_LMT", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                getCRZ_MAJOR_LMTMethod = KKControllerType.GetMethod("get_CRZ_MAJOR_LMT", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                setCRZ_MAJOR_LMTMethod = KKControllerType.GetMethod("set_CRZ_MAJOR_LMT", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                getAutoTimerMethod = KKControllerType.GetMethod("get_AutoTimer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            }

            private Object actualKKAPI;
            private Object actualKKControllerAPI;

            #region Methods

            private MethodInfo getCLMT_BSE_DRN_FTRMethod;
            private MethodInfo setCLMT_BSE_DRN_FTRMethod;

            /// <summary>
            /// Get and Set the CLMT_BSE_DRN_FTR of KabinKraziness KKSettings class from KabinKraziness
            /// </summary>
            /// <param name="value">double value to set the variable to</param>
            /// <returns>double value of field</returns>
            internal double CLMT_BSE_DRN_FTR
            {
                get
                {
                    try
                    {
                        return (double) getCLMT_BSE_DRN_FTRMethod.Invoke(actualKKControllerAPI, null);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness get_CLMT_BSE_DRN_FTR Method");
                        LogFormatted("Exception: {0}", ex);
                        return 0;
                        //throw;
                    }
                }

                set
                {
                    try
                    {
                        setCLMT_BSE_DRN_FTRMethod.Invoke(actualKKControllerAPI, new object[] {value});
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness set_CLMT_BSE_DRN_FTR Method");
                        LogFormatted("Exception: {0}", ex);
                    }
                }
            }

            private MethodInfo getCLMT_TGT_TMPMethod;
            private MethodInfo setCLMT_TGT_TMPMethod;

            /// <summary>
            /// Get and Set the CLMT_TGT_TMP of KabinKraziness KKSettings class from KabinKraziness
            /// </summary>
            /// <param name="value">float value to set the variable to</param>
            /// <returns>float value of field</returns>
            internal float CLMT_TGT_TMP
            {
                get
                {
                    try
                    {
                        return (float)getCLMT_TGT_TMPMethod.Invoke(actualKKControllerAPI, null);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness get_CLMT_TGT_TMP Method");
                        LogFormatted("Exception: {0}", ex);
                        return 0;
                        //throw;
                    }
                }

                set
                {
                    try
                    {
                        setCLMT_TGT_TMPMethod.Invoke(actualKKControllerAPI, new object[] { value });
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness set_CLMT_TGT_TMP Method");
                        LogFormatted("Exception: {0}", ex);
                    }
                }
            }

            private MethodInfo getMSG_BSE_DRN_FTRMethod;
            private MethodInfo setMSG_BSE_DRN_FTRMethod;

            /// <summary>
            /// Get and Set the CLMT_BSE_DRN_FTR of KabinKraziness KKSettings class from KabinKraziness
            /// </summary>
            /// <param name="value">double value to set the variable to</param>
            /// <returns>double value of field</returns>
            internal double MSG_BSE_DRN_FTR
            {
                get
                {
                    try
                    {
                        return (double)getMSG_BSE_DRN_FTRMethod.Invoke(actualKKControllerAPI, null);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness get_MSG_BSE_DRN_FTR Method");
                        LogFormatted("Exception: {0}", ex);
                        return 0;
                        //throw;
                    }
                }

                set
                {
                    try
                    {
                        setMSG_BSE_DRN_FTRMethod.Invoke(actualKKControllerAPI, new object[] { value });
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness set_MSG_BSE_DRN_FTR Method");
                        LogFormatted("Exception: {0}", ex);
                    }
                }
            }

            private MethodInfo getCRZ_BSE_DRN_FTRMethod;
            private MethodInfo setCRZ_BSE_DRN_FTRMethod;

            /// <summary>
            /// Get and Set the CLMT_BSE_DRN_FTR of KabinKraziness KKSettings class from KabinKraziness
            /// </summary>
            /// <param name="value">double value to set the variable to</param>
            /// <returns>double value of field</returns>
            internal double CRZ_BSE_DRN_FTR
            {
                get
                {
                    try
                    {
                        return (double)getCRZ_BSE_DRN_FTRMethod.Invoke(actualKKControllerAPI, null);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness get_CRZ_BSE_DRN_FTR Method");
                        LogFormatted("Exception: {0}", ex);
                        return 0;
                        //throw;
                    }
                }

                set
                {
                    try
                    {
                        setCRZ_BSE_DRN_FTRMethod.Invoke(actualKKControllerAPI, new object[] { value });
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness set_CRZ_BSE_DRN_FTR Method");
                        LogFormatted("Exception: {0}", ex);
                    }
                }
            }

            private MethodInfo getCRZ_CTE_UNC_FTRMethod;
            private MethodInfo setCRZ_CTE_UNC_FTRMethod;

            /// <summary>
            /// Get and Set the CLMT_BSE_DRN_FTR of KabinKraziness KKSettings class from KabinKraziness
            /// </summary>
            /// <param name="value">double value to set the variable to</param>
            /// <returns>double value of field</returns>
            internal double CRZ_CTE_UNC_FTR
            {
                get
                {
                    try
                    {
                        return (double)getCRZ_CTE_UNC_FTRMethod.Invoke(actualKKControllerAPI, null);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness get_CRZ_CTE_UNC_FTR Method");
                        LogFormatted("Exception: {0}", ex);
                        return 0;
                        //throw;
                    }
                }

                set
                {
                    try
                    {
                        setCRZ_CTE_UNC_FTRMethod.Invoke(actualKKControllerAPI, new object[] { value });
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness set_CRZ_CTE_UNC_FTR Method");
                        LogFormatted("Exception: {0}", ex);
                    }
                }
            }

            private MethodInfo getCRZ_CTE_RED_FTRMethod;
            private MethodInfo setCRZ_CTE_RED_FTRMethod;

            /// <summary>
            /// Get and Set the CLMT_BSE_DRN_FTR of KabinKraziness KKSettings class from KabinKraziness
            /// </summary>
            /// <param name="value">double value to set the variable to</param>
            /// <returns>double value of field</returns>
            internal double CRZ_CTE_RED_FTR
            {
                get
                {
                    try
                    {
                        return (double)getCRZ_CTE_RED_FTRMethod.Invoke(actualKKControllerAPI, null);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness get_CRZ_CTE_RED_FTR Method");
                        LogFormatted("Exception: {0}", ex);
                        return 0;
                        //throw;
                    }
                }

                set
                {
                    try
                    {
                        setCRZ_CTE_RED_FTRMethod.Invoke(actualKKControllerAPI, new object[] { value });
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness set_CRZ_CTE_RED_FTR Method");
                        LogFormatted("Exception: {0}", ex);
                    }
                }
            }

            private MethodInfo getCRZ_RDO_RED_FTRMethod;
            private MethodInfo setCRZ_RDO_RED_FTRMethod;

            /// <summary>
            /// Get and Set the CLMT_BSE_DRN_FTR of KabinKraziness KKSettings class from KabinKraziness
            /// </summary>
            /// <param name="value">double value to set the variable to</param>
            /// <returns>double value of field</returns>
            internal double CRZ_RDO_RED_FTR
            {
                get
                {
                    try
                    {
                        return (double)getCRZ_RDO_RED_FTRMethod.Invoke(actualKKControllerAPI, null);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness get_CRZ_RDO_RED_FTR Method");
                        LogFormatted("Exception: {0}", ex);
                        return 0;
                        //throw;
                    }
                }

                set
                {
                    try
                    {
                        setCRZ_RDO_RED_FTRMethod.Invoke(actualKKControllerAPI, new object[] { value });
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness set_CRZ_RDO_RED_FTR Method");
                        LogFormatted("Exception: {0}", ex);
                    }
                }
            }

            private MethodInfo getCRZ_MSG_RED_FTRMethod;
            private MethodInfo setCRZ_MSG_RED_FTRMethod;

            /// <summary>
            /// Get and Set the CLMT_BSE_DRN_FTR of KabinKraziness KKSettings class from KabinKraziness
            /// </summary>
            /// <param name="value">double value to set the variable to</param>
            /// <returns>double value of field</returns>
            internal double CRZ_MSG_RED_FTR
            {
                get
                {
                    try
                    {
                        return (double)getCRZ_MSG_RED_FTRMethod.Invoke(actualKKControllerAPI, null);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness get_CRZ_MSG_RED_FTR Method");
                        LogFormatted("Exception: {0}", ex);
                        return 0;
                        //throw;
                    }
                }

                set
                {
                    try
                    {
                        setCRZ_MSG_RED_FTRMethod.Invoke(actualKKControllerAPI, new object[] { value });
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness set_CRZ_MSG_RED_FTR Method");
                        LogFormatted("Exception: {0}", ex);
                    }
                }
            }

            private MethodInfo getCRZ_MINOR_LMTMethod;
            private MethodInfo setCRZ_MINOR_LMTMethod;

            /// <summary>
            /// Get and Set the CLMT_BSE_DRN_FTR of KabinKraziness KKSettings class from KabinKraziness
            /// </summary>
            /// <param name="value">double value to set the variable to</param>
            /// <returns>double value of field</returns>
            internal double CRZ_MINOR_LMT
            {
                get
                {
                    try
                    {
                        return (double)getCRZ_MINOR_LMTMethod.Invoke(actualKKControllerAPI, null);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness get_CRZ_MINOR_LMT Method");
                        LogFormatted("Exception: {0}", ex);
                        return 0;
                        //throw;
                    }
                }

                set
                {
                    try
                    {
                        setCRZ_MINOR_LMTMethod.Invoke(actualKKControllerAPI, new object[] { value });
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness set_CRZ_MINOR_LMT Method");
                        LogFormatted("Exception: {0}", ex);
                    }
                }
            }

            private MethodInfo getCRZ_MAJOR_LMTMethod;
            private MethodInfo setCRZ_MAJOR_LMTMethod;

            /// <summary>
            /// Get and Set the CLMT_BSE_DRN_FTR of KabinKraziness KKSettings class from KabinKraziness
            /// </summary>
            /// <param name="value">double value to set the variable to</param>
            /// <returns>double value of field</returns>
            internal double CRZ_MAJOR_LMT
            {
                get
                {
                    try
                    {
                        return (double)getCRZ_MAJOR_LMTMethod.Invoke(actualKKControllerAPI, null);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness get_CRZ_MAJOR_LMT Method");
                        LogFormatted("Exception: {0}", ex);
                        return 0;
                        //throw;
                    }
                }

                set
                {
                    try
                    {
                        setCRZ_MAJOR_LMTMethod.Invoke(actualKKControllerAPI, new object[] { value });
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness set_CRZ_MAJOR_LMT Method");
                        LogFormatted("Exception: {0}", ex);
                    }
                }
            }

            private MethodInfo getAutoTimerMethod;
            
            /// <summary>
            /// Get  the AutoTimer ScreenMessage of KabinKraziness KKSettings class from KabinKraziness
            /// </summary>
            /// <returns>ScreenMessage object</returns>
            internal ScreenMessage AutoTimer
            {
                get
                {
                    try
                    {
                        return (ScreenMessage)getAutoTimerMethod.Invoke(actualKKControllerAPI, null);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke KabinKraziness get_AutoTimer Method");
                        LogFormatted("Exception: {0}", ex);
                        return null;
                        //throw;
                    }
                }
            }

            #endregion Methods

            #region Fields

            private FieldInfo AutoPilotDisabledField;

            /// <summary>
            /// If the AutoPilot is to be disabled or not
            /// </summary>
            public bool AutoPilotDisabled
            {
                get { return (bool)AutoPilotDisabledField.GetValue(actualKKControllerAPI); }
                set { AutoPilotDisabledField.SetValue(actualKKControllerAPI, value); }
            }

            private FieldInfo autoPilotDisTimeField;

            /// <summary>
            /// If the AutoPilot is to be disabled or not
            /// </summary>
            public double autoPilotDisTime
            {
                get { return (double)autoPilotDisTimeField.GetValue(actualKKControllerAPI); }
                set { autoPilotDisTimeField.SetValue(actualKKControllerAPI, value); }
            }

            private FieldInfo autoPilotDisCounterField;

            /// <summary>
            /// If the AutoPilot is to be disabled or not
            /// </summary>
            public double autoPilotDisCounter
            {
                get { return (double)autoPilotDisCounterField.GetValue(actualKKControllerAPI); }
                set { autoPilotDisCounterField.SetValue(actualKKControllerAPI, value); }
            }

            private FieldInfo firstMajCrazyWarningField;

            /// <summary>
            /// If the AutoPilot is to be disabled or not
            /// </summary>
            public bool firstMajCrazyWarning
            {
                get { return (bool)firstMajCrazyWarningField.GetValue(actualKKControllerAPI); }
                set { firstMajCrazyWarningField.SetValue(actualKKControllerAPI, value); }
            }

            private FieldInfo firstMinCrazyWarningField;

            /// <summary>
            /// If the AutoPilot is to be disabled or not
            /// </summary>
            public bool firstMinCrazyWarning
            {
                get { return (bool)firstMinCrazyWarningField.GetValue(actualKKControllerAPI); }
                set { firstMinCrazyWarningField.SetValue(actualKKControllerAPI, value); }
            }





            #endregion Fields

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