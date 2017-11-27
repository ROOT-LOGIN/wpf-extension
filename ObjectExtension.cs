#define THROW_EXCEPTION

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Reflection;

namespace ExtensionLibrary.Presentation.XamlMarkupExtension
{
    public delegate object ObjectFactory(params object[] Args);

#if DEBUG
	public class OBJECT_FATORY
	{
		static object _DtyFactory(params object[] Args)
		{
			var typary = Type.EmptyTypes;
			if(Args != null && Args.Length > 0)
			{
				typary = new Type[Args.Length];
				for(int i = 0; i < Args.Length; i++)
				{
					typary[i] = Args[i].GetType();
				}
			}
			return Activator.CreateInstance(
				typeof(DtyPlus<,>).MakeGenericType(typary), Args);
		}

		public static ObjectFactory DtyFactory { get { return _DtyFactory; } }
	}
#endif

    /// <summary>
    /// Provides basic construct of referencing CLR object.
    /// </summary>
    [MarkupExtensionReturnType(typeof(object))]
    public class BoxingExtension : MarkupExtension
    {
        [ConstructorArgument("value")]
        public object Value { get; set; }
        
        public BoxingExtension() { }

        public BoxingExtension(ValueType value) { Value = value; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Value;
        }
    }

	/// <summary>
	/// Using factory method to generate any kind of object.
	/// </summary>
	[MarkupExtensionReturnType(typeof(object)), ContentProperty("Arguments")]
	public class ObjectExtension : MarkupExtension
	{
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return Factory(Arguments.ToArray());
		}

        public ObjectExtension( )
        {
            Arguments = new List<object>();
        }

		public ObjectExtension(ObjectFactory factory) : this()
		{
			Factory = factory;
		}
        
        [ConstructorArgument("factory")]
		public ObjectFactory Factory { get; set; }
		public List<object> Arguments { get; set; }
	}

	public enum StaticMemberType
	{
		Property, Field, Method
	}

	/// <summary>
	/// Nested syntax is Not supported. 
	/// that means Only Type.MorPorF is accepted.
	/// </summary>
	[MarkupExtensionReturnType(typeof(object)), ContentProperty("Index")]
	public class StaticExtension : MarkupExtension
	{
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			var path = Member.Split('.');
			if(path.Length != 2)
				throw new NotSupportedException("Nested member access is Not supported!");

			var tyext = new TypeExtension(path[0]);
			var Ty = tyext.ProvideValue(serviceProvider) as Type;
			if(Type == StaticMemberType.Property)
			{
				var idx = Index.ToArray();
				if(idx.Length < 1)
					idx = null;
				return Ty.GetProperty(path[1], _Flags).GetValue(null, idx);
			}
			else if(Type == StaticMemberType.Field)
			{
				return Ty.GetField(path[1], _Flags).GetValue(null);
			}
			else if(Type == StaticMemberType.Method)
			{
				var method = Ty.GetMethod(path[1], _Flags);
				if(DelegateType == null)
					return method;
				return Delegate.CreateDelegate(DelegateType, method);				
			}
			else
				throw new InvalidOperationException(string.Format("Unknown member type {0}", Type));
		}

        public StaticExtension()
        {
            Type = StaticMemberType.Property;
            _Flags = BindingFlags.Public | BindingFlags.Static;
            Index = new List<object>(4);
        }


		public StaticExtension(string member) : this()
		{
			Member = member;
		}

		public StaticMemberType Type { get; set; }
		BindingFlags _Flags;
		public BindingFlags Flags 
		{
			get { return _Flags; }
			set
			{
				_Flags = value | BindingFlags.Static;
				if(value.HasFlag(BindingFlags.Instance))
#if THROW_EXCEPTION
					throw new ArgumentException("Instance member is not supported!");
#else
					_Flags = & (~BindingFlags.Instance);
#endif
			}
		}
        
        [ConstructorArgument("member")]
		public string Member { get; set; }
		public List<object> Index { get; set; }

		public Type DelegateType { get; set; }
	}
}
