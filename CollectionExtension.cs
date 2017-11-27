using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Collections;
using System.Windows;

namespace ExtensionLibrary.Presentation.XamlMarkupExtension
{
	/// <summary>
	/// Returns a Generic List object.
	/// </summary>
	[MarkupExtensionReturnType(typeof(List<>)), ContentProperty("Elements")]
	public class ListExtension : MarkupExtension
	{
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if(Elements.Count < 1)
				return Activator.CreateInstance(typeof(List<>).MakeGenericType(ElementType));

			var lst = Activator.CreateInstance(
				typeof(List<>).MakeGenericType(ElementType), Elements.Count) as IList;
			foreach(var obj in Elements)
			{
				if(ElementType.IsInstanceOfType(obj))
					lst.Add(obj);
			}
			return lst;
		}

		public ListExtension(Type elementType)
		{
			ElementType = elementType;
			Elements = new List<object>();
		}

        public ListExtension() : this(typeof(object))
        {
        }

		Type _ElementType;
        [ConstructorArgument("elementType")]
		public Type ElementType 
		{
			get { return _ElementType; }
			set 
			{ 
				if(value == null) 
					throw new InvalidOperationException();
				_ElementType = value;
			}
		}
		public List<object> Elements { get; set; }
	}

	/// <summary>
	/// Returns a Generic Array object.
	/// </summary>
	[MarkupExtensionReturnType(typeof(Array)), ContentProperty("Elements")]
	public class ArrayExtension : ListExtension
	{
		public override object ProvideValue(IServiceProvider serviceProvider)
		{			
			var lst = Activator.CreateInstance(
				typeof(List<>).MakeGenericType(ElementType), Elements.Count) as List<object>;
			int i = 0;
			foreach(var obj in Elements)
			{
				if(ElementType.IsInstanceOfType(obj))
					i++;
			}
			if(i<1)
				return Activator.CreateInstance(ElementType.MakeArrayType());
			var ary = Activator.CreateInstance(ElementType.MakeArrayType(), i) as IList;
			i = 0;
			foreach(var obj in Elements)
			{
				if(ElementType.IsInstanceOfType(obj))
					ary[i] = obj;
				i++;
			}
			return ary;
		}

		public ArrayExtension(Type elementType) : base(elementType)
		{
		}

        public ArrayExtension() : base()
        {
        }
	}

	/// <summary>
	/// Returns a Generic KeyValuePair object.
	/// </summary>
	[MarkupExtensionReturnType(typeof(KeyValuePair<,>))]
	public class KeyValuePairExtension : MarkupExtension
	{
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return Activator.CreateInstance(
				typeof(KeyValuePair<,>).MakeGenericType(KeyType, ValueType), Key, Value);
		}

		public Type KeyType { get; set; }
		public Type ValueType { get; set; }
		object _Key;
		public object Key 
		{
			get { return _Key; }
			set
			{
				KeyType = value.GetType();
				_Key = value;
			}
		}
		object _Value;
		public object Value
		{
			get { return _Value; }
			set
			{
                ValueType = value == null ? typeof(object) : value.GetType();
				_Value = value;
			}
		}
		
		public KeyValuePairExtension()
		{
			KeyType = typeof(object);
			ValueType = typeof(object);
		}
	}

	/// <summary>
	/// Returns a Generic Dictionary object.
	/// </summary>
	[MarkupExtensionReturnType(typeof(Dictionary<,>)), ContentProperty("Pairs")]
	public class DictionaryExtension : MarkupExtension
	{
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			var dict = Activator.CreateInstance(
				typeof(Dictionary<,>).MakeGenericType(KeyType, ValueType), Pairs.Count) as IDictionary;
			var kvpType = typeof(KeyValuePair<,>).MakeGenericType(KeyType, ValueType);
			foreach(var obj in Pairs)
			{
				if(kvpType.IsInstanceOfType(obj))
				{
					dict.Add
					(
						kvpType.GetProperty("Key").GetValue(obj, null),
						kvpType.GetProperty("Value").GetValue(obj, null)
					);
				}
			}
			return dict;
		}

		public Type KeyType { get; set; }
		public Type ValueType { get; set; }
		public List<object> Pairs { get; set; }

		public DictionaryExtension( )
		{
			KeyType = typeof(object);
			ValueType = typeof(object);
			Pairs = new List<object>();
		}

	}
}
