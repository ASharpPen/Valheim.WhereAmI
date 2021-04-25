using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valheim.WhereAmI.Startup;

namespace Valheim.WhereAmI.Location
{
    [HarmonyPatch(typeof(ZNet))]
    public static class MultiplayerLocationService
    {
		private static Dictionary<Vector2i, SimpleLocation> _simpleLocationsByZone { get; set; }

		private static bool HaveReceivedLocations = false;

		static MultiplayerLocationService()
		{
			StateResetter.Subscribe(() =>
			{
				HaveReceivedLocations = false;
			});
		}

		public static SimpleLocation FindLocation(Vector3 position)
		{
			if (_simpleLocationsByZone is not null)
			{
	 			var zoneId = ZoneSystem.instance.GetZone(position);

				if (_simpleLocationsByZone.TryGetValue(zoneId, out SimpleLocation location))
				{
					return location;
				}
			}

			return null;
		}

		[HarmonyPatch("OnNewConnection")]
		[HarmonyPostfix]
		private static void TransferLocationData(ZNet __instance, ZNetPeer peer)
		{
			if (ZNet.instance.IsServer())
			{
				Log.LogDebug("Registering server RPC for sending location data on request from client.");
				peer.m_rpc.Register(nameof(RPC_RequestLocationsWhereAmI), new ZRpc.RpcMethod.Method(RPC_RequestLocationsWhereAmI));
			}
			else
			{
				Log.LogDebug("Registering client RPC for receiving location data from server.");
				peer.m_rpc.Register<ZPackage>(nameof(RPC_ReceiveLocationsWhereAmI), new Action<ZRpc, ZPackage>(RPC_ReceiveLocationsWhereAmI));

				Log.LogDebug("Requesting location data from server.");
				peer.m_rpc.Invoke(nameof(RPC_RequestLocationsWhereAmI));
			}
		}

		private static void RPC_RequestLocationsWhereAmI(ZRpc rpc)
		{
			try
			{
				if (!ZNet.instance.IsServer())
				{
					Log.LogWarning("Non-server instance received request for location data. Ignoring request.");
					return;
				}

				Log.LogInfo($"Sending location data.");

				ZPackage package = new ZPackage();

				var locations = ZoneSystem.instance.m_locationInstances;

				if (locations is null)
				{
					Log.LogWarning("Unable to get locations from zonesystem to send to client.");
					return;
				}

				package.Write(SerializeLocationInfo(locations));

				Log.LogDebug("Sending locations package.");

				rpc.Invoke(nameof(RPC_ReceiveLocationsWhereAmI), new object[] { package });

				Log.LogDebug("Finished sending locations package.");
			}
			catch (Exception e)
			{
				Log.LogError("Unexpected error while attempting to create and send locations package from server to client.", e);
			}
		}

		private static void RPC_ReceiveLocationsWhereAmI(ZRpc rpc, ZPackage pkg)
		{
			Log.LogDebug("Received locations package.");
			try
			{
				if (HaveReceivedLocations)
				{
					Log.LogDebug("Already received locations previously. Skipping.");
					return;
				}

				var serialized = pkg.ReadByteArray();

				LoadLocationInfo(serialized);
				HaveReceivedLocations = true;

				Log.LogDebug("Successfully received locations package.");
			}
			catch (Exception e)
			{
				Log.LogError("Error while attempting to read received locations package.", e);
			}
		}

		private static void LoadLocationInfo(byte[] serialized)
		{
			using (MemoryStream memStream = new MemoryStream(serialized))
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				var responseObject = binaryFormatter.Deserialize(memStream);

				if (responseObject is List<SimpleLocationDTO> locations)
				{
#if DEBUG
					Log.LogDebug($"Deserialized {locations.Count} locations.");
#endif


					IEnumerable<SimpleLocation> simpleLocations = locations.Select(x => x.ToSimpleLocation());

					_simpleLocationsByZone = new Dictionary<Vector2i, SimpleLocation>();

#if DEBUG
					Log.LogDebug($"Assigning locations.");
#endif

					foreach (var location in simpleLocations)
					{
						_simpleLocationsByZone[location.ZonePosition] = location;
					}
				}
			}
		}

		private static byte[] SerializeLocationInfo(Dictionary<Vector2i, ZoneSystem.LocationInstance> locationInstances)
		{
#if DEBUG
			Log.LogDebug($"Serializing {locationInstances.Count} location instances.");
#endif

			List<SimpleLocationDTO> simpleLocations = new List<SimpleLocationDTO>();

			foreach (var location in locationInstances)
			{
				simpleLocations.Add(new SimpleLocationDTO(location.Key, location.Value.m_position, location.Value.m_location.m_prefabName));
			}

			using (MemoryStream memStream = new MemoryStream())
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				binaryFormatter.Serialize(memStream, simpleLocations);

				byte[] serializedLocations = memStream.ToArray();

#if DEBUG
				Log.LogDebug($"Serialized {serializedLocations.Length} bytes of locations.");
#endif

				return serializedLocations;
			}
		}
	}
}
