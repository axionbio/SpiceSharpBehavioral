﻿using SpiceSharp;
using System;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// Supporting functions for behavioral models.
    /// </summary>
    public static class Functions
    {
        /// <summary>
        /// Divides two numbers while avoiding division by 0 using a fudge factor.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <param name="fudgeFactor">The fudge factor.</param>
        /// <returns>
        /// The division.
        /// </returns>
        public static double SafeDivide(double left, double right, double fudgeFactor)
        {
            if (right < 0)
                right -= fudgeFactor;
            else
                right += fudgeFactor;
            if (right.Equals(0.0))
                return double.PositiveInfinity;
            return left / right;
        }

        /// <summary>
        /// Checks if two numbers are equal.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <param name="relTol">The relative tolerance.</param>
        /// <param name="absTol">The absolute tolerance.</param>
        /// <returns>
        /// <c>true</c> if the two numbers are within tolerance; otherwise <c>false</c>.
        /// </returns>
        public static bool Equals(double left, double right, double relTol, double absTol)
        {
            var tol = Math.Max(Math.Abs(left), Math.Abs(right)) * relTol + absTol;
            if (Math.Abs(left - right) <= tol)
                return true;
            return false;
        }

        /// <summary>
        /// Takes the natural logarithm.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The natural logarithm.</returns>
        public static double Log(double arg)
        {
            if (arg < 0)
                return double.PositiveInfinity;
            return Math.Log(arg);
        }

        /// <summary>
        /// Takes the logarithm base 10.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The logarithm baes 10.</returns>
        public static double Log10(double arg)
        {
            if (arg < 0)
                return double.PositiveInfinity;
            return Math.Log10(arg);
        }

        /// <summary>
        /// Raises a number to a power. The function is made symmetrical.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The result.</returns>
        public static double Power(double left, double right) => Math.Pow(Math.Abs(left), right);

        /// <summary>
        /// Raises a number to a power. The function is made antisymmetrical.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The result.</returns>
        public static double Power2(double left, double right)
        {
            if (left < 0)
                return -Math.Pow(-left, right);
            else
                return Math.Pow(left, right);
        }

        /// <summary>
        /// The step function.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The result.</returns>
        public static double Step(double arg)
        {
            if (arg < 0.0)
                return 0.0;
            else if (arg > 0.0)
                return 1.0;
            else
                return 0.5; /* Ick! */
        }

        /// <summary>
        /// The step function with an initial ramp.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The result.</returns>
        public static double Step2(double arg)
        {
            if (arg <= 0.0)
                return 0.0;
            else if (arg <= 1.0)
                return arg;
            else /* if (arg > 1.0) */
                return 1.0;
        }

        /// <summary>
        /// The derivative of <see cref="Step2(double)"/>.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        public static double Step2Derivative(double arg)
        {
            if (arg <= 0.0)
                return 0.0;
            else if (arg <= 1.0)
                return 1.0;
            else
                return 0.0;
        }

        /// <summary>
        /// The ramp function.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The result.</returns>
        public static double Ramp(double arg)
        {
            if (arg < 0)
                return 0.0;
            return arg;
        }

        /// <summary>
        /// The derivative of <see cref="RampDerivative(double)"/>
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        public static double RampDerivative(double arg)
        {
            if (arg < 0)
                return 0.0;
            return 1.0;
        }

        /// <summary>
        /// Piece-wise linear interpolation.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="data">The interpolation data.</param>
        /// <returns>The interpolated value.</returns>
        public static double Pwl(double arg, Point[] data)
        {
            // Narrow in on the index for the piece-wise linear function
            int k0 = 0, k1 = data.Length;
            while (k1 - k0 > 1)
            {
                int k = (k0 + k1) / 2;
                if (data[k].X > arg)
                    k1 = k;
                else
                    k0 = k;
            }
            return data[k0].Y + (arg - data[k0].X) * (data[k1].Y - data[k0].Y) / (data[k1].X - data[k0].X);
        }

        /// <summary>
        /// The derivative of <see cref="Pwl(double, Point[])" />.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="data">The interpolation data.</param>
        /// <returns>The interpolated value.</returns>
        public static double PwlDerivative(double arg, Point[] data)
        {
            // Narrow in on the index for the piece-wise linear function
            int k0 = 0, k1 = data.Length;
            while (k1 - k0 > 1)
            {
                int k = (k0 + k1) / 2;
                if (data[k].X > arg)
                    k1 = k;
                else
                    k0 = k;
            }
            return (data[k1].Y - data[k0].Y) / (data[k1].X - data[k0].X);
        }

        /// <summary>
        /// Squares a value.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The result.</returns>
        public static double Square(double arg) => arg * arg;

        /// <summary>
        /// Calculates the square root of a number.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The result.</returns>
        public static double Sqrt(double arg)
        {
            if (arg < 0.0)
                return double.PositiveInfinity;
            return Math.Sqrt(arg);
        }

        /// <summary>
        /// Returns the sign of the argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>-1.0 if negative; 1.0 if positive; otherwise 0.0.</returns>
        public static double Sign(double arg)
        {
            if (arg < 0)
                return -1.0;
            if (arg > 0)
                return 1.0;
            return 0.0;
        }

        /// <summary>
        /// Parses a number literal.
        /// </summary>
        /// <param name="literal">The literal.</param>
        /// <returns>The parsed number.</returns>
        /// <exception cref="Exception">Thrown if the number can't be parsed.</exception>
        public static double ParseNumber(string literal)
        {
            double value = 0.0;
            int index = 0;
            if ((literal[index] < '0' || literal[index] > '9') && literal[index] != '.')
                throw new Exception("Cannot read the number '{0}'".FormatString(literal));

            // Read integer part
            while (index < literal.Length && (literal[index] >= '0' && literal[index] <= '9'))
                value = (value * 10.0) + (literal[index++] - '0');

            // Read decimal part
            if (index < literal.Length && (literal[index] == '.'))
            {
                index++;
                double mult = 1.0;
                while (index < literal.Length && (literal[index] >= '0' && literal[index] <= '9'))
                {
                    value = (value * 10.0) + (literal[index++] - '0');
                    mult *= 10.0;
                }

                value /= mult;
            }

            if (index < literal.Length)
            {
                // Scientific notation
                if (literal[index] == 'e' || literal[index] == 'E')
                {
                    index++;
                    var exponent = 0;
                    var neg = false;
                    if (index < literal.Length && (literal[index] == '+' || literal[index] == '-'))
                    {
                        if (literal[index] == '-')
                            neg = true;
                        index++;
                    }

                    // Get the exponent
                    while (index < literal.Length && (literal[index] >= '0' && literal[index] <= '9'))
                        exponent = (exponent * 10) + (literal[index++] - '0');

                    // Integer exponentation
                    var mult = 1.0;
                    var b = 10.0;
                    while (exponent != 0)
                    {
                        if ((exponent & 0x01) == 0x01)
                            mult *= b;

                        b *= b;
                        exponent >>= 1;
                    }

                    if (neg)
                        value /= mult;
                    else
                        value *= mult;
                }
                else
                {
                    // Spice modifiers
                    switch (literal[index])
                    {
                        case 't':
                        case 'T': value *= 1.0e12; index++; break;
                        case 'g':
                        case 'G': value *= 1.0e9; index++; break;
                        case 'x':
                        case 'X': value *= 1.0e6; index++; break;
                        case 'k':
                        case 'K': value *= 1.0e3; index++; break;
                        case 'u':
                        case 'µ':
                        case 'U': value /= 1.0e6; index++; break;
                        case 'n':
                        case 'N': value /= 1.0e9; index++; break;
                        case 'p':
                        case 'P': value /= 1.0e12; index++; break;
                        case 'f':
                        case 'F': value /= 1.0e15; index++; break;
                        case 'm':
                        case 'M':
                            if (index + 2 < literal.Length &&
                                (literal[index + 1] == 'e' || literal[index + 1] == 'E') &&
                                (literal[index + 2] == 'g' || literal[index + 2] == 'G'))
                            {
                                value *= 1.0e6;
                                index += 3;
                            }
                            else if (index + 2 < literal.Length &&
                                (literal[index + 1] == 'i' || literal[index + 1] == 'I') &&
                                (literal[index + 2] == 'l' || literal[index + 2] == 'L'))
                            {
                                value *= 25.4e-6;
                                index += 3;
                            }
                            else
                            {
                                value /= 1.0e3;
                                index++;
                            }
                            break;
                    }
                }
            }
            return value;
        }
    }
}
