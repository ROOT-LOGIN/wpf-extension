using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Xaml.Schema;
using System.Runtime.Serialization;
using System.Resources;

namespace ExtensionLibrary.Presentation.XamlMarkupExtension.XamlTypeParser
{
	internal enum GenericTypeNameScannerToken
	{
		NONE,
		ERROR,
		OPEN,
		CLOSE,
		COLON,
		COMMA,
		SUBSCRIPT,
		NAME
	}

	public enum GenericTypeNamePartDelimeter : byte
	{
		Tilde = (byte)'~',
		GraveQuote = (byte)'`',
		Exclamation = (byte)'!',
		At = (byte)'@',
		Pound = (byte)'#',
		Dolar = (byte)'$',
		Percent = (byte)'%',
		Caret = (byte)'^',
		Star = (byte)'*',
		Pipe = (byte)'|',
		Bar = (byte)'|',
		Slash = (byte)'/',
		BackSlash = (byte)'\\',
		Question = (byte)'?',
		Assign = (byte)'='
	}

	internal class Sample_StringParserBase
	{
		public static readonly char[] WhitespaceChars = new char[] { ' ', '\t', '\n', '\r', '\f' };

		// Fields
		protected int _idx;
		protected string _inputText;
		protected const char NullChar = '\0';

		// Methods
		public Sample_StringParserBase(string text)
		{
			this._inputText = text;
			this._idx = 0;
		}

		protected bool Advance( )
		{
			this._idx++;
			if(this.IsAtEndOfInput)
			{
				this._idx = this._inputText.Length;
				return false;
			}
			return true;
		}

		protected bool AdvanceOverWhitespace( )
		{
			bool flag = true;
			while(!this.IsAtEndOfInput && IsWhitespaceChar(this.CurrentChar))
			{
				flag = true;
				this.Advance();
			}
			return flag;
		}

		protected static bool IsWhitespaceChar(char ch)
		{
			if(((ch != WhitespaceChars[0]) && 
                (ch != WhitespaceChars[1])) &&
                (((ch != WhitespaceChars[2]) && 
                (ch != WhitespaceChars[3])) && 
                (ch != WhitespaceChars[4])))
			{
				return false;
			}
			return true;
		}

		// Properties
		protected char CurrentChar
		{
			get
			{
				return this._inputText[this._idx];
			}
		}

		public bool IsAtEndOfInput
		{
			get
			{
				return (this._idx >= this._inputText.Length);
			}
		}
	}

	internal abstract class XamlName
	{
		// Fields
		protected string _namespace;
		protected string _prefix;
		public const char Dot = '.';
		public const char PlusSign = '+';
		public const char UnderScore = '_';

		// Methods
		protected XamlName( )
			: this(string.Empty)
		{
		}

		public XamlName(string name)
		{
			this.Name = name;
		}

		public XamlName(string prefix, string name)
		{
			this.Name = name;
			this._prefix = prefix ?? string.Empty;
		}

		public static bool ContainsDot(string name)
		{
			return name.Contains(".");
		}

