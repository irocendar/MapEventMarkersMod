#nullable enable

using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu;


public interface IBetterGameMenuApi
{

    #region Menu Class Access

    /// <summary>
	/// The current page of the active screen's current Better Game Menu,
	/// if one is open, else <c>null</c>.
	/// </summary>
	IClickableMenu? ActivePage { get; }

    #endregion

}
