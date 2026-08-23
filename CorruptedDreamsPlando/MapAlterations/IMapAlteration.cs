using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorruptedDreamsPlando.MapAlterations
{
    public interface IMapAlteration
    {
        public abstract void Initialize();

        public abstract void Unload();
    }
}
