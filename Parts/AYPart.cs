/**
 * AYPart.cs
 *
 * AmpYear power management.
 * The original code and concept of AmpYear rights go to SodiumEyes on the Kerbal Space Program Forums, which was covered by GNU License GPL (no version stated).
 * As such this code continues to be covered by GNU GPL license.
 *
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 *
 *  This file is part of AmpYear.
 *
 *  AmpYear is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  AmpYear is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with AmpYear.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

using System;
using RSTUtils;

namespace AY
{
    // This PartModule is added to EVERY part in KSP. Which is unfortunate but the only way to save/load the AmpYear settings for each part.
    // It's primary and only purpose is to save and load the AmpYear Emergency Shutdown Procedure settings for each part.
    [KSPModule("AmpYear Part Circuitry")]
    class AYPart : PartModule
    {
        public string VesselProdPartsListConfigNodeName = "VesselProdPartsList";
        public string VesselConsPartsListConfigNodeName = "VesselConsPartsList";
        public string ConfigNodeName = "AYPwrPartList";
        
        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            if (Utilities.GameModeisFlight || Utilities.GameModeisEditor)
            {
                try
                {
                    var thisPartId = part.craftID;
                    ConfigNode vesselProdPartsnode = node.AddNode(VesselProdPartsListConfigNodeName);
                    foreach (var entry in AYVesselPartLists.VesselProdPartsList)
                    {
                        string partModuleName = "";
                        uint partId = AYVesselPartLists.GetPartKeyVals(entry.Key, out partModuleName);
                        if (thisPartId == partId)
                        {
                            ConfigNode prodPartsNode = entry.Value.Save(vesselProdPartsnode);
                            Utilities.Log_Debug("AYPart Saving ProdPart = " + entry.Key);
                            prodPartsNode.AddValue("ProdPartKey", entry.Key);
                        }
                    }
                    ConfigNode vesselConsPartsnode = node.AddNode(VesselConsPartsListConfigNodeName);
                    foreach (var entry in AYVesselPartLists.VesselConsPartsList)
                    {
                        string partModuleName = "";
                        uint partId = AYVesselPartLists.GetPartKeyVals(entry.Key, out partModuleName);
                        if (thisPartId == partId)
                        {
                            ConfigNode consPartsNode = entry.Value.Save(vesselConsPartsnode);
                            Utilities.Log_Debug("AYPart Saving ConsPart = " + entry.Key);
                            consPartsNode.AddValue("ConsPartKey", entry.Key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utilities.Log("AYPart Exception occurred during part save");
                    Utilities.Log("Exception: {0}", ex);
                }
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            if (Utilities.GameModeisFlight || Utilities.GameModeisEditor)
            {
                try
                {
                    if (node.HasNode(VesselProdPartsListConfigNodeName))
                    {
                        ConfigNode vslProdPartsListNode = node.GetNode(VesselProdPartsListConfigNodeName);
                        var prodPartsNodes = vslProdPartsListNode.GetNodes(PwrPartList.ConfigNodeName);
                        foreach (ConfigNode prodPartsNode in prodPartsNodes)
                        {
                            if (prodPartsNode.HasValue("ProdPartKey"))
                            {
                                string id = prodPartsNode.GetValue("ProdPartKey");
                                Utilities.Log_Debug("AYPart Loading ProdPartKey = " + id);
                                PwrPartList prodPartInfo = PwrPartList.Load(prodPartsNode);
                                if (AYVesselPartLists.VesselProdPartsList.ContainsKey(id))
                                {
                                    Utilities.Log_Debug("AYPart Loading - Ignoring Duplicate Keys :" + id);
                                }
                                else
                                {
                                    AYVesselPartLists.VesselProdPartsList.Add(id, prodPartInfo);
                                }
                            }
                        }
                    }
                    if (node.HasNode(VesselConsPartsListConfigNodeName))
                    {
                        ConfigNode vslConsPartsListNode = node.GetNode(VesselConsPartsListConfigNodeName);
                        var consPartsNodes = vslConsPartsListNode.GetNodes(PwrPartList.ConfigNodeName);
                        foreach (ConfigNode consPartsNode in consPartsNodes)
                        {
                            if (consPartsNode.HasValue("ConsPartKey"))
                            {
                                string id = consPartsNode.GetValue("ConsPartKey");
                                Utilities.Log_Debug("VesselInfo Loading ConsPartKey = " + id);
                                PwrPartList consPartInfo = PwrPartList.Load(consPartsNode);
                                if (AYVesselPartLists.VesselConsPartsList.ContainsKey(id))
                                {
                                    Utilities.Log_Debug("AYPart Loading - Ignoring Duplicate Keys :" + id);
                                }
                                else
                                {
                                    AYVesselPartLists.VesselConsPartsList.Add(id, consPartInfo);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utilities.Log("AYPart Exception occurred during part load");
                    Utilities.Log("Exception: {0}", ex);
                }
            }
        }
    }
}
