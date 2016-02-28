/**
 * KKInterfaces.cs
 *
 * Kabin Kraziness.
 * (C) Copyright 2015, Jamie Leighton
 *
 * This software is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
 * creative commons license. You should have received a copy of the License along with KabinKraziness.
 *  See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode> for full details.
 *
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 *
 *  This file is part of KabinKraziness.
 *
 *  KabinKraziness is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 *
 */

namespace KabinKraziness
{
    public interface Ikkaddon
    {
        bool AutoPilotDisabled { get; set; }

        double AutoPilotDisTime { get; set; }

        double AutoPilotDisCounter { get; set; }

        bool FirstMajCrazyWarning { get; }

        bool FirstMinCrazyWarning { get; }

        double CLMT_BSE_DRN_FTR { get; set; }

        float CLMT_TGT_TMP { get; set; }

        double MSG_BSE_DRN_FTR { get; set; }

        double CRZ_BSE_DRN_FTR { get; set; }

        double CRZ_CTE_UNC_FTR { get; set; }

        double CRZ_CTE_RED_FTR { get; set; }

        double CRZ_RDO_RED_FTR { get; set; }

        double CRZ_MSG_RED_FTR { get; set; }

        double CRZ_MINOR_LMT { get; set; }

        double CRZ_MAJOR_LMT { get; set; }

        ScreenMessage AutoTimer { get; }
    }
}