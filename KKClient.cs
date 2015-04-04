/**
 * KKClient.cs
 *
 * AmpYear power management.
 * The original code and concept of AmpYear rights go to SodiumEyes on the Kerbal Space Program Forums, which was covered by GNU License GPL (no version stated).
 * As such this code continues to be covered by GNU GPL license.
 *
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 *
 *  This file is part of AmpYear.
 *
 *  AmpYear is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  AmpYear is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with AmpYear.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

using System;
using System.Linq;
using System.Reflection;

namespace AY
{
    internal class KKClient
    {
        private static KabinKraziness.Ikkaddon _KK = null;
        private static bool? _KKAvailable = null;

        public static KabinKraziness.Ikkaddon GetKK()
        {
            Type KKAddonType = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetExportedTypes()).SingleOrDefault(t => t.FullName == "KabinKraziness.KKController");
            if (KKAddonType != null)
            {
                object realKKAddon = KKAddonType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
                _KK = (KabinKraziness.Ikkaddon)realKKAddon;
            }
            return _KK;
        }

        public static bool KKInstalled
        {
            get
            {
                _KKAvailable = GetKK() != null;                
                return (bool)_KKAvailable;
            }
        }
    }
}