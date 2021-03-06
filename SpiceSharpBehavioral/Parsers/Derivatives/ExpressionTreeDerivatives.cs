﻿using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A specific implementation of derivatives for expression trees.
    /// </summary>
    public class ExpressionTreeDerivatives : Derivatives<Expression>
    {
        /// <summary>
        /// Zero.
        /// </summary>
        public static readonly Expression Zero = Expression.Constant(0.0);

        /// <summary>
        /// One.
        /// </summary>
        public static readonly Expression One = Expression.Constant(1.0);

        /// <summary>
        /// Method info for Log().
        /// </summary>
        public static readonly MethodInfo LogInfo = typeof(Math).GetTypeInfo().GetMethod("Log", new[] { typeof(double) });

        /// <summary>
        /// Method info for Pow().
        /// </summary>
        public static readonly MethodInfo PowInfo = typeof(ExpressionTreeDerivatives).GetTypeInfo().GetMethod("SafePow", new[] { typeof(double), typeof(double) });

        /// <summary>
        /// Method info for Abs().
        /// </summary>
        public static readonly MethodInfo AbsInfo = typeof(Math).GetTypeInfo().GetMethod("Abs", new[] { typeof(double) });

        /// <summary>
        /// Method info for Square().
        /// </summary>
        public static readonly MethodInfo SquareInfo = typeof(ExpressionTreeDerivatives).GetTypeInfo().GetMethod("Square", new[] { typeof(double) });

        /// <summary>
        /// Method for division.
        /// </summary>
        private static readonly MethodInfo Division = typeof(ExpressionTreeDerivatives).GetTypeInfo().GetMethod("SafeDivide", new[] { typeof(double), typeof(double) });

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionTreeDerivatives"/>.
        /// </summary>
        public ExpressionTreeDerivatives()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionTreeDerivatives"/>.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public ExpressionTreeDerivatives(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Get a method that computes the derivative.
        /// </summary>
        /// <param name="index">The derivative index, 0 for the function value.</param>
        /// <returns></returns>
        public Func<double> GetDerivative(int index)
        {
            if (this[index] == null)
                return null;
            return Expression.Lambda<Func<double>>(this[index]).Compile();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Squares an input.
        /// </summary>
        /// <param name="x">The input.</param>
        /// <returns>The squared value.</returns>
        public static double Square(double x) => x * x;

        /// <summary>
        /// Or the derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// The derivatives.
        /// </returns>
        public override Derivatives<Expression> Or(Derivatives<Expression> b)
        {
            var result = new ExpressionTreeDerivatives();
            result[0] = Expression.Condition(
                Expression.And(
                    Expression.Equal(this[0], Zero),
                    Expression.Equal(b[0], Zero)), Zero, One);
            return result;
        }

        /// <summary>
        /// And the derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// The derivatives.
        /// </returns>
        public override Derivatives<Expression> And(Derivatives<Expression> b)
        {
            var result = new ExpressionTreeDerivatives();
            result[0] = Expression.Condition(
                Expression.Or(
                    Expression.Equal(this[0], Zero),
                    Expression.Equal(b[0], Zero)), Zero, One);
            return result;
        }

        /// <summary>
        /// Conditional derivatives.
        /// </summary>
        /// <param name="iftrue">Argument if true.</param>
        /// <param name="iffalse">Argument if false.</param>
        /// <returns>
        /// The derivatives.
        /// </returns>
        public override Derivatives<Expression> IfThenElse(Derivatives<Expression> iftrue, Derivatives<Expression> iffalse)
        {
            if (this[0] == null)
                return iffalse;
            var size = Math.Max(iftrue.Count, iffalse.Count);
            var result = new ExpressionTreeDerivatives(size);
            var notcondition = Expression.Equal(this[0], Zero);
            for (var i = 0; i < size; i++)
            {
                var iftruei = iftrue[i];
                var iffalsei = iffalse[i];
                if (iftruei == null && iffalsei == null)
                    continue;
                result[i] = Expression.Condition(notcondition, iffalsei ?? Zero, iftruei ?? Zero);
            }
            return result;
        }

        /// <summary>
        /// Raises the derivatives to a power.
        /// </summary>
        /// <param name="exponent">The exponent.</param>
        /// <returns>
        /// The raised power.
        /// </returns>
        public override Derivatives<Expression> Pow(Derivatives<Expression> exponent)
        {
            var size = Math.Max(Count, exponent.Count);
            var result = new ExpressionTreeDerivatives(size);
            Expression a0 = this[0], b0 = exponent[0];
            if (a0 == null && b0 == null)
            {
                // This doesn't make much sense (0^0 is invalid) but Math.Pow does it like that so we will too
                result[0] = One;
                return result;
            }
            if (a0 == null)
                a0 = Zero;
            if (b0 == null)
            {
                // (fx)^0 = 1
                result[0] = One;
                return result;
            }
            result[0] = Expression.Call(PowInfo, a0, b0);
            if (a0 != One && a0 != Zero)
                a0 = Expression.Call(AbsInfo, a0);
            for (var i = 1; i < size; i++)
            {
                // (fx^b)' = b * fx^(b-1) * f'x
                if (this[i] != null)
                {
                    result[i] = Expression.Multiply(b0, Expression.Multiply(this[i],
                            Expression.Call(PowInfo, a0, Expression.Subtract(b0, One))));
                }

                // (fx^gx)' = (e^(gx*ln(fx)))'
                // = fx^(gx-1)*f'x + fx^gx*ln(fx)*g'x
                if (exponent[i] != null)
                {
                    var contribution = Expression.Multiply(Expression.Call(PowInfo, a0, b0),
                        Expression.Multiply(Expression.Call(LogInfo, a0), exponent[i]));
                    result[i] = result[i] == null ? contribution :
                        Expression.Add(result[i], contribution);
                }
            }
            return result;
        }

        /// <summary>
        /// Safe version of computing powers.
        /// </summary>
        /// <param name="a">The base.</param>
        /// <param name="b">The exponent.</param>
        /// <returns></returns>
        public static double SafePow(double a, double b)
        {
            if (a.Equals(0.0) && b < 0)
                a += FudgeFactor;
            if (a < 0)
                return -Math.Pow(-a, b);
            return Math.Pow(a, b);
        }

        /// <summary>
        /// Check for equality.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// A value representing true if equal.
        /// </returns>
        public override Derivatives<Expression> Equal(Derivatives<Expression> b)
        {
            var result = new ExpressionTreeDerivatives();
            result[0] = Expression.Condition(Expression.Equal(this[0], b[0]), One, Zero);
            return result;
        }

        /// <summary>
        /// Check for inequality.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// A value representing true if not equal.
        /// </returns>
        public override Derivatives<Expression> NotEqual(Derivatives<Expression> b)
        {
            var result = new ExpressionTreeDerivatives();
            result[0] = Expression.Condition(Expression.NotEqual(this[0], b[0]), One, Zero);
            return result;
        }

        /// <summary>
        /// Negate the derivatives.
        /// </summary>
        /// <returns>
        /// The derivatives.
        /// </returns>
        public override Derivatives<Expression> Negate()
        {
            var result = new ExpressionTreeDerivatives(Count);
            for (var i = 0; i < Count; i++)
            {
                if (this[i] != null)
                    result[i] = Expression.Negate(this[i]);
            }
            return result;
        }

        /// <summary>
        /// Not (binary) the derivatives.
        /// </summary>
        /// <returns>
        /// The derivatives.
        /// </returns>
        public override Derivatives<Expression> Not()
        {
            var result = new ExpressionTreeDerivatives(Count);
            if (this[0] != null)
                result[0] = Expression.Condition(Expression.Equal(this[0], Zero), One, Zero);
            else
                result[0] = One;
            return result;
        }

        /// <summary>
        /// Add derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// The sum.
        /// </returns>
        public override Derivatives<Expression> Add(Derivatives<Expression> b)
        {
            var size = Math.Max(Count, b.Count);
            var result = new ExpressionTreeDerivatives(size);
            for (var i = 0; i < size; i++)
            {
                if (this[i] == null)
                    result[i] = b[i];
                else
                {
                    if (b[i] == null)
                        result[i] = this[i];
                    else
                        result[i] = Expression.Add(this[i], b[i]);
                }
            }
            return result;
        }

        /// <summary>
        /// Subtract derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// The difference.
        /// </returns>
        public override Derivatives<Expression> Subtract(Derivatives<Expression> b)
        {
            var size = Math.Max(Count, b.Count);
            var result = new ExpressionTreeDerivatives(size);
            for (var i = 0; i < size; i++)
            {
                if (this[i] == null)
                {
                    if (b[i] == null)
                        result[i] = null;
                    else
                        result[i] = Expression.Negate(b[i]);
                }
                else
                {
                    if (b[i] == null)
                        result[i] = this[i];
                    else
                        result[i] = Expression.Subtract(this[i], b[i]);
                }
            }
            return result;
        }

        /// <summary>
        /// Multiply derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// The multiplied result.
        /// </returns>
        public override Derivatives<Expression> Multiply(Derivatives<Expression> b)
        {
            var size = Math.Max(Count, b.Count);
            var result = new ExpressionTreeDerivatives(size);
            Expression a0 = this[0], b0 = b[0];

            if (a0 == null || b0 == null)
                return result; // x*0 = 0 and so is all its derivatives
            else
                result[0] = Expression.Multiply(a0, b0);

            for (var i = 1; i < size; i++)
            {
                if (this[i] == null)
                {
                    if (b[i] == null)
                        result[i] = null;
                    else
                        // (a*gx)' = a*g'x
                        result[i] = Expression.Multiply(a0, b[i]);
                }
                else
                {
                    if (b[i] == null)
                        // (fx*b)' = f'x*b
                        result[i] = Expression.Multiply(this[i], b0);
                    else
                        // (fx*gx)' = fx*g'x + f'x*gx
                        result[i] = Expression.Add(
                            Expression.Multiply(a0, b[i]),
                            Expression.Multiply(this[i], b0));
                }
            }
            return result;
        }

        /// <summary>
        /// Divide derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// The divided result.
        /// </returns>
        public override Derivatives<Expression> Divide(Derivatives<Expression> b)
        {
            var size = Math.Max(Count, b.Count);
            var result = new ExpressionTreeDerivatives(size);

            Expression a0 = this[0], b0 = b[0];
            if (a0 == null)
                return null;
            if (b0 == null)
                throw new DivideByZeroException();
            result[0] = Expression.Call(Division, a0, b0);
            for (var i = 1; i < size; i++)
            {
                if (this[i] == null)
                {
                    if (b[i] == null)
                        result[i] = null;
                    else
                        // (a/gx)' = -a/gx^2*g'x
                        result[i] = Expression.Call(Division,
                            Expression.Negate(Expression.Multiply(a0, b[i])),
                            Expression.Call(SquareInfo, b0));
                }
                else
                {
                    if (b[i] == null)
                        // (fx/b)' = f'x/b
                        result[i] = Expression.Call(Division, this[i], b0);
                    else
                        // (fx/gx)' = (f'x*gx-fx*g'x)/gx^2
                        result[i] = Expression.Call(Division,
                            Expression.Subtract(
                                Expression.Multiply(this[i], b0),
                                Expression.Multiply(a0, b[i])),
                            Expression.Call(SquareInfo, b0));
                }
            }
            return result;
        }

        /// <summary>
        /// Safe version of division where division by 0 is avoided.
        /// </summary>
        /// <param name="a">The numerator.</param>
        /// <param name="b">The denominator.</param>
        /// <returns></returns>
        public static double SafeDivide(double a, double b)
        {
            if (b >= 0)
                b += FudgeFactor;
            else
                b -= FudgeFactor;
            return a / b;
        }
        
        /// <summary>
        /// Modulo operation on derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// The remainder of the division.
        /// </returns>
        public override Derivatives<Expression> Modulo(Derivatives<Expression> b)
        {
            var result = new ExpressionTreeDerivatives();
            if (this[0] == null)
                return result;
            if (b[0] == null)
                throw new Exception("Modulo by 0");
            result[0] = Expression.Modulo(this[0], b[0]);
            return result;
        }

        /// <summary>
        /// Check greater than.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// A value representing true if this is greater.
        /// </returns>
        public override Derivatives<Expression> GreaterThan(Derivatives<Expression> b)
        {
            var result = new ExpressionTreeDerivatives();
            Expression a0 = this[0], b0 = b[0];
            if (a0 == null && b0 == null)
                return result;
            a0 = a0 ?? Zero;
            b0 = b0 ?? Zero;
            result[0] = Expression.Condition(Expression.GreaterThan(a0, b0), One, Zero);
            return result;
        }

        /// <summary>
        /// Check less than.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// A value representing true if this is less.
        /// </returns>
        public override Derivatives<Expression> LessThan(Derivatives<Expression> b)
        {
            var result = new ExpressionTreeDerivatives();
            Expression a0 = this[0], b0 = b[0];
            if (a0 == null && b0 == null)
                return result;
            a0 = a0 ?? Zero;
            b0 = b0 ?? Zero;
            result[0] = Expression.Condition(Expression.LessThan(a0, b0), One, Zero); return result;
        }

        /// <summary>
        /// Check greater or equal.
        /// </summary>
        /// <param name="b">The operand.</param>
        /// <returns>
        /// A value representing true if this is greater or equal.
        /// </returns>
        public override Derivatives<Expression> GreaterOrEqual(Derivatives<Expression> b)
        {
            var result = new ExpressionTreeDerivatives();
            Expression a0 = this[0], b0 = b[0];
            if (a0 == null && b0 == null)
                return result;
            a0 = a0 ?? Zero;
            b0 = b0 ?? Zero;
            result[0] = Expression.Condition(Expression.GreaterThanOrEqual(a0, b0), One, Zero); return result;
        }

        /// <summary>
        /// Check less or equal.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// A value representing true if this is less or equal.
        /// </returns>
        public override Derivatives<Expression> LessOrEqual(Derivatives<Expression> b)
        {
            var result = new ExpressionTreeDerivatives();
            Expression a0 = this[0], b0 = b[0];
            if (a0 == null && b0 == null)
                return result;
            a0 = a0 ?? Zero;
            b0 = b0 ?? Zero;
            result[0] = Expression.Condition(Expression.LessThanOrEqual(a0, b0), One, Zero);
            return result;
        }
    }
}
