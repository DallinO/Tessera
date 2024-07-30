namespace Tessera.Models.Chapter
{
    public class StatusList
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
}
