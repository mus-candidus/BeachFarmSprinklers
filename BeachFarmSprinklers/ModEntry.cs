using Microsoft.Xna.Framework;

using HarmonyLib;

using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;


namespace BeachFarmSprinklers {
    public class ModEntry : Mod {
        public override void Entry(IModHelper helper) {
            Harmony harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.ApplySprinkler)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Before_Object_ApplySprinkler))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Before_Object_placementAction))
            );
        }

        /// <summary>
        /// Patch method to disable the NoSprinklers check.
        /// </summary>
        private static bool Before_Object_ApplySprinkler(SObject __instance, GameLocation location, Vector2 tile) {
            // Check terrain state and water if possible.
            if (location.terrainFeatures.ContainsKey(tile)
             && location.terrainFeatures[tile] is HoeDirt hoeDirt
             && hoeDirt.state.Value != 2) {
                hoeDirt.state.Value = 1;
            }

            // Disable original method.
            return false;
        }

        /// <summary>
        /// Patch method to disable the NoSprinklers check.
        /// </summary>
        private static bool Before_Object_placementAction(SObject __instance, GameLocation location, int x, int y, Farmer who, ref bool __result) {
            // Enable original method if object to place is not a sprinkler.
            if (!__instance.IsSprinkler()) {
                return true;
            }

            __instance.setHealth(10);
            __instance.owner.Value = (who != null)
                                   ? who.UniqueMultiplayerID
                                   : Game1.player.UniqueMultiplayerID;

            // Place sprinkler.
            if (!__instance.performDropDownAction(who)) {
                Vector2 placementTile = new Vector2(x / 64, y / 64);

                SObject item = __instance.getOne() as SObject;
                item.shakeTimer = 50;
                item.TileLocation = placementTile;
                item.performDropDownAction(who);

                location.objects.Add(placementTile, item);
            }
            
            location.playSound("woodyStep");

            // Set return value.
            __result = true;

            // Disable original method.
            return false;
        }
    }
}
