/**
 * ModuleAmpYearPoweredRCS.cs
 *
 * AmpYear power management.
 * The original code and concept of AmpYear rights go to SodiumEyes on the Kerbal Space Program Forums, which was covered by GNU License GPL (no version stated).
 * As such this code continues to be covered by GNU GPL license.
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 * This code uses modified version of ModuleRCSFX written by ialdabaoth and updated by NathanKell
 * LICENSE remains the ialdabaoth license (CC-BY-SA + tweaks).
 * SOURCE is https://github.com/NathanKell/ModuleRCSFX

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

using UnityEngine;

namespace AY
{
    public class ModuleAmpYearPoweredRCS : ModuleRCS
    {
        //private Propellant definition;
        private string ElecChge = "ElectricCharge";

        private string Xenon = "XenonGas";

        // New context menu info
        [KSPField(isPersistant = true, guiName = "AmpYear Managed", guiActive = true)]
        public bool isManaged = false;

        [KSPField(isPersistant = true, guiName = "Xenon Ratio", guiUnits = "U/s", guiFormat = "F3", guiActive = true)]
        public float xenonRatio = 0f;

        [KSPField(isPersistant = true, guiName = "Power Ratio", guiUnits = "U/s", guiFormat = "F3", guiActive = true)]
        public float powerRatio = 0f;


        //[KSPField(guiActive = true)]
        float inputAngularX;
        //[KSPField(guiActive = true)]
        float inputAngularY;
        //[KSPField(guiActive = true)]
        float inputAngularZ;
        //[KSPField(guiActive = true)]
        float inputLinearX;
        //[KSPField(guiActive = true)]
        float inputLinearY;
        //[KSPField(guiActive = true)]
        float inputLinearZ;

        private Vector3 inputLinear;
        private Vector3 inputAngular;
        private bool precision;
        /// <summary>
        /// Stock KSP lever compensation in precision mode (instead of just reduced thrsut
        /// Defaults to false (reduce thrust uniformly
        /// </summary>
        [KSPField]
        public bool useLever = false;
        /// <summary>
        /// Always use the full thrust of the thruster, don't decrease it when off-alignment
        /// </summary>
        [KSPField]
        public bool fullThrust = false; // always use full thrust

        /// <summary>
        /// If fullThrust = true, if thrust ratio is < this, do not apply full thrust (leave thrust unchanged)
        /// </summary>
        [KSPField]
        public float fullThrustMin = 0.2f; // if thrust amount from dots < this, don't do full thrust

        /// <summary>
        /// The factor by which thrust is multiplied in precision mode (if lever compensation is off
        /// </summary>
        [KSPField]
        public float precisionFactor = 0.1f;

        /// <summary>
        /// If control actuation < this, ignore.
        /// </summary>
        [KSPField]
        float EPSILON = 0.05f; // 5% control actuation
        //[KSPField(guiActive = true)]
        public float curThrust = 0f;
        /// <summary>
        /// Fuel flow in tonnes/sec
        /// </summary>
        public double fuelFlow = 0f;
        private double exhaustVel = 0d;

        public double flowMult = 1d;
        public double ispMult = 1d;
        private double invG = 1d / 9.80665d;

        public bool rcs_active;

        public float electricityUse
        {
            get
            {
                float ElecUsedTmp = 0f;
                foreach (Propellant propellant in this.propellants)
                {
                    if (propellant.name == ElecChge)
                    {
                        ElecUsedTmp = (float)propellant.currentRequirement;
                    }
                }
                return ElecUsedTmp;
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            if (!node.HasNode("PROPELLANT") && node.HasValue("resourceName") && (propellants == null || propellants.Count == 0))
            {
                ConfigNode c = new ConfigNode("PROPELLANT");
                c.SetValue("name", node.GetValue("resourceName"));
                if (node.HasValue("ratio"))
                    c.SetValue("ratio", node.GetValue("ratio"));
                else
                    c.SetValue("ratio", "1.0");
                if (node.HasValue("resourceFlowMode"))
                    c.SetValue("resourceFlowMode", node.GetValue("resourceFlowMode"));
                else
                    c.SetValue("resourceFlowMode", "ALL_VESSEL");
                node.AddNode(c);
            }
            base.OnLoad(node);

            Log_Debug("AYIONRCS", "#of transforms=" + this.thrusterTransforms.Count);
            foreach (Transform t in this.thrusterTransforms)
            {
                Log_Debug("AYIONRCS", "Transform pos=" + t.localPosition +",rot=" + t.localRotation);
            }

            foreach (Propellant propellant in propellants)
            {
                Log_Debug("AYIONRCS", "Propellant=" + propellant.name + ",ratio=" + propellant.ratio + ",flowmode=" + propellant.GetFlowMode().ToString());
                
                if (propellant.name == ElecChge)
                {
                    powerRatio = propellant.ratio;
                }
                if (propellant.name == Xenon)
                {
                    xenonRatio = propellant.ratio;
                }
            }
            G = 9.80665f;
            fuelFlow = (double)thrusterPower / (double)atmosphereCurve.Evaluate(0f) * invG;
        }

        public override string GetInfo()
        {
            string text = base.GetInfo();
            return text;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            //Log_Debug("AYIONRCS", "#of transforms=" + this.thrusterTransforms.Count);
            //foreach (Transform t in this.thrusterTransforms)
            //{
            //    Log_Debug("AYIONRCS", "Transform pos=" + t.localPosition + ",rot=" + t.localRotation);
            //}
        }

        new public void Update()
        {
            if (this.part.vessel == null)
                return;

            inputLinear = vessel.ReferenceTransform.rotation * new Vector3(vessel.ctrlState.X , vessel.ctrlState.Z , vessel.ctrlState.Y );
            inputAngular = vessel.ReferenceTransform.rotation * new Vector3(vessel.ctrlState.pitch , vessel.ctrlState.roll , vessel.ctrlState.yaw );
            
            // Epsilon checks (min values)
            float EPSILON2 = EPSILON * EPSILON;
            inputAngularX = inputAngular.x;
            inputAngularY = inputAngular.y;
            inputAngularZ = inputAngular.z;
            inputLinearX = inputLinear.x;
            inputLinearY = inputLinear.y;
            inputLinearZ = inputLinear.z;
            if (inputAngularX * inputAngularX < EPSILON2)
                inputAngularX = 0f;
            if (inputAngularY * inputAngularY < EPSILON2)
                inputAngularY = 0f;
            if (inputAngularZ * inputAngularZ < EPSILON2)
                inputAngularZ = 0f;
            if (inputLinearX * inputLinearX < EPSILON2)
                inputLinearX = 0f;
            if (inputLinearY * inputLinearY < EPSILON2)
                inputLinearY = 0f;
            if (inputLinearZ * inputLinearZ < EPSILON2)
                inputLinearZ = 0f;
            inputLinear.x = inputLinearX;
            inputLinear.y = inputLinearY;
            inputLinear.z = inputLinearZ;
            inputAngular.x = inputAngularX;
            inputAngular.y = inputAngularY;
            inputAngular.z = inputAngularZ;

            precision = FlightInputHandler.fetch.precisionMode;
            //Log_Debug("AYIONRCS", "inputLinear=" + inputLinear + ",inputAngular=" + inputAngular);
        }

        new public void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsEditor)
                return;
            if (!isManaged) return;
            int fxC = thrusterFX.Count;
            if (TimeWarp.CurrentRate > 1.0f && TimeWarp.WarpMode == TimeWarp.Modes.HIGH)
            {

                for (int i = 0; i < fxC; ++i)
                {
                    FXGroup fx = thrusterFX[i];
                    fx.setActive(false);
                    fx.Power = 0f;
                }
                return;
            }

            // set starting params for loop
            bool success = false;
            curThrust = 0f;

            // set Isp/EV
            realISP = atmosphereCurve.Evaluate((float)(vessel.staticPressurekPa * PhysicsGlobals.KpaToAtmospheres));
            exhaustVel = (double)realISP * (double)G * ispMult;
            //Log_Debug("AYIONRCS", "realISP=" + realISP + ",exhaustVel=" + exhaustVel);
            thrustForces.Clear();

            if (rcsEnabled && !part.ShieldedFromAirstream)
            {
                

                if (vessel.ActionGroups[KSPActionGroup.RCS] != rcs_active)
                {
                    rcs_active = vessel.ActionGroups[KSPActionGroup.RCS];
                }
                if (vessel.ActionGroups[KSPActionGroup.RCS] && (inputAngular != Vector3.zero || inputLinear != Vector3.zero))
                {

                    // rb_velocity should include timewarp, right?
                    Vector3 CoM = vessel.CoM + vessel.rb_velocity * Time.fixedDeltaTime;

                    float effectPower = 0f;
                    int xformCount = thrusterTransforms.Count;
                    for (int i = 0; i < xformCount; ++i)
                    {
                        Transform xform = thrusterTransforms[i];
                        if (xform.position != Vector3.zero)
                        {
                            Vector3 position = xform.position;
                            Vector3 torque = Vector3.Cross(inputAngular.normalized, (position - CoM).normalized);

                            Vector3 thruster;
                            //if (useZaxis)
                            //    thruster = xform.forward;
                            //else
                                thruster = xform.up;
                            float thrust = Mathf.Max(Vector3.Dot(thruster, torque), 0f);
                            thrust += Mathf.Max(Vector3.Dot(thruster, inputLinear.normalized), 0f);
                            //Log_Debug("IONRCS", "thrust=" + thrust);
                            // thrust should now be normalized 0-1.

                            if (thrust > 0f)
                            {
                                if (fullThrust && thrust >= fullThrustMin)
                                    thrust = 1f;

                                if (precision)
                                {
                                    if (useLever)
                                    {
                                        //leverDistance = GetLeverDistanceOriginal(predictedCOM);
                                        float leverDistance = GetLeverDistance(-thruster, CoM);

                                        if (leverDistance > 1)
                                        {
                                            thrust /= leverDistance;
                                        }
                                    }
                                    else
                                    {
                                        thrust *= precisionFactor;
                                    }
                                }

                                UpdatePropellantStatus();
                                float thrustForce = CalculateThrust(thrust, out success);

                                if (success)
                                {
                                    curThrust += thrustForce;
                                    thrustForces.Add(thrustForce);
                                    if (!isJustForShow)
                                    {
                                        Vector3 force = -thrustForce * thruster;

                                        part.Rigidbody.AddForceAtPosition(force, position, ForceMode.Force);
                                        //Log_Debug("IONRCS", "Part " + part.name + " adding force " + force.x + "," + force.y + "," + force.z + " at " + position);
                                    }

                                    thrusterFX[i].Power = Mathf.Clamp(thrust, 0.1f, 1f);
                                    if (effectPower < thrusterFX[i].Power)
                                        effectPower = thrusterFX[i].Power;
                                    thrusterFX[i].setActive(thrustForce > 0f);
                                }
                                else
                                {
                                    thrusterFX[i].Power = 0f;

                                    /*if (!(flameoutEffectName.Equals("")))
                                        part.Effect(flameoutEffectName, 1.0f);*/
                                }
                            }
                            else
                            {
                                thrusterFX[i].Power = 0f;
                            }
                        }
                    }
                    /*if(!(runningEffectName.Equals("")))
                        part.Effect(runningEffectName, effectPower);*/
                }
            }
            if (!success)
            {
                for (int i = 0; i < fxC; ++i)
                {
                    FXGroup fx = thrusterFX[i];
                    fx.setActive(false);
                    fx.Power = 0f;
                }
            }

        }

        private void UpdatePropellantStatus()
        {
            if ((object)propellants != null)
            {
                int pCount = propellants.Count;
                for (int i = 0; i < pCount; ++i)
                {
                    propellants[i].UpdateConnectedResources(part);
                    //Log_Debug("IONRCS", "prop=" + propellants[i].name + ",isdeprived=" + propellants[i].isDeprived);
                }
                    
            }
        }

        new public float CalculateThrust(float totalForce, out bool success)
        {
            double massFlow = flowMult * fuelFlow * (double)totalForce;

            double propAvailable = 1.0d;

            //if (!CheatOptions.InfiniteRCS)
                propAvailable = RequestPropellant(massFlow * TimeWarp.fixedDeltaTime);
            //Log_Debug("IONRCS", "propAvailable=" + propAvailable);
            totalForce = (float)(massFlow * exhaustVel * propAvailable);

            success = (propAvailable > 0f); // had some fuel
            return totalForce;
        }
                                     
        public void Log_Debug(string context, string message)
        {
            Debug.Log(context + "[][" + Time.time.ToString("0.00") + "]: " + message);
        }
    }
}