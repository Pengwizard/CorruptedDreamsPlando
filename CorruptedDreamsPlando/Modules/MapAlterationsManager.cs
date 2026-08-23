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

        private IEnumerable<Type> GetAllTypesThatImplementInterface<T>()
        {
            return System.Reflection.Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => typeof(T).IsAssignableFrom(type) && !type.IsInterface);
        }

        public void Initialize()
        {
            if (alterations.Count == 0)
            {
                var alterationsList = GetAllTypesThatImplementInterface<IMapAlteration>();
                foreach (var alter in alterationsList)
                {
                    var newAlter = (IMapAlteration)Activator.CreateInstance(alter);
                    alterations.Add(newAlter);
                }
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
