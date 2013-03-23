using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace AmpYear
{
	public class AmpYearModule : PartModule
	{

		public enum Subsystem
		{
			TURNING,
			POWER_TURN,
			SAS,
			ASAS,
			RCS,
			FLIGHT_COMPUTER,
			HEATER,
			COOLER,
			MUSIC,
			MASSAGE
		}

		public enum GUISection
		{
			SUBSYSTEM,
			RESERVE,
			LUXURY
		}

		//Constants

		public const double MANAGER_ACTIVE_DRAIN = 1.0 / 300.0;
		public const double ASAS_DRAIN = 2.0 / 60.0;
		public const double RCS_DRAIN = 1.0 / 60.0;
		public const double FLIGHT_COMPUTER_DRAIN = 1.0 / 2.0;

		public const double TURN_ROT_POWER_DRAIN_FACTOR = 1.0 / 40.0;
		public const float TURN_INACTIVE_ROT_FACTOR = 0.1f;

		public const double SAS_BASE_DRAIN = 1.0 / 60.0;
		public const double SAS_TORQUE_DRAIN_FACTOR = 1.0 / 160.0;
		public const double POWER_TURN_DRAIN_FACTOR = 1.0 / 5.0;
		public const float SAS_POWER_TURN_TORQUE_FACTOR = 0.25f;

		public const float HEATER_HEAT_RATE = 5.0f;
		public const double HEATER_BASE_DRAIN_FACTOR = 1.0;
		public const double HEATER_CAPACITY_DRAIN_FACTOR = 0.5;
		public const float HEATER_TARGET_TEMP = 20.0f;
		public const float COOLER_TARGET_TEMP = 15.0f;

		public readonly double[] RESERVE_TRANSFER_INCREMENTS = new double[3] { 0.25, 0.1, 0.01 };
		public const int MAX_TRANSFER_ATTEMPTS = 8;

		public const double RECHARGE_RESERVE_THRESHOLD = 0.95;
		public const double RECHARGE_RESERVE_RATE = 30.0 / 60.0;
		public const double RECHARGE_OVERFLOW_AVOID_FACTOR = 1.0;

		public const float WINDOW_WIDTH = 200;
		public const float WINDOW_BASE_HEIGHT = 140;

		public const double DRAIN_ESTIMATE_INTERVAL = 0.5;

		public const String MAIN_POWER_NAME = "ElectricCharge";
		public const String RESERVE_POWER_NAME = "ReservePower";

		public const String SUBSYS_STATE_LABEL = "Subsys";
		public const String GUI_SECTION_LABEL = "Section";

		//Properties

		public int windowID = new System.Random().Next();

		public AmpYearPart ayPart
		{
			private set;
			get;
		}
		public CommandPod commandPod
		{
			private set;
			get;
		}
		public Vessel lastVessel;
		public FlightInputCallback flightCallback;

		private RemoteTech.FlightComputer flightComputer;
		private RemoteTech.FlightComputerGUI flightComputerGUI;

		private bool managerEnabled = true;
		private bool[] subsystemToggle = new bool [Enum.GetValues(typeof(Subsystem)).Length];
		private double[] subsystemDrain = new double [Enum.GetValues(typeof(Subsystem)).Length];
		private bool hasPower = true;
		private bool hasReservePower = true;
		private bool isPrimaryPart = false;

		private bool subsysLoaded = false;
		private bool guiSectionLoaded = false;
		private bool guiEnabled = true;

		public List<Part> crewablePartList = new List<Part>();

		float totalDefaultRotPower = 0.0f;
		float commandPodDefaultRotPower = 0.0f;
		float sasTorque = 0.0f;
		float sasAdditionalRotPower = 0.0f;
		double turningFactor = 0.0;
		double totalElectricCharge = 0.0;
		double totalElectricChargeCapacity = 0.0;
		double totalReservePower = 0.0;
		double totalReservePowerCapacity = 0.0;

		bool hasRCS = false;
		float currentRCSThrust = 0.0f;
		float currentPoweredRCSDrain = 0.0f;

		int totalHeatedParts = 0;
		int totalCooledParts = 0;

		double lastEstimatedDrainTime = 0.0;
		double lastEstimatedDrainTimeUT = 0.0;
		double estimateLastTotalCharge = 0.0;

		double estimatedDrain = 0.0;

		bool sasWasFirst = false;
		bool reenableRCS = false;
		bool reenableSAS = false;
		bool reenableFlightComp = false;
		RemoteTech.AttitudeMode reenableAttitudeMode = RemoteTech.AttitudeMode.KILLROT;

		//GUI Properties

		public bool[] guiSectionEnableFlag = new bool[Enum.GetValues(typeof(GUISection)).Length];

		public GUIStyle sectionTitleStyle, subsystemButtonStyle, subsystemConsumptionStyle, statusStyle, warningStyle, powerSinkStyle;
		public GUILayoutOption[] subsystemButtonOptions;

		//PartModule

		public bool primaryCheck
		{
			get
			{
				foreach (Part p in vessel.parts)
				{
					if (p is AmpYearPart)
						return p == part;
				}
				return false;
			}
		}

		public bool partIsActive
		{
			get
			{
				return FlightGlobals.ready && FlightGlobals.ActiveVessel != null && vessel == FlightGlobals.ActiveVessel;
			}
		}

		public override void OnAwake()
		{
			base.OnAwake();

			AYSettings.loadGlobalSettings();
		}

		public override void OnStart(PartModule.StartState state)
		{
			base.OnStart(state);

			Debug.Log("AY Start");

			//Init properties
			ayPart = (AmpYearPart)part;
			commandPod = null;

			if (!subsysLoaded)
			{
				for (int i = 0; i < Enum.GetValues(typeof(Subsystem)).Length; i++)
					subsystemToggle[i] = false;

				subsystemToggle[(int)Subsystem.TURNING] = true;
				subsystemToggle[(int)Subsystem.ASAS] = true;
				subsystemToggle[(int)Subsystem.FLIGHT_COMPUTER] = true;
			}

			if (!guiSectionLoaded)
			{
				for (int i = 0; i < Enum.GetValues(typeof(GUISection)).Length; i++)
					guiSectionEnableFlag[i] = false;

				guiSectionEnableFlag[(int)GUISection.SUBSYSTEM] = true;
			}

			flightComputerGUI = new RemoteTech.FlightComputerGUI();
			flightComputer = new RemoteTech.FlightComputer(part, flightComputerGUI);
			flightComputerGUI.computer = flightComputer;

			flightCallback = new FlightInputCallback(this.flightControl);

			RenderingManager.AddToPostDrawQueue(3, new Callback(this.drawGUI));

		}

		public override void OnUpdate()
		{

			base.OnUpdate();

			if (FlightGlobals.ready && FlightGlobals.ActiveVessel != null)
			{
				//Check if the part's vessel has changed
				if (lastVessel != vessel)
				{
					if (lastVessel != null)
					{
						//Remove the flight callback from the previous vessel
						try
						{
							lastVessel.OnFlyByWire -= flightCallback;
						}
						catch { }
					}

					//Add the flight callback to the current vessel
					try
					{
						vessel.OnFlyByWire += flightCallback;
					}
					catch { }

					lastVessel = vessel;
				}
			
				//Compile information about the vessel and its parts
				totalDefaultRotPower = 0.0f;
				sasAdditionalRotPower = 0.0f;
				sasTorque = 0.0f;

				totalElectricCharge = 0.0;
				totalElectricChargeCapacity = 0.0;
				totalReservePower = 0.0;
				totalReservePowerCapacity = 0.0;

				totalHeatedParts = 0;
				totalCooledParts = 0;

				hasRCS = false;
				currentRCSThrust = 0.0f;
				currentPoweredRCSDrain = 0.0f;

				crewablePartList.Clear();

				bool determined_primary = false;

				bool command_module_correct = false;

				foreach (Part current_part in vessel.parts)
				{
					if (!determined_primary && current_part is AmpYearPart)
					{
						isPrimaryPart = current_part == part;
						determined_primary = true;
						if (!isPrimaryPart)
							break;
					}

					if (current_part is CommandPod)
					{
						if (commandPod == null)
						{
							//Find the ship's command pod
							commandPod = (CommandPod)current_part;
							commandPodDefaultRotPower = commandPod.rotPower;
						}

						if (commandPod == current_part)
						{
							totalDefaultRotPower += commandPodDefaultRotPower;
							command_module_correct = true;
						}
						else
							totalDefaultRotPower += ((CommandPod)current_part).rotPower;

						sasTorque += ((CommandPod)current_part).maxTorque;
					}

					if (current_part is SASModule)
					{
						SASModule sas_module = (SASModule)current_part;
						sasTorque += sas_module.maxTorque;
						sasAdditionalRotPower += sas_module.maxTorque * SAS_POWER_TURN_TORQUE_FACTOR;
					}

					bool has_alternator = false;

					foreach (PartModule module in current_part.Modules)
					{
						if (module is ModuleAlternator)
							has_alternator = true;

						if (module is ModuleRCS)
						{
							hasRCS = true;

							ModuleRCS rcs = (ModuleRCS)module;
							foreach (float thrust in rcs.thrustForces)
								currentRCSThrust += thrust;

							if (module is ModuleAmpYearPoweredRCS)
							{
								currentPoweredRCSDrain += ((ModuleAmpYearPoweredRCS)module).electricityUse;
								((ModuleAmpYearPoweredRCS)module).isManaged = true; //Inform the thruster that it is being managed
							}
						}
					}

					//Sum up the power capacity of all parts
					if (!has_alternator) //Ignore parts with alternators in power-capacity calculate because they don't actually store power
					{
						foreach (PartResource resource in current_part.Resources)
						{
							if (resource.resourceName == MAIN_POWER_NAME)
							{
								totalElectricCharge += resource.amount;
								totalElectricChargeCapacity += resource.maxAmount;
							}
							else if (resource.resourceName == RESERVE_POWER_NAME)
							{
								totalReservePower += resource.amount;
								totalReservePowerCapacity += resource.maxAmount;
							}
						}
					}

					if (current_part.CrewCapacity > 0)
					{
						if (current_part.temperature >= HEATER_TARGET_TEMP)
							totalHeatedParts++;

						if (current_part.temperature <= COOLER_TARGET_TEMP)
							totalCooledParts++;

						crewablePartList.Add(current_part);
					}

				}

				//Update command module rot-power to account for power turn
				if (commandPod != null && command_module_correct)
				{
					if (subsystemPowered(Subsystem.POWER_TURN))
						commandPod.rotPower = commandPodDefaultRotPower + sasAdditionalRotPower;
					else
						commandPod.rotPower = commandPodDefaultRotPower;
				}
				else
				{
					if (commandPod != null)
						commandPod.rotPower = commandPodDefaultRotPower;
					commandPod = null;
				}

				//Estimate the amount of power drain
				if (UnityEngine.Time.time - lastEstimatedDrainTime > DRAIN_ESTIMATE_INTERVAL)
				{
					if (lastEstimatedDrainTimeUT > 0.0)
					{
						double time_delta = Planetarium.GetUniversalTime() - lastEstimatedDrainTimeUT;
						estimatedDrain = ((estimateLastTotalCharge - totalElectricCharge) / time_delta);
					}

					lastEstimatedDrainTime = UnityEngine.Time.time;
					lastEstimatedDrainTimeUT = Planetarium.GetUniversalTime();
					estimateLastTotalCharge = totalElectricCharge;
				}

				if (isPrimaryPart)
					subsystemUpdate();
				else
					ayPart.ASASActive = false;
			}
			else {
				if (ayPart != null)
					ayPart.ASASActive = false;
			}

		}

		public override void OnSave(ConfigNode node)
		{
			Debug.Log("AY Save");

			AYSettings.saveGlobalSettings();

			try
			{
				node.AddValue(SUBSYS_STATE_LABEL, subsystemStateToInt());
				node.AddValue(GUI_SECTION_LABEL, guiSectionStateToInt());
			}
			catch
			{
			}

		}

		public override void OnLoad(ConfigNode node)
		{
			Debug.Log("AY Load");

			subsysLoaded = false;
			guiSectionLoaded = false;

			try
			{
				int state = 0;
				if (int.TryParse(node.GetValue(SUBSYS_STATE_LABEL), out state))
				{
					setSubsystemStateFromInt(state);
					subsysLoaded = true;
				}

				if (int.TryParse(node.GetValue(GUI_SECTION_LABEL), out state))
				{
					setGuiSectionStateFromInt(state);
					guiSectionLoaded = true;
				}
			}
			catch
			{
			}

		}

		[KSPEvent(guiActive = true, guiName = "Toggle GUI")]
		public void toggleGUI()
		{
			guiEnabled = !guiEnabled;
		}

		//Manager

		public bool timewarpIsValid
		{
			get
			{
				return TimeWarp.CurrentRate <= 4;
			}
		}

		public bool managerIsActive
		{
			get
			{
				return timewarpIsValid && managerEnabled && (hasPower || hasReservePower);
			}
		}

		public double managerActiveDrain
		{
			get
			{
				return MANAGER_ACTIVE_DRAIN;
			}
		}

		public double managerCurrentDrain
		{
			get
			{
				if (managerIsActive)
					return managerActiveDrain;
				else
					return 0.0;
			}
		}

		//Subsystem

		public bool subsystemEnabled(Subsystem subsystem)
		{
			switch (subsystem)
			{
				case Subsystem.SAS:
					return vessel.ActionGroups[KSPActionGroup.SAS];

				case Subsystem.RCS:
					return vessel.ActionGroups[KSPActionGroup.RCS];

				default:
					return subsystemToggle[(int)subsystem];
			}		
		}

		public void setSubsystemEnabled(Subsystem subsystem, bool enabled)
		{
			switch (subsystem)
			{
				case Subsystem.SAS:
					vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, enabled);
					break;

				case Subsystem.RCS:
					vessel.ActionGroups.SetGroup(KSPActionGroup.RCS, enabled);
					break;

				default:
					subsystemToggle[(int)subsystem] = enabled;
					break;
			}	
		}

		public bool subsystemActive(Subsystem subsystem)
		{
			if (!subsystemEnabled(subsystem))
				return false;

			switch (subsystem)
			{
				case Subsystem.POWER_TURN:
					return subsystemEnabled(Subsystem.TURNING);

				case Subsystem.ASAS:
					return subsystemEnabled(Subsystem.SAS);

				case Subsystem.FLIGHT_COMPUTER:
					return subsystemEnabled(Subsystem.TURNING) && flightComputer.AttitudeActive;

				case Subsystem.HEATER:
					return totalHeatedParts < crewablePartList.Count;

				case Subsystem.COOLER:
					return totalCooledParts < crewablePartList.Count;

				default:
					return true;
			}
		}

		public bool subsystemPowered(Subsystem subsystem)
		{
			return hasPower && managerIsActive && subsystemActive(subsystem);
		}

		public bool subsystemVisible(Subsystem subsystem)
		{
			switch (subsystem)
			{
				case Subsystem.POWER_TURN:
					return sasAdditionalRotPower > 0.0f;

				case Subsystem.RCS:
					return hasRCS;

				case Subsystem.HEATER:
				case Subsystem.COOLER:
				case Subsystem.MUSIC:
					return crewablePartList.Count > 0;

				case Subsystem.MASSAGE:
					return vessel.GetCrewCount() > 0;

				default:
					return true;
			}
		}

		public double subsystemActiveDrain(Subsystem subsystem)
		{
			switch (subsystem)
			{
				case Subsystem.TURNING:
					return totalDefaultRotPower * TURN_ROT_POWER_DRAIN_FACTOR;

				case Subsystem.SAS:
					return sasTorque * SAS_TORQUE_DRAIN_FACTOR;

				case Subsystem.RCS:
					return RCS_DRAIN + currentPoweredRCSDrain;

				case Subsystem.ASAS:
					return ASAS_DRAIN;

				case Subsystem.FLIGHT_COMPUTER:
					return FLIGHT_COMPUTER_DRAIN;

				case Subsystem.POWER_TURN:
					return sasAdditionalRotPower * POWER_TURN_DRAIN_FACTOR;

				case Subsystem.HEATER:
				case Subsystem.COOLER:
					if (commandPod != null)
						return HEATER_HEAT_RATE
							* (crewablePartList.Count * HEATER_BASE_DRAIN_FACTOR + HEATER_CAPACITY_DRAIN_FACTOR * vessel.GetCrewCapacity());
					else
						return 0.0;

				case Subsystem.MUSIC:
					return 1.0 / 120.0 * crewablePartList.Count;

				case Subsystem.MASSAGE:
					return 5.0 * vessel.GetCrewCount();

				default:
					return 0.0;
			}
		}

		public double subsystemCurrentDrain(Subsystem subsystem)
		{
			if (!subsystemActive(subsystem) || !managerIsActive || !hasPower)
				return 0.0;

			switch (subsystem)
			{
				case Subsystem.RCS:
					if (currentRCSThrust > 0.0f)
						return subsystemActiveDrain(subsystem);
					else
						return 0.0;

				case Subsystem.TURNING:
					return turningFactor * subsystemActiveDrain(subsystem);

				case Subsystem.POWER_TURN:
					if (subsystemEnabled(Subsystem.TURNING))
						return turningFactor * subsystemActiveDrain(subsystem);
					else
						return 0.0;

				default:
					return subsystemActiveDrain(subsystem);
			}
		}

		public static string subsystemName(Subsystem subsystem)
		{
			switch (subsystem)
			{
				case Subsystem.TURNING:
					return "Turning";

				case Subsystem.POWER_TURN:
					return "PowerTurn";

				case Subsystem.SAS:
					return "SAS";

				case Subsystem.RCS:
					return "RCS";

				case Subsystem.ASAS:
					return "ASAS";

				case Subsystem.FLIGHT_COMPUTER:
					return "FlightComp.";

				case Subsystem.HEATER:
					return "Heater";

				case Subsystem.COOLER:
					return "Air Cond.";

				case Subsystem.MUSIC:
					return "Smooth Jazz";
					 
				case Subsystem.MASSAGE:
					return "Massage Chair";

				default:
					return String.Empty;
			}	
		}

		public static bool subsystemIsLuxury(Subsystem subsystem)
		{
			switch (subsystem)
			{

				case Subsystem.HEATER:
				case Subsystem.COOLER:
				case Subsystem.MUSIC:
				case Subsystem.MASSAGE:
					return true;

				default:
					return false;
			}
		}

		private void subsystemUpdate()
		{

			if (ayPart != null)
				ayPart.ASASActive = subsystemPowered(Subsystem.ASAS);

			if (vessel.ActionGroups[KSPActionGroup.RCS] && !subsystemPowered(Subsystem.RCS))
			{
				//Disable RCS when the subsystem isn't powered
				vessel.ActionGroups.SetGroup(KSPActionGroup.RCS, false);
				reenableRCS = true;
			}

			if (vessel.ActionGroups[KSPActionGroup.SAS]) {

				if (!subsystemPowered(Subsystem.SAS))
				{
					//Disable SAS when the subsystem isn't powered
					vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, false);
					reenableSAS = true;
				}
				else
				{
					if (subsystemPowered(Subsystem.FLIGHT_COMPUTER))
					{
						//Both SAS and flight computer are active
						if (sasWasFirst)
						{
							//Turn off SAS if the flight computer was turned on second
							vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, false);
							sasWasFirst = false;
						}
						else
						{
							//Turn off flight computer if SAS was turned on second
							sasWasFirst = true;
							deactivateFlightComputer();
						}
					}
					else
						sasWasFirst = true;
				}
			}

			if (!subsystemEnabled(Subsystem.FLIGHT_COMPUTER) || !hasPower || !managerIsActive)
			{
				RemoteTech.AttitudeMode reenable_mode = RemoteTech.AttitudeMode.KILLROT;

				//Turn off flight computer if it isn't enabled and receiving power
				if (deactivateFlightComputer(ref reenable_mode) && subsystemEnabled(Subsystem.FLIGHT_COMPUTER))
				{
					reenableFlightComp = true;
					reenableAttitudeMode = reenable_mode;
				}
			}

			if (managerIsActive && hasPower)
			{
				//Re-enable SAS/RCS if they were shut off by the manager and can be run again
				if (reenableSAS)
				{
					setSubsystemEnabled(Subsystem.SAS, true);
					reenableSAS = false;
				}

				if (reenableRCS)
				{
					setSubsystemEnabled(Subsystem.RCS, true);
					reenableRCS = false;
				}

				if (reenableFlightComp)
				{
					foreach (RemoteTech.AttitudeStateButton button in flightComputerGUI.attitudeButtons)
					{
						if (button.mode == reenableAttitudeMode)
						{
							button.on = true;
							button.Update();
							break;
						}
					}
					reenableFlightComp = false;
				}
			}

			if (subsystemPowered(Subsystem.FLIGHT_COMPUTER) && !subsystemPowered(Subsystem.SAS)) 
				sasWasFirst = false;

			if (subsystemPowered(Subsystem.HEATER))
				changeCrewedPartsTemperature(HEATER_TARGET_TEMP, true);

			if (subsystemPowered(Subsystem.COOLER))
				changeCrewedPartsTemperature(COOLER_TARGET_TEMP, false);

			flightComputerGUI.update();

			//Calculate total drain from subsystems
			double subsystem_drain = 0.0;
			foreach (Subsystem subsystem in Enum.GetValues(typeof(Subsystem)))
			{
				subsystemDrain[(int)subsystem] = subsystemCurrentDrain(subsystem);
				subsystem_drain += subsystemDrain[(int)subsystem];
			}

			turningFactor = 0.0f;

			double manager_drain = managerCurrentDrain;
			double total_manager_drain = subsystem_drain + manager_drain;

			//Recharge reserve power if main power is above a certain threshold
			if (managerIsActive && totalElectricCharge > 0 && totalElectricCharge / totalElectricChargeCapacity > RECHARGE_RESERVE_THRESHOLD
				&& totalReservePower < totalReservePowerCapacity)
				transferMainToReserve(RECHARGE_RESERVE_RATE * TimeWarp.deltaTime);

			//Drain main power
			double timestep_drain = total_manager_drain * TimeWarp.deltaTime;
			double minimum_sufficient_charge = managerActiveDrain * TimeWarp.deltaTime;

			if (totalElectricCharge >= minimum_sufficient_charge)
			{
				hasPower = timestep_drain <= 0.0 || requestResource(MAIN_POWER_NAME, timestep_drain) >= (timestep_drain * 0.99);
			}
			else
				hasPower = false;

			if (!hasPower && totalReservePower > minimum_sufficient_charge)
			{
				//If main power is insufficient, drain reserve power for manager
				double manager_timestep_drain = manager_drain * TimeWarp.deltaTime;
				hasReservePower = manager_drain <= 0.0
					|| requestResource(RESERVE_POWER_NAME, manager_timestep_drain) >= (manager_timestep_drain * 0.99);
			}
			else
				hasReservePower = totalReservePower > minimum_sufficient_charge;
		}

		private int subsystemStateToInt()
		{
			int val = 0;
			foreach (Subsystem subsystem in Enum.GetValues(typeof(Subsystem)))
			{
				if (subsystemEnabled(subsystem))
					val |= (1 << (int)subsystem);
			}
			return val;
		}

		private void setSubsystemStateFromInt(int state)
		{
			foreach (Subsystem subsystem in Enum.GetValues(typeof(Subsystem)))
				setSubsystemEnabled(subsystem, (state & (1 << (int)subsystem)) != 0);
		}

		void changeCrewedPartsTemperature(double target_temp, bool heat)
		{
			foreach (Part crewed_part in crewablePartList) {
				if (heat)
				{
					if (crewed_part.temperature < target_temp)
						crewed_part.temperature += HEATER_HEAT_RATE * TimeWarp.deltaTime;
				}
				else
				{
					if (crewed_part.temperature > target_temp)
						crewed_part.temperature -= HEATER_HEAT_RATE * TimeWarp.deltaTime;
				}
			}
		}

		private bool deactivateFlightComputer()
		{
			RemoteTech.AttitudeMode mode = RemoteTech.AttitudeMode.KILLROT;
			return deactivateFlightComputer(ref mode);
		}

		private bool deactivateFlightComputer(ref RemoteTech.AttitudeMode mode)
		{
			foreach (RemoteTech.AttitudeStateButton button in flightComputerGUI.attitudeButtons)
			{
				if (button.on)
				{
					button.on = false;
					button.Update();
					mode = button.mode;
					return true;
				}
			}

			return false;
		}

		//GUI Section

		public static string guiSectionName(GUISection section)
		{
			switch (section)
			{
				case GUISection.SUBSYSTEM:
					return "Subsys";

				case GUISection.RESERVE:
					return "Reserve";

				case GUISection.LUXURY:
					return "Luxury";

				default:
					return String.Empty;
			}
		}

		public bool guiSectionEnabled(GUISection section)
		{
			return guiSectionEnableFlag[(int)section];
		}

		private int guiSectionStateToInt()
		{
			int val = 0;
			for (int i = 0; i < Enum.GetValues(typeof(GUISection)).Length; i++)
			{
				if (guiSectionEnableFlag[i])
					val |= (1 << i);
			}
			return val;
		}

		private void setGuiSectionStateFromInt(int state)
		{
			for (int i = 0; i < Enum.GetValues(typeof(GUISection)).Length; i++)
			{
				guiSectionEnableFlag[i] = (state & (1 << i)) != 0;
			}
		}

		//Turning

		private void flightControl(FlightCtrlState state)
		{
			if (isPrimaryPart)
			{
				flightComputer.drive(state);

				if (!subsystemPowered(Subsystem.TURNING))
				{
					//Reduce the turning rate if turning subsystem isn't active
					state.pitch *= TURN_INACTIVE_ROT_FACTOR;
					state.yaw *= TURN_INACTIVE_ROT_FACTOR;
					state.roll *= TURN_INACTIVE_ROT_FACTOR;
				}

				turningFactor += (Math.Abs(state.pitch) + Math.Abs(state.roll) + Math.Abs(state.yaw)) / 3.0;
			}
		}

		//Resources

		public double requestResource(String name, double amount)
		{
			return requestResource(name, amount, part);
		}

		public static double requestResource(String name, double amount, Part part)
		{
			if (amount <= 0.0)
				return 0.0;

			double total_received = 0.0;

			double request_amount = amount;
			for (int attempts = 0; attempts < MAX_TRANSFER_ATTEMPTS && amount > 0.0; attempts++)
			{
				double received = part.RequestResource(name, request_amount);
				total_received += received;
				amount -= received;

				if (received <= 0.0)
					request_amount = amount * 0.5;
				else
					request_amount = amount;
			}

			return total_received;
		}

		private void transferReserveToMain(double amount)
		{
			//Debug.Log("main: " + totalElectricCharge);
			//Debug.Log("reserve: " + totalReservePower);
			if (amount > totalReservePower * RECHARGE_OVERFLOW_AVOID_FACTOR)
				amount = totalReservePower * RECHARGE_OVERFLOW_AVOID_FACTOR;

			if (amount > (totalElectricChargeCapacity - totalElectricCharge))
				amount = (totalElectricChargeCapacity - totalElectricCharge);

			//Debug.Log("amount: " + amount);

			double received = requestResource(RESERVE_POWER_NAME, amount);
			//Debug.Log("received: " + received);

			int transfer_attempts = 0;
			while (received > 0.0 && transfer_attempts < MAX_TRANSFER_ATTEMPTS)
			{
				received += part.RequestResource(MAIN_POWER_NAME, -received);
				transfer_attempts++;
			}

			//Debug.Log("generated: " + (amount-received).ToString());
		}

		private void transferMainToReserve(double amount)
		{
			if (amount > totalElectricCharge * RECHARGE_OVERFLOW_AVOID_FACTOR)
				amount = totalElectricCharge * RECHARGE_OVERFLOW_AVOID_FACTOR;

			if (amount > (totalReservePowerCapacity - totalReservePower))
				amount = (totalReservePowerCapacity - totalReservePower);

			double received = requestResource(MAIN_POWER_NAME, amount);
			//Debug.Log("received: " + received);

			int transfer_attempts = 0;
			while (received > 0.0 && transfer_attempts < MAX_TRANSFER_ATTEMPTS)
			{
				received += part.RequestResource(RESERVE_POWER_NAME, -received);
				transfer_attempts++;
			}

			//Debug.Log("generated: " + (amount - received).ToString());
		}

		//GUI

		private void drawGUI()
		{
			//Debug.Log("draw start");

			if (partIsActive && isPrimaryPart && guiEnabled)
			{

				GUI.skin = HighLogic.Skin;

				AYSettings.windowPos = GUILayout.Window(
					windowID,
					AYSettings.windowPos,
					window,
					"AmpYear Power Manager",
					GUILayout.Width(WINDOW_WIDTH),
					GUILayout.Height(WINDOW_BASE_HEIGHT)
					);

				if (subsystemEnabled(Subsystem.FLIGHT_COMPUTER))
				{
					AYSettings.flightCompWindowPos = GUILayout.Window(
						flightComputerGUI.ATTITUDE_ID,
						AYSettings.flightCompWindowPos,
						flightComputerGUI.AttitudeGUI,
						"Attitude",
						GUILayout.Width(30),
						GUILayout.Height(60)
						);
				}
			}

			//Debug.Log("draw end");
		}

		private void window(int id)
		{
			//Init styles
			sectionTitleStyle = new GUIStyle(GUI.skin.label);
			sectionTitleStyle.alignment = TextAnchor.MiddleCenter;
			sectionTitleStyle.stretchWidth = true;
			sectionTitleStyle.fontStyle = FontStyle.Bold;

			subsystemConsumptionStyle = new GUIStyle(GUI.skin.label);
			subsystemConsumptionStyle.alignment = TextAnchor.LowerRight;
			subsystemConsumptionStyle.stretchWidth = true;
			//subsystemConsumptionStyle.margin.top = 4;

			powerSinkStyle = new GUIStyle(GUI.skin.label);
			powerSinkStyle.alignment = TextAnchor.LowerLeft;
			powerSinkStyle.stretchWidth = true;

			statusStyle = new GUIStyle(GUI.skin.label);
			statusStyle.alignment = TextAnchor.MiddleCenter;
			statusStyle.stretchWidth = true;
			statusStyle.normal.textColor = Color.white;

			warningStyle = new GUIStyle(GUI.skin.label);
			warningStyle.alignment = TextAnchor.MiddleCenter;
			warningStyle.stretchWidth = true;
			warningStyle.fontStyle = FontStyle.Bold;
			warningStyle.normal.textColor = Color.red;

			subsystemButtonStyle = new GUIStyle(GUI.skin.toggle);
			subsystemButtonStyle.margin.top = 0;
			subsystemButtonStyle.margin.bottom = 0;
			subsystemButtonStyle.padding.top = 0;
			subsystemButtonStyle.padding.bottom = 0;

			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			foreach (GUISection section in Enum.GetValues(typeof(GUISection)))
			{
				guiSectionEnableFlag[(int)section]
					= GUILayout.Toggle(guiSectionEnableFlag[(int)section], guiSectionName(section), GUI.skin.button);
			}
			GUILayout.EndHorizontal();

			//Manager status+drain
			if (timewarpIsValid)
			{
				GUILayout.BeginHorizontal();
				managerEnabled = GUILayout.Toggle(managerEnabled, "Manager", subsystemButtonStyle, subsystemButtonOptions);
				if (managerIsActive)
					consumptionLabel(managerCurrentDrain, false);
				else
					consumptionLabel(managerActiveDrain, true);
				GUILayout.EndHorizontal();
			}

			//Manager status label
			if (hasPower || hasReservePower)
			{
				if (managerIsActive)
				{
					if (hasPower)
					{
						if (totalElectricChargeCapacity > 0.0)
						{
							double power_percent = (totalElectricCharge / totalElectricChargeCapacity) * 100.0;
							GUILayout.Label("Power: " + power_percent.ToString("0.00") + '%', statusStyle);
						}
					}
					else
						GUILayout.Label("Running on Reserve Power!", warningStyle);
				}
				else if (timewarpIsValid)
					GUILayout.Label("Manager Disabled", warningStyle);
				else
					GUILayout.Label("Auto-Hibernation", statusStyle);
			}
			else
				GUILayout.Label("Insufficient Power", warningStyle);

			if (managerIsActive)
			{

				//Subsystems
				if (guiSectionEnabled(GUISection.SUBSYSTEM))
				{
					GUILayout.Label("Subsystems", sectionTitleStyle);
					foreach (Subsystem subsystem in Enum.GetValues(typeof(Subsystem)))
					{
						if (!subsystemIsLuxury(subsystem) && subsystemVisible(subsystem))
						{
							GUILayout.BeginHorizontal();
							subsystemButton(subsystem);
							subsystemConsumptionLabel(subsystem);
							GUILayout.EndHorizontal();
						}
					}
				}

				//Luxury
				if (guiSectionEnabled(GUISection.LUXURY))
				{
					GUILayout.Label("Luxury", sectionTitleStyle);
					foreach (Subsystem subsystem in Enum.GetValues(typeof(Subsystem)))
					{
						if (subsystemIsLuxury(subsystem) && subsystemVisible(subsystem))
						{
							GUILayout.BeginHorizontal();
							subsystemButton(subsystem);
							subsystemConsumptionLabel(subsystem);
							GUILayout.EndHorizontal();
						}
					}
				}

				//Reserve
				if (guiSectionEnabled(GUISection.RESERVE))
				{
					GUILayout.Label("Reserve Power", sectionTitleStyle);

					//Reserve status label
					if (totalReservePowerCapacity > 0.0)
					{
						if (hasReservePower)
						{
							double reserve_percent = (totalReservePower / totalReservePowerCapacity) * 100.0;
							GUILayout.Label("Reserve Power: " + reserve_percent.ToString("0.00") + '%', statusStyle);
						}
						else
							GUILayout.Label("Reserve Power Depleted", warningStyle);
					}
					else
						GUILayout.Label("Reserve Power not Found!", warningStyle);

					//Reserve transfer
					String[] increment_percent_string = new String[RESERVE_TRANSFER_INCREMENTS.Length];
	
					GUILayout.BeginHorizontal();
					GUILayout.Label("Reserve to Main");
					for (int i = 0; i < RESERVE_TRANSFER_INCREMENTS.Length; i++)
					{
						increment_percent_string[i] = (RESERVE_TRANSFER_INCREMENTS[i] * 100).ToString("F0") + '%';
						if (GUILayout.Button(increment_percent_string[i]))
							transferReserveToMain(totalReservePowerCapacity * RESERVE_TRANSFER_INCREMENTS[i]);
					}

					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("Main to Reserve");
					for (int i = 0; i < RESERVE_TRANSFER_INCREMENTS.Length; i++)
					{
						if (GUILayout.Button(increment_percent_string[i]))
							transferMainToReserve(totalReservePowerCapacity * RESERVE_TRANSFER_INCREMENTS[i]);
					}
					GUILayout.EndHorizontal();
				}

			}

			GUILayout.EndVertical();

			GUI.DragWindow();
		}

		private void subsystemButton(Subsystem subsystem)
		{
			setSubsystemEnabled(
				subsystem,
				GUILayout.Toggle(subsystemEnabled(subsystem), subsystemName(subsystem), subsystemButtonStyle, GUILayout.Width(WINDOW_WIDTH / 2.0f))
				);
		}

		private void subsystemConsumptionLabel(Subsystem subsystem)
		{
			double drain = subsystemDrain[(int)subsystem];
			if (drain == 0.0)
			{
				drain = subsystemActiveDrain(subsystem);
				consumptionLabel(drain, true);
			}
			else
				consumptionLabel(drain, false);
		}

		private void consumptionLabel(double drain, bool greyed = false)
		{

			if (drain == 0.0 || greyed)
				subsystemConsumptionStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
			else if (drain > 0.0)
				subsystemConsumptionStyle.normal.textColor = Color.red;
			else
				subsystemConsumptionStyle.normal.textColor = Color.green;

			GUILayout.Label(drain.ToString("0.000")+"/s", subsystemConsumptionStyle);
		}

	}
}