		public static bool IsValidNameChar(char ch)
		{
			if(!IsValidNameStartChar(ch) && !char.IsDigit(ch))
			{
				UnicodeCategory unicodeCategory = char.GetUnicodeCategory(ch);
				if((unicodeCategory != UnicodeCategory.NonSpacingMark) && (unicodeCategory != UnicodeCategory.SpacingCombiningMark))
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsValidNameStartChar(char ch)
		{
			if(!char.IsLetter(ch))
			{
				return (ch == '_');
			}
			return true;
		}

		public static bool IsValidQualifiedNameChar(char ch)
		{
			if(ch != '.')
			{
				return IsValidNameChar(ch);
			}
			return true;
		}

		public static bool IsValidQualifiedNameCharPlus(char ch, GenericTypeNamePartDelimeter customeGenericTypeNamePartDelimeter)
		{
			if(!IsValidQualifiedNameChar(ch))
			{
				return (ch == '+' || ch == (char)customeGenericTypeNamePartDelimeter);
			}
			return true;
		}

		public static bool IsValidXamlName(string name)
		{
			if(name.Length == 0)
			{
				return false;
			}
			if(!IsValidNameStartChar(name[0]))
			{
				return false;
			}
			for(int i = 1; i < name.Length; i++)
			{
				if(!IsValidNameChar(name[i]))
				{
					return false;
				}
			}
			return true;
		}

		// Properties
		public string Name { get; protected set; }

		public string Namespace
		{
			get
			{
				return this._namespace;
			}
		}

		public string Prefix
		{
			get
			{
				return this._prefix;
			}
		}

		public abstract string ScopedName { get; }
	}

	internal class GenericTypeNameScanner : Sample_StringParserBase
	{
		// Fields
		private char _lastChar;
		private int _multiCharTokenLength;
		private int _multiCharTokenStartIdx;
		private GenericTypeNameScannerToken _pushedBackSymbol;
		private State _state;
		private GenericTypeNameScannerToken _token;
		private string _tokenText;
		public const char CloseBracket = ']';
		public const char CloseParen = ')';
		public const char Colon = ':';
		public const char Comma = ',';
		public const char OpenBracket = '[';
		public const char OpenParen = '(';
		public const char Space = ' ';

		GenericTypeNamePartDelimeter CustomeGenericTypeNamePartDelimeter;
		// Methods
		public GenericTypeNameScanner(string text, GenericTypeNamePartDelimeter customeGenericTypeNamePartDelimeter)
			: base(text)
		{
			this._state = State.START;
			this._pushedBackSymbol = GenericTypeNameScannerToken.NONE;
			CustomeGenericTypeNamePartDelimeter = customeGenericTypeNamePartDelimeter;
		}

		private void AddToMultiCharToken( )
		{
			this._multiCharTokenLength++;
		}

		private string CollectMultiCharToken( )
		{
			if((this._multiCharTokenStartIdx == 0) && (this._multiCharTokenLength == base._inputText.Length))
			{
				return base._inputText;
			}
			return base._inputText.Substring(this._multiCharTokenStartIdx, this._multiCharTokenLength);
		}

		internal static int ParseSubscriptSegment(string subscript, ref int pos)
		{
			bool flag = false;
			int num = 1;
			do
			{
				switch(subscript[pos])
				{
					case '[':
						if(!flag)
						{
							flag = true;
							break;
						}
						return 0;

					case ']':
						if(flag)
						{
							pos++;
							return num;
						}
						return 0;

					case ',':
						if(!flag)
						{
							return 0;
						}
						num++;
						break;

					default:
						if(!Sample_StringParserBase.IsWhitespaceChar(subscript[pos]))
						{
							return 0;
						}
						break;
				}
				pos++;
			}
			while(pos < subscript.Length);
			return 0;
		}

		public void Read( )
		{
			if(this._pushedBackSymbol != GenericTypeNameScannerToken.NONE)
			{
				this._token = this._pushedBackSymbol;
				this._pushedBackSymbol = GenericTypeNameScannerToken.NONE;
			}
			else
			{
				this._token = GenericTypeNameScannerToken.NONE;
				this._tokenText = string.Empty;
				this._multiCharTokenStartIdx = -1;
				this._multiCharTokenLength = 0;
				while(this._token == GenericTypeNameScannerToken.NONE)
				{
					if(base.IsAtEndOfInput)
					{
						if(this._state == State.INNAME)
						{
							this._token = GenericTypeNameScannerToken.NAME;
							this._state = State.START;
						}
						if(this._state == State.INSUBSCRIPT)
						{
							this._token = GenericTypeNameScannerToken.ERROR;
							this._state = State.START;
						}
						break;
					}
					switch(this._state)
					{
						case State.START:
							this.State_Start();
							break;

						case State.INNAME:
							this.State_InName();
							break;

						case State.INSUBSCRIPT:
							this.State_InSubscript();
							break;
					}
				}
				if((this._token == GenericTypeNameScannerToken.NAME) || (this._token == GenericTypeNameScannerToken.SUBSCRIPT))
				{
					this._tokenText = this.CollectMultiCharToken();
				}
			}
		}

		private void StartMultiCharToken( )
		{
			this._multiCharTokenStartIdx = base._idx;
			this._multiCharTokenLength = 1;
		}

		private void State_InName( )
		{
			if((base.IsAtEndOfInput || Sample_StringParserBase.IsWhitespaceChar(base.CurrentChar)) || (base.CurrentChar == '['))
			{
				this._token = GenericTypeNameScannerToken.NAME;
				this._state = State.START;
			}
			else
			{
				switch(base.CurrentChar)
				{
					case '(':
						this._pushedBackSymbol = GenericTypeNameScannerToken.OPEN;
						this._token = GenericTypeNameScannerToken.NAME;
						this._state = State.START;
						break;

					case ')':
						this._pushedBackSymbol = GenericTypeNameScannerToken.CLOSE;
						this._token = GenericTypeNameScannerToken.NAME;
						this._state = State.START;
						break;

					case ',':
						this._pushedBackSymbol = GenericTypeNameScannerToken.COMMA;
						this._token = GenericTypeNameScannerToken.NAME;
						this._state = State.START;
						break;

					case ':':
						this._pushedBackSymbol = GenericTypeNameScannerToken.COLON;
						this._token = GenericTypeNameScannerToken.NAME;
						this._state = State.START;
						break;

					default:
						if(XamlName.IsValidQualifiedNameCharPlus(base.CurrentChar, CustomeGenericTypeNamePartDelimeter))
						{
							this.AddToMultiCharToken();
						}
						else
						{
							this._token = GenericTypeNameScannerToken.ERROR;
						}
						break;
				}
				this._lastChar = base.CurrentChar;
				base.Advance();
			}
		}

		private void State_InSubscript( )
		{
			if(base.IsAtEndOfInput)
			{
				this._token = GenericTypeNameScannerToken.ERROR;
				this._state = State.START;
			}
			else
			{
				switch(base.CurrentChar)
				{
					case ',':
						this.AddToMultiCharToken();
						break;

					case ']':
						this.AddToMultiCharToken();
						this._token = GenericTypeNameScannerToken.SUBSCRIPT;
						this._state = State.START;
						break;

					default:
						if(Sample_StringParserBase.IsWhitespaceChar(base.CurrentChar))
						{
							this.AddToMultiCharToken();
						}
						else
						{
							this._token = GenericTypeNameScannerToken.ERROR;
						}
						break;
				}
				this._lastChar = base.CurrentChar;
				base.Advance();
			}
		}

		private void State_Start( )
		{
			base.AdvanceOverWhitespace();
			if(base.IsAtEndOfInput)
			{
				this._token = GenericTypeNameScannerToken.NONE;
			}
			else
			{
				switch(base.CurrentChar)
				{
					case '(':
						this._token = GenericTypeNameScannerToken.OPEN;
						break;

					case ')':
						this._token = GenericTypeNameScannerToken.CLOSE;
						break;

					case ',':
						this._token = GenericTypeNameScannerToken.COMMA;
						break;

					case ':':
						this._token = GenericTypeNameScannerToken.COLON;
						break;

					case '[':
						this.StartMultiCharToken();
						this._state = State.INSUBSCRIPT;
						break;

					default:
						if(XamlName.IsValidNameStartChar(base.CurrentChar))
						{
							this.StartMultiCharToken();
							this._state = State.INNAME;
						}
						else
						{
							this._token = GenericTypeNameScannerToken.ERROR;
						}
						break;
				}
				this._lastChar = base.CurrentChar;
				base.Advance();
			}
		}

		internal static string StripSubscript(string typeName, out string subscript)
		{
			int index = typeName.IndexOf('[');
			if(index < 0)
			{
				subscript = null;
				return typeName;
			}
			subscript = typeName.Substring(index);
			return typeName.Substring(0, index);
		}

		// Properties
		public char ErrorCurrentChar
		{
			get
			{
				return this._lastChar;
			}
		}

		public string MultiCharTokenText
		{
			get
			{
				return this._tokenText;
			}
		}

		public GenericTypeNameScannerToken Token
		{
			get
			{
				return this._token;
			}
		}

		// Nested Types
		internal enum State
		{
			START,
			INNAME,
			INSUBSCRIPT
		}
	}

	internal class TypeNameFrame
	{
		// Fields
		private List<XamlTypeName> _typeArgs;

		// Methods
		public void AllocateTypeArgs( )
		{
			this._typeArgs = new List<XamlTypeName>();
		}

		// Properties
		public string Name { get; set; }

		public string Namespace { get; set; }

		public List<XamlTypeName> TypeArgs
		{
			get
			{
				return this._typeArgs;
			}
		}
	}

	internal class XamlQualifiedName : XamlName
	{
		// Methods
		public XamlQualifiedName(string prefix, string name)
			: base(prefix, name)
		{
		}

		internal static bool IsNameValid(string name)
		{
			if(name.Length == 0)
			{
				return false;
			}
			if(!XamlName.IsValidNameStartChar(name[0]))
			{
				return false;
			}
			for(int i = 1; i < name.Length; i++)
			{
				if(!XamlName.IsValidQualifiedNameChar(name[i]))
				{
					return false;
				}
			}
			return true;
		}

		internal static bool IsNameValid_WithPlus(string name, GenericTypeNamePartDelimeter customeGenericTypeNamePartDelimeter)
		{
			if(name.Length == 0)
			{
				return false;
			}
			if(!XamlName.IsValidNameStartChar(name[0]))
			{
				return false;
			}
			for(int i = 1; i < name.Length; i++)
			{
				if(!XamlName.IsValidQualifiedNameCharPlus(name[i], customeGenericTypeNamePartDelimeter))
				{
					return false;
				}
			}
			return true;
		}

		public static bool Parse(string longName, GenericTypeNamePartDelimeter genericTypeNamePartDelimeter, out string prefix, out string name)
		{
			int startIndex = 0;
			int index = longName.IndexOf(':');
			prefix = string.Empty;
			name = string.Empty;
			if(index != -1)
			{
				prefix = longName.Substring(startIndex, index);
				if(string.IsNullOrEmpty(prefix) || !IsNameValid(prefix))
				{
					return false;
				}
				startIndex = index + 1;
			}
			name = (startIndex == 0) ? longName : longName.Substring(startIndex);
			return (!string.IsNullOrEmpty(name) && IsNameValid_WithPlus(name, genericTypeNamePartDelimeter));
		}

		// Properties
		public override string ScopedName
		{
			get
			{
				if(!string.IsNullOrEmpty(base.Prefix))
				{
					return (base.Prefix + ":" + base.Name);
				}
				return base.Name;
			}
		}
	}

	internal class GenericTypeNameParser
	{
		// Fields
		private string _inputText;
		private Func<string, string> _prefixResolver;
		private GenericTypeNameScanner _scanner;
		private Stack<TypeNameFrame> _stack;

		// Methods
		public GenericTypeNameParser(Func<string, string> prefixResolver)
		{
			this._prefixResolver = prefixResolver;
		}

		private void Callout_EndOfType( )
		{
			TypeNameFrame frame = this._stack.Pop();
			XamlTypeName item = new XamlTypeName(frame.Namespace, frame.Name, frame.TypeArgs);
			frame = this._stack.Peek();
			if(frame.TypeArgs == null)
			{
				frame.AllocateTypeArgs();
			}
			frame.TypeArgs.Add(item);
		}

		private void Callout_FoundName(string prefix, string name)
		{
			TypeNameFrame item = new TypeNameFrame
			{
				Name = name
			};
			string str = this._prefixResolver(prefix);
			if(str == null)
			{
				throw new TypeNameParserException(SR.Get("PrefixNotFound", new object[] { prefix }));
			}
			item.Namespace = str;
			this._stack.Push(item);
		}

		private void Callout_Subscript(string subscript)
		{
			TypeNameFrame local1 = this._stack.Peek();
			local1.Name = local1.Name + subscript;
		}

		private XamlTypeName CollectNameFromStack( )
		{
			if(this._stack.Count != 1)
			{
				throw new TypeNameParserException(SR.Get("InvalidTypeString", new object[] { this._inputText }));
			}
			TypeNameFrame frame = this._stack.Peek();
			if(frame.TypeArgs.Count != 1)
			{
				throw new TypeNameParserException(SR.Get("InvalidTypeString", new object[] { this._inputText }));
			}
			return frame.TypeArgs[0];
		}

		private IList<XamlTypeName> CollectNameListFromStack( )
		{
			if(this._stack.Count != 1)
			{
				throw new TypeNameParserException(SR.Get("InvalidTypeString", new object[] { this._inputText }));
			}
			return this._stack.Peek().TypeArgs;
		}

		private void P_NameListExt( )
		{
			this._scanner.Read();
			this.P_XamlTypeName();
		}

		private void P_RepeatingSubscript( )
		{
			do
			{
				this.Callout_Subscript(this._scanner.MultiCharTokenText);
				this._scanner.Read();
			}
			while(this._scanner.Token == GenericTypeNameScannerToken.SUBSCRIPT);
		}

		private void P_SimpleTypeName( )
		{
			string prefix = string.Empty;
			string multiCharTokenText = this._scanner.MultiCharTokenText;
			this._scanner.Read();
			if(this._scanner.Token == GenericTypeNameScannerToken.COLON)
			{
				prefix = multiCharTokenText;
				this._scanner.Read();
				if(this._scanner.Token != GenericTypeNameScannerToken.NAME)
				{
					this.ThrowOnBadInput();
				}
				multiCharTokenText = this._scanner.MultiCharTokenText;
				this._scanner.Read();
			}
			this.Callout_FoundName(prefix, multiCharTokenText);
		}

		private void P_TypeParameters( )
		{
			this._scanner.Read();
			this.P_XamlTypeNameList();
			if(this._scanner.Token != GenericTypeNameScannerToken.CLOSE)
			{
				this.ThrowOnBadInput();
			}
			this._scanner.Read();
		}

		private void P_XamlTypeName( )
		{
			if(this._scanner.Token != GenericTypeNameScannerToken.NAME)
			{
				this.ThrowOnBadInput();
			}
			this.P_SimpleTypeName();
			if(this._scanner.Token == GenericTypeNameScannerToken.OPEN)
			{
				this.P_TypeParameters();
			}
			if(this._scanner.Token == GenericTypeNameScannerToken.SUBSCRIPT)
			{
				this.P_RepeatingSubscript();
			}
			this.Callout_EndOfType();
		}

		private void P_XamlTypeNameList( )
		{
			this.P_XamlTypeName();
			while(this._scanner.Token == GenericTypeNameScannerToken.COMMA)
			{
				this.P_NameListExt();
			}
		}

		public static XamlTypeName ParseIfTrivalName(string text, GenericTypeNamePartDelimeter genericTypeNamePartDelimeter, Func<string, string> prefixResolver, out string error)
		{
			string str;
			string str2;
			int index = text.IndexOf('(');
			int num2 = text.IndexOf('[');
			if((index != -1) || (num2 != -1))
			{
				error = string.Empty;
				return null;
			}
			error = string.Empty;
			if(!XamlQualifiedName.Parse(text, genericTypeNamePartDelimeter, out str, out str2))
			{
				error = SR.Get("InvalidTypeString", new object[] { text });
				return null;
			}
			string str3 = prefixResolver(str);
			if(string.IsNullOrEmpty(str3))
			{
				error = SR.Get("PrefixNotFound", new object[] { str });
				return null;
			}
			return new XamlTypeName(str3, str2);
		}

		public IList<XamlTypeName> ParseList(string text, GenericTypeNamePartDelimeter customeGenericTypeNamePartDelimeter, out string error)
		{
			this._scanner = new GenericTypeNameScanner(text, customeGenericTypeNamePartDelimeter);
			this._inputText = text;
			this.StartStack();
			error = string.Empty;
			try
			{
				this._scanner.Read();
				this.P_XamlTypeNameList();
				if(this._scanner.Token != GenericTypeNameScannerToken.NONE)
				{
					this.ThrowOnBadInput();
				}
			}
			catch(TypeNameParserException exception)
			{
				error = exception.Message;
			}
			IList<XamlTypeName> list = null;
			if(string.IsNullOrEmpty(error))
			{
				list = this.CollectNameListFromStack();
			}
			return list;
		}

		public XamlTypeName ParseName(string text, GenericTypeNamePartDelimeter customeGenericTypeNamePartDelimeter, out string error)
		{
			error = string.Empty;
			this._scanner = new GenericTypeNameScanner(text, customeGenericTypeNamePartDelimeter);
			this._inputText = text;
			this.StartStack();
			try
			{
				this._scanner.Read();
				this.P_XamlTypeName();
				if(this._scanner.Token != GenericTypeNameScannerToken.NONE)
				{
					this.ThrowOnBadInput();
				}
			}
			catch(TypeNameParserException exception)
			{
				error = exception.Message;
			}
			XamlTypeName name = null;
			if(string.IsNullOrEmpty(error))
			{
				name = this.CollectNameFromStack();
			}
			return name;
		}

		private void StartStack( )
		{
			this._stack = new Stack<TypeNameFrame>();
			TypeNameFrame item = new TypeNameFrame();
			this._stack.Push(item);
		}

		private void ThrowOnBadInput( )
		{
			throw new TypeNameParserException(SR.Get("InvalidCharInTypeName", new object[] { this._scanner.ErrorCurrentChar, this._inputText }));
		}

		// Nested Types
		[Serializable]
		private class TypeNameParserException : Exception
		{
			// Methods
			public TypeNameParserException(string message)
				: base(message)
			{
			}

			protected TypeNameParserException(SerializationInfo si, StreamingContext sc)
				: base(si, sc)
			{
			}
		}
	}

	internal static class SR
	{
		// Methods
		internal static string Get(string id)
		{
			string str = ExtensionLibrary.Presentation.Properties.Resources.ResourceManager.GetString(id);
			if(str == null)
			{
				str = ExtensionLibrary.Presentation.Properties.Resources.ResourceManager.GetString("Unavailable");
			}
			return str;
		}

		internal static string Get(string id, params object[] args)
		{
			string format = ExtensionLibrary.Presentation.Properties.Resources.ResourceManager.GetString(id);
			if(format == null)
			{
				return ExtensionLibrary.Presentation.Properties.Resources.ResourceManager.GetString("Unavailable");
			}
			if((args != null) && (args.Length > 0))
			{
				format = string.Format(CultureInfo.CurrentCulture, format, args);
			}
			return format;
		}
	}
}
