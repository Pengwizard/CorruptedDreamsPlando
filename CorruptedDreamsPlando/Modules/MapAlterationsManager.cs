using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CorruptedDreamsPlando.MapAlterations;

namespace CorruptedDreamsPlando.Modules
{
    public class MapAlterationsManager
    {
        private List<IMapAlteration> alterations = new();

        public void Initialize()
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
            }

            foreach (var alter in alterations)
            {
                alter.Initialize();
            }
        }

        public void Unload()
        {
            foreach (var alter in alterations)
            {
                alter.Unload();
            }
        }
    }
}
