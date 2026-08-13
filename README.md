# ModNameTooltip

Yes this is named after the minecraft mod and does the same thing.


## Providing Translated Mod Names

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
