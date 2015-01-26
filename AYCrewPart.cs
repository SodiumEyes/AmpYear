/**
 * AYCrewPart.cs
 * 
 * AmpYear power management. 
 * (C) Copyright 2015, Jamie Leighton
 * 
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 * 
 * This code is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
 * creative commons license. See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>
 * for full details.
 * 
 * Attribution — You are free to modify this code, so long as you mention that the resulting
 * work is based upon or adapted from this code.
 * 
 * Non-commercial - You may not use this work for commercial purposes.
 * 
 * Share Alike — If you alter, transform, or build upon this work, you may distribute the
 * resulting work only under the same or similar license to the CC BY-NC-SA 3.0 license.
 *  
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace AY
{
    class AYCrewPart : PartModule
    {

        // New context menu info
        [KSPField(isPersistant = true, guiName = "Cabin Temperature ", guiUnits = "C", guiFormat = "F1", guiActive = true)]
        public float CabinTemp = 0f;       


        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (CabinTemp == 0f)
            CabinTemp = base.part.temperature;
            
            Utilities.LogFormatted_DebugOnly("AYCrewPart Onstart " + base.part.name + " " + base.part.flightID + " CabinTemp = " + CabinTemp);
        }

        public override void  OnUpdate()
        {           
            Utilities.LogFormatted_DebugOnly("AYcrewpart cabintemp = " + CabinTemp);
            float ambient = vessel.flightIntegrator.getExternalTemperature();
            Utilities.LogFormatted_DebugOnly("AYcrewpart ambienttemp = " + ambient);
            float CabinTmpRngLow = ambient - 0.5f;
            float CabinTmpRngHgh = ambient + 0.5f;
            if (CabinTemp < CabinTmpRngHgh && CabinTemp > CabinTmpRngLow)
                Utilities.LogFormatted_DebugOnly("AYcrewpart cabintemp almost outside temp ");   
            else
            {
                if (CabinTemp < ambient)
                {
                    CabinTemp += TimeWarp.deltaTime * 0.05f;
                }
                else
                {
                    CabinTemp -= TimeWarp.deltaTime * 0.05f;
                }
                Utilities.LogFormatted_DebugOnly("AYcrewpart adjusted temp = " + CabinTemp);
            }            
            base.OnUpdate();
        }
        

        [KSPEvent(guiActive = true, guiName = "Toggle AY GUI")]
        public void toggleGUI()
        {
            AYMenu AYM;
            AYM = FindObjectOfType<AYMenu>();
            AYM.GuiVisible = !AYM.GuiVisible;
        }
    }
}
