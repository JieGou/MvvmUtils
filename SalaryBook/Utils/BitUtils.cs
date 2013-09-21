﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Pellared.Utils
{
    public static class BitUtils
    {
        public static byte DecimalToBcd(int dec)
        {
            if (dec > 99) throw new ArgumentOutOfRangeException("dec", "Number is above 99");

            return (byte)(((dec / 10) << 4) + (dec % 10));
        }

        public static int BcdToDecimal(byte bcd)
        {
            if ((bcd >> 4) < 10) throw new ArgumentOutOfRangeException("bcd", "High digit is above 9");
            if ((bcd % 16) < 10) throw new ArgumentOutOfRangeException("bcd", "Low digit is above 9");

            return ((bcd >> 4) * 10) + bcd % 16;
        }

        public static int BcdToDecimal(IEnumerable<byte> bcd)
        {
            Contract.Requires<ArgumentNullException>(bcd != null, "bcd");
            
            int result = 0;
            int exping = 1;
            foreach (var item in bcd)
            {
                result = (exping * result) + BcdToDecimal(item);
                exping *= 100;
            }

            return result;
        }

        /// <summary>
        /// Creates an array from a BitArray.
        /// </summary>
        /// <param name="bits">BitArray instance.</param>
        /// <returns>Array of bytes.</returns>
        public static byte[] ToByteArray(this BitArray bits, bool fromOldestBit)
        {
            Contract.Requires<ArgumentNullException>(bits != null, "bits");

            int numBytes = ((bits.Length - 1) / 8) + 1;

            byte[] bytes = new byte[numBytes];
            int byteIndex = 0, bitIndex = 0;

            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    if (fromOldestBit)
                        bytes[byteIndex] |= (byte)(1 << (7 - bitIndex));
                    else
                        bytes[byteIndex] |= (byte)(1 << bitIndex);

                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return bytes;
        }

        /// <summary>
        /// Get the bit value in a byte.
        /// </summary>
        /// <param name="pByte">The byte where the value is encoded.</param>
        /// <param name="bitNo">The number of the bit (zero-based index).</param>
        /// <returns>Value of the bit.</returns>
        public static bool GetBit(this byte pByte, int bitNo)
        {
            return (pByte & (1 << bitNo)) != 0;
        }

        /// <summary>
        /// Set the bit value in a byte.
        /// </summary>
        /// <param name="pByte">The byte where the value is encoded.</param>
        /// <param name="bitNo">The number of the bit (zero-based index).</param>
        /// <param name="value">Value of the bit.</param>
        /// <returns>Byte with changed bit.</returns>
        public static byte SetBit(this byte pByte, int bitNo, bool value)
        {
            byte result;
            if (value)
                result = Convert.ToByte(pByte | (1 << bitNo));
            else
                result = Convert.ToByte(pByte & ~(1 << bitNo));
            return result;
        }

        /// <summary>
        /// Decodes a value encoded in a byte.
        /// </summary>
        /// <param name="pByte">The byte where the value is encoded.</param>
        /// <param name="bitStart">The number of the youngest bit (zero-based index).</param>
        /// <param name="bitEnd">The number of the oldest bit (zero-based index).</param>
        /// <returns>Value encoded in interval of the byte.</returns>
        public static byte GetValue(this byte pByte, int bitStart, int bitEnd)
        {
            Contract.Requires<ArgumentOutOfRangeException>(bitStart >= 0, "bitStart must be an natural number");
            Contract.Requires<ArgumentException>((bitEnd - bitStart) > 0, "bitEnd must be greater than bitStart");
            Contract.Requires<ArgumentOutOfRangeException>(bitEnd < 8, "bitEnd be in range [0, 7]");

            byte value = Convert.ToByte(pByte >> bitStart);
            int count = bitEnd - bitStart + 1;
            byte mask = Convert.ToByte((1 << count) - 1);

            value &= mask;

            return value;
        }

        /// <summary>
        /// Get the bit value in a byte.
        /// </summary>
        /// <param name="array">The byte where the value is encoded.</param>
        /// <param name="bitNo">The number of the bit (zero-based index).</param>
        /// <returns>Value of the bit.</returns>
        public static bool GetBit(this byte[] array, int bitNo)
        {
            Contract.Requires<ArgumentNullException>(array != null, "array");
            Contract.Requires<ArgumentOutOfRangeException>((bitNo / 8) < array.Length, "bitNo exceeds the array");
            Contract.Requires<ArgumentOutOfRangeException>(bitNo >= 0, "bitNo must be an natural number");

            var mask = 1 << (7 - (bitNo % 8));
            return (array[bitNo / 8] & mask) != 0;
        }

        /// <summary>
        /// Set the bit valie in a byte.
        /// </summary>
        /// <param name="array">The byte where the value is encoded.</param>
        /// <param name="bitNo">The number of the bit (zero-based index).</param>
        /// <param name="value">Value of the bit.</param>
        /// <returns>Byte with changed bit.</returns>
        public static void SetBit(this byte[] array, int bitNo, bool value)
        {
            Contract.Requires<ArgumentNullException>(array != null, "array");
            Contract.Requires<ArgumentOutOfRangeException>((bitNo / 8) < array.Length, "bitNo exceeds the array");
            Contract.Requires<ArgumentOutOfRangeException>(bitNo >= 0, "bitNo must be an natural number");

            var mask = 1 << (7 - (bitNo % 8));
            if (value)
                array[bitNo / 8] = Convert.ToByte(array[bitNo / 8] | mask);
            else
                array[bitNo / 8] = Convert.ToByte(array[bitNo / 8] & ~mask);
        }

        public static byte[] Cut(this byte[] array, int bitStart, int bitEnd)
        {
            Contract.Requires<ArgumentNullException>(array != null, "array");
            Contract.Requires<ArgumentOutOfRangeException>(bitStart >= 0, "bitStart must be an natural number");
            Contract.Requires<ArgumentException>((bitEnd - bitStart) > 0, "bitEnd must be greater than bitStart");
            Contract.Requires<ArgumentOutOfRangeException>((bitEnd / 8) < array.Length, "bitEnd exceeds the array");

            int interval = bitEnd - bitStart + 1;
            byte[] result = new byte[((interval - 1) / 8) + 1];

            int sourceBitIndex = bitStart;
            int displacement = (interval % 8 == 0) ? 0 : 8 - interval % 8;
            for (int i = displacement; i < interval + displacement; i++)
            {
                bool value = array.GetBit(sourceBitIndex);
                result.SetBit(i, value);
                sourceBitIndex++;
            }

            return result;
        }

        public static int GetInteger(this byte[] array, int bitStart, int bitEnd)
        {
            Contract.Requires<ArgumentNullException>(array != null, "array");
            Contract.Requires<ArgumentOutOfRangeException>(bitStart >= 0, "bitStart must be an natural number");
            Contract.Requires<ArgumentException>((bitEnd - bitStart) > 0, "bitEnd must be greater than bitStart");
            Contract.Requires<ArgumentOutOfRangeException>((bitEnd / 8) < array.Length, "bitEnd exceeds the array");

            byte[] masked = array.Cut(bitStart, bitEnd);
            int result = BitsToInt32(masked);
            return result;
        }

        private static int BitsToInt32(byte[] masked)
        {
            Contract.Requires(masked.Length < 5);

            var array = new byte[4];
            for (int i = 0; i < masked.Length; i++)
            {
                array[i] = masked[masked.Length - i - 1];
            }

            int result = BitConverter.ToInt32(array, 0);
            return result;
        }
    }
}