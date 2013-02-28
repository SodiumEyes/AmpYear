using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/* This code was adapted from the RemoteTech plugin (http://kerbalspaceport.com/remotetech-3/) by The_Duck and JDP */

namespace RemoteTech
{

    public static class RTShared
    {
        public static bool show = true;
        public static Dictionary<String, RTVesselState> State = new Dictionary<String, RTVesselState>();
        public static Dictionary<String, bool> ReloadNetwork = new Dictionary<String, bool>();
        //Used in RemoteCore
        public static bool listComsats = false;
        public static bool showPathInMapView = true;
        public static Rect windowPos = new Rect(Screen.width / 4, Screen.height / 4, 350, 200);
        public static Vector2 comsatListScroll = new Vector2();
        //public static bool localControl = false;

        //Used in RelayNetwork
        public static bool advTrack = true;
        public static int RemoteCommandCrew = 3;

        //Used in SatSettings
        public static Rect SettingPos = new Rect(Screen.width / 4, Screen.height / 4, 100, 200);

        //Used in Flight Computer
        public static bool showFC = false;
        public static Rect AttitudePos = new Rect(Screen.width / 5, Screen.height / 5, 100, 200);
        public static Rect ThrottlePos = new Rect(Screen.width / 4.5F, Screen.height / 4.5F, 100, 200);
        public static bool AttitudeComputerActive = false;

        public static double speedOfLight = 300000000.0;

        public static void SaveData()
        {
            string s =
                windowPos.xMin / Screen.width + "\n" + windowPos.yMin / Screen.height + "\n" +
                SettingPos.xMin / Screen.width + "\n" + SettingPos.yMin / Screen.height + "\n" +
                AttitudePos.xMin / Screen.width + "\n" + AttitudePos.yMin / Screen.height + "\n" +
                ThrottlePos.xMin / Screen.width + "\n" + ThrottlePos.yMin / Screen.height + "\n" +
                showPathInMapView + "\n" +
                showFC + "\n" +
                listComsats + "\n" +
                show;


			KSP.IO.File.WriteAllText<AmpYear.AmpYearModule>(s, "Data.dat");   
        }

        public static void LoadData()
        {
            if (KSP.IO.File.Exists<AmpYear.AmpYearModule>("Data.dat"))
            {
                try
                {
					string[] ls = KSP.IO.File.ReadAllLines<AmpYear.AmpYearModule>("Data.dat");
                    windowPos.xMin = Mathf.Clamp(float.Parse(ls[0]), 0, 1) * Screen.width;
                    windowPos.yMin = Mathf.Clamp(float.Parse(ls[1]), 0, 1) * Screen.height;
                    SettingPos.xMin = Mathf.Clamp(float.Parse(ls[2]), 0, 1) * Screen.width;
                    SettingPos.yMin = Mathf.Clamp(float.Parse(ls[3]), 0, 1) * Screen.height;
                    AttitudePos.xMin = Mathf.Clamp(float.Parse(ls[4]), 0, 1) * Screen.width;
                    AttitudePos.yMin = Mathf.Clamp(float.Parse(ls[5]), 0, 1) * Screen.height;
                    ThrottlePos.xMin = Mathf.Clamp(float.Parse(ls[6]), 0, 1) * Screen.width;
                    ThrottlePos.yMin = Mathf.Clamp(float.Parse(ls[7]), 0, 1) * Screen.height;
                    showPathInMapView = bool.Parse(ls[8]);
                    showFC = bool.Parse(ls[9]);
                    listComsats = bool.Parse(ls[10]);
                    show = bool.Parse(ls[11]);
                }
                catch
                {
                }
            }
        }


        public static void Load()
        {
			if (KSP.IO.File.Exists<AmpYear.AmpYearModule>("Settings.cfg"))
            {
				string[] ls = KSP.IO.File.ReadAllLines<AmpYear.AmpYearModule>("Settings.cfg");
                string SPEEDOFLIGHT = "";
                string DETTRACK = "";
                string RCC = "";
                foreach (string s in ls)
                {
                    if (!s.StartsWith("//") && s.Length > 2)
                    {
                        if (s.StartsWith("Speed of Light"))
                            SPEEDOFLIGHT = s;
                        if (s.StartsWith("Detailed satellite tracking"))
                            DETTRACK = s;
                        if (s.StartsWith("RemoteCommand Crew"))
                            RCC = s;
                    }
                }

                string[] temp = SPEEDOFLIGHT.Split("=".ToCharArray());
                string tmp = temp[temp.Length - 1];
                temp = tmp.Split(" ".ToCharArray());
                try
                {
                    speedOfLight = double.Parse(temp[temp.Length - 1]);
                }
                catch (Exception)
                {
                    speedOfLight = 300000000.0;
                }

                temp = DETTRACK.Split("=".ToCharArray());
                tmp = temp[temp.Length - 1];
                temp = tmp.Split(" ".ToCharArray());
                try
                {
                    if (temp[temp.Length - 1].ToLower().Equals("on")) advTrack = true;
                    if (temp[temp.Length - 1].ToLower().Equals("off")) advTrack = false;
                }
                catch (Exception)
                {
                    advTrack = true;
                }
                temp = RCC.Split("=".ToCharArray());
                tmp = temp[temp.Length - 1];
                temp = tmp.Split(" ".ToCharArray());
                try
                {
                    int crew = int.Parse(temp[temp.Length - 1]);
                    if (crew > 0)
                        RemoteCommandCrew = crew;
                    else
                        RemoteCommandCrew = 1;
                }
                catch (Exception)
                {
                    RemoteCommandCrew = 3;
                }
            }
            else
                KSP.IO.File.WriteAllText<AmpYear.AmpYearModule>("//Here you can edit the speed of light used to calculate control delay in m/s (Default: 300000000):\nSpeed of Light = 300000000\n\n//Here you can choose if you want detailed satellite tracking (on/off, Default: on)\nDetailed satellite tracking = on\n\n//Here you can edit the required crew for a command station (Minimum: 1, Default: 3)\nRemoteCommand Crew = 3", "Settings.cfg");
        }

    }
}