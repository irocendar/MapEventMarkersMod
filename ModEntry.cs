using LightRadiusMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.WorldMaps;
using Object = StardewValley.Object;

namespace MapEventMarkersMod
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        
        /*********
         ** Properties
         *********/
        /// <summary>The mod configuration from the player.</summary>
        private ModConfig Config;
        
        /*********
         ** Public methods
         *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            
        }

        /*********
         ** Private methods
         *********/
        
        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (Game1.player.currentLocation.InIslandContext())
                return;

            if (!Config.Enabled)
                return;
            
            foreach (GameLocation gameLocation in this.PendingEvents())
            {
                DrawMarker(gameLocation);
            }
        }
        
        private List<GameLocation> PendingEvents()
        {
            List<GameLocation> eventLocations = new List<GameLocation>();
            foreach (GameLocation loc in Game1.locations)
            {
                Dictionary<string, string> locEvents;
                string eventAssetName;
                try
                {
                    loc.TryGetLocationEvents(out eventAssetName, out locEvents);
                }
                catch (Exception)
                {
                    continue;
                }
                foreach (string @event in locEvents.Keys)
                {
                    List<string> array = Event.SplitPreconditions(@event).ToList();
                    if (!int.TryParse(array[0], out _))
                        continue;
                    List<string> filtered = new List<string>();
                    List<char> ignored = new List<char>() { 'L', 'B', 'a', 'r'  };
                    bool skip = false;
                    foreach (string precondition in array)
                        if (precondition.Length > 0 && !ignored.Contains(precondition[0]))
                            filtered.Add(precondition);
                        else if (precondition[0] == 'r')
                            skip = true;
                        else if (precondition[0] == 'p')
                        {
                            string characterName = precondition.Split(' ')[1];
                            NPC character = loc.getCharacterFromName(characterName);
                            if (!loc.characters.Contains(character))
                                skip = true;
                        }
                    if (!skip && loc.checkEventPrecondition(String.Join('/', array)) != "-1")
                    {
                        eventLocations.Add(loc);
                        break;
                    }
                }
            }
            return eventLocations;
        }

        private void DrawMarker(GameLocation location)
        {
            Point playerTile = Game1.player.TilePoint;
            if (playerTile.X < 0 || playerTile.Y < 0)
            {
                playerTile = new Point(Math.Max(0, playerTile.X), Math.Max(0, playerTile.Y));
            }
            MapAreaPositionWithContext? mapPosition = WorldMapManager.GetPositionData(Game1.player.currentLocation, playerTile) ?? WorldMapManager.GetPositionData(Game1.getFarm(), Point.Zero);
            MapRegion? mapRegion = mapPosition?.Data.Region ?? WorldMapManager.GetMapRegions().First();
            Rectangle mapBounds = mapRegion.GetMapPixelBounds();

            Vector2 mapVec = Utility.getTopLeftPositionForCenteringOnScreen(mapBounds.Width * 4, mapBounds.Height * 4);

            SpriteBatch spriteBatch = Game1.spriteBatch;

            int mapTabIndex = Constants.TargetPlatform == GamePlatform.Android ? 4 : GameMenu.mapTab;

            if (Game1.activeClickableMenu is not GameMenu gameMenu || gameMenu.currentTab != mapTabIndex)
                return;
            
            EventMarker eventMarker = new EventMarker(location);

            var childMenu = gameMenu.GetChildMenu();
            
            if (childMenu is not null)
                return;

            Vector2 position = eventMarker.MapPosition + mapVec;

            if (location.Name == "BoatTunnel")
                return;
            
            spriteBatch.Draw(Game1.mouseCursors2, 
                position, 
                new Rectangle(114, 53, 6, 10), 
                Color.White, 
                0f, 
                Vector2.Zero, 
                2.5f, 
                SpriteEffects.None, 
                0.5f);
            
            if (childMenu is not null)
                childMenu.drawMouse(spriteBatch);
            else
                gameMenu.drawMouse(spriteBatch);
        }
        
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;
            
            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
            );
            
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Enabled",
                tooltip: () => "Untick to completely disable the mod.",
                getValue: () => this.Config.Enabled,
                setValue: value => this.Config.Enabled = value
            );
        }
    }
}