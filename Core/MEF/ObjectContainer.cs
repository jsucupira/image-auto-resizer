using System.ComponentModel.Composition.Hosting;

namespace Core.MEF
{
    public class ObjectContainer
    {
        private ObjectContainer()
        {  }

        public static CompositionContainer Container { get; private set; }

        public static void SetContainer(CompositionContainer container)
        {
            if (Container == null)
            {
                lock (_syncRoot)
                {
                    if (Container == null)
                        Container = container;
                }
            }
        }

        public static T Resolve<T>()
        {
            return Container.ResolveExportedValue<T>();
        }

        private static readonly object _syncRoot = new object();
    }
}
