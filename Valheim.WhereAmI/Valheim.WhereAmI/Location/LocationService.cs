using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valheim.WhereAmI.Startup;

namespace Valheim.WhereAmI.Location
{
    public static class LocationService
    {
        public static bool IsMultiplayer = false;

        static LocationService()
        {
            StateResetter.Subscribe(() =>
            {
                IsMultiplayer = false;
            });
        }

        public static string GetLocation(Vector3 pos)
        {
            string locationName = "---";

            if (IsMultiplayer)
            {
                locationName = MultiplayerLocationService.FindLocation(pos)?.LocationName ?? "---";
            }
            else
            {
                if (ZoneSystem.instance.m_locationInstances.TryGetValue(ZoneSystem.instance.GetZone(pos), out ZoneSystem.LocationInstance locationInstance))
                {
                    locationName = locationInstance.m_location?.m_prefabName ?? "---";
                }
            }

            return locationName;
        }
    }
}
