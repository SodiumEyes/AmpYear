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
    /// The Wrapper class to access USILS
    /// </summary>
    public class USILSWrapper
    {
        protected static System.Type USIMLSType;
        protected static System.Type USIMLSRType;

        /// <summary>
        /// Whether we found the USILS assembly in the loadedassemblies.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean AssemblyExists { get { return USIMLSType != null; } }

        /// <summary>
        /// Whether we managed to wrap all the methods/functions from the instance.
        ///
        /// SET AFTER INIT
        /// </summary>
        private static Boolean _USILSWrapped;

        /// <summary>
        /// Whether the object has been wrapped
        /// </summary>
        public static Boolean APIReady { get { return _USILSWrapped; } }

        /// <summary>
        /// This method will set up the USILS object and wrap all the methods/functions
        /// </summary>
        /// <returns></returns>
        public static Boolean InitUSILSWrapper()
        {
            //reset the internal objects
            _USILSWrapped = false;
            LogFormatted_DebugOnly("Attempting to Grab USI LS Types...");

            //find the USILS part module type
            USIMLSType = getType("LifeSupport.ModuleLifeSupport"); 

            if (USIMLSType == null)
            {
                return false;
            }

            //find the USILS part recycler module type
            USIMLSRType = getType("LifeSupport.ModuleLifeSupportRecycler"); 

            if (USIMLSRType == null)
            {
                return false;
            }

            LogFormatted("USI LS Version:{0}", USIMLSType.Assembly.GetName().Version.ToString());

            _USILSWrapped = true;
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

        public class ModuleLifeSupport
        {
            internal ModuleLifeSupport(Object a)
            {
                actualModuleLifeSupport = a;

                LifeSupportRecipeMethod = USIMLSType.GetMethod("get_ECRecipe", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }

            private Object actualModuleLifeSupport;

            private MethodInfo LifeSupportRecipeMethod;

            /// <summary>
            /// Get the Resource Converter Recipe
            /// </summary>
            /// <returns>ConversionRecipe or NULL</returns>
            public ConversionRecipe LifeSupportRecipe
            {
                get
                {
                    try
                    {
                        return (ConversionRecipe)LifeSupportRecipeMethod.Invoke(actualModuleLifeSupport, null);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Invoke LifeSupportRecipe Method failed: {0}", ex.Message);
                        return null;
                    }
                }
            }
        }

        public class ModuleLifeSupportRecycler
        {
            internal ModuleLifeSupportRecycler(Object a)
            {
                actualModuleLifeSupportRecycler = a;

                RecyclerIsActiveField = USIMLSRType.GetField("RecyclerIsActive", BindingFlags.Public | BindingFlags.Instance);
                RecyclePercentField = USIMLSRType.GetField("RecyclePercent", BindingFlags.Public | BindingFlags.Instance);
            }

            private Object actualModuleLifeSupportRecycler;

            private FieldInfo RecyclerIsActiveField;

            /// <summary>
            /// Get the Recycler Active Status
            /// </summary>
            /// <returns>Bool indicating if recycler is active or not</returns>
            public bool RecyclerIsActive
            {
                get
                {
                    try
                    {
                        return (bool)RecyclerIsActiveField.GetValue(actualModuleLifeSupportRecycler); 
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Invoke LifeSupportRecycler RecyclerisActive Field failed: {0}", ex.Message);
                        return false;
                    }
                }
            }

            private FieldInfo RecyclePercentField;

            /// <summary>
            /// Get the Recycler Percentage
            /// </summary>
            /// <returns>float value from module or zero</returns>
            public float RecyclePercent
            {
                get
                {
                    try
                    {
                        return (float)RecyclePercentField.GetValue(actualModuleLifeSupportRecycler);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Invoke LifeSupportRecycler RecyclePercent Field failed: {0}", ex.Message);
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
