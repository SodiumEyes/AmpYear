using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RemoteTech
{

    class AttitudeStateButton
    {
        public bool on = false;
        public bool lastOn = false;

        FlightComputerGUI computer;
        public AttitudeMode mode;
        public AttitudeButtonState state = new AttitudeButtonState();
        Queue<AttitudeButtonState> states = new Queue<AttitudeButtonState>();
        string name;
        float HDG = 0;
        float PIT = 0;
        string HDGs, PITs;
        double lastActTime = -10;

        public AttitudeStateButton(FlightComputerGUI computerin, AttitudeMode modein, string namein)
        {
            this.computer = computerin;
            this.mode = modein;
            this.name = namein;
            if (mode == AttitudeMode.SURFACE)
            {
                HDG = PIT = 90;
                HDGs = PITs = HDG.ToString();
            }
        }

        public bool sending
        {
            get
            {
                return states.Count > 0;
            }
        }

        public void Draw()
        {
            bool locked = on;
            GUILayout.BeginHorizontal();
            if (state.Active)
            {
                Color savedContentColor = GUI.contentColor;
                GUI.contentColor = Color.green;
                on = GUILayout.Toggle(on, name, GUI.skin.textField, GUILayout.Width(100));
                GUI.contentColor = savedContentColor;
            }
            else
                on = GUILayout.Toggle(on, name, GUI.skin.textField, GUILayout.Width(100));

            //GUILayout.Label(sending ? computer.arrows : "", GUI.skin.textField, GUILayout.Width(50));
            //GUILayout.Label(sending ? RTUtils.time((lastActTime - Planetarium.GetUniversalTime() > 0) ? lastActTime - Planetarium.GetUniversalTime() : 0) : "", GUI.skin.textField, GUILayout.Width(90));
            GUILayout.EndHorizontal();

            if (mode != AttitudeMode.SURFACE || !on) return;
            GUILayout.BeginHorizontal();

            GUILayout.Label("Pitch:", GUI.skin.textField, GUILayout.Width(100));
            PITs = GUILayout.TextField(PITs, GUILayout.Width(50));
            PITs = RTUtils.FormatNumString(PITs);

            if (GUILayout.Button("+", GUI.skin.textField, GUILayout.Width(21.0F)))
            {
                float tmp = Convert.ToSingle(PITs);
                tmp += 1;
                if (tmp >= 360.0F)
                {
                    tmp -= 360.0F;
                }
                PITs = Mathf.RoundToInt(tmp).ToString();
            }
            if (GUILayout.Button("-", GUI.skin.textField, GUILayout.Width(21.0F)))
            {
                float tmp = Convert.ToSingle(PITs);
                tmp -= 1;
                if (tmp < 0)
                {
                    tmp += 360.0F;
                }
                PITs = Mathf.RoundToInt(tmp).ToString();
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.Label("Heading:", GUI.skin.textField, GUILayout.Width(100));
            HDGs = GUILayout.TextField(HDGs, GUILayout.Width(50));
            HDGs = RTUtils.FormatNumString(HDGs);

            if (GUILayout.Button("+", GUI.skin.textField, GUILayout.Width(21.0F)))
            {
                float tmp = Convert.ToSingle(HDGs);
                tmp += 1;
                if (tmp >= 360.0F)
                {
                    tmp -= 360.0F;
                }
                HDGs = Mathf.RoundToInt(tmp).ToString();
            }
            if (GUILayout.Button("-", GUI.skin.textField, GUILayout.Width(21.0F)))
            {
                float tmp = Convert.ToSingle(HDGs);
                tmp -= 1;
                if (tmp < 0)
                {
                    tmp += 360.0F;
                }
                HDGs = Mathf.RoundToInt(tmp).ToString();
            }

            GUILayout.EndHorizontal();
            if (GUILayout.Button("Update", GUI.skin.textField))
            {
                if (PITs.EndsWith("."))
                    PITs = PITs.Substring(0, PITs.Length - 1);

                if (HDGs.EndsWith("."))
                    HDGs = HDGs.Substring(0, HDGs.Length - 1);

                PIT = Convert.ToSingle(PITs);
                HDG = Convert.ToSingle(HDGs);
                lastOn = false;
            }

        }




        public void Update()
        {
            if (on != lastOn)
            {
                AttitudeButtonState tmp = new AttitudeButtonState();
                tmp.Active = lastOn = on;
                tmp.ActTime = lastActTime = Planetarium.GetUniversalTime();
                tmp.HDG = this.HDG;
                tmp.PIT = this.PIT;
                states.Enqueue(tmp);
            }

            if (sending && states.Peek().ActTime <= Planetarium.GetUniversalTime())
            {
                state = states.Dequeue();
                foreach (AttitudeStateButton b in computer.attitudeButtons)
                {
                    if (b != this)
                    {
                        b.on = b.lastOn = b.state.Active = false;
                    }
                }
                computer.computer.SetMode(state.Active, mode, state.HDG, state.PIT);
            }

        }

    }


    class SimpleThrottle
    {
        FlightComputerGUI computer;
        SimpleThrotteState state = new SimpleThrotteState();
        Queue<SimpleThrotteState> states = new Queue<SimpleThrotteState>();
        double lastActTime;
        float ThrottleBar = 0;
        string BTS = "";
        double speedT0 = 0;

        public SimpleThrottle(FlightComputerGUI computerin)
        {
            this.computer = computerin;
            state.Target = -10;
        }

        bool doOnce = false;
        float ThrottleIncrement = 0;

        public void update()
        {
            if (sending && states.Peek().ActTime <= Planetarium.GetUniversalTime())
            {
                state = states.Dequeue();
                if (state.Bt)
                    state.Target = state.Target + Planetarium.GetUniversalTime();
                else
                {
                    speedT0 = RTUtils.ForwardSpeed(computer.computer.part.vessel);
                }
            }

            if (burning)
            {
                doOnce = true;
                if (ThrottleIncrement < state.Throttle)
                {
                    ThrottleIncrement = Mathf.Clamp(ThrottleIncrement + 0.1F, 0, 1);
                    computer.computer.SetThrottle(ThrottleIncrement);
                }
                else
                    computer.computer.SetThrottle(Mathf.Clamp(state.Throttle, 0, 1));
            }
            else
                if (doOnce)
                {
                    ThrottleIncrement = 0;
                    computer.computer.SetThrottle(ThrottleIncrement);
                    doOnce = false;
                    if (!state.Bt)
                    {
                        state.Target = -10;
                        state.Bt = true;
                    }
                }
        }


        public bool sending
        {
            get
            {
                return states.Count > 0;
            }
        }

        public bool burning
        {
            get
            {
                if (state.Bt)
                    return state.Target >= Planetarium.GetUniversalTime();
                else
                    return Math.Abs(speedT0 - RTUtils.ForwardSpeed(computer.computer.part.vessel)) < state.Target;
            }
        }

        bool BT = true;
        public void draw()
        {
            GUILayout.Label("Throttle: " + Mathf.RoundToInt(ThrottleBar * 100) + "%", GUI.skin.textField);
            ThrottleBar = GUILayout.HorizontalSlider(ThrottleBar, 0, 1);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(BT ? "Burn time (s)" : "ΔV (m/s)", GUI.skin.textField, GUILayout.Width(100)))
                BT = !BT;
            BTS = GUILayout.TextField(BTS, GUILayout.Width(50));
            BTS = RTUtils.FormatNumString(BTS, false);
            if (GUILayout.Button("Send", GUI.skin.textField))
            {
                SimpleThrotteState tmp = new SimpleThrotteState();
                tmp.Throttle = ThrottleBar;
                if (BTS.EndsWith("."))
                    BTS = BTS.Substring(0, BTS.Length - 1);
                tmp.Target = Convert.ToSingle(BTS);
                tmp.Bt = BT;
                lastActTime = tmp.ActTime = Planetarium.GetUniversalTime();
                states.Enqueue(tmp);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.Label(sending ? "Sending " + computer.arrows : (burning && state.Bt ? "Burning" : ""), GUI.skin.textField, GUILayout.Width(100));


            GUILayout.Label(sending ? RTUtils.time((lastActTime - Planetarium.GetUniversalTime() > 0) ? lastActTime - Planetarium.GetUniversalTime() : 0) :
                (burning && state.Bt ? RTUtils.time((state.Target - Planetarium.GetUniversalTime() > 0) ? state.Target - Planetarium.GetUniversalTime() : 0) : "")
            , GUI.skin.textField, GUILayout.Width(100));


            GUILayout.EndHorizontal();


        }




    }


    class FlightComputerGUI
    {
		public Part part
		{
			get
			{
				return computer.part;
			}
		}
		public FlightComputer computer;
        public int ATTITUDE_ID = 72138;
        public int THROTTLE_ID = 72238;
        public SimpleThrottle throttle;
        public List<AttitudeStateButton> attitudeButtons = new List<AttitudeStateButton>();

        public FlightComputerGUI()
        {
            throttle = new SimpleThrottle(this);

            attitudeButtons.Add(new AttitudeStateButton(this, AttitudeMode.KILLROT, "KillRot"));
            attitudeButtons.Add(new AttitudeStateButton(this, AttitudeMode.PROGRADE, "Prograde"));
            attitudeButtons.Add(new AttitudeStateButton(this, AttitudeMode.RETROGRADE, "Retrograde"));
            attitudeButtons.Add(new AttitudeStateButton(this, AttitudeMode.NORMAL_PLUS, "NML +"));
            attitudeButtons.Add(new AttitudeStateButton(this, AttitudeMode.NORMAL_MINUS, "NML -"));
            attitudeButtons.Add(new AttitudeStateButton(this, AttitudeMode.RADIAL_PLUS, "RAD +"));
            attitudeButtons.Add(new AttitudeStateButton(this, AttitudeMode.RADIAL_MINUS, "RAD -"));
            attitudeButtons.Add(new AttitudeStateButton(this, AttitudeMode.MANEUVERNODE, "Maneuver"));
            attitudeButtons.Add(new AttitudeStateButton(this, AttitudeMode.SURFACE, "Surface"));
        }



        double t = 0;
        public string arrows = "";
        public void update()
        {
            if (t <= Math.Round(Planetarium.GetUniversalTime(), 0))
            {
                t = Math.Round(Planetarium.GetUniversalTime(), 0) + 1;

                arrows += "»";
                if (arrows.Length > 4)
                    arrows = "";
            }

            throttle.update();
            foreach (AttitudeStateButton b in attitudeButtons)
                b.Update();
        }


        public void ThrottleGUI(int windowID)
        {
            throttle.draw();

            GUI.DragWindow();
        }

        public void AttitudeGUI(int windowID)
        {
            foreach (AttitudeStateButton b in attitudeButtons)
            {
                if (b.mode == AttitudeMode.MANEUVERNODE)
                {
                    if (computer.part.vessel.patchedConicSolver.maneuverNodes.Count > 0)
                        b.Draw();
                }
                else
                b.Draw();
            }
            GUI.DragWindow();

        }


    }
}
