using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RemoteTech
{


    class FlightComputer
    {
        double torqueRAvailable, torquePYAvailable, torqueThrustPYAvailable;

        private int[] rapidToggleX = { 0, 0, 0, 0 }; //Array size determines how many toggles required to trigger the axis rest, init array to 0s
        private int[] rapidToggleY = { 0, 0, 0, 0 }; //Negative values indicate the vector was at the minimum threshold
        private int[] rapidToggleZ = { 0, 0, 0, 0 };

        private int rapidRestX = 0; //How long the axis should rest for if rapid toggle is detected
        private int rapidRestY = 0;
        private int rapidRestZ = 0;
        private int rapidToggleRestFactor = 8; //Factor the axis is divided by if the rapid toggle resting is activated

        private Vector3d integral = Vector3d.zero;
        private Vector3d[] a_avg_act = new Vector3d[2];
        Vector3d prev_err;

        public float Kp = 120.0F;
        public Quaternion Target;
        public AttitudeReference Reference;
        bool attitideActive = false;
        bool throttleActive = false;
        float throttle;
        double attitudeError;
		public Part part
		{
			set;
			get;
		}
        Vessel vessel
        {
            get
            {
                return part.vessel;
            }
        }
		public FlightComputerGUI gui;

		public FlightComputer(Part partIn, FlightComputerGUI gui)
        {
			this.part = partIn;
			this.gui = gui;
        }

        public double AttitudeError
        {
            get
            {
                return this.attitudeError;
            }
        }

        public Quaternion Direction(Vector3d d, AttitudeReference referer)
        {
            double ang_diff = Math.Abs(Vector3d.Angle(attitudeGetReferenceRotation(referer) * Target * Vector3d.forward, attitudeGetReferenceRotation(referer) * d));
            Vector3 up, dir = d;
            if ((ang_diff > 45))
            {
                up = attitudeWorldToReference(-part.vessel.ReferenceTransform.forward, referer);
            }
            else
            {
                up = attitudeWorldToReference(attitudeReferenceToWorld(Target * Vector3d.up, Reference), referer);
            }
            Vector3.OrthoNormalize(ref dir, ref up);
            return Quaternion.LookRotation(dir, up);
        }

        public void SetThrottle(float throttlein)
        {
            if (throttlein > 0)
                this.throttleActive = true;
            this.throttle = throttlein;
        }

        bool ManeuverNodeMode = false;

        void updateMN()
        {
            
            if (part.vessel.patchedConicSolver.maneuverNodes.Count > 0)
            {
                Target = Quaternion.LookRotation(part.vessel.patchedConicSolver.maneuverNodes[0].GetBurnVector(part.vessel.orbit).normalized, -part.vessel.ReferenceTransform.forward);
            }
            else
            {
                if (gui.attitudeButtons[7].on && gui.attitudeButtons[7].state.Active)
                {
                    gui.attitudeButtons[7].lastOn = true;
                    gui.attitudeButtons[7].on = false;
                }
            }

        }

        public void SetMode(bool active, AttitudeMode mode, float HDG, float PIT)
        {
            this.attitideActive = RTShared.AttitudeComputerActive = active;
            ManeuverNodeMode = false;
            if (!active)
            {
                return;
            }
            switch (mode)
            {
                case AttitudeMode.MANEUVERNODE:
                    if (part.vessel.patchedConicSolver.maneuverNodes.Count > 0)
                    {
                        ManeuverNodeMode = true;
                        updateMN();
                        Reference = AttitudeReference.INERTIAL;
                    }
                    else
                    {
                        Target = Quaternion.LookRotation(part.vessel.ReferenceTransform.up, -part.vessel.ReferenceTransform.forward);
                        Reference = AttitudeReference.INERTIAL;
                    }
                    break;
                case AttitudeMode.KILLROT:
                    Target = Quaternion.LookRotation(part.vessel.ReferenceTransform.up, -part.vessel.ReferenceTransform.forward);
                    Reference = AttitudeReference.INERTIAL;
                    break;
                case AttitudeMode.PROGRADE:
                    Target = Direction(Vector3d.forward, AttitudeReference.ORBIT);
                    Reference = AttitudeReference.ORBIT;
                    break;
                case AttitudeMode.RETROGRADE:
                    Target = Direction(Vector3d.back, AttitudeReference.ORBIT);
                    Reference = AttitudeReference.ORBIT;
                    break;
                case AttitudeMode.SRF_PROGRADE:
                    Target = Direction(Vector3d.forward, AttitudeReference.SURFACE_VELOCITY);
                    Reference = AttitudeReference.SURFACE_VELOCITY;
                    break;
                case AttitudeMode.SRF_RETROGRADE:
                    Target = Direction(Vector3d.back, AttitudeReference.SURFACE_VELOCITY);
                    Reference = AttitudeReference.SURFACE_VELOCITY;
                    break;
                case AttitudeMode.NORMAL_PLUS:
                    Target = Direction(Vector3d.left, AttitudeReference.ORBIT);
                    Reference = AttitudeReference.ORBIT;
                    break;
                case AttitudeMode.NORMAL_MINUS:
                    Target = Direction(Vector3d.right, AttitudeReference.ORBIT);
                    Reference = AttitudeReference.ORBIT;
                    break;
                case AttitudeMode.RADIAL_PLUS:
                    Target = Direction(Vector3d.up, AttitudeReference.ORBIT);
                    Reference = AttitudeReference.ORBIT;
                    break;
                case AttitudeMode.RADIAL_MINUS:
                    Target = Direction(Vector3d.down, AttitudeReference.ORBIT);
                    Reference = AttitudeReference.ORBIT;
                    break;
                case AttitudeMode.SURFACE:
                    Target = Direction(Quaternion.AngleAxis(HDG, Vector3.up) * Quaternion.AngleAxis(-PIT, Vector3.right) * Vector3d.forward, AttitudeReference.SURFACE_NORTH);
                    Reference = AttitudeReference.SURFACE_NORTH;
                    break;
            }

        }

        public bool AttitudeActive
        {
            get
            {
                return this.attitideActive;
            }
        }

        public void drive(FlightCtrlState s)
        {
            if (attitideActive)
            {

                updateAvailableTorque();

                if (ManeuverNodeMode)
                    updateMN();

                attitudeError = Math.Abs(Vector3d.Angle(attitudeGetReferenceRotation(Reference) * Target * Vector3d.forward, part.vessel.ReferenceTransform.up));
                // Used in the killRot activation calculation and drive_limit calculation
                double precision = Math.Max(0.5, Math.Min(10.0, (torquePYAvailable + torqueThrustPYAvailable * s.mainThrottle) * 20.0 / MoI.magnitude));

                // Direction we want to be facing
                Quaternion target = attitudeGetReferenceRotation(Reference) * Target;
                Quaternion delta = Quaternion.Inverse(Quaternion.Euler(90, 0, 0) * Quaternion.Inverse(part.vessel.ReferenceTransform.rotation) * target);

                Vector3d deltaEuler = new Vector3d(
                                                      (delta.eulerAngles.x > 180) ? (delta.eulerAngles.x - 360.0F) : delta.eulerAngles.x,
                                                      -((delta.eulerAngles.y > 180) ? (delta.eulerAngles.y - 360.0F) : delta.eulerAngles.y),
                                                      (delta.eulerAngles.z > 180) ? (delta.eulerAngles.z - 360.0F) : delta.eulerAngles.z
                                                  );

                Vector3d torque = new Vector3d(
                                                      torquePYAvailable + torqueThrustPYAvailable * s.mainThrottle,
                                                      torqueRAvailable,
                                                      torquePYAvailable + torqueThrustPYAvailable * s.mainThrottle
                                              );

                Vector3d inertia = Vector3d.Scale(
                                                      RTUtils.Sign(angularMomentum) * 1.8f,
                                                      Vector3d.Scale(
                                                          Vector3d.Scale(angularMomentum, angularMomentum),
                                                          RTUtils.Invert(Vector3d.Scale(torque, MoI))
                                                      )
                                                 );

                // Determine the current level of error, this is then used to determine what act to apply
                Vector3d err = deltaEuler * Math.PI / 180.0F;
                err += RTUtils.Reorder(inertia, 132);
                err.Scale(Vector3d.Scale(MoI, RTUtils.Invert(torque)));
                prev_err = err;

                // Make sure we are taking into account the correct timeframe we are working with
                integral += err * TimeWarp.fixedDeltaTime;

                // The inital value of act
                // Act is ultimately what determines what the pitch, yaw and roll will be set to
                // We make alterations to act between here and passing it into the pod controls
                Vector3d act = Mathf.Clamp((float)attitudeError, 1.0F, 3.0F) * Kp * err;
                //+ Ki * integral + Kd * deriv; //Ki and Kd are always 0 so they have been commented out

                // The maximum value the controls may be
                float drive_limit = Mathf.Clamp01((float)(err.magnitude * 450 / precision));

                // Reduce z by reduceZfactor, z seems to act far too high in relation to x and y
                act.z = act.z / 18.0F;

                // Reduce all 3 axis to a maximum of drive_limit
                act.x = Mathf.Clamp((float)act.x, drive_limit * -1, drive_limit);
                act.y = Mathf.Clamp((float)act.y, drive_limit * -1, drive_limit);
                act.z = Mathf.Clamp((float)act.z, drive_limit * -1, drive_limit);

                // Met is the time in seconds from take off
                double met = Planetarium.GetUniversalTime() - part.vessel.launchTime;

                // Reduce effects of controls after launch and returns them gradually
                // This helps to reduce large wobbles experienced in the first few seconds
                if (met < 3.0F)
                {
                    act = act * ((met / 3.0F) * (met / 3.0F));
                }

                // Averages out the act with the last few results, determined by the size of the a_avg_act array
                act = RTUtils.averageVector3d(a_avg_act, act);

                // Looks for rapid toggling of each axis and if found, then rest that axis for a while
                // This helps prevents wobbles from getting larger
                rapidRestX = RTUtils.restForPeriod(rapidToggleX, act.x, rapidRestX);
                rapidRestY = RTUtils.restForPeriod(rapidToggleY, act.y, rapidRestY);
                rapidRestZ = RTUtils.restForPeriod(rapidToggleZ, act.z, rapidRestZ);

                // Reduce axis by rapidToggleRestFactor if rapid toggle rest has been triggered
                if (rapidRestX > 0) act.x = act.x / rapidToggleRestFactor;
                if (rapidRestY > 0) act.y = act.y / rapidToggleRestFactor;
                if (rapidRestZ > 0) act.z = act.z / rapidToggleRestFactor;

                // Sets the SetFlightCtrlState for pitch, yaw and roll
                if (!double.IsNaN(act.z)) s.roll = Mathf.Clamp((float)(act.z), -1, 1);
                if (!double.IsNaN(act.x)) s.pitch = Mathf.Clamp((float)(act.x), -1, 1);
                if (!double.IsNaN(act.y)) s.yaw = Mathf.Clamp((float)(act.y), -1, 1);
            }
            if (throttleActive)
            {
                s.mainThrottle = throttle;
                if (throttle == 0)
                    throttleActive = false;

            }

        }


        public Quaternion attitudeGetReferenceRotation(AttitudeReference reference)
        {
            Quaternion rotRef = Quaternion.identity;
            switch (reference)
            {
                case AttitudeReference.ORBIT:
                    rotRef = Quaternion.LookRotation(velocityVesselOrbitNorm, up);
                    break;
                case AttitudeReference.ORBIT_HORIZONTAL:
                    rotRef = Quaternion.LookRotation(Vector3d.Exclude(up, velocityVesselOrbitNorm), up);
                    break;
                case AttitudeReference.SURFACE_NORTH:
                    rotRef = rotationSurface;
                    break;
                case AttitudeReference.SURFACE_VELOCITY:
                    rotRef = Quaternion.LookRotation(velocityVesselSurfaceUnit, up);
                    break;
            }
            return rotRef;
        }

        public Vector3d attitudeWorldToReference(Vector3d vector, AttitudeReference reference)
        {
            return Quaternion.Inverse(attitudeGetReferenceRotation(reference)) * vector;
        }

        public Vector3d attitudeReferenceToWorld(Vector3d vector, AttitudeReference reference)
        {
            return attitudeGetReferenceRotation(reference) * vector;
        }



        public Vector3d velocityVesselSurfaceUnit
        {
            get
            {
                return (part.vessel.orbit.GetVel() - part.vessel.mainBody.getRFrmVel(CoM)).normalized;
            }
        }

        public Vector3d velocityVesselOrbitNorm
        {
            get
            {
                return part.vessel.orbit.GetVel().normalized;
            }
        }

        public Vector3d up
        {
            get
            {
                return (part.vessel.findWorldCenterOfMass() - part.vessel.mainBody.position).normalized;
            }
        }

        public Vector3d CoM
        {
            get
            {
                return part.vessel.findWorldCenterOfMass();
            }
        }

        public Quaternion rotationSurface
        {
            get
            {
                return Quaternion.LookRotation(Vector3d.Exclude(up, (part.vessel.mainBody.position + part.vessel.mainBody.transform.up * (float)part.vessel.mainBody.Radius) - CoM).normalized, up);
            }
        }

        public Vector3d MoI;

        public Vector3d angularMomentum
        {
            get
            {
                return new Vector3d((Quaternion.Inverse(part.vessel.ReferenceTransform.rotation) * part.vessel.rigidbody.angularVelocity).x * MoI.x, (Quaternion.Inverse(part.vessel.ReferenceTransform.rotation) * part.vessel.rigidbody.angularVelocity).y * MoI.y, (Quaternion.Inverse(part.vessel.ReferenceTransform.rotation) * part.vessel.rigidbody.angularVelocity).z * MoI.z);
            }
        }

        public void updateAvailableTorque()
        {
            MoI = part.vessel.findLocalMOI(CoM);
            torqueRAvailable = torquePYAvailable = torqueThrustPYAvailable = 0;

            foreach (Part p in part.vessel.parts)
            {
                MoI += p.Rigidbody.inertiaTensor;
                if (((p.State == PartStates.ACTIVE) || ((Staging.CurrentStage > Staging.lastStage) && (p.inverseStage == Staging.lastStage))))
                {
                    if (p is LiquidEngine && p.RequestFuel(p, 0, Part.getFuelReqId()) && ((LiquidEngine)p).thrustVectoringCapable)
                    {
                        torqueThrustPYAvailable += Math.Sin(Math.Abs(((LiquidEngine)p).gimbalRange) * Math.PI / 180) * ((LiquidEngine)p).maxThrust * (p.Rigidbody.worldCenterOfMass - CoM).magnitude;
                    }
                    else if (p is LiquidFuelEngine && p.RequestFuel(p, 0, Part.getFuelReqId()) && ((LiquidFuelEngine)p).thrustVectoringCapable)
                    {
                        torqueThrustPYAvailable += Math.Sin(Math.Abs(((LiquidFuelEngine)p).gimbalRange) * Math.PI / 180) * ((LiquidFuelEngine)p).maxThrust * (p.Rigidbody.worldCenterOfMass - CoM).magnitude;
                    }
                    else if (p is AtmosphericEngine && p.RequestFuel(p, 0, Part.getFuelReqId()) && ((AtmosphericEngine)p).thrustVectoringCapable)
                    {
                        torqueThrustPYAvailable += Math.Sin(Math.Abs(((AtmosphericEngine)p).gimbalRange) * Math.PI / 180) * ((AtmosphericEngine)p).maximumEnginePower * ((AtmosphericEngine)p).totalEfficiency * (p.Rigidbody.worldCenterOfMass - CoM).magnitude;
                    }
                    if (p.Modules.Contains("ModuleGimbal") && p.Modules.Contains("ModuleEngines"))
                    {
                         torqueThrustPYAvailable += Math.Sin(Math.Abs(((ModuleGimbal)p.Modules["ModuleGimbal"]).gimbalRange) * Math.PI / 180) * ((ModuleEngines)p.Modules["ModuleEngines"]).maxThrust * (p.Rigidbody.worldCenterOfMass - CoM).magnitude;
                    }
                }

                if ((p is RCSModule || p.Modules.Contains("ModuleRCS")))
                {
                    double maxT = 0;
                    for (int i = 0; i < 6; i++)
                    {
                        if (p is RCSModule && ((RCSModule)p).thrustVectors[i] != Vector3.zero)
                        {
                            maxT = Math.Max(maxT, ((RCSModule)p).thrusterPowers[i]);
                        }
                    }
                    torquePYAvailable += maxT * (p.Rigidbody.worldCenterOfMass - CoM).magnitude;
                    
                    foreach (PartModule m in p.Modules)
                    {
                        if(m is ModuleRCS)
                        torquePYAvailable += ((ModuleRCS)m).thrusterPower * (p.Rigidbody.worldCenterOfMass - CoM).magnitude;
                    }
                }

                if (p is CommandPod)
                {
                    torqueRAvailable += Math.Abs(((CommandPod)p).rotPower);
                    torquePYAvailable += Math.Abs(((CommandPod)p).rotPower);
                }
            }
        }

    }
}
