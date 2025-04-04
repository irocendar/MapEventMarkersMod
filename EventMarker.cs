﻿using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.WorldMaps;

namespace MapEventMarkersMod
{
    public class EventMarker
    {
       
        public Vector2 MapPosition;
        public GameLocation Location;
        public string Event;
        
        public EventMarker() {}

        public EventMarker(GameLocation location, string @event=null)
        {
            Location = location;
            Event = @event;
            
            var positionData = WorldMapManager.GetPositionData(location, new Point(0, 0));
            if (positionData.HasValue)
            {
                MapPosition = positionData.Value.GetMapPixelPosition();
            }
        }

        public string GetHoverText(MapPage mapPage, Vector2 topLeft)
        {
            string hoverText = "";

            foreach (ClickableComponent point in mapPage.points.Values)
                if (point.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)))
                    hoverText = point.label;

            return hoverText;
        }
    }
}