using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using LightningBug.Reflection;

namespace LightningBug.Data
{
    /// <summary>
    /// <see cref="System.Data.IDataReader"/> implementation backed by <see cref="IEnumerable{T}"/>
    /// </summary>
    /// <typeparam name="T">Type of instance to read from</typeparam>
    public class ObjectDataReader<T> : IDataReader
    {
        private IEnumerator<T> _dataEnumerator;

        /// <summary>
        /// Constructs an <see cref="ObjectDataReader{T}"/> backed by <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="data"><see cref="IEnumerable{T}"/> containing data returned by this <see cref="IDataReader"/></param>
        public ObjectDataReader(IEnumerable<T> data)
        {
            _dataEnumerator = data.GetEnumerator();
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            if (_dataEnumerator == null)
                throw new ObjectDisposedException("ObjectDataReader");
            return _dataEnumerator.MoveNext();
        }

        public int Depth { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_dataEnumerator != null)
                {
                    _dataEnumerator.Dispose();
                    _dataEnumerator = null;
                }
            }
        }

        public void Close()
        {
            Dispose();
        }

        public DataTable GetSchemaTable()
        {
            return DataTableExtensions.GetSchemaTable<T>();
        }

        public bool IsClosed { get { return _dataEnumerator == null; } }
        public int RecordsAffected { get; private set; }

        public bool IsDBNull(int i)
        {
            return ReferenceEquals(GetValue(i), DBNull.Value);
        }

        public int FieldCount { get { return GetterDelegateCache<T>.ReadablePropertyCount; } }

        object IDataRecord.this[int i]
        {
            get { return GetValue(i); }
        }

        object IDataRecord.this[string name]
        {
            get { return GetValue(GetOrdinal(name)); }
        }

        public int GetValues(object[] values)
        {
            var max = Math.Min(values.Length, GetterDelegateCache<T>.ReadablePropertyCount);
            for (var i = 0; i < max; i++)
                values[i] = GetValue(i);
            return max;
        }

        public int GetOrdinal(string name)
        {
            return GetterDelegateCache<T>.OrdinalLookup[name];
        }

        public bool GetBoolean(int i)
        {
            return (bool) GetValue(i);
        }

        public byte GetByte(int i)
        {
            return (byte) GetValue(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            var value = (byte[]) GetValue(i);
            Array.Copy(value, fieldOffset, buffer, bufferoffset, length);
            return length;
        }

        public char GetChar(int i)
        {
            return (char) GetValue(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            var value = (char[]) GetValue(i);
            Array.Copy(value, fieldoffset, buffer, bufferoffset, length);
            return length;
        }

        public Guid GetGuid(int i)
        {
            return (Guid) GetValue(i);
        }

        public short GetInt16(int i)
        {
            return (short) GetValue(i);
        }

        public int GetInt32(int i)
        {
            return (int) GetValue(i);
        }

        public long GetInt64(int i)
        {
            return (long) GetValue(i);
        }

        public float GetFloat(int i)
        {
            return (float) GetValue(i);
        }

        public double GetDouble(int i)
        {
            return (double) GetValue(i);
        }

        public string GetString(int i)
        {
            return (string) GetValue(i);
        }

        public decimal GetDecimal(int i)
        {
            return (decimal) GetValue(i);
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime) GetValue(i);
        }

        public IDataReader GetData(int i)
        {
            throw new NotSupportedException();
        }

        public string GetName(int i)
        {
            return GetterDelegateCache<T>.OrdinalLookup
                .Single(kv => kv.Value == i).Key;
        }

        public string GetDataTypeName(int i)
        {
            return GetFieldType(i).Name;
        }

        public Type GetFieldType(int i)
        {
            return GetterDelegateCache<T>.Types[GetName(i)];
        }

        public object GetValue(int i)
        {
            return GetterDelegateCache<T>.Read(i, _dataEnumerator.Current);
        }

    }
}
