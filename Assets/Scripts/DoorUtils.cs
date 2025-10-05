using UnityEngine;

public static class DoorUtils
{
    /// Opens every door on the current map. 
    /// Uses toggle only if the door is currently closed.
    public static void OpenAllDoors()
    {
        if (Main.map == null || Main.map.data == null) return;

        int w = Main.map.data.GetLength(0);
        int h = Main.map.data.GetLength(1);

        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
        {
            var cell = Main.map.data[x, y];
            if (cell == null || !cell.hasFeature()) continue;

            var feat = cell.getFeature();
            if (feat == null || feat.type != "door") continue;

            // 'active' == door open in your project; 'blocking' mirrors that.
            if (!feat.active) feat.toggle();         // animate state change if closed
            feat.blocking = false;                   // ensure passable
        }
        Debug.Log("[DoorUtils] Opened all doors.");
    }

    /// Closes every door (handy for testing).
    public static void CloseAllDoors()
    {
        if (Main.map == null || Main.map.data == null) return;

        int w = Main.map.data.GetLength(0);
        int h = Main.map.data.GetLength(1);

        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
        {
            var cell = Main.map.data[x, y];
            if (cell == null || !cell.hasFeature()) continue;

            var feat = cell.getFeature();
            if (feat == null || feat.type != "door") continue;

            if (feat.active) feat.toggle();          // animate state change if open
            feat.blocking = true;                    // ensure impassable
        }
        Debug.Log("[DoorUtils] Closed all doors.");
    }
}
