using MapChanger;
using MapChanger.MonoBehaviours;

namespace CorruptedDreamsPlando;

public class VmmPinGroup : MapObject
{
    public override void Initialize()
    {
        base.Initialize();

        ActiveModifiers.Add(PoolSettingOn);

        MapObjectUpdater.Add(this);
    }

    private bool PoolSettingOn()
    {
        return CorruptedDreamsPlandoMod.LS.CollectedDreamerMap;
    }
}