using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xaml;

namespace ExtensionLibrary.Presentation.XamlMarkupExtension
{
	/// <summary>
	/// Delegate represents IValueConverter Convert() and Convertback() methods
	/// </summary>
	public delegate object DLG_CONVERT(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture);

	/// <summary>
	/// A pair for wrappers IValueConverter Convert() and Convertback() methods
	/// </summary>
	internal class ConvertPair
	{
		public DLG_CONVERT To;
		public DLG_CONVERT Back;

		public ConvertPair()
		{
		}

		public ConvertPair(DLG_CONVERT to, DLG_CONVERT back)
		{
			To = to; Back = back;
		}
	}

	/// <summary>
	/// Converter for convert object to ExpressionKey
	/// </summary>
	internal class ExpressionKeyTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return true;
		}

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if(value == null) throw new ArgumentNullException("value");

			return new ExpressionKey(value);
		}
	}

	/// <summary>
	/// The key for ExpressionDictionary
	/// </summary>
	[TypeConverter(typeof(ExpressionKeyTypeConverter))]
	public struct ExpressionKey : IEquatable<ExpressionKey>
	{
		object Key;

		public ExpressionKey(object key)
		{
			Key = key;
		}

		public bool Equals(ExpressionKey other)
		{
			return Key.ToString() == other.Key.ToString();
		}

		public override bool Equals(object obj)
		{
			if(obj is ExpressionKey)
				return Equals((ExpressionKey)obj);
			return false;
		}

		public override int GetHashCode( )
		{
			return Key == null ? 0 : Key.ToString().GetHashCode();
		}

		public override string ToString( )
		{
			if(Key == null) return null;

			return string.Format("{0}[\"{1}\"]", base.ToString(), Key);
		}
	}

	/// <summary>
	/// A Dictionary for holding IValueConverter Convert() and Convertback() methods
	/// </summary>
	internal sealed class ExpressionDictionary
	{
		readonly Dictionary<ExpressionKey, ConvertPair> Impl;

		static ExpressionDictionary _i_Instance;
		public static ExpressionDictionary Instance
		{
			get
			{
				if(_i_Instance == null) _i_Instance = new ExpressionDictionary();
				return _i_Instance;
			}
		}

		internal ExpressionDictionary()
		{
			Impl = new Dictionary<ExpressionKey, ConvertPair>();
		}

		public void Add(ExpressionKey key, ConvertPair value)
		{
			Impl.Add(key, value);
		}

		public void Add(ExpressionKey key, DLG_CONVERT to, DLG_CONVERT back)
		{
			Impl.Add(key, new ConvertPair(to, back));
		}

		public bool ContainsKey(ExpressionKey key)
		{
			return Impl.ContainsKey(key);
		}

		public bool Remove(ExpressionKey key)
		{
			return Impl.Remove(key);
		}

		public bool TryGetValue(ExpressionKey key, out ConvertPair value)
		{
			return Impl.TryGetValue(key, out value);
		}

		public ConvertPair this[ExpressionKey key]
		{
			get
			{
				return Impl[key];
			}
			set
			{
				Impl[key] = value;
			}
		}

		public void SetAt(ExpressionKey key, DLG_CONVERT to, DLG_CONVERT back)
		{
			ConvertPair pair;
			if(Impl.TryGetValue(key, out pair))
			{
				pair.To = to; pair.Back = back;
			}
			else
				Impl[key] = new ConvertPair(to, back);
		}

		public void Clear( )
		{
			Impl.Clear();
		}

		public int Count
		{
			get { return Impl.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

	}

	/// <summary>
	/// The value converter for ExpressionBindingExtension
	/// </summary>
	internal sealed class ExpressionValueConverter : IValueConverter
	{
		internal static object To_DoNothing(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value;
		}

		internal static object Back_DoNothing(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value;
		}

		public ExpressionValueConverter(ExpressionKey expressionKey)
		{
			var pair = ExpressionDictionary.Instance[expressionKey];
			To = pair.To ?? To_DoNothing;
			Back = pair.Back ?? Back_DoNothing;
		}

		DLG_CONVERT To;
		DLG_CONVERT Back;
		
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return To(value, targetType, parameter, culture);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return Back(value, targetType, parameter, culture);
		}
	}

	/// <summary>
	/// A WPF markup extension for linking a value of property to other properties.
	/// the source object can be ControlTemplate or Itself
	/// </summary>
	[MarkupExtensionReturnType(typeof(object))]
	public sealed class ExpressionBindingExtension : MarkupExtension
	{
		/// <summary>
		/// Registers the expression.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="to">The Convert method impl.</param>
		/// <param name="back">The ConvertBack method impl.</param>
		public static void RegisterExpression(ExpressionKey key, DLG_CONVERT to, DLG_CONVERT back)
		{
			ExpressionDictionary.Instance[key] = new ConvertPair(to, back);
		}

		/// <summary>
		/// Reregisters the expression.
		/// The expression must be registered before.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="to">The Convert method impl.</param>
		/// <param name="back">The ConvertBack method impl.</param>
		public static void ReregisterExpression(ExpressionKey key, DLG_CONVERT to, DLG_CONVERT back)
		{
			var pair = ExpressionDictionary.Instance[key];
			pair.To = to;
			pair.Back = back;
		}

		/// <summary>
		/// Unregisters the expression.
		/// </summary>
		/// <param name="key">The key.</param>
		public static void UnregisterExpression(ExpressionKey key)
		{
			ExpressionDictionary.Instance.Remove(key);
		}

		/// <summary>
		/// Inidicats whether binding to Ttself or TemplatedParent
		/// </summary>
		public bool BindSelf { get; set; }

		/// <summary>
		/// The target property name.
		/// </summary>
		public string Property { get; set; }

		/// <summary>
		/// The key for expression.
		/// </summary>
		public ExpressionKey Key { get; set; }

		/// <summary>
		/// The parameter pass to the expression.
		/// </summary>
		public object Parameter { get; set; }

		/// <summary>
		/// The requested culture info.
		/// </summary>
		public System.Globalization.CultureInfo Culture { get; set; }

		/// <summary>
		/// The binding mode
		/// </summary>
		[DefaultValue(BindingMode.OneWay)]
		public BindingMode Mode { get; set; }

		public ExpressionBindingExtension( )
		{
			Mode = BindingMode.OneWay;
		}

		public ExpressionBindingExtension(string Property) : this()
		{
			this.Property = Property;
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			var ret = new Binding();
			ret.Mode = Mode;
			ret.Path = new PropertyPath(Property);
			ret.RelativeSource = new RelativeSource(BindSelf ? RelativeSourceMode.Self : RelativeSourceMode.TemplatedParent);
			ret.Converter = new ExpressionValueConverter(Key);
			ret.ConverterParameter = Parameter;
			ret.ConverterCulture = Culture ?? System.Globalization.CultureInfo.CurrentUICulture;

			// It returns BindingBase directly so that 
			// can do the same as Binding markup extension
			return ret;
		}
	}
}
