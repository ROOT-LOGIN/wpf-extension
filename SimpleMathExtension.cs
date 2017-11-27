using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;

namespace ExtensionLibrary.Presentation.XamlMarkupExtension
{
    /// <summary>
    /// Base class for implementing simple mathmatic markup extension.
    /// </summary>
    public abstract class _MathExtensionBase : MarkupExtension
    {
        /// <summary>
        /// The factor.
        /// </summary>
        public double Factor { get; set; }

        /// <summary>
        /// When implemented in a derived class, returns an object that is provided as the value of the target property for this markup extension.
        /// _MathExtensionBase implement it as sealed, Derived class should override Value property instead.
        /// </summary>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// </returns>
        public sealed override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Value;
        }

        /// <summary>
        /// The calculated value.
        /// </summary>
        protected abstract double Value { get; }

    }

    /// <summary>
    /// Base class for implementing triangle mathmatic markup extension.
    /// </summary>
    public abstract class _TriangleMathExtensionBase : _MathExtensionBase
    {
        /// <summary>
        /// Indicating whether the factor is in degree or radian.
        /// </summary>
        public bool IsDeg { get; set; }

        protected double DegToRad(double value)
        {
            return IsDeg ? Math.PI * value / 180.0 : value;
        }

        protected double RadToDeg(double value)
        {
            return IsDeg ? value * 180 / Math.PI : value;
        }
    }

    /// <summary>
    /// Returns the sine value.
    /// </summary>
    public sealed class SinExtension : _TriangleMathExtensionBase
    {
        protected override double Value { get { return Math.Sin(DegToRad(Factor)); } }
    }

    /// <summary>
    /// Returns the hyperbolic sine value.
    /// </summary>
    public sealed class SinhExtension : _TriangleMathExtensionBase
    {
        protected override double Value { get { return Math.Sinh(DegToRad(Factor)); } }
    }

    /// <summary>
    /// Returns the cosine value.
    /// </summary>
    public sealed class CosExtension : _TriangleMathExtensionBase
    {
        protected override double Value { get { return Math.Cos(DegToRad(Factor)); } }
    }

    /// <summary>
    /// Returns the hyperbolic cosine value.
    /// </summary>
    public sealed class CoshExtension : _TriangleMathExtensionBase
    {
        protected override double Value { get { return Math.Cosh(DegToRad(Factor)); } }
    }

    /// <summary>
    /// Returns the tangent value.
    /// </summary>
    public sealed class TanExtension : _TriangleMathExtensionBase
    {
        protected override double Value { get { return Math.Tan(DegToRad(Factor)); } }
    }

    /// <summary>
    /// Returns the hyperbolic tangent value.
    /// </summary>
    public sealed class TanhExtension : _TriangleMathExtensionBase
    {
        protected override double Value { get { return Math.Tanh(DegToRad(Factor)); } }
    }

    /// <summary>
    /// Returns the arcsine value.
    /// </summary>
    public sealed class ArcSinExtension : _TriangleMathExtensionBase
    {
        protected override double Value { get { return RadToDeg(Math.Asin(Factor)); } }
    }

    /// <summary>
    /// Returns the arccos value.
    /// </summary>
    public sealed class ArcCosExtension : _TriangleMathExtensionBase
    {
        protected override double Value { get { return RadToDeg(Math.Acos(Factor)); } }
    }

    /// <summary>
    /// Returns the arctan value.
    /// </summary>
    public sealed class ArcTanExtension : _TriangleMathExtensionBase
    {
        protected override double Value { get { return RadToDeg(Math.Atan(Factor)); } }
    }

    /// <summary>
    /// Returns the log-e value.
    /// </summary>
    public sealed class LnExtension : _MathExtensionBase
    {
        protected override double Value { get { return Math.Log(Factor); } }
    }

    /// <summary>
    /// Returns the log-10 value.
    /// </summary>
    public sealed class LgExtension : _MathExtensionBase
    {
        protected override double Value { get { return Math.Log10(Factor); } }
    }

    /// <summary>
    /// Returns the log-r(default to 2) value.
    /// </summary>
    public sealed class LogExtension : _MathExtensionBase
    {
        public LogExtension( ) { Base = 2; }
        public double Base { get; set; }
        protected override double Value { get { return Math.Log(Factor, Base); } }
    }

    /// <summary>
    /// Returns the squal root value.
    /// </summary>
    public sealed class SqrtExtension : _MathExtensionBase
    {
        protected override double Value { get { return Math.Sqrt(Factor); } }
    }

    /// <summary>
    /// Returns the absolute value.
    /// </summary>
    public sealed class AbsExtension : _MathExtensionBase
    {
        protected override double Value { get { return Math.Abs(Factor); } }
    }

    /// <summary>
    /// Returns the value of e raised to the specified power.
    /// </summary>
    public sealed class ExpExtension : _MathExtensionBase
    {
        protected override double Value { get { return Math.Exp(Factor); } }
    }

    /// <summary>
    /// Returns the area of the circle with specified radius.
    /// </summary>
    public sealed class AreaExtension : _MathExtensionBase
    {
        protected override double Value { get { return Math.PI * (Factor * Factor); } }
    }
    
    /// <summary>
    /// Returns the circumference of the circle with specified radius.
    /// </summary>
    public sealed class CircExtension : _MathExtensionBase
    {
        protected override double Value { get { return 2 * Math.PI * Factor; } }
    }
}
