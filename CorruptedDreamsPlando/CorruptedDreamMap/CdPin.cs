using System.Collections.Generic;
using MapChanger;
using MapChanger.Defs;
using MapChanger.MonoBehaviours;
using UnityEngine;

namespace CorruptedDreamsPlando;

internal class VmmPin : BorderedBackgroundPin
{
    private const float TINY_SCALE = 0.47f;
    private const float SMALL_SCALE = 0.56f;
    private const float MEDIUM_SCALE = 0.67f;
    private const float LARGE_SCALE = 0.8f;
    private const float HUGE_SCALE = 0.96f;
    private protected const float NO_BORDER_MULTIPLIER = 1.3f;

    internal MapRoomPosition Mlp { get; private set; }

    internal void Initialize(MapLocationDef mld, VmmPinGroup parent)
    {
        base.Initialize();

        ActiveModifiers.AddRange([CorrectMapOpen, ActiveByCurrentMode, LocationNotCleared]);

        parent.AddChild(this);
        Mlp = new MapRoomPosition(mld.MapLocations);
        MapPosition = Mlp;
        Sprite = GetSpriteFromPoolGroup(mld.Name);
    }

    public override void OnMainUpdate(bool active)
    {
        if (!active)
            return;

        UpdatePinSize();
        UpdatePinShape();
    }

    internal void UpdatePinSize()
    {
        Size = SMALL_SCALE * NO_BORDER_MULTIPLIER;
    }

    internal void UpdatePinShape()
    {
        BackgroundSprite = null;
        BorderSprite = null;
    }

    private bool CorrectMapOpen()
    {
        return States.WorldMapOpen || (States.QuickMapOpen && States.CurrentMapZone == Mlp.MapZone);
    }

    private bool ActiveByCurrentMode()
    {
        return true;
    }

    private bool LocationNotCleared()
    {
        return !Tracker.HasClearedLocation(name);
    }

    private static Sprite GetSpriteFromPoolGroup(string mldName)
    {
        var spriteName = mldName switch
        {
            "LostKinPin" => "LKPin",
            _ => "Unknown",
        };

        return new EmbeddedSprite(spriteName).Value;
    }
}