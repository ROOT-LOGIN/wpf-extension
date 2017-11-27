using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;

namespace ExtensionLibrary.Presentation.XamlMarkupExtension
{
    public class SO
    {
        public static Guid Guid { get { return Guid.NewGuid(); } }
    }

    public class ObjectCollection : List<object>
    {
        internal ObjectCollection( ) : base(4) { }
    }

    public class FixedObjectCollection : IList<object>
    {
        IList<object> datas;
        int ptr;
        internal FixedObjectCollection(int size) {
            datas = new object[size]; ptr = -1;
        }

        public int IndexOf(object item)
        {
            return datas.IndexOf(item);
        }

        public void Insert(int index, object item)
        {
            datas[index] = item;
        }

        public void RemoveAt(int index)
        {
            datas[index] = null;
        }

        public object this[int index]
        {
            get { return datas[index]; }
            set { datas[index] = value; }
        }

        public void Add(object item)
        {
            ptr++; datas[ptr] = item;
        }

        public void Clear( )
        {
            datas.Clear();
        }

        public bool Contains(object item)
        {
            return datas.Contains(item);
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            datas.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return datas.Count; }
        }

        public bool IsReadOnly
        {
            get { return datas.IsReadOnly; }
        }

        public bool Remove(object item)
        {
            return datas.Remove(item);
        }

        public IEnumerator<object> GetEnumerator( )
        {
            return datas.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator( )
        {
            return GetEnumerator();
        }
    }

    [MarkupExtensionReturnType(typeof(string)), ContentProperty("Arguments")]
    public class StringExtension : MarkupExtension
    {
        [ConstructorArgument("format")]
        public string Format { get; set; }

        public ObjectCollection Arguments { get; set; }

        public StringExtension( ) { Arguments = new ObjectCollection(); }
        public StringExtension(string format) : this() { Format = format; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if(Format == null | Arguments.Count < 1)
                return Format;
            return string.Format(Format, Arguments.ToArray());
        }
    }
}
