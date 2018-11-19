using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vulpine.SpirV
{
    public class EntryPoint
    {
        public ExecutionModel ExecutionModel;
        public int ID;
        public string Name;
        public List<int> InterfaceIDs;

        public EntryPoint(ExecutionModel executionModel, int id, string name, List<int> interfaceIDs = null)
        {
            ExecutionModel = executionModel;
            ID = id;
            Name = name;
            if (interfaceIDs == null)
                InterfaceIDs = new List<int>();
            else
                InterfaceIDs = interfaceIDs;
        }

        public override string ToString()
        {
            return $"[EntryPoint  { ID } \"{ Name }\"  Interface IDs: { string.Join(", ", InterfaceIDs) }]";
        }
    }
}
