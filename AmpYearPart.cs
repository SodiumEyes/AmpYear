using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmpYear
{
	public class AmpYearPart : AdvSASModule
	{

		private bool setASASActive;
		private bool _ASASActive;

		public bool ASASActive {
			set
			{
				setASASActive = value;
			}
			get
			{
				return setASASActive;
			}
		}

		protected override void onPartStart()
		{
			base.onPartStart();

			_ASASActive = false;
		}

		protected override void onPartFixedUpdate()
		{
			if (_ASASActive && FlightGlobals.ready && FlightGlobals.ActiveVessel == vessel)
			{
				bool restore_sas = vessel.ActionGroups[KSPActionGroup.SAS];

				if (!setASASActive)
					vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, false);

				base.onPartFixedUpdate();

				vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, restore_sas);
			}

			_ASASActive = setASASActive;
		}
	}
}
