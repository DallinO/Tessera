using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.Models.Chapter
{
    public class ListDto : LeafDto
    {
        private readonly List<ColumnDto> _columns;
        private readonly List<RowDto> _rows;

        public ListDto()
        {
            _columns = new List<ColumnDto>();
            _rows = new List<RowDto>();
        }

        public void AddColumn(string name, Type dataType)
        {
            if (_columns.Any(c => c.Name == name))
            {
                throw new InvalidOperationException($"Column '{name}' already exists.");
            }

            _columns.Add(new ColumnDto(name, dataType));

            // Update existing rows to include new column with null value
            foreach (var row in _rows)
            {
                row[name] = null;
            }
        }

        public void RemoveColumn(string name)
        {
            var column = _columns.FirstOrDefault(c => c.Name == name);
            if (column == null)
            {
                throw new KeyNotFoundException($"Column '{name}' does not exist.");
            }

            _columns.Remove(column);

            // Remove the column from all rows
            foreach (var row in _rows)
            {
                row.RemoveColumn(name);
            }
        }

        public void AddRow(RowDto row)
        {
            // Ensure the row matches the table's columns
            if (row.GetType() != typeof(RowDto) || _columns.Count != row.GetType().GetProperties().Length)
            {
                throw new InvalidOperationException("Row does not match table columns.");
            }

            _rows.Add(row);
        }

        public void RemoveRow(RowDto row)
        {
            _rows.Remove(row);
        }

        public IEnumerable<ColumnDto> GetColumns() => _columns.AsReadOnly();

        public IEnumerable<RowDto> GetRows() => _rows.AsReadOnly();
    }


    public class ColumnDto
    {
        public string Name { get; set; }
        public Type DataType { get; set; }


        public ColumnDto(string name, Type dataType)
        {
            Name = name;
            DataType = dataType;
        }

    }


    public class RowDto
    {
        private readonly Dictionary<string, object> _values;
        public string Status { get; set; }

        public RowDto(IEnumerable<ColumnDto> columns)
        {
            _values = new Dictionary<string, object>();

            foreach (var column in columns)
            {
                _values[column.Name] = null; // Initialize all values to null
            }
        }

        public object this[string columnName]
        {
            get => _values[columnName];
            set
            {
                if (_values.ContainsKey(columnName))
                {
                    _values[columnName] = value;
                }
                else
                {
                    throw new KeyNotFoundException($"Column '{columnName}' does not exist.");
                }
            }
        }

        public void RemoveColumn(string columnName)
        {
            if (_values.ContainsKey(columnName))
            {
                _values.Remove(columnName);
            }
            else
            {
                throw new KeyNotFoundException($"Column '{columnName}' does not exist.");
            }
        }
    }


    public class ListEntity
    {
        [Key]
        public int Id { get; set; }
        public int ChapterId { get; set; }
        public string Name { get; set; }
    }


    public class ColumnEntity
    {
        [Key]
        public int Id { get; set; }
        public int ListId { get; set; }
        public string Name { get; set; }
        public string DataType { get; set; }
    }


    public class DataEntity
    {
        [Key]
        public int Id { get; set; }
        public int ListId { get; set; }
        public int RowId { get; set; }
        public int ColumnId { get; set; }
        public string Value { get; set; }
    }
}
