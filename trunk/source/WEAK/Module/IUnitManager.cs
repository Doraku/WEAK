using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEAK.Module
{
    public interface IUnitManager
    {
        bool Add<T>() where T : IUnit;

        void Load(bool doInParallel);

        IEnumerable<Type> LoadedUnits { get; }

        event Action<Type> UnitLoaded;

        event Action<Type> UnitLoadError;
    }
}
