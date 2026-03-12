using ItemChanger;
using MapChanger;
using MapChanger.Defs;
using MapChanger.MonoBehaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

namespace CorruptedDreamsPlando;

internal static class VmmPinManager
{
    private const float OFFSETZ_BASE = -1.4f;
    private const float OFFSETZ_RANGE = 0.4f;

    internal static Dictionary<string, VmmPin> Pins { get; private set; } = [];
    internal static VmmPinGroup PinGroup { get; private set; }

    internal static void MakePins(GameObject goMap)
    {
        CorruptedDreamsPlandoMod.Instance.LogDebug("Make Pins");
        PinGroup = new VmmPinGroup();

        var pinGroup = Utils.MakeMonoBehaviour<VmmPinGroup>(goMap, "Corrupted Dreams Pins");
        pinGroup.Initialize();

        PinGroup = pinGroup;

        var lkpMapLocation = new MapLocation()
        {
            MappedScene = SceneNames.Abyss_19,
            X = 0,
            Y = 0
        };

        var lostKinPin = new MapLocationDef()
        {
            Name = "LostKinPin",
            SceneName = SceneNames.Abyss_19,
            MapLocations = new MapLocation[] {lkpMapLocation}  
        };

        var mlds = new MapLocationDef[] { lostKinPin };

        foreach (var mld in mlds)
        {
            var pin = Utils.MakeMonoBehaviour<VmmPin>(goMap, mld.Name);
            pin.Initialize(mld, PinGroup);

            Pins[mld.Name] = pin;
        }

        // Stagger the Z offset of each pin
        IEnumerable<MapObject> pinsSorted = Pins
            .Values.OrderBy(mapObj => mapObj.transform.position.x)
            .ThenBy(mapObj => mapObj.transform.position.y);

        for (var i = 0; i < pinsSorted.Count(); i++)
        {
            var transform = pinsSorted.ElementAt(i).transform;
            transform.localPosition = new(
                transform.localPosition.x,
                transform.localPosition.y,
                OFFSETZ_BASE + ((float)i / Pins.Count() * OFFSETZ_RANGE)
            );
        }
    }

    internal static string GetPoolGroupCounter()
    {
        string text;

        IReadOnlyCollection<MapObject> pins = PinGroup.Children;

        if (IsPersistent())
        {
            text = "";
        }
        else
        {
            text = pins.Where(pin => Tracker.HasClearedLocation(pin.name)).Count().ToString() + " / ";
        }

        return text + pins.Count().ToString();
    }

    private static bool IsPersistent()
    {
        return false;
    }
}