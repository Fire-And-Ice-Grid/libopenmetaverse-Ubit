/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Globalization;

namespace OpenMetaverse
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4 : IComparable<Vector4>, IEquatable<Vector4>
    {
        /// <summary>X value</summary>
        public float X;
        /// <summary>Y value</summary>
        public float Y;
        /// <summary>Z value</summary>
        public float Z;
        /// <summary>W value</summary>
        public float W;

        #region Constructors

        public Vector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Vector4(Vector2 value, float z, float w)
        {
            X = value.X;
            Y = value.Y;
            Z = z;
            W = w;
        }

        public Vector4(Vector3 value, float w)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = w;
        }

        public Vector4(float value)
        {
            X = value;
            Y = value;
            Z = value;
            W = value;
        }

        /// <summary>
        /// Constructor, builds a vector from a byte array
        /// </summary>
        /// <param name="byteArray">Byte array containing four four-byte floats</param>
        /// <param name="pos">Beginning position in the byte array</param>
        public Vector4(byte[] byteArray, int pos)
        {
            X = Utils.BytesToFloatSafepos(byteArray, pos);
            Y = Utils.BytesToFloatSafepos(byteArray, pos + 4);
            Z = Utils.BytesToFloatSafepos(byteArray, pos + 8);
            W = Utils.BytesToFloatSafepos(byteArray, pos + 12);
        }

        public Vector4(Vector4 value)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = value.W;
        }

        #endregion Constructors

        #region Public Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Abs()
        {
            X = Math.Abs(X);
            Y = Math.Abs(Y);
            Z = Math.Abs(Z);
            W = Math.Abs(W);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Vector4 v)
        {
            X += v.X;
            Y += v.Y;
            Z += v.Z;
            W += v.W;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sub(Vector4 v)
        {
            X -= v.X;
            Y -= v.Y;
            Z -= v.Z;
            W -= v.W;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clamp(float min, float max)
        {
            X = Utils.Clamp(X, min, max);
            Y = Utils.Clamp(Y, min, max);
            Z = Utils.Clamp(Z, min, max);
            W = Utils.Clamp(W, min, max);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Min(Vector4 v)
        {
            if (v.X < X) X = v.X;
            if (v.Y < Y) Y = v.Y;
            if (v.Z < Z) Z = v.Z;
            if (v.W < W) W = v.W;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Max(Vector4 v)
        {
            if (v.X > X) X = v.X;
            if (v.Y > Y) Y = v.Y;
            if (v.Z > Z) Z = v.Z;
            if (v.W > W) W = v.W;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float LengthSquared()
        {
            return (X * X) + (Y * Y) + (Z * Z) + (W * W);
        }

        public void Normalize()
        {
            float factor = LengthSquared();
            if (factor > 1e-6)
            {
                factor = 1f / (float)Math.Sqrt(factor);
                X *= factor;
                Y *= factor;
                Z *= factor;
                W *= factor;
            }
            else
                this = new Vector4();
        }

        /// <summary>
        /// Test if this vector is equal to another vector, within a given
        /// tolerance range
        /// </summary>
        /// <param name="vec">Vector to test against</param>
        /// <param name="tolerance">The acceptable magnitude of difference
        /// between the two vectors</param>
        /// <returns>True if the magnitude of difference between the two vectors
        /// is less than the given tolerance, otherwise false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ApproxEquals(Vector4 vec, float tolerance)
        {
            return Utils.ApproxEqual(X, vec.X, tolerance) &&
                    Utils.ApproxEqual(Y, vec.Y, tolerance) &&
                    Utils.ApproxEqual(Z, vec.Z, tolerance) &&
                    Utils.ApproxEqual(W, vec.W, tolerance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ApproxEquals(Vector4 vec)
        {
            return Utils.ApproxEqual(X, vec.X) &&
                    Utils.ApproxEqual(Y, vec.Y) &&
                    Utils.ApproxEqual(Z, vec.Z) &&
                    Utils.ApproxEqual(W, vec.W);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsZero()
        {
            if (X != 0)
                return false;
            if (Y != 0)
                return false;
            if (Z != 0)
                return false;
            if (W != 0)
                return false;
            return true;
        }
        /// <summary>
        /// IComparable.CompareTo implementation
        /// </summary>
        public int CompareTo(Vector4 vector)
        {
            return Length().CompareTo(vector.Length());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Dot(Vector4 value2)
        {
            return (X * value2.X) + (Y * value2.Y) + (Z * value2.Z) + (W * value2.W);
        }
        /// <summary>
        /// Test if this vector is composed of all finite numbers
        /// </summary>
        public bool IsFinite()
        {
            return (Utils.IsFinite(X) && Utils.IsFinite(Y) && Utils.IsFinite(Z) && Utils.IsFinite(W));
        }

        /// <summary>
        /// Builds a vector from a byte array
        /// </summary>
        /// <param name="byteArray">Byte array containing a 16 byte vector</param>
        /// <param name="pos">Beginning position in the byte array</param>
        public void FromBytes(byte[] byteArray, int pos)
        {
            X = Utils.BytesToFloatSafepos(byteArray, pos);
            Y = Utils.BytesToFloatSafepos(byteArray, pos + 4);
            Z = Utils.BytesToFloatSafepos(byteArray, pos + 8);
            W = Utils.BytesToFloatSafepos(byteArray, pos + 12);
        }

        /// <summary>
        /// Returns the raw bytes for this vector
        /// </summary>
        /// <returns>A 16 byte array containing X, Y, Z, and W</returns>
        public byte[] GetBytes()
        {
            byte[] dest = new byte[16];
            Utils.FloatToBytesSafepos(X, dest, 0);
            Utils.FloatToBytesSafepos(Y, dest, 4);
            Utils.FloatToBytesSafepos(Z, dest, 8);
            Utils.FloatToBytesSafepos(W, dest, 12);
            return dest;
        }

        /// <summary>
        /// Writes the raw bytes for this vector to a byte array
        /// </summary>
        /// <param name="dest">Destination byte array</param>
        /// <param name="pos">Position in the destination array to start
        /// writing. Must be at least 16 bytes before the end of the array</param>
        public void ToBytes(byte[] dest, int pos)
        {
            Utils.FloatToBytesSafepos(X, dest, pos);
            Utils.FloatToBytesSafepos(Y, dest, pos + 4);
            Utils.FloatToBytesSafepos(Z, dest, pos + 8);
            Utils.FloatToBytesSafepos(W, dest, pos + 12);
        }

        #endregion Public Methods

        #region Static Methods

        public static Vector4 Add(Vector4 value1, Vector4 value2)
        {
            return new Vector4(
                value1.W + value2.W,
                value1.X + value2.X,
                value1.Y + value2.Y,
                value1.Z + value2.Z
                );
        }

        public static Vector4 Clamp(Vector4 value1, float min, float max)
        {
            return new Vector4(
                Utils.Clamp(value1.X, min, max),
                Utils.Clamp(value1.Y, min, max),
                Utils.Clamp(value1.Z, min, max),
                Utils.Clamp(value1.W, min, max));
        }

        public static Vector4 Clamp(Vector4 value1, Vector4 min, Vector4 max)
        {
            return new Vector4(
                Utils.Clamp(value1.X, min.X, max.X),
                Utils.Clamp(value1.Y, min.Y, max.Y),
                Utils.Clamp(value1.Z, min.Z, max.Z),
                Utils.Clamp(value1.W, min.W, max.W));
        }

        public static float Distance(Vector4 value1, Vector4 value2)
        {
            return (float)Math.Sqrt(DistanceSquared(value1, value2));
        }

        public static float DistanceSquared(Vector4 value1, Vector4 value2)
        {
            return
                (value1.X - value2.X) * (value1.X - value2.X) +
                (value1.Y - value2.Y) * (value1.Y - value2.Y) +
                (value1.Z - value2.Z) * (value1.Z - value2.Z) +
                (value1.W - value2.W) * (value1.W - value2.W);
        }

        public static Vector4 Divide(Vector4 value1, Vector4 value2)
        {
            return new Vector4(
                value1.X / value2.X,
                value1.Y / value2.Y,
                value1.Z / value2.Z,
                value1.W / value2.W
                );
        }

        public static Vector4 Divide(Vector4 value1, float divider)
        {
            float factor = 1f / divider;
            return new Vector4(
                value1.X * factor,
                value1.Y * factor,
                value1.Z * factor,
                value1.W * factor
                );
        }

        public static float Dot(Vector4 vector1, Vector4 vector2)
        {
            return (vector1.X * vector2.X) + (vector1.Y * vector2.Y) + (vector1.Z * vector2.Z) + (vector1.W * vector2.W);
        }

        public static Vector4 Lerp(Vector4 value1, Vector4 value2, float amount)
        {
            return new Vector4(
                Utils.Lerp(value1.X, value2.X, amount),
                Utils.Lerp(value1.Y, value2.Y, amount),
                Utils.Lerp(value1.Z, value2.Z, amount),
                Utils.Lerp(value1.W, value2.W, amount));
        }

        public static Vector4 Max(Vector4 value1, Vector4 value2)
        {
            return new Vector4(
               Math.Max(value1.X, value2.X),
               Math.Max(value1.Y, value2.Y),
               Math.Max(value1.Z, value2.Z),
               Math.Max(value1.W, value2.W));
        }

        public static Vector4 Min(Vector4 value1, Vector4 value2)
        {
            return new Vector4(
               Math.Min(value1.X, value2.X),
               Math.Min(value1.Y, value2.Y),
               Math.Min(value1.Z, value2.Z),
               Math.Min(value1.W, value2.W));
        }

        public static Vector4 Multiply(Vector4 value1, Vector4 value2)
        {
            return new Vector4(
                value1.X * value2.X,
                value1.Y * value2.Y,
                value1.Z * value2.Z,
                value1.W * value2.W);
        }

        public static Vector4 Multiply(Vector4 value1, float scaleFactor)
        {
            return new Vector4(
                value1.X * scaleFactor,
                value1.Y * scaleFactor,
                value1.Z * scaleFactor,
                value1.W * scaleFactor);
        }

        public static Vector4 Negate(Vector4 value)
        {
            return new Vector4(
                -value.X,
                -value.Y,
                -value.Z,
                -value.W);
        }

        public static Vector4 Normalize(Vector4 vector)
        {
            float factor = vector.LengthSquared();
            if (factor > 1e-6)
            {
                factor = 1f / (float)Math.Sqrt(factor);
                return new Vector4(
                    vector.X * factor,
                    vector.Y * factor,
                    vector.Z * factor,
                    vector.W * factor);
            }
            return Vector4.Zero;
        }

        public static Vector4 SmoothStep(Vector4 value1, Vector4 value2, float amount)
        {
            return new Vector4(
                Utils.SmoothStep(value1.X, value2.X, amount),
                Utils.SmoothStep(value1.Y, value2.Y, amount),
                Utils.SmoothStep(value1.Z, value2.Z, amount),
                Utils.SmoothStep(value1.W, value2.W, amount));
        }

        public static Vector4 Subtract(Vector4 value1, Vector4 value2)
        {
            return new Vector4(
                value1.W - value2.W,
                value1.X - value2.X,
                value1.Y - value2.Y,
                value1.Z - value2.Z);
        }

        public static Vector4 Transform(Vector2 position, Matrix4 matrix)
        {
            return new Vector4(
                (position.X * matrix.M11) + (position.Y * matrix.M21) + matrix.M41,
                (position.X * matrix.M12) + (position.Y * matrix.M22) + matrix.M42,
                (position.X * matrix.M13) + (position.Y * matrix.M23) + matrix.M43,
                (position.X * matrix.M14) + (position.Y * matrix.M24) + matrix.M44);
        }

        public static Vector4 Transform(Vector3 position, Matrix4 matrix)
        {
            return new Vector4(
                (position.X * matrix.M11) + (position.Y * matrix.M21) + (position.Z * matrix.M31) + matrix.M41,
                (position.X * matrix.M12) + (position.Y * matrix.M22) + (position.Z * matrix.M32) + matrix.M42,
                (position.X * matrix.M13) + (position.Y * matrix.M23) + (position.Z * matrix.M33) + matrix.M43,
                (position.X * matrix.M14) + (position.Y * matrix.M24) + (position.Z * matrix.M34) + matrix.M44);
        }

        public static Vector4 Transform(Vector4 vector, Matrix4 matrix)
        {
            return new Vector4(
                (vector.X * matrix.M11) + (vector.Y * matrix.M21) + (vector.Z * matrix.M31) + (vector.W * matrix.M41),
                (vector.X * matrix.M12) + (vector.Y * matrix.M22) + (vector.Z * matrix.M32) + (vector.W * matrix.M42),
                (vector.X * matrix.M13) + (vector.Y * matrix.M23) + (vector.Z * matrix.M33) + (vector.W * matrix.M43),
                (vector.X * matrix.M14) + (vector.Y * matrix.M24) + (vector.Z * matrix.M34) + (vector.W * matrix.M44));
        }

        public static Vector4 Parse(string val)
        {
            char[] splitChar = { ',' };
            string[] split = val.Replace("<", String.Empty).Replace(">", String.Empty).Split(splitChar);
            return new Vector4(
                float.Parse(split[0].Trim(), Utils.EnUsCulture),
                float.Parse(split[1].Trim(), Utils.EnUsCulture),
                float.Parse(split[2].Trim(), Utils.EnUsCulture),
                float.Parse(split[3].Trim(), Utils.EnUsCulture));
        }

        public static bool TryParse(string val, out Vector4 result)
        {
            try
            {
                result = Parse(val);
                return true;
            }
            catch (Exception)
            {
                result = new Vector4();
                return false;
            }
        }

        #endregion Static Methods

        #region Overrides

        public override bool Equals(object obj)
        {
            if (!(obj is Vector4))
                return false;

            Vector4 other = (Vector4)obj;
            if (X != other.X)
                return false;
            if (Y != other.Y)
                return false;
            if (Z != other.Z)
                return false;
            if (W != other.W)
                return false;
            return true;
        }

        public bool Equals(Vector4 other)
        {
            if (X != other.X)
                return false;
            if (Y != other.Y)
                return false;
            if (Z != other.Z)
                return false;
            if (W != other.W)
                return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NotEqual(Vector4 other)
        {
            if (X != other.X)
                return true;
            if (Y != other.Y)
                return true;
            if (Z != other.Z)
                return true;
            if (W != other.W)
                return true;
            return false;
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format(Utils.EnUsCulture, "<{0}, {1}, {2}, {3}>", X, Y, Z, W);
        }

        /// <summary>
        /// Get a string representation of the vector elements with up to three
        /// decimal digits and separated by spaces only
        /// </summary>
        /// <returns>Raw string representation of the vector</returns>
        public string ToRawString()
        {
            CultureInfo enUs = new CultureInfo("en-us");
            enUs.NumberFormat.NumberDecimalDigits = 3;

            return String.Format(enUs, "{0} {1} {2} {3}", X, Y, Z, W);
        }

        #endregion Overrides

        #region Operators

        public static bool operator ==(Vector4 value1, Vector4 value2)
        {
            if (value1.X != value2.X)
                return false;
            if (value1.Y != value2.Y)
                return false;
            if (value1.Z != value2.Z)
                return false;
            if (value1.W != value2.W)
                return false;
            return true;
        }

        public static bool operator !=(Vector4 value1, Vector4 value2)
        {
            if (value1.X != value2.X)
                return true;
            if (value1.Y != value2.Y)
                return true;
            if (value1.Z != value2.Z)
                return true;
            if (value1.W != value2.W)
                return true;
            return false;
        }

        public static Vector4 operator +(Vector4 value1, Vector4 value2)
        {
            return new Vector4(
                value1.X + value2.X,
                value1.Y + value2.Y,
                value1.Z + value2.Z,
                value1.W + value2.W);
        }

        public static Vector4 operator -(Vector4 value)
        {
            return new Vector4(-value.X, -value.Y, -value.Z, -value.W);
        }

        public static Vector4 operator -(Vector4 value1, Vector4 value2)
        {
            return new Vector4(
                value1.X - value2.X,
                value1.Y - value2.Y,
                value1.Z - value2.Z,
                value1.W - value2.W);
        }

        public static Vector4 operator *(Vector4 value1, Vector4 value2)
        {
            return new Vector4(
                value1.X * value2.X,
                value1.Y * value2.Y,
                value1.Z * value2.Z,
                value1.W * value2.W);
        }

        public static Vector4 operator *(Vector4 value1, float scaleFactor)
        {
            return new Vector4(
                value1.X * scaleFactor,
                value1.Y * scaleFactor,
                value1.Z * scaleFactor,
                value1.W * scaleFactor);
        }

        public static Vector4 operator /(Vector4 value1, Vector4 value2)
        {
            return new Vector4(
                value1.X / value2.X,
                value1.Y / value2.Y,
                value1.Z / value2.Z,
                value1.W / value2.W);
        }

        public static Vector4 operator /(Vector4 value1, float divider)
        {
            float scaleFactor = 1f / divider;
            return new Vector4(
                value1.X * scaleFactor,
                value1.Y * scaleFactor,
                value1.Z * scaleFactor,
                value1.W * scaleFactor);
        }

        #endregion Operators

        /// <summary>A vector with a value of 0,0,0,0</summary>
        public readonly static Vector4 Zero = new Vector4();
        /// <summary>A vector with a value of 1,1,1,1</summary>
        public readonly static Vector4 One = new Vector4(1f, 1f, 1f, 1f);
        /// <summary>A vector with a value of 1,0,0,0</summary>
        public readonly static Vector4 UnitX = new Vector4(1f, 0f, 0f, 0f);
        /// <summary>A vector with a value of 0,1,0,0</summary>
        public readonly static Vector4 UnitY = new Vector4(0f, 1f, 0f, 0f);
        /// <summary>A vector with a value of 0,0,1,0</summary>
        public readonly static Vector4 UnitZ = new Vector4(0f, 0f, 1f, 0f);
        /// <summary>A vector with a value of 0,0,0,1</summary>
        public readonly static Vector4 UnitW = new Vector4(0f, 0f, 0f, 1f);
        public readonly static Vector4 MinValue = new Vector4(float.MinValue, float.MinValue, float.MinValue, float.MinValue);
        public readonly static Vector4 MaxValue = new Vector4(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
    }
}
