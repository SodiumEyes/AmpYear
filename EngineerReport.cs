using System.Collections.Generic;

namespace AY
{
    public class AYEngReport : PreFlightTests.DesignConcernBase
    {
        // Is the Test OK ?
        public override bool TestCondition()
        {
            this.Log_Debug("AYEngReport Test condition");
            if (AYController.totalPowerDrain > AYController.totalPowerProduced)
            {
                this.Log_Debug("AYEngReport Total Power Drain > total Power Produced");
                return false;
            }
            else
            {
                this.Log_Debug("AYEngReport Total Power Drain <= total Power Produced");
                return true;
            }
        }

        // List of affected parts
        public override List<Part> GetAffectedParts()
        {
            return AYController.Instance.crewablePartList;
        }

        // Title of the problem description
        public override string GetConcernTitle()
        {
            return "AmpYear";
        }

        // problem description
        public override string GetConcernDescription()
        {
            return "Your Vessel will consume more ElectricCharge than it can Produce.";
        }

        // how bad is the problem
        public override PreFlightTests.DesignConcernSeverity GetSeverity()
        {
            return PreFlightTests.DesignConcernSeverity.NOTICE;
        }

        // does it applies to Rocket, plane or both
        public override EditorFacilities GetEditorFacilities()
        {
            return EditorFacilities.ALL;
        }
    }
}