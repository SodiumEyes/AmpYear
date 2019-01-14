using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RSTUtils;

namespace AY
{
    internal static class AYVesselPartLists
    {
        internal static Dictionary<string, PwrPartList> VesselProdPartsList { get; set; }
        internal static Dictionary<string, PwrPartList> VesselConsPartsList { get; set; }
        
        internal static void InitDictionaries()
        {
            VesselProdPartsList = new Dictionary<string, PwrPartList>();
            VesselConsPartsList = new Dictionary<string, PwrPartList>();
        }

        internal static void ResetDictionaries()
        {
            try
            {
                VesselProdPartsList.Clear();
                VesselConsPartsList.Clear();
            }
            catch (Exception ex)
            {
                Utilities.Log("AYVesselPartLists Exception occurred resetting the dictionaries");
                Utilities.Log("Exception: {0}", ex);
            }

        }

        //Current Vessel Parts list maintenance - used primarily in editor for parts list, also used for emergency shutdown procedure
        internal static PwrPartList partFnd;
        internal static KeyValuePair<ValidEmergencyPartModule, ESPValues> tmpESPDfltValue;

        /// <summary>
        /// Add or Amend part in the VesselProdPartsList or VesselConsPartsList
        /// </summary>
        /// <param name="pkey">the CraftID</param>
        /// <param name="prtName">the part Name</param>
        /// <param name="prtModuleName">the partmodule Module Name</param>
        /// <param name="prtSubsystem">True if it is an AmpYear Subsystem, otherwise False</param>
        /// <param name="prtActive">True if part is Active, otherwise False</param>
        /// <param name="prtPowerF">The Amount of EC being drawn (float)</param>
        /// <param name="prodPrt">True if it is producing Power, otherwise False</param>
        /// <param name="partSolar">True if the part is Solar dependant, otherwise False</param>
        /// <param name="partref">Reference to the Part. May also be null for vessel wide things</param>
        internal static void AddPart(uint pkey, string prtName, string prtTitle, string prtModuleName, bool prtSubsystem, bool prtActive, double prtPowerF, bool prodPrt, bool partSolar, Part partref)
        {
            string keyValue = CreatePartKey(pkey, prtModuleName);

            if (prodPrt) // Producer part list
            {
                if (VesselProdPartsList.TryGetValue(keyValue, out partFnd))   //Try to get entry with index = 1 for key. If found, find the next available index
                {
                    partFnd.PrtActive = prtActive;
                    partFnd.PrtName = prtName;
                    partFnd.PrtTitle = prtTitle;
                    partFnd.PrtSubsystem = prtSubsystem;
                    if (!AYController.Emergencypowerdownactivated && !AYController.Emergencypowerdownreset 
                        && !AYController.EmgcyShutOverrideStarted)
                        //VesselProdPartsList[keyValue].PrtPreEmergShutDnStateActive = prtActive;
                        partFnd.PrtPreEmergShutDnStateActive = prtActive;
                    if (AYController.ShowDarkSideWindow && partSolar)
                        partFnd.PrtEditorInclude = false;
                    if (prtModuleName != "ModuleResourceConverter" || partFnd.PrtPowerF == 0.0)
                    {
                        if (partFnd.PrtEditorInclude && prtActive)
                            AYController.Instance.TotalPowerProduced += prtPowerF;
                        partFnd.PrtPowerF += (float)prtPowerF;
                        partFnd.PrtPower = partFnd.PrtPowerF.ToString("####0.###");
                    }
                    partFnd.PrtSolarDependant = partSolar;
                    tmpESPDfltValue =
                        AYController.Instance.AYsettings.PartModuleEmergShutDnDflt.FirstOrDefault(a => a.Key.Name == prtModuleName);
                    if (tmpESPDfltValue.Key != null)
                        partFnd.ValidprtEmergShutDn = tmpESPDfltValue.Value.EmergShutDnDflt;
                    else
                    {
                        partFnd.ValidprtEmergShutDn = false;
                    }
                }
                else
                {
                    PwrPartList newProdPart = new PwrPartList(prtName, prtTitle, prtModuleName, prtSubsystem, "", (float)prtPowerF, prtActive, partSolar, partref);
                    if (AYController.ShowDarkSideWindow && partSolar)
                    {
                        newProdPart.PrtEditorInclude = false;
                        newProdPart.PrtUserEditorInclude = true;
                    }
                    else
                    {
                        newProdPart.PrtEditorInclude = true;
                        newProdPart.PrtUserEditorInclude = true;
                    }
                    if (newProdPart.PrtEditorInclude && prtActive)
                        AYController.Instance.TotalPowerProduced += prtPowerF;
                    newProdPart.PrtPower = newProdPart.PrtPowerF.ToString("####0.###");
                    newProdPart.PrtSolarDependant = partSolar;
                    tmpESPDfltValue =
                        AYController.Instance.AYsettings.PartModuleEmergShutDnDflt.FirstOrDefault(a => a.Key.Name == prtModuleName);
                    if (tmpESPDfltValue.Key != null)
                    {
                        newProdPart.ValidprtEmergShutDn = tmpESPDfltValue.Value.EmergShutDnDflt;
                        newProdPart.PrtEmergShutDnPriority = tmpESPDfltValue.Value.EmergShutPriority;
                    }
                    else
                    {
                        newProdPart.ValidprtEmergShutDn = false;
                        newProdPart.PrtEmergShutDnPriority = ESPPriority.MEDIUM;
                    }
                    VesselProdPartsList.Add(keyValue, newProdPart);
                }
            }
            else // consumer part list
            {
                if (VesselConsPartsList.TryGetValue(keyValue, out partFnd))
                {
                    partFnd.PrtActive = prtActive;
                    partFnd.PrtName = prtName;
                    partFnd.PrtTitle = prtTitle;
                    partFnd.PrtSubsystem = prtSubsystem;
                    if (!AYController.Emergencypowerdownactivated && !AYController.Emergencypowerdownreset 
                        && !AYController.EmgcyShutOverrideStarted)
                        partFnd.PrtPreEmergShutDnStateActive = prtActive;
                    if (prtModuleName != "ModuleResourceConverter" || partFnd.PrtPowerF == 0.0)
                    {
                        if (partFnd.PrtEditorInclude && prtActive)
                            AYController.Instance.TotalPowerDrain += prtPowerF;
                        partFnd.PrtPowerF += (float)prtPowerF;
                        partFnd.PrtPower = partFnd.PrtPowerF.ToString("####0.###");
                    }
                    partFnd.PrtSolarDependant = partSolar;
                    tmpESPDfltValue =
                        AYController.Instance.AYsettings.PartModuleEmergShutDnDflt.FirstOrDefault(a => a.Key.Name == prtModuleName);
                    if (tmpESPDfltValue.Key != null)
                        partFnd.ValidprtEmergShutDn = tmpESPDfltValue.Value.EmergShutDnDflt;
                    else
                    {
                        partFnd.ValidprtEmergShutDn = false;
                    }
                }
                else
                {
                    PwrPartList newConsPart = new PwrPartList(prtName, prtTitle, prtModuleName, prtSubsystem, "", (float)prtPowerF, prtActive, partSolar, partref);
                    if (prtActive)
                        AYController.Instance.TotalPowerDrain += prtPowerF;
                    newConsPart.PrtPowerF = (float)prtPowerF;
                    newConsPart.PrtPower = newConsPart.PrtPowerF.ToString("####0.###");
                    newConsPart.PrtSolarDependant = partSolar;
                    tmpESPDfltValue =
                        AYController.Instance.AYsettings.PartModuleEmergShutDnDflt.FirstOrDefault(a => a.Key.Name == prtModuleName);
                    if (tmpESPDfltValue.Key != null)
                    {
                        newConsPart.ValidprtEmergShutDn = tmpESPDfltValue.Value.EmergShutDnDflt;
                        newConsPart.PrtEmergShutDnPriority = tmpESPDfltValue.Value.EmergShutPriority;
                    }
                    else
                    {
                        newConsPart.ValidprtEmergShutDn = false;
                        newConsPart.PrtEmergShutDnPriority = ESPPriority.MEDIUM;
                    }
                    VesselConsPartsList.Add(keyValue, newConsPart);
                }
            }
            if (AYController.Instance.PartsToDelete.Contains(keyValue))
            {
                AYController.Instance.PartsToDelete.Remove(keyValue);
            }
        }

