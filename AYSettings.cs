using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace AmpYear
{
	class AYSettings
	{
		public const string GLOBAL_SETTINGS_FILENAME = "globalsettings.txt";
		public const string VESSEL_SETTINGS_FILENAME = "vesselsettings.txt";

		public static Rect windowPos = new Rect(40, Screen.height/2-100, AmpYearModule.WINDOW_WIDTH, 200);
		public static Rect flightCompWindowPos = new Rect(40 + AmpYearModule.WINDOW_WIDTH, Screen.height / 2 - 100, 100, 200);

		public static void saveGlobalSettings()
		{
			//Get the global settings
			AYGlobalSettings global_settings = new AYGlobalSettings();
			global_settings.windowPosX = windowPos.x;
			global_settings.windowPosY = windowPos.y;

			global_settings.flightCompWindowPosX = flightCompWindowPos.x;
			global_settings.flightCompWindowPosY = flightCompWindowPos.y;

			//Serialize global settings to file
			try
			{
				byte[] serialized = KSP.IO.IOUtils.SerializeToBinary(global_settings);
				KSP.IO.File.WriteAllBytes<AmpYearModule>(serialized, GLOBAL_SETTINGS_FILENAME);
			}
			catch (KSP.IO.IOException)
			{
			}

		}

		public static void loadGlobalSettings()
		{
			try
			{
				if (KSP.IO.File.Exists<AmpYearModule>(GLOBAL_SETTINGS_FILENAME))
				{
					//Deserialize global settings from file
					byte[] bytes = KSP.IO.File.ReadAllBytes<AmpYearModule>(GLOBAL_SETTINGS_FILENAME);
					object deserialized = KSP.IO.IOUtils.DeserializeFromBinary(bytes);
					if (deserialized is AYGlobalSettings)
					{
						AYGlobalSettings global_settings = (AYGlobalSettings)deserialized;

						//Apply deserialized global settings
						windowPos.x = global_settings.windowPosX;
						windowPos.y = global_settings.windowPosY;

						flightCompWindowPos.x = global_settings.flightCompWindowPosX;
						flightCompWindowPos.y = global_settings.flightCompWindowPosY;
					}
				}
			}
			catch (KSP.IO.IOException)
			{
			}
		}

		public static void saveVesselSettings(AmpYearModule module)
		{
			Debug.Log("Begin save");

			//Get the vessel settings
			AYVesselSettings settings = new AYVesselSettings();
			foreach (AmpYearModule.Subsystem subsystem in Enum.GetValues(typeof(AmpYearModule.Subsystem)))
			{
				settings.subsystemToggle[(int)subsystem] = module.subsystemEnabled(subsystem);
			}

			Debug.Log("Mid save");

			//Serialize settings to file
			try
			{
				byte[] serialized = KSP.IO.IOUtils.SerializeToBinary(settings);
				KSP.IO.File.WriteAllBytes<AmpYearModule>(serialized, VESSEL_SETTINGS_FILENAME, module.vessel);
				Debug.Log("End save");
			}
			catch (KSP.IO.IOException)
			{
			}
		}

		public static void loadVesselSettings(AmpYearModule module)
		{

			Debug.Log("Begin load");

			try
			{
				if (KSP.IO.File.Exists<AmpYearModule>(VESSEL_SETTINGS_FILENAME, module.vessel))
				{

					Debug.Log("Mid load");
					//Deserialize settings from file
					byte[] bytes = KSP.IO.File.ReadAllBytes<AmpYearModule>(VESSEL_SETTINGS_FILENAME, module.vessel);
					object deserialized = KSP.IO.IOUtils.DeserializeFromBinary(bytes);
					if (deserialized is AYVesselSettings)
					{
						AYVesselSettings settings = (AYVesselSettings)deserialized;

						if (settings.subsystemToggle != null
							&& settings.subsystemToggle.Length >= Enum.GetValues(typeof(AmpYearModule.Subsystem)).Length)
						{
							//Apply deserialized settings
							foreach (AmpYearModule.Subsystem subsystem in Enum.GetValues(typeof(AmpYearModule.Subsystem)))
							{
								module.setSubsystemEnabled(subsystem, settings.subsystemToggle[(int)subsystem]);
							}
							Debug.Log("End load");
						}

						
					}
				}
			}
			catch (KSP.IO.IOException)
			{
			}
		}
	}
}
