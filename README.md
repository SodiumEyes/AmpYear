# AmpYear
A KSP plugin that allows better management of electricity and adds ION RCS & Pulsed Plasma Thruster RCS.
Fork of original AmpYear by SodiumEyes until he returns or gives permission to take over the Mod. 

    Dependency: Requires Module Manager (not in this distribution).
    Optional: Blizzy's Tool Bar.
    To install unzip to gamedata folder where you have KSP installed. (remove any previous versions from Gamedata/AmpYear first).


Notes for Operation:
Manager

    The manager function is attached to all probes and command pods through the power of Module Manager - thanks Sarbian.
    The AmpYear menu can be switched on/off via integration with Stock or Toolbar by blizzy.
    Emergency Shutdown procedure - when All power is critical will activate solar panels (if attached and not in atmosphere) and shutdown non-essential parts using electricity.
    Automatically Saves all settings and window positions per vehicle to persistent save file.
    Gives %age indicators of Current Battery capacities, Electric Charge usage and generation. You can transfer charge from reserve to main batteries. Parts that produce electric charge will fill Main batteries first and then reserve batteries.
    In the Editor you can get indications of total power production and consumption of your vessel. You can also get a full list of all power production and consumption parts.
	Functions using stock toolbar or Blizzy's Toolbar (can be changed via Settings menu from the Space center).
	Includes correct power calculations if you have the following Mods installed (but not required for mod to function): Regolith, Near Future Electrical/Solar, KAS, RemoteTech, ScanSat, Telemachus, TAC LS, AntennaRange.

ION RCS

    Thrusters that use Xenon-gas and electricity for incredibly efficient RCS. Some limitations:
    Has half the Isp of normal Ion engines.
    One quarter of the thrust of normal monopropellant RCS thrusters.
    Only linear thrusters are available.
    Integrated to TechTree at ionPropulsion.

Pulsed Plasma Thrusters RCS

    Each thruster contains a fixed amount of Teflon and uses this along with Electric Charge for incredibly efficient propulsion for small craft or maneuvering. Some limitations:
    Has roughly quarter the ISP of normal RCS thrusters.
    One eighth of the thrust of normal monopropellant RCS thrusters.
    Only linear thrusters are available.
    Integrated to TechTree at advFlightControl.
    See (http://en.wikipedia.org/wiki/Pulsed_plasma_thruster).

TurnBooster

    Allows you to use electrical power to harness SAS/Reaction Wheel modules for additional turning power. Boosting them by 25%.

Reserve Power

    Attach Reserve Power batteries to your ship to save power for emergencies. Ideal for keeping your probes from dying.
    Power can be transferred to and from main and reserve power resources.
    Electrical power generators (solar panels, etc) will re-charge main power followed by reserve power when a threshold is reached.


Auto-Hibernate 

    All AmpYear functions will now continue to run during timewarp up to 7x at which time it will Auto-Hibernate. A Warning Pop-up will appear and stop warp if the ElectricCharge falls below a set percentage (configurable via the spacecenter window).

Show Crew

    Show Crew feature - with 0.90 - I added this to show the crew and their roles rather than having to switch to the map view all the time.



ChangeLog:

V0.13 Removed KabinKraziness - Will be released as a separate mod. Re-implmented Auto_hibernate at high (7x) time warp.
V0.12e Set the Default value for AmpYear Power Manager to ON
V0.12d Fixed Vessel switching bug (save settings correctly). Kraziness balancing. Removed Auto-Hibernate and added timewarp/low power warning pop-up. Updated to latest versions of other mods.
V0.12c Fixed duplicated batteries in distribution zip file. Completely delete the AmpYear folder in your gamedata directory and re-install.
V0.12b Tweaks to Cabin Craziness calculations. Correctly includes craziness increases based on how far from Kerbin you are. Tweaked base values in calculations. Merged the Heater/Cooler function into one Climate Control function.
V0.12a Loading fix and Gui Improvements Added distance from Kerbal in the Cabin Craziness Calculation.
V0.12 - Forked from SodiumEyes - Updated and Upgraded for KSP 0.90. Too many changes to mention. Refer to the Mod forum link for details.
