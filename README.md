# Mod Name Tooltip

Yes this is named after the minecraft mod and does the same thing.

## Tooltip

Show which mod added a particular item.

This only applies to item tooltips.

## HUD

Show a small HUD menu that displays the subject name and mod name of a supported subject under your cursor.

This supports:
- NPCs
- Farm Animals
- Objects
- Crops/Trees/FruitTrees

## Configuration

- ``

## For Mod Authors

### C# API

This mod determines what mod adds a particular item by tracing the content pipeline, making it more accurate than simply deriving that from item id.

You can use the (provided API)[ModNameTooltip/IModNameAPI.cs] to fetch this info.

### Providing a Translated Mod Name

For content patcher, do an edit like this in your content.json
```
{
    "Action": "EditData",
    "Target": "mushymato.ModNameTooltip/ModNames",
    "Entries": {
        // need a "mod.name" key in your i18n
        "{{ModId}}": "{{i18n: mod.name}}"
    }
}
```

This asset is a `Dictionary<string, string>` so C# mods can edit it like this.

```
private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
{
    if (e.Name.IsEquivalentTo("mushymato.ModNameTooltip/ModNames"))
    {
        e.Edit(
            (asset) =>
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                data[ModManifest.UniqueID] = Helper.Translation.Get("mod-name");
            },
            AssetEditPriority.Default
        );
    }
}
```

### 
