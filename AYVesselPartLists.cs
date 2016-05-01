using System;
using System.Collections.Generic;
using System.Linq;
using RSTUtils;

namespace AY
{
    internal static class AYVesselPartLists
    {
        internal static SortedDictionary<string, PwrPartList> VesselProdPartsList { get; set; }
        internal static SortedDictionary<string, PwrPartList> VesselConsPartsList { get; set; }
        
        internal static void InitDictionaries()
        {
            VesselProdPartsList = new SortedDictionary<string, PwrPartList>();
            VesselConsPartsList = new SortedDictionary<string, PwrPartList>();
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

        internal static void AddPart(uint pkey, PwrPartList partAdd, bool prodPrt, bool partSolar)
        {
            PwrPartList partFnd;

            string keyValue = CreatePartKey(pkey, partAdd.PrtModuleName);

            if (prodPrt) // Producer part list
            {
                if (VesselProdPartsList.TryGetValue(keyValue, out partFnd))   //Try to get entry with index = 1 for key. If found, find the next available index
                {
                    partAdd.PrtEditorInclude = VesselProdPartsList[keyValue].PrtEditorInclude;
                    partAdd.PrtUserEditorInclude = VesselProdPartsList[keyValue].PrtUserEditorInclude;
                    partAdd.PrtEmergShutDnPriority = VesselProdPartsList[keyValue].PrtEmergShutDnPriority;
                    partAdd.PrtEmergShutDnInclude = VesselProdPartsList[keyValue].PrtEmergShutDnInclude;
                    if (!AYController.Emergencypowerdownactivated && !AYController.Emergencypowerdownreset 
                        && !AYController.EmgcyShutOverrideStarted)
                        partAdd.PrtPreEmergShutDnStateActive = partAdd.PrtActive;
                    if (AYController.ShowDarkSideWindow && partSolar)
                        partAdd.PrtEditorInclude = false;
                    if (partAdd.PrtModuleName != "ModuleResourceConverter" || partFnd.PrtPowerF == 0.0)
                    {
                        if (partAdd.PrtEditorInclude && partAdd.PrtActive)
                            AYController.TotalPowerProduced += partAdd.PrtPowerF;
                        partAdd.PrtPowerF += partFnd.PrtPowerF;
                        partAdd.PrtPower = partAdd.PrtPowerF.ToString("####0.##");
                    }
                    partAdd.PrtSolarDependant = partSolar;
                    var tmpEntry =
                        AYController.Instance.AYsettings.PartModuleEmergShutDnDflt.FirstOrDefault(a => a.Key == partAdd.PrtModuleName);
                    if (tmpEntry.Key != null)
                        partAdd.ValidprtEmergShutDn = tmpEntry.Value.EmergShutDnDflt;
                    else
                    {
                        partAdd.ValidprtEmergShutDn = false;
                    }
                    VesselProdPartsList[keyValue] = partAdd;
                }
                else
                {
                    if (AYController.ShowDarkSideWindow && partSolar)
                    {
                        partAdd.PrtEditorInclude = false;
                        partAdd.PrtUserEditorInclude = true;
                    }
                    else
                    {
                        partAdd.PrtEditorInclude = true;
                        partAdd.PrtUserEditorInclude = true;
                    }
                    if (partAdd.PrtEditorInclude && partAdd.PrtActive)
                        AYController.TotalPowerProduced += partAdd.PrtPowerF;
                    partAdd.PrtPower = partAdd.PrtPowerF.ToString("####0.##");
                    partAdd.PrtSolarDependant = partSolar;
                    var tmpEntry =
                        AYController.Instance.AYsettings.PartModuleEmergShutDnDflt.FirstOrDefault(a => a.Key == partAdd.PrtModuleName);
                    if (tmpEntry.Key != null)
                    {
                        partAdd.ValidprtEmergShutDn = tmpEntry.Value.EmergShutDnDflt;
                        partAdd.PrtEmergShutDnPriority = tmpEntry.Value.EmergShutPriority;
                    }
                    else
                    {
                        partAdd.ValidprtEmergShutDn = false;
                        partAdd.PrtEmergShutDnPriority = ESPPriority.MEDIUM;
                    }
                    VesselProdPartsList.Add(keyValue, partAdd);
                }
            }
            else // consumer part list
            {
                if (VesselConsPartsList.TryGetValue(keyValue, out partFnd))
                {
                    partAdd.PrtEditorInclude = VesselConsPartsList[keyValue].PrtEditorInclude;
                    partAdd.PrtEmergShutDnPriority = VesselConsPartsList[keyValue].PrtEmergShutDnPriority;
                    partAdd.PrtEmergShutDnInclude = VesselConsPartsList[keyValue].PrtEmergShutDnInclude;
                    if (!AYController.Emergencypowerdownactivated && !AYController.Emergencypowerdownreset 
                        && !AYController.EmgcyShutOverrideStarted)
                        partAdd.PrtPreEmergShutDnStateActive = partAdd.PrtActive;
                    if (partAdd.PrtModuleName != "ModuleResourceConverter" || partFnd.PrtPowerF == 0.0)
                    {
                        if (partAdd.PrtEditorInclude && partAdd.PrtActive)
                            AYController.TotalPowerDrain += partAdd.PrtPowerF;
                        partAdd.PrtPowerF += partFnd.PrtPowerF;
                        partAdd.PrtPower = partAdd.PrtPowerF.ToString("####0.##");
                    }
                    partAdd.PrtSolarDependant = partSolar;
                    var tmpEntry =
                        AYController.Instance.AYsettings.PartModuleEmergShutDnDflt.FirstOrDefault(a => a.Key == partAdd.PrtModuleName);
                    if (tmpEntry.Key != null)
                        partAdd.ValidprtEmergShutDn = tmpEntry.Value.EmergShutDnDflt;
                    else
                    {
                        partAdd.ValidprtEmergShutDn = false;
                    }
                    VesselConsPartsList[keyValue] = partAdd;
                }
                else
                {
                    if (partAdd.PrtActive)
                        AYController.TotalPowerDrain += partAdd.PrtPowerF;
                    partAdd.PrtPower = partAdd.PrtPowerF.ToString("####0.##");
                    partAdd.PrtSolarDependant = partSolar;
                    var tmpEntry =
                        AYController.Instance.AYsettings.PartModuleEmergShutDnDflt.FirstOrDefault(a => a.Key == partAdd.PrtModuleName);
                    if (tmpEntry.Key != null)
                    {
                        partAdd.ValidprtEmergShutDn = tmpEntry.Value.EmergShutDnDflt;
                        partAdd.PrtEmergShutDnPriority = tmpEntry.Value.EmergShutPriority;
                    }
                    else
                    {
                        partAdd.ValidprtEmergShutDn = false;
                        partAdd.PrtEmergShutDnPriority = ESPPriority.MEDIUM;
                    }
                    VesselConsPartsList.Add(keyValue, partAdd);
                }
            }
            if (AYController.Instance.PartsToDelete.Contains(keyValue))
            {
                AYController.Instance.PartsToDelete.Remove(keyValue);
            }
        }

        internal static string CreatePartKey(uint partId, string partModuleName)
        {
            string returnval = partId + "," + partModuleName;
            return returnval;
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
