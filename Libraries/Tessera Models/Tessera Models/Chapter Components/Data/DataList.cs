using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessera.Models.ChapterComponents;

namespace Tessera.Models.ChapterComponents.Data
{

    // Application Model
    public class DataList : LeafModel
    {
        public List<DataColumn> Columns { get; set; } = new List<DataColumn>();

        public void AddColumn(string name, Type dataType, object value = null)
        {
            Columns.Add(new DataColumn
            {
                Name = name,
                DataType = dataType,
                Value = value
            });
        }

        public void AddColumn<T>(string name, T value)
        {
            Columns.Add(new DataColumn
            {
                Name = name,
                DataType = typeof(T),
                Value = value
            });
        }
    }

    public class DataColumn
    {
        public string Name { get; set; }
        public Type DataType { get; set; }
        public object Value { get; set; }
    }
}
