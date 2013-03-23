using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace AmpYear
{
	public class ModuleAmpYearPoweredRCS : ModuleRCS
	{

		public bool isManaged = false;

		[KSPField]
		public float powerRatio = 30.0f;

		public float electricityUse
		{
			get
			{
				float total_thrust = 0.0f;
				foreach (float t in thrustForces)
					total_thrust += t * thrusterPower;

				float isp = atmosphereCurve.Evaluate((float)vessel.altitude);

				if (isp > 0.0f) {
					PartResourceDefinition definition = PartResourceLibrary.Instance.GetDefinition(resourceName);
					if (definition != null)
					{
						float flow_rate = ((total_thrust * 1000) / (isp * 9.81f)) / 1000.0f; //Calculate flow rate in Mg/s 
						return (flow_rate / definition.density) * powerRatio;
					}
					else
						return 0.0f;
				}
				else
					return 0.0f;
			}
		}

		public override void OnUpdate()
		{

			base.OnUpdate();

			enabled = isManaged;
			isManaged = false;

		}

	}
}
