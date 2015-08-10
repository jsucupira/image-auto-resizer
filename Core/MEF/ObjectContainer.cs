using System.ComponentModel.Composition.Hosting;

namespace Core.MEF
{
    public class ObjectContainer
    {
        private ObjectContainer()
        {  }

        private static CompositionContainer _container;
        public static CompositionContainer Container { get { return _container; } }

        public static void SetContainer(CompositionContainer container)
        {
            if (_container == null)
            {
                lock (_syncRoot)
                {
                    if (_container == null)
                        _container = container;
                }
            }
        }

        private static readonly object _syncRoot = new object();
    }
}
