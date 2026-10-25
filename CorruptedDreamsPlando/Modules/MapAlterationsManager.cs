using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CorruptedDreamsPlando.MapAlterations;
using ItemChanger.Modules;

namespace CorruptedDreamsPlando.Modules
{
    public class MapAlterationsManager : ItemChanger.Modules.Module
    {
        private List<IMapAlteration> alterations = new();

        public override void Initialize()
        {
            if (alterations.Count == 0)
            {
                alterations.Add(new AbridgedColo());
                alterations.Add(new AMoundSoulTotem());
                alterations.Add(new CorruptedDreamShields());
                alterations.Add(new EnemyDisabler());
                alterations.Add(new FuckRightSideShrogo());
                alterations.Add(new LoopTransitions());
                alterations.Add(new LurienElevatorOutOfService());
                alterations.Add(new NoDreamPlats());
                alterations.Add(new UpDirectionSign());
                alterations.Add(new VerticalClimbs());
                alterations.Add(new UnkillableMyla());
                alterations.Add(new GpzAlwaysAvailable());
            }

            foreach (var alter in alterations)
            {
                alter.Initialize();
            }
        }

        public override void Unload()
        {
            foreach (var alter in alterations)
            {
                alter.Unload();
            }
        }
    }
}
