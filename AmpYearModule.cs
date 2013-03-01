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
			FLIGHT_COMPUTER
		}

		//Constants

		public const double MANAGER_ACTIVE_DRAIN = 1.0 / 300.0;
		public const double ASAS_DRAIN = 2.0 / 60.0;
		public const double RCS_DRAIN = 1.0 / 60.0;
		public const double FLIGHT_COMPUTER_DRAIN = 1.0 / 2.0;

		public const double TURN_ROT_POWER_DRAIN_FACTOR = 1.0 / 40.0;
		public const float TURN_INACTIVE_ROT_FACTOR = 0.1f;

		public const double SAS_BASE_DRAIN = 1.0 / 60.0;
		public const double SAS_ROT_POW_DRAIN_FACTOR = 1.0 / 80.0;
		public const double SAS_TORQUE_DRAIN_FACTOR = 1.0 / 160.0;
		public const double POWER_TURN_DRAIN_FACTOR = 1.0 / 5.0;
		public const float SAS_POWER_TURN_TORQUE_FACTOR = 0.25f;

		public const double RESERVE_TRANSFER_INCREMENT_FACTOR = 0.25;
		public const int MAX_TRANSFER_ATTEMPTS = 4;

		public const double RECHARGE_RESERVE_THRESHOLD = 0.95;
		public const double RECHARGE_RESERVE_RATE = 30.0 / 60.0;

		public const float WINDOW_WIDTH = 200;
		public const float WINDOW_BASE_HEIGHT = 140;
		public const float SECTION_HEIGHT_SUBSYSTEM = 240;
		public const float SECTION_HEIGHT_RESERVE = 120;

		public const double DRAIN_ESTIMATE_INTERVAL = 0.5;

		public const String MAIN_POWER_NAME = "ElectricCharge";
		public const String RESERVE_POWER_NAME = "ReservePower";

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

		private RemoteTech.FlightComputer flightComputer;
		private RemoteTech.FlightComputerGUI flightComputerGUI;

		private bool managerEnabled = true;
		private bool[] subsystemToggle = new bool [Enum.GetValues(typeof(Subsystem)).Length];
		private double[] subsystemDrain = new double [Enum.GetValues(typeof(Subsystem)).Length];
		private bool hasPower = true;
		private bool hasReservePower = true;
		private bool isPrimaryPart = false;

		float totalDefaultRotPower = 0.0f;
		float commandPodDefaultRotPower = 0.0f;
		float sasTorque = 0.0f;
		float sasAdditionalRotPower = 0.0f;
		double turningFactor = 0.0;
		double totalElectricCharge = 0.0;
		double totalElectricChargeCapacity = 0.0;
		double totalReservePower = 0.0;
		double totalReservePowerCapacity = 0.0;

		double lastEstimatedDrainTime = 0.0;
		double lastEstimatedDrainTimeUT = 0.0;
		double estimateLastTotalCharge = 0.0;

		double estimatedDrain = 0.0;

		bool sasWasFirst = false;

		//GUI Properties

		public bool sectionGUIEnabledSubsystem = true;
		public bool sectionGUIEnabledReserve = false;

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
				return FlightGlobals.ready && vessel == FlightGlobals.ActiveVessel;
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

			if (partIsActive)
			{
				RenderingManager.AddToPostDrawQueue(3, new Callback(this.drawGUI));

				ayPart = (AmpYearPart)part;

				for (int i = 0; i < Enum.GetValues(typeof(Subsystem)).Length; i++)
				{
					subsystemToggle[i] = false;
				}

				subsystemToggle[(int)Subsystem.TURNING] = true;
				subsystemToggle[(int)Subsystem.ASAS] = true;
				subsystemToggle[(int)Subsystem.FLIGHT_COMPUTER] = true;

				try
				{
					part.vessel.OnFlyByWire += new FlightInputCallback(this.flightControl);
				}
				catch { }

				flightComputerGUI = new RemoteTech.FlightComputerGUI();
				flightComputer = new RemoteTech.FlightComputer(part, flightComputerGUI);
				flightComputerGUI.computer = flightComputer;

				if (primaryCheck)
				{
					AYSettings.loadVesselSettings(this);
				}
			}
		}

		public override void OnUpdate()
		{
			base.OnUpdate();

			if (partIsActive)
			{
				//Compile information about the vessel and its parts
				totalDefaultRotPower = 0.0f;
				sasAdditionalRotPower = 0.0f;
				sasTorque = 0.0f;

				totalElectricCharge = 0.0;
				totalElectricChargeCapacity = 0.0;
				totalReservePower = 0.0;
				totalReservePowerCapacity = 0.0;

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
					}

					if (current_part is SASModule)
					{
						SASModule sas_module = (SASModule)current_part;
						sasTorque += sas_module.maxTorque;
						sasAdditionalRotPower += sas_module.maxTorque * SAS_POWER_TURN_TORQUE_FACTOR;
					}

					//Ignore parts with alternators in power-capacity calculate because they don't actually store power
					if (!part.Modules.Contains("ModuleAlternator"))
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
			}

			if (isPrimaryPart)
				subsystemUpdate();
			else if (ayPart != null)
				ayPart.ASASActive = false;

		}

		public override void OnSave(ConfigNode node)
		{
			base.OnSave(node);

			if (primaryCheck)
			{
				AYSettings.saveGlobalSettings();
				AYSettings.saveVesselSettings(this);
			}
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

				default:
					return true;
			}
		}

		public bool subsystemPowered(Subsystem subsystem)
		{
			return hasPower && managerIsActive && subsystemActive(subsystem);
		}

		public double subsystemActiveDrain(Subsystem subsystem)
		{
			switch (subsystem)
			{
				case Subsystem.TURNING:
					return totalDefaultRotPower * TURN_ROT_POWER_DRAIN_FACTOR;

				case Subsystem.SAS:
					return sasTorque * SAS_TORQUE_DRAIN_FACTOR + totalDefaultRotPower * SAS_ROT_POW_DRAIN_FACTOR;

				case Subsystem.RCS:
					return RCS_DRAIN;

				case Subsystem.ASAS:
					return ASAS_DRAIN;

				case Subsystem.FLIGHT_COMPUTER:
					return FLIGHT_COMPUTER_DRAIN;

				case Subsystem.POWER_TURN:
					return sasAdditionalRotPower * POWER_TURN_DRAIN_FACTOR;

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

				default:
					return String.Empty;
			}	
		}

		private void subsystemUpdate()
		{

			if (ayPart != null)
				ayPart.ASASActive = subsystemPowered(Subsystem.ASAS);

			if (vessel.ActionGroups[KSPActionGroup.RCS] && !subsystemPowered(Subsystem.RCS))
				vessel.ActionGroups.SetGroup(KSPActionGroup.RCS, false);

			if (vessel.ActionGroups[KSPActionGroup.SAS]) {

				if (!subsystemPowered(Subsystem.SAS))
					vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, false);
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
						else {
							//Turn off flight computer if SAS was turned on second
							sasWasFirst = true;
							foreach (RemoteTech.AttitudeStateButton button in flightComputerGUI.attitudeButtons)
							{
								button.on = false;
								button.Update();
							}
						}
					}
					else
						sasWasFirst = true; 
				}
				
				
			}

			if (subsystemPowered(Subsystem.FLIGHT_COMPUTER) && !subsystemPowered(Subsystem.SAS)) 
				sasWasFirst = false;

			/*
			if (commandPod != null && commandPod.temperature < 20.0)
			{
				commandPod.temperature += 1.0f * TimeWarp.deltaTime;
			}
			 */

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

			if (totalElectricCharge > minimum_sufficient_charge)
				hasPower = timestep_drain <= 0.0 || part.RequestResource(MAIN_POWER_NAME, timestep_drain) >= (timestep_drain * 0.99);
			else
				hasPower = false;

			if (!hasPower && totalReservePower > minimum_sufficient_charge)
			{
				//If main power is insufficient, drain reserve power for manager
				double manager_timestep_drain = manager_drain * TimeWarp.deltaTime;
				hasReservePower = manager_drain <= 0.0
					|| part.RequestResource(RESERVE_POWER_NAME, manager_timestep_drain) >= (manager_timestep_drain * 0.99);
			}
			else
				hasReservePower = totalReservePower > minimum_sufficient_charge;

		}

		//Turning

		private void flightControl(FlightCtrlState state)
		{
			if (subsystemPowered(Subsystem.FLIGHT_COMPUTER))
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

		//Reserve Power

		private void transferReserveToMain(double amount)
		{
			Debug.Log("main: " + totalElectricCharge);
			Debug.Log("reserve: " + totalReservePower);
			if (amount > totalReservePower)
				amount = totalReservePower;

			if (amount > totalElectricChargeCapacity - totalElectricCharge)
				amount = (totalElectricChargeCapacity - totalElectricCharge);

			double received = part.RequestResource(RESERVE_POWER_NAME, amount);
			Debug.Log("received: " + received);

			int transfer_attempts = 0;
			while (received > 0.0 && transfer_attempts < MAX_TRANSFER_ATTEMPTS)
			{
				received += part.RequestResource(MAIN_POWER_NAME, -received);
				transfer_attempts++;
			}

			Debug.Log("generated: " + (amount-received).ToString());
		}

		private void transferMainToReserve(double amount)
		{
			if (amount > totalElectricCharge)
				amount = totalElectricCharge;

			if (amount > totalReservePowerCapacity - totalReservePower)
				amount = (totalReservePowerCapacity - totalReservePower);

			double received = part.RequestResource(MAIN_POWER_NAME, amount);
			Debug.Log("received: " + received);

			int transfer_attempts = 0;
			while (received > 0.0 && transfer_attempts < MAX_TRANSFER_ATTEMPTS)
			{
				received += part.RequestResource(RESERVE_POWER_NAME, -received);
				transfer_attempts++;
			}

			Debug.Log("generated: " + (amount - received).ToString());
		}

		//GUI

		private void drawGUI()
		{
			if (partIsActive && isPrimaryPart)
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
			sectionGUIEnabledSubsystem = GUILayout.Toggle(sectionGUIEnabledSubsystem, "Subsystem", GUI.skin.button);
			sectionGUIEnabledReserve = GUILayout.Toggle(sectionGUIEnabledReserve, "Reserve", GUI.skin.button);
			GUILayout.EndHorizontal();

			//Manager status+drain
			if (!timewarpIsValid)
				GUILayout.Label("Auto-Hibernation", statusStyle);
			else
			{
				GUILayout.BeginHorizontal();
				managerEnabled = GUILayout.Toggle(managerEnabled, "Manager", GUI.skin.button, subsystemButtonOptions);
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
					else if (managerIsActive)
						GUILayout.Label("Running on Reserve Power!", warningStyle);
				}
				else
					GUILayout.Label("Manager Disabled", warningStyle);
			}
			else
				GUILayout.Label("Insufficient Power", warningStyle);

			//Subsystems
			if (sectionGUIEnabledSubsystem)
			{
				GUILayout.Label("Subsystems", sectionTitleStyle);
				foreach (Subsystem subsystem in Enum.GetValues(typeof(Subsystem)))
				{
					GUILayout.BeginHorizontal();
					subsystemButton(subsystem);
					subsystemConsumptionLabel(subsystem);
					GUILayout.EndHorizontal();
				}
			}

			//Reserve
			if (sectionGUIEnabledReserve)
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
				if (GUILayout.Button("Transfer Reserve to Main"))
					transferReserveToMain(totalReservePowerCapacity * RESERVE_TRANSFER_INCREMENT_FACTOR);
				if (GUILayout.Button("Transfer Main to Reserve"))
					transferMainToReserve(totalReservePowerCapacity * RESERVE_TRANSFER_INCREMENT_FACTOR);
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
