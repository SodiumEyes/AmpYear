# AmpYear
A KSP plugin that allows better management of electricity and adds ION RCS & Pulsed Plasma Thruster RCS.
Fork of original AmpYear by SodiumEyes until he returns or gives permission to take over the Mod. 
Requires Module Manager by Sarbian. Link available in the KSP forum Post.

Notes for Operation:
The manager function must be switched on (via the menu) for a small Electrical cost to manage power consumption across the vehicle. You cannot activate autopilot/SAS, subysstems, luxury items unless the Manager is active.
Gives indicators of Current Battery capacities, Electric Charge usage and generation.
Reserve Batteries : Used to store, well reserve Electric Charge, which will be used when main Charge runs out. You can transfer charge from reserve to main batteries. Parts that produce electric charge will fill Main electric charge batteries first and then reserve batteries.
In the Editor you can can indicatications of total power production and consumption. You can also get a full list of all power production and consumption parts.

ION RCS (linear only) - Add them to your vehicle, but don't forget to add Xenon Gas tanks and ensure you are generating enough electrical charge for them to operate.

PPT RCS (linear only) - Add them to your vehicle, they include a portion of Teflon (check google/wiki for how they function in real life) and ensure you are generating enough electrical charge for them to operate.
Reserve Batteries - Come in various sizes. Command pods and probes come with 50 of Reserve power by default. Adding Batteries gives you more.

Cabin Craziness - attribute attached to parts that have crew capacity. Increases over time and is influenced by crew capacity and space. When percentages are reached random events will occur. 
Has two levels - Minor and Major (configurable via settings menu on Spacecenter screen).
Once these levels are reached there are random changes that the crew may:-
Disable Autopilot functions for a period of time (cumulative).
Throw out (dump) some resources.
Throw out (dump) some science experiments.
A crew member may go randomly decide to go on EVA.

Turn Booster - When active uses Electric Charge to boost reaction wheels by 25%.
Once the Manager is active via the GUI you can switch Subsystems, Luxury and Reserve power areas of the GUI on and off.
Read-outs in the GUI give you Power production, capacity, stored, and used. All colour coded.

Luxury Items : Reduce CabinCraziness. The heater/Cooler changes crewable parts internal temperature, you can view the Temperature via the Right-click menu of each part. Crewable parts internal temperatures slowly change towards the ambient external temperature over time. Use of the heater and cooler will control the internal climate based on set values (via the settings menu). The music and Massage chairs - reduce the Cabin Fever percentage of each crewable part (can be seen via the part right-click menu). Warnings are given when Cabin Fever reaches certain percentages - and random events may occur.

Changes from Original Version include: Removed original AmpYear Part - no longer requires special part to function. 
Instead you just need a command pod or probe core. 
Removes the original AmpYear ASAS, not really required any more as ASAS modules available in stock. (see further comments below). 
Removed the 'copied' RemoteTech flight computer. If you want a flight computer install RemoteTech. 
Removed obsolete functions around SAS and ASAS electrical usage. 
Amended the PowerTurn function :- Powerturn function remains whilst ASAS was removed. 
Base SAS as per .90 is supported. Powerturn provides additional ability to use Electrical Power to Power-Assist ReactionWheels and SAS (both seperate parts and where the modules are used in Pods and Probes). 
Added ModuleManager dependency to add default small amount of ReservePower to command pods and probe cores. 
Additional Reserve Power Batteries can be added to vessels.
Functions using stock toolbar or Blizzy's Toolbar (can be changed via Settings menu from the Spacecentre).
Added Config Settings to SpaceCentre to change various settings. 
Corrected calculations for Electricity Consumption, Generation and GUI improvements. 
In Editor (VAB) you can see Power Production and Consumption parts and totals. - Still somewhat BUGGED and pretty rough. Don't know if I want to include this on-going as there are other mods that do this a lot better. 
Includes power calculations if you have the following Mods installed (but not required for mod to function): 
Near Future Electrical KAS RemoteTech KAS ScanSat Telemachus TAC LS AntennaRange 

Planned/Ideas: 
Complete testing and bug fixes. 
Review Turn Booster calculations.
Revision of Luxury components and the Cabin Fever attribute - Not sure where this is going, would prefer to integrate to other such mods. but can't find anything suitable at this time. Extend Cabin Fever calculations to be relative to other factors such as distance from Kerbin.
Improve subsystems and other mod integration, such as Interstellar, Near Future Solar, Karbonite mods, AviationLights, Infernal robotics. 
