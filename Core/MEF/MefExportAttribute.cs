using System;
using System.ComponentModel.Composition;

namespace Core.MEF
{
    [AttributeUsage(AttributeTargets.Class), MetadataAttribute]
    public class MefExportAttribute : ExportAttribute, INameMetaData
    {
        public MefExportAttribute(Type contractType, string name): base(contractType)
        {
            Name = name.ToString();
        }

        public string Name { get; private set; }
    }
}
