using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmpYear
{
	[Serializable]
	class AYVesselSettings
	{
		public bool[] subsystemToggle;

		public AYVesselSettings()
		{
			subsystemToggle = new bool[Enum.GetValues(typeof(AmpYearModule.Subsystem)).Length];
		}
	}
}
