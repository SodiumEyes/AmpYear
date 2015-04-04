/**
 * AYCrewPart.cs
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

namespace AY
{
    public class AYCrewPart : PartModule
    {
        // New context menu info
        [KSPField(isPersistant = true, guiName = "Cabin Temperature", guiUnits = "C", guiFormat = "F1", guiActive = true)]
        public float CabinTemp = 0f;

        [KSPField(isPersistant = true, guiName = "Outside Temperature", guiUnits = "C", guiFormat = "F1", guiActive = true)]
        public float ambient = 0f;

        [KSPField(isPersistant = true, guiName = "KabinKraziness", guiUnits = "%", guiFormat = "N", guiActive = true)]
        public float CabinCraziness = 0f;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (CabinTemp == 0f)
                CabinTemp = base.part.temperature;

            this.Log_Debug("AYCrewPart Onstart " + base.part.name + " " + base.part.flightID + " CabinTemp = " + CabinTemp);
        }

        public override void OnUpdate()
        {
            //Update the Cabin Temperature slowly towards the outside ambient temperature.
            ambient = vessel.flightIntegrator.getExternalTemperature();
            float CabinTmpRngLow = ambient - 0.5f;
            float CabinTmpRngHgh = ambient + 0.5f;
            if (CabinTemp > CabinTmpRngHgh || CabinTemp < CabinTmpRngLow)
            {
                if (CabinTemp < ambient)
                {
                    CabinTemp += TimeWarp.deltaTime * 0.05f;
                }
                else
                {
                    CabinTemp -= TimeWarp.deltaTime * 0.05f;
                }
            }
            base.OnUpdate();
        }
    }
}