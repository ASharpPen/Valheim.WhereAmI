using HarmonyLib;
using SpawnThat.Utilities.Extensions;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Valheim.WhereAmI;

[HarmonyPatch(typeof(Hud))]
public class HudPatch
{
    private static GameObject HudGameObject;
    private static RectTransform HudTransform;

    private static string FontName = "AveriaSerifLibre-Bold";
    private static int FontSize = 12;

    public static Vector2 HudPosition = new Vector2(Screen.width / 10, (Screen.height / 4) * 3);
    public static Vector2 Offset = new Vector2(0, 0);

    public static float UpdateInterval = 0.1f;
    public static float UpdateTime = 0f;

    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void Instantiate(Hud __instance)
    {
        HudGameObject = new GameObject("WhereAmI_Hud");
        HudTransform = HudGameObject.AddComponent<RectTransform>();

        HudTransform.SetParent(__instance.m_rootObject.transform);
        HudTransform.localScale = Vector3.one;
        HudTransform.anchoredPosition = Vector2.zero;

        Text text = HudGameObject.AddComponent<Text>();
        text.font = GetFont(FontName, FontSize);

        HudTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 300);
        HudTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 200);
    }

    [HarmonyPatch("Update")]
    [HarmonyPrefix]
    static void OnUpdate(Hud __instance)
    {
        if (!Player.m_localPlayer || Player.m_localPlayer is null)
        {
            return;
        }

        UpdateTime += Time.deltaTime;
        if (UpdateTime < UpdateInterval)
        {
            return;
        }
        UpdateTime = 0;


        Vector3 hudPos = new Vector3(HudPosition.x, HudPosition.y, 0);


        if (HudGameObject != null)
        {
            HudGameObject.transform.position = hudPos + new Vector3(Offset.x, Offset.y, 0);

            var playerPos = Player.m_localPlayer.transform.position;

            var locationName = SpawnThat.World.Locations.LocationManager.GetLocation(playerPos);

            string roomName = "---";

            var roomData = SpawnThat.World.Dungeons.RoomManager.GetContainingRoom(playerPos);

            if (roomData is not null)
            {
                roomName = roomData.Name;
                roomName = roomName.Split(new[] { '(' }).First();
            }

            var zone = SpawnThat.World.Zone.ZoneManager.GetZone(playerPos.GetZoneId());

            Text text = HudGameObject.GetComponent<Text>();
            string hud =
                $"Position: {playerPos}\n" +
                $"Biome: {EnvMan.instance.GetBiome()}\n" +
                $"Location: {locationName?.LocationName ?? "---"}\n" +
                $"Room: {roomName}\n" +
                $"Zone ID: {playerPos.GetZoneId()}\n"
                ;



            // Additional Debugging:

            hud +=
                $"Zone Biome: {zone.Biome}\n" +
                $"Zone Calc Biome: {zone.GetBiome(playerPos)}\n" +
                $"Zone Corners: {zone.BiomeCorners.Join()}\n" +
                $"World Biome for player: {WorldGenerator.instance.GetBiome(playerPos)}\n" +
                $"World Biome for zone: {WorldGenerator.instance.GetBiome(zone.ZonePos)}\n"
                ;

            text.text = hud;
        }
    }

    private static Font GetFont(string fontName, int fontSize)
    {
        Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
        foreach (Font font in fonts)
        {
            if (font.name == fontName)
            {
                return font;
            }
        }
        return Font.CreateDynamicFontFromOSFont(fontName, fontSize);
    }
}