        internal static StringBuilder TempKey = new StringBuilder();
        internal static string CreatePartKey(uint partId, string partModuleName)
        {
            TempKey.Length = 0;
            TempKey.Append(partId);
            TempKey.Append(',');
            TempKey.Append(partModuleName);
            return TempKey.ToString();
        }

        internal static uint GetPartKeyVals(string inputKeyString, out string partModuleName)
        {
            try
            {
                string[] keyValsStrings = inputKeyString.Split(',');
                partModuleName = keyValsStrings.Length == 2 ? keyValsStrings[1] : string.Empty;
                uint partId = uint.Parse(keyValsStrings[0]);
                return partId;
            }
            catch (Exception ex)
            {
                Utilities.Log("AYVesselPartLists Exception occurred getting Key values for key {0}", inputKeyString);
                Utilities.Log("Exception: {0}", ex);
                partModuleName = "";
                return 999999;
            }
            
        }

        internal static int FindFirstIndex(uint partId, string partModuleName, bool prodPartslst)
        {

            //Find the first available Index for PartModule and Part (cater for multiple modules of the same name on the same part, eg: Resource
            int returnvalue = 1;
            
            if (prodPartslst)  //Checking the EC Producing Parts List
            {
                bool notFound = true;
                do
                {
                    string keyValue = CreatePartKey(partId, partModuleName);
                    PwrPartList partFnd;
                    if (VesselProdPartsList.TryGetValue(keyValue, out partFnd))
                    {
                        //First the first available index.
                        returnvalue++;
                    }
                    else
                    {
                        notFound = false;
                    }
                } while (notFound);


            }
            else  //Checking the EC Consuming Parts List
            {
                bool notFound = true;
                do
                {
                    string keyValue = CreatePartKey(partId, partModuleName);
                    PwrPartList partFnd;
                    if (VesselConsPartsList.TryGetValue(keyValue, out partFnd))
                    {
                        //First the first available index.
                        returnvalue++;
                    }
                    else
                    {
                        notFound = false;
                    }
                } while (notFound);
            }
            return returnvalue;
        }

        internal static void ResetSolarPartToggles()
        {
            foreach (KeyValuePair<string, PwrPartList> entry in VesselProdPartsList)
            {
                if (entry.Value.PrtSolarDependant)
                {
                    entry.Value.PrtEditorInclude = entry.Value.PrtUserEditorInclude;
                }
            }
        }

    }

    
}
