using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows;
using System.Xaml;
using System.Xaml.Schema;
using System.Text.RegularExpressions;
using SysXamlTypeName = System.Xaml.Schema.XamlTypeName;
using XamlTypeName = ExtensionLibrary.Presentation.XamlMarkupExtension.XamlTypeParser.XamlTypeName;
using TypeExtensionDelimeter = ExtensionLibrary.Presentation.XamlMarkupExtension.XamlTypeParser.GenericTypeNamePartDelimeter;

namespace ExtensionLibrary.Presentation.XamlMarkupExtension
{
#if DEBUG
	public class DtyPlus<T1, T2>
	{
		public T1 Val1 { get; set; }
		public T2 Val2 { get; set; }

		public DtyPlus(T1 val1, T2 val2)
		{
			Val1 = val1; Val2 = val2;
		}
		
		public DtyPlus(T1 val1)
		{
			Val1 = val1;
		}

		public DtyPlus( )
		{
		}
	}

	public class StaticDtyPlus
	{
		public static string Property { get { return "Property"; } }
		public static string Field = "Field";
		public static string Method( ) { return "Method"; }
	}

#endif

	/// <summary>
	/// Extends system x:Type, Returns a Type object(Including Generic Type).
	/// </summary>
	[MarkupExtensionReturnType(typeof(Type)), ContentProperty("TypeString")]
	public class TypeExtension : MarkupExtension
	{
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			var resolver = serviceProvider.GetService(typeof(IXamlNamespaceResolver)) as IXamlNamespaceResolver;
			SchCtxt = (serviceProvider.GetService(typeof(IXamlSchemaContextProvider)) as IXamlSchemaContextProvider).SchemaContext;
			if(resolver != null && SchCtxt != null)
			{
				try
				{
					var n = XamlTypeName.Parse(_TypeString, _Delimeter, resolver);
					var sn = BuildSysXamlTypeName(n);
					return TypeFromXamlType(sn);
				}
				catch(Exception ex)
				{
#if DEBUG
					return ex.GetType();
#else
					throw ex;
#endif
				}
			}
			return null;
		}

		SysXamlTypeName BuildSysXamlTypeName(XamlTypeName xty)
		{
			string name = xty.Name;
			if(_Delimeter != (TypeExtensionDelimeter)'`')
				name = xty.Name.Replace((char)_Delimeter, '`');

			if(xty.TypeArguments.Count > 0)
			{
				var args = new SysXamlTypeName[xty.TypeArguments.Count];
				for(int i=0;i<args.Length;i++)
				{
					args[i] = BuildSysXamlTypeName(xty.TypeArguments[i]);
				}
				return new SysXamlTypeName(xty.Namespace, name, args);
			}
			return new SysXamlTypeName(xty.Namespace, name);
		}

		public static Type TypeFromXamlType(SysXamlTypeName xty, XamlSchemaContext sch)
		{
			if(xty.TypeArguments.Count > 0)
			{
				var nxty = new SysXamlTypeName(xty.Namespace, xty.Name);
				var tyary = new Type[xty.TypeArguments.Count];
				for(int i = 0; i < tyary.Length; i++)
				{
					tyary[i] = TypeFromXamlType(xty.TypeArguments[i], sch);
				}
				return sch.GetXamlType(nxty).UnderlyingType.MakeGenericType(tyary);
			}
			return sch.GetXamlType(xty).UnderlyingType;
		}

		XamlSchemaContext SchCtxt;

		Type TypeFromXamlType(SysXamlTypeName xty)
		{
			if(xty.TypeArguments.Count > 0)
			{
				var nxty = new SysXamlTypeName(xty.Namespace, xty.Name);
				var tyary = new Type[xty.TypeArguments.Count];
				for(int i = 0; i < tyary.Length; i++)
				{
					tyary[i] = TypeFromXamlType(xty.TypeArguments[i]);
				}
				return SchCtxt.GetXamlType(nxty).UnderlyingType.MakeGenericType(tyary);
			}
			return SchCtxt.GetXamlType(xty).UnderlyingType;
		}

		public TypeExtension( )
		{
			_TypeString = string.Empty;
			_Delimeter = DEFAUT_DELIMETER;
		}

		public TypeExtension(string typeString)
		{
			_TypeString = typeString;
			_Delimeter = DEFAUT_DELIMETER;
		}

		string _TypeString;
		public string TypeString
		{
			get { return _TypeString; }
			set { _TypeString = value; } 
		}

		const TypeExtensionDelimeter DEFAUT_DELIMETER = TypeExtensionDelimeter.Slash;

		public TypeExtensionDelimeter _Delimeter;
		public TypeExtensionDelimeter Delimeter
		{
			get { return _Delimeter; }
			set 
			{
				if(Enum.IsDefined(typeof(TypeExtensionDelimeter), value))
					_Delimeter = value;
				else
					_Delimeter = DEFAUT_DELIMETER;
			}
		}
	}
}
