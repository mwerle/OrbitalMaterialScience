/*
    This file was orgininally part of Station Science by ethernet http://forum.kerbalspaceprogram.com/threads/54774-0-23-5-Station-Science-(fourth-alpha-low-tech-docking-port-experiment-pod-models.

    Station Science is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Station Science is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Station Science.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace NE_Science
{
    class ScienceHelper
    {
        public static ExperimentSituations getScienceSituation(Vessel vessel)
        {
            CelestialBody body = vessel.mainBody;
            return getScienceSituation(vessel.altitude, vessel.situation, body);
        }

        public static ExperimentSituations getScienceSituation(double altitude, Vessel.Situations situation, CelestialBody body)
        {
            CelestialBodyScienceParams pars = body.scienceValues;
            if (situation == Vessel.Situations.LANDED || situation == Vessel.Situations.PRELAUNCH)
                return ExperimentSituations.SrfLanded;
            else if (situation == Vessel.Situations.SPLASHED)
                return ExperimentSituations.SrfSplashed;
            else if (body.atmosphere && altitude <= pars.flyingAltitudeThreshold)
                return ExperimentSituations.FlyingLow;
            /* MKW - this calculation change needs testing! */
            else if (body.atmosphere && altitude <= body.atmosphereDepth)
                return ExperimentSituations.FlyingHigh;
            else if (altitude <= pars.spaceAltitudeThreshold)
                return ExperimentSituations.InSpaceLow;
            else
                return ExperimentSituations.InSpaceHigh;
        }

        public static float getScienceMultiplier(Vessel vessel)
        {
            CelestialBody body = vessel.mainBody;
            /*print("");
            print("Altitude: " + vessel.altitude);
            print("Landed Value: " + pars.LandedDataValue);
            print("Splashed Value: " + pars.SplashedDataValue);
            print("Fly Low: " + pars.FlyingLowDataValue + " at " + pars.flyingAltitudeThreshold);
            print("High Low: " + pars.FlyingHighDataValue + " at " + body.atmosphereDepth);
            print("Orbit Low: " + pars.InSpaceLowDataValue + " at " + pars.spaceAltitudeThreshold);
            print("Orbit Low: " + pars.InSpaceHighDataValue);*/
            return getScienceMultiplier(getScienceSituation(vessel), body);
        }

        public static float getScienceMultiplier(ExperimentSituations situation, CelestialBody body)
        {
            CelestialBodyScienceParams pars = body.scienceValues;
            switch (situation)
            {
                case ExperimentSituations.SrfLanded:
                  return pars.LandedDataValue;
                case ExperimentSituations.SrfSplashed:
                  return pars.SplashedDataValue;
                case ExperimentSituations.FlyingLow:
                  return pars.FlyingLowDataValue;
                case ExperimentSituations.FlyingHigh:
                  return pars.FlyingHighDataValue;
                case ExperimentSituations.InSpaceLow:
                  return pars.InSpaceLowDataValue;
                case ExperimentSituations.InSpaceHigh:
                  return pars.InSpaceHighDataValue;
            }
            return 1;
        }

        public static ScienceSubject getScienceSubject(string name, Vessel vessel)
        {
            if (name == "") return null;
            ScienceExperiment experiment = ResearchAndDevelopment.GetExperiment(name);
            if (experiment == null) return null;
            ExperimentSituations situation = getScienceSituation(vessel);
            CelestialBody body = vessel.mainBody;
            string biome = "";
            string displayBiome = "";
            if(vessel.LandedOrSplashed)
            {
                biome = vessel.landedAt;
                displayBiome = vessel.displaylandedAt;
            }
            ScienceSubject subject = ResearchAndDevelopment.GetExperimentSubject(experiment, situation, body, biome, displayBiome);
            return subject;
        }
    }
}
