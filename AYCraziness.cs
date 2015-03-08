/**
 * AYCraziness.cs
 * (C) Copyright 2015, Jamie Leighton
 * AmpYear power management.
 * The original code and concept of AmpYear rights go to SodiumEyes on the Kerbal Space Program Forums, which was covered by GNU License GPL (no version stated).
 *
 * As such this code continues to be covered by GNU GPL license.
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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AY
{
    public partial class AYController : MonoBehaviour, Savable
    {
        //Craziness vars
        private System.Random rnd = new System.Random();
        private float TimeSinceLastCrazyCheck = Time.realtimeSinceStartup;
        private bool AutoPilotDisabled = false;
        private double AutoPilotDisTime = 0f;
        private double AutoPilotDisCounter = 0f;
        private double UnivTime = 0f;
        private bool FirstMajCrazyWarning = false;
        private bool FirstMinCrazyWarning = false;
        private ScreenMessage AutoTimer;
        private ScreenMessage CrazyAlert;

        public void CalcPartCraziness(Vessel vessel, Part current_part, PartModule module, float sumDeltaTime)
        {
            // Craziness increases only for crewed parts
            this.Log_Debug("CALCCRAZY for part = " + current_part.name);
            if (current_part.protoModuleCrew.Count > 0)
            {
                if (AutoPilotDisabled) // do autopilot disabled counter
                {
                    double TimeDiff = 0f;
                    if (UnivTime != 0f) //should only be 0f if we just switched vessel or resuming a saved game
                    {
                        TimeDiff = Planetarium.GetUniversalTime() - UnivTime;
                    }
                    UnivTime = Planetarium.GetUniversalTime();
                    AutoPilotDisCounter += TimeDiff;
                    if (AutoPilotDisCounter >= AutoPilotDisTime) // time is up
                    {
                        EnableAutoPilot(vessel);
                    }
                    else
                    {
                        ScreenMessages.RemoveMessage(AutoTimer);
                        AutoTimer = ScreenMessages.PostScreenMessage(" Autopilot disabled for " + Utilities.formatTime(AutoPilotDisTime - AutoPilotDisCounter));
                    }
                }
                double basecrazy = AYsettings.CRAZY_BASE_DRAIN_FACTOR;
                double reducecrazy = 0.0;
                // Craziness multiplier based on how cramped the part is
                double CrewDiff = (double)current_part.protoModuleCrew.Count / current_part.CrewCapacity;
                basecrazy = basecrazy * CrewDiff;
                //Craziness added to by how far from Kerbin we are
                double DstFrmHome = Utilities.DistanceFromHomeWorld(vessel);
                double DistDiff = DistanceMultiplier(DstFrmHome);
                this.Log_Debug("BaseCrazy = " + basecrazy.ToString("0.0000000") + " DistDiff = " + DistDiff.ToString("0.000000000000"));
                basecrazy += DistDiff; // Add the distance factor
                this.Log_Debug("DistMultApplied = " + basecrazy.ToString("0.0000000000"));
                if (!CrewTempComfortable) //Cabin Temperature is not comfortable so add the uncomfortable factor
                {                    
                    basecrazy += AYsettings.CRAZY_CLIMATE_UNCOMF_FACTOR;
                    this.Log_Debug("CabinTemp is not comfortable, Basecrazy increased to " + basecrazy.ToString("0.0000000"));
                }
                // If Luxury items are on, craziness is reduced
                if (subsystemPowered(Subsystem.CLIMATE)) 
                    reducecrazy += AYsettings.CRAZY_CLIMATE_REDUCE_FACTOR;
                if (subsystemPowered(Subsystem.MUSIC)) 
                    reducecrazy += AYsettings.CRAZY_RADIO_REDUCE_FACTOR;
                if (subsystemPowered(Subsystem.MASSAGE)) 
                    reducecrazy += AYsettings.CRAZY_MASSAGE_REDUCE_FACTOR;
                //Calculate the final craziness amount
                double timestep_drain = basecrazy - reducecrazy;
                this.Log_Debug("CALCCRAZY craziness before sumdelta calc = " + timestep_drain);
                timestep_drain = timestep_drain * sumDeltaTime;
                float CabinCraziness = 0f;
                //Set the parts craziness
                CabinCraziness = ((AYCrewPart)module).CabinCraziness;
                CabinCraziness = CabinCraziness + (float)timestep_drain;
                //Craziness is capped between 0% and 100%
                if (CabinCraziness < 0.0) CabinCraziness = 0.0f;
                if (CabinCraziness > 100.0) CabinCraziness = 100.0f;
                ((AYCrewPart)module).CabinCraziness = CabinCraziness;
                this.Log_Debug("CALCCRAZY crewdiff = " + CrewDiff.ToString("0.0000000") + " basecrazy = " + basecrazy.ToString("0.0000000") + " reducecrazy = " + reducecrazy.ToString("0.0000000") +
                    " timestep_drain = " + timestep_drain.ToString("0.0000000") + " cabincraziness = " + CabinCraziness.ToString("00.00000"));
                CheckCrazinessLevels(vessel, current_part, CabinCraziness);
            }
        }

        public void CheckCrazinessLevels(Vessel vessel, Part current_part, double Craziness)
        {
            if (Craziness <= AYsettings.CRAZY_MAJOR_LIMIT && FirstMajCrazyWarning)
                FirstMajCrazyWarning = false;
            if (Craziness <= AYsettings.CRAZY_MINOR_LIMIT && FirstMinCrazyWarning)
                FirstMinCrazyWarning = false;
            if (Craziness > AYsettings.CRAZY_MAJOR_LIMIT)
            {
                ScreenMessages.RemoveMessage(CrazyAlert);
                CrazyAlert = ScreenMessages.PostScreenMessage("Craziness " + Craziness.ToString("00.00") + "% - Major Alert");
                if (!FirstMajCrazyWarning)
                {
                    ScreenMessages.PostScreenMessage(current_part.name + " - Things are about to get Crazy in here!", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                    FirstMajCrazyWarning = true;
                }
                // Something Major might happen code to be put here
                float TimeDiff = Time.realtimeSinceStartup - TimeSinceLastCrazyCheck;
                if (TimeDiff > 120f)
                {
                    TimeSinceLastCrazyCheck = Time.realtimeSinceStartup;
                    int dice = RandomDice(2);
                    int dice2 = RandomDice(2);
                    if (dice == dice2)
                    {
                        DumpSomething(vessel, current_part, true);
                    }
                }
            }
            else
            {
                if (Craziness > AYsettings.CRAZY_MINOR_LIMIT)
                {
                    ScreenMessages.RemoveMessage(CrazyAlert);
                    CrazyAlert = ScreenMessages.PostScreenMessage("Craziness " + Craziness.ToString("00.00") + "% - Minor Alert");
                    if (!FirstMinCrazyWarning)
                    {
                        ScreenMessages.PostScreenMessage(current_part.name + " - It's Getting Crazy in here!", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                        FirstMinCrazyWarning = true;
                    }
                    // Something Minor might happen code to be put here
                    float TimeDiff = Time.realtimeSinceStartup - TimeSinceLastCrazyCheck;
                    this.Log_Debug("Timediff = " + TimeDiff);
                    if (TimeDiff > 180f)
                    {
                        TimeSinceLastCrazyCheck = Time.realtimeSinceStartup;
                        int dice = RandomDice(3);
                        int dice2 = RandomDice(3);
                        this.Log_Debug("dice1 =  " + dice + " dice2 = " + dice2);
                        if (dice == dice2)
                        {
                            DumpSomething(vessel, current_part, false);
                        }
                    }
                }
            }
        }

        private int RandomDice(int upper)
        {
            int dice = rnd.Next(1, upper);
            return dice;
        }

        private void DumpSomething(Vessel vessel, Part current_part, bool Major)
        {
            this.Log_Debug("Crazy DumpSomething start");
            List<IScienceDataContainer> Scicandidates = ConsScienceCandidates(vessel);
            bool HasScience = Scicandidates.Count > 0;
            List<PartResource> Rescandidates = ConsResCandidates(vessel);
            bool HasRes = Rescandidates.Count > 0;
            int selectevent = RandomDice(4);
            this.Log_Debug("HasSci = " + HasScience);
            this.Log_Debug("HasRes = " + HasRes);
            this.Log_Debug("SelectEvent = " + selectevent);
            if (selectevent == 2 && !HasScience) // change to event 3
                selectevent = 3;
            if (selectevent == 3 && !HasRes) // change to event 4
                selectevent = 4;
            if (selectevent == 4 && !Major) //change to event 1
                selectevent = 1;

            if (selectevent == 1) // disable the autopilot selected
            {
                DisableAutoPilot(vessel, Major);
            }
            if (selectevent == 2 && HasScience) // dump a random science experiment
            {
                DumpScience(Scicandidates, Major);
            }
            if (selectevent == 3 && HasRes) //dump a random resource
            {
                DumpResource(Rescandidates, Major);
            }
            if (selectevent == 4 && vessel.GetCrewCount() > 0) //someone is going eva
            {
                GoOverboard(vessel, current_part, Major);
            }
        }

        private void DisableAutoPilot(Vessel vessel, bool Major)
        {
            vessel.Autopilot.Disable();
            vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, false);
            this.Log_Debug("Disable the autopilot");
            if (AutoPilotDisabled) // already disabled so extend the time
            {
                double ExtDisTime = 0;
                if (Major)
                    ExtDisTime = RandomDice(5);
                else
                    ExtDisTime = RandomDice(2);
                //AutoPilotDisCounter = 0f;
                ExtDisTime = ExtDisTime * 60;
                AutoPilotDisTime += ExtDisTime;
                ScreenMessages.PostScreenMessage(" The crazy crew have disabled the Autopilot for another " + Utilities.formatTime(ExtDisTime), 10.0f, ScreenMessageStyle.UPPER_CENTER);
            }
            else // not already disabled
            {
                AutoPilotDisabled = true;
                if (Major)
                    AutoPilotDisTime = RandomDice(5);
                else
                    AutoPilotDisTime = RandomDice(2);
                AutoPilotDisTime = AutoPilotDisTime * 60;
                ScreenMessages.PostScreenMessage(" The crazy crew have disabled the Autopilot for " + Utilities.formatTime(AutoPilotDisTime), 10.0f, ScreenMessageStyle.UPPER_CENTER);
                AutoPilotDisCounter = 0f;
            }
            this.Log_Debug("Autopilot disabled for autopilotdistime = " + AutoPilotDisTime);
        }

        private void EnableAutoPilot(Vessel vessel)
        {
            this.Log_Debug("Enable the autopilot");
            vessel.Autopilot.Enable();
            AutoPilotDisabled = false;
            AutoPilotDisCounter = 0f;
            AutoPilotDisTime = 0f;
            ScreenMessages.RemoveMessage(AutoTimer);
            ScreenMessages.PostScreenMessage(" The crew have re-enabled the Autopilot", 10.0f, ScreenMessageStyle.UPPER_CENTER);
        }

        private void DumpResource(List<PartResource> candidates, bool Major)
        {
            this.Log_Debug("Dump a resource");
            if (!Major) //only minor craziness so we just dump one random resource
            {
                int selectcandidate = RandomDice(candidates.Count);
                double dumpamt = candidates[selectcandidate].amount / 3;
                candidates[selectcandidate].amount -= dumpamt;
                ScreenMessages.PostScreenMessage("The crazy crew just threw out " + dumpamt.ToString("0.00") + " of " + candidates[selectcandidate].resourceName, 10.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            if (Major) // major craziness so dump all resource
            {
                int selectcandidate = RandomDice(candidates.Count);
                double dumpamt = candidates[selectcandidate].amount;
                candidates[selectcandidate].amount = 0;
                ScreenMessages.PostScreenMessage("The crazy crew just threw out " + dumpamt + " of " + candidates[selectcandidate].resourceName, 10.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
        }

        private List<PartResource> ConsResCandidates(Vessel vessel)
        {
            //Construct candidate resource parts list
            List<PartResource> candidates = new List<PartResource>();
            foreach (Part part in vessel.parts)
            {
                foreach (PartResource resource in part.Resources)
                {
                    if (resource.amount > 0)
                    {
                        candidates.Add(resource);
                    }
                }
            }
            return candidates;
        }

        private List<IScienceDataContainer> ConsScienceCandidates(Vessel vessel)
        {
            // Construct candidate science container list
            List<IScienceDataContainer> candidates = new List<IScienceDataContainer>();
            List<Part> parts = vessel.Parts.Where(p => p.FindModulesImplementing<IScienceDataContainer>().Count > 0).ToList();
            foreach (Part p in parts)
            {
                List<IScienceDataContainer> containers = p.FindModulesImplementing<IScienceDataContainer>().ToList();
                List<IScienceDataContainer> contwsci = containers.FindAll(s => s.GetScienceCount() > 0);
                foreach (IScienceDataContainer x in contwsci)
                {
                    candidates.Add(x);
                }
            }
            this.Log_Debug("got candidates = " + candidates.Count);
            return candidates;
        }

        private void DumpScience(List<IScienceDataContainer> candidates, bool Major)
        {
            this.Log_Debug("Dump science");
            if (!Major) //only minor craziness so we just dump one random science
            {
                DumpOneScience(candidates);
                return;
            }

            if (Major) // major craziness so dump all science
                DumpAllScience(candidates);
        }

        private void DumpOneScience(List<IScienceDataContainer> candidates)
        {
            // dump a random container
            this.Log_Debug("Dumping a science container");
            int selectcandidate = 0;
            if (candidates.Count == 1)
                selectcandidate = 0;
            else
                selectcandidate = RandomDice(candidates.Count);
            this.Log_Debug("Candidates count = " + candidates.Count);
            this.Log_Debug("Selectcandidate = " + selectcandidate);
            ScienceData[] selectdata = candidates[selectcandidate].GetData();
            this.Log_Debug("selectdata = " + selectdata.ToString());
            candidates[selectcandidate].DumpData(selectdata[0]);
            ScreenMessages.PostScreenMessage("The crazy crew just threw out the collected science: " + selectdata[0].title, 10.0f, ScreenMessageStyle.UPPER_CENTER);
        }

        private void DumpAllScience(List<IScienceDataContainer> candidates)
        {
            //dump all science
            foreach (IScienceDataContainer container in candidates)
            {
                ScienceData[] data = container.GetData();
                foreach (ScienceData d in data)
                {
                    if (d != null)
                    {
                        container.DumpData(d);
                    }
                }
            }
            ScreenMessages.PostScreenMessage("The crazy crew just threw out all collected science", 10.0f, ScreenMessageStyle.UPPER_CENTER);
        }

        private void GoOverboard(Vessel vessel, Part current_part, bool Major)
        {
            //someone is going for a spacewalk
            if (!Major) return;
            this.Log_Debug("GoOverboard start");
            List<ProtoCrewMember> vslcrew = current_part.protoModuleCrew;
            int crewcnt = vslcrew.Count;
            int selcrew = RandomDice(crewcnt);
            selcrew = selcrew - 1;
            this.Log_Debug("Crew member going for a walk :" + vslcrew[selcrew].name);
            ScreenMessages.PostScreenMessage(vslcrew[selcrew].name + " decided to go outside for some fresh ai.. err..", 10.0f, ScreenMessageStyle.UPPER_CENTER);
            FlightEVA.fetch.spawnEVA(vslcrew[selcrew], current_part, current_part.airlock);
        }

        private double DistanceMultiplier(double DistFromHome)
        {
            double Multiplier = 0;
            if (DistFromHome < (double)10000000)
                Multiplier = ((double)3 / (double)24 / (double)60);
            else
                if (DistFromHome < (double)50000000)
                    Multiplier = ((double)5 / (double)24 / (double)60);
                else
                    if (DistFromHome < (double)100000000)
                        Multiplier = ((double)7 / (double)24 / (double)60);
                    else
                        if (DistFromHome < (double)250000000)
                            Multiplier = ((double)8 / (double)24 / (double)60);
                        else
                            if (DistFromHome < (double)350000000)
                                Multiplier = ((double)9 / (double)24 / (double)60);
                            else
                                if (DistFromHome < (double)999000000)
                                    Multiplier = ((double)10 / (double)24 / (double)60);
                                else
                                    if (DistFromHome < (double)5000000000)
                                        Multiplier = ((double)11 / (double)24 / (double)60);
                                    else
                                        if (DistFromHome < (double)15000000000)
                                            Multiplier = ((double)13 / (double)24 / (double)60);
                                        else
                                            if (DistFromHome < (double)30000000000)
                                                Multiplier = ((double)14 / (double)24 / (double)60);
                                            else
                                                if (DistFromHome < (double)50000000000)
                                                    Multiplier = ((double)15 / (double)24 / (double)60);
                                                else
                                                    if (DistFromHome < (double)70000000000)
                                                        Multiplier = ((double)17 / (double)24 / (double)60);
                                                    else
                                                        Multiplier = ((double)19 / (double)24 / (double)60);

            this.Log_Debug("Multiplier = " + Multiplier.ToString("00.000000000000000"));
            return Multiplier;
        }
    }
}