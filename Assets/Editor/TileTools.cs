#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class TileTools
{
    // Top menu (Window bar)
    [MenuItem("PathQueue/Tiles/Convert Selected To/Tile (Normal)")]
    public static void ToTile() => ConvertSelectionTo<Tile>();
    [MenuItem("PathQueue/Tiles/Convert Selected To/GoalTile")]
    public static void ToGoal() => ConvertSelectionTo<GoalTile>();
    [MenuItem("PathQueue/Tiles/Convert Selected To/WallTile")]
    public static void ToWall() => ConvertSelectionTo<WallTile>();
    [MenuItem("PathQueue/Tiles/Convert Selected To/StickyTile")]
    public static void ToSticky() => ConvertSelectionTo<StickyTile>();
    [MenuItem("PathQueue/Tiles/Convert Selected To/RisingTile")]
    public static void ToRising() => ConvertSelectionTo<RisingTile>();
    [MenuItem("PathQueue/Tiles/Convert Selected To/IceTile")]
    public static void ToIce() => ConvertSelectionTo<IceTile>();
    [MenuItem("PathQueue/Tiles/Convert Selected To/FireTile")]
    public static void ToFire() => ConvertSelectionTo<FireTile>();
    [MenuItem("PathQueue/Tiles/Convert Selected To/GlassTile")]
    public static void ToGlass() => ConvertSelectionTo<GlassTile>();
    [MenuItem("PathQueue/Tiles/Convert Selected To/SpringTile")]
    public static void ToSpring() => ConvertSelectionTo<SpringTile>();

    // Hierarchy right-click menu
    [MenuItem("GameObject/PathQueue/Tiles/Convert Selected To/Tile (Normal)", false, 10)]
    public static void GO_ToTile(MenuCommand cmd) => ToTile();
    [MenuItem("GameObject/PathQueue/Convert Selected To/GoalTile", false, 10)]
    public static void GO_ToGoal(MenuCommand cmd) => ToGoal();
    [MenuItem("GameObject/PathQueue/Convert Selected To/WallTile", false, 10)]
    public static void GO_ToWall(MenuCommand cmd) => ToWall();
    [MenuItem("GameObject/PathQueue/Convert Selected To/StickyTile", false, 10)]
    public static void GO_ToSticky(MenuCommand cmd) => ToSticky();
    [MenuItem("GameObject/PathQueue/Convert Selected To/RisingTile", false, 10)]
    public static void GO_ToRising(MenuCommand cmd) => ToRising();
    [MenuItem("GameObject/PathQueue/Convert Selected To/IceTile", false, 10)]
    public static void GO_ToIce(MenuCommand cmd) => ToIce();
    [MenuItem("GameObject/PathQueue/Convert Selected To/FireTile", false, 10)]
    public static void GO_ToFire(MenuCommand cmd) => ToFire();
    [MenuItem("GameObject/PathQueue/Convert Selected To/GlassTile", false, 10)]
    public static void GO_ToGlass(MenuCommand cmd) => ToGlass();
    [MenuItem("GameObject/PathQueue/Convert Selected To/SpringTile", false, 10)]
    public static void GO_ToSpring(MenuCommand cmd) => ToSpring();

    // Component context menu on Tile (right-click on the component)
    [MenuItem("CONTEXT/Tile/Convert To/Tile (Normal)")]
    public static void CTX_ToTile(MenuCommand cmd) => ConvertGameObject(((Tile)cmd.context).gameObject, typeof(Tile));
    [MenuItem("CONTEXT/Tile/Convert To/GoalTile")]
    public static void CTX_ToGoal(MenuCommand cmd) => ConvertGameObject(((Tile)cmd.context).gameObject, typeof(GoalTile));
    [MenuItem("CONTEXT/Tile/Convert To/WallTile")]
    public static void CTX_ToWall(MenuCommand cmd) => ConvertGameObject(((Tile)cmd.context).gameObject, typeof(WallTile));
    [MenuItem("CONTEXT/Tile/Convert To/StickyTile")]
    public static void CTX_ToSticky(MenuCommand cmd) => ConvertGameObject(((Tile)cmd.context).gameObject, typeof(StickyTile));
    [MenuItem("CONTEXT/Tile/Convert To/RisingTile")]
    public static void CTX_ToRising(MenuCommand cmd) => ConvertGameObject(((Tile)cmd.context).gameObject, typeof(RisingTile));
    [MenuItem("CONTEXT/Tile/Convert To/IceTile")]
    public static void CTX_ToIce(MenuCommand cmd) => ConvertGameObject(((Tile)cmd.context).gameObject, typeof(IceTile));
    [MenuItem("CONTEXT/Tile/Convert To/FireTile")]
    public static void CTX_ToFire(MenuCommand cmd) => ConvertGameObject(((Tile)cmd.context).gameObject, typeof(FireTile));
    [MenuItem("CONTEXT/Tile/Convert To/GlassTile")]
    public static void CTX_ToGlass(MenuCommand cmd) => ConvertGameObject(((Tile)cmd.context).gameObject, typeof(GlassTile));
    [MenuItem("CONTEXT/Tile/Convert To/SpringTile")]
    public static void CTX_ToSpring(MenuCommand cmd) => ConvertGameObject(((Tile)cmd.context).gameObject, typeof(SpringTile));

    // Core helpers
    static void ConvertSelectionTo<T>() where T : Tile
    {
        foreach (var go in Selection.gameObjects)
        {
            ConvertGameObject(go, typeof(T));
        }
    }

    static void ConvertGameObject(GameObject go, System.Type targetType)
    {
        if (go == null) return;

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Convert Tile");

        foreach (var t in go.GetComponents<Tile>())
        {
            if (t.GetType() != targetType)
            {
                Undo.DestroyObjectImmediate(t);
            }
        }

        if (!go.GetComponent(targetType))
        {
            Undo.AddComponent(go, targetType);
        }

        EditorUtility.SetDirty(go);
        Undo.CollapseUndoOperations(group);
    }
}
#endif
