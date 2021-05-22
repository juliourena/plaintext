// Author: Ryan Cobb (@cobbr_io)
// Project: SharpSploit (https://github.com/cobbr/SharpSploit)
// License: BSD 3-Clause

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

/// <summary>
/// Old Utilities using for SharpShell and PassTheHash
/// </summary>
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SharpSploit.Misc
{
    public static class Utilities
    {
        /// <summary>
        /// Checks that a file is signed and has a valid signature.
        /// </summary>
        /// <param name="FilePath">Path of file to check.</param>
        /// <returns></returns>
        public static bool FileHasValidSignature(string FilePath)
        {
            X509Certificate2 FileCertificate;
            try
            {
                X509Certificate signer = X509Certificate.CreateFromSignedFile(FilePath);
                FileCertificate = new X509Certificate2(signer);
            }
            catch
            {
                return false;
            }

            X509Chain CertificateChain = new X509Chain();
            CertificateChain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
            CertificateChain.ChainPolicy.RevocationMode = X509RevocationMode.Offline;
            CertificateChain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;

            return CertificateChain.Build(FileCertificate);
        }

        private static string[] manifestResources = Assembly.GetExecutingAssembly().GetManifestResourceNames();

        public static byte[] GetEmbeddedResourceBytes(string resourceName)
        {
            string resourceFullName = manifestResources.FirstOrDefault(N => N.Contains(resourceName + ".comp"));
            if (resourceFullName != null)
            {
                return Decompress(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceFullName).ReadFully());
            }
            else if ((resourceFullName = manifestResources.FirstOrDefault(N => N.Contains(resourceName))) != null)
            {
                return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceFullName).ReadFully();
            }
            return null;
        }

        public static byte[] ReadFully(this Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static byte[] Compress(byte[] Bytes)
        {
            byte[] compressedBytes;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
                {
                    deflateStream.Write(Bytes, 0, Bytes.Length);
                }
                compressedBytes = memoryStream.ToArray();
            }
            return compressedBytes;
        }

        public static byte[] Decompress(byte[] compressed)
        {
            using (MemoryStream inputStream = new MemoryStream(compressed.Length))
            {
                inputStream.Write(compressed, 0, compressed.Length);
                inputStream.Seek(0, SeekOrigin.Begin);
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (DeflateStream deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        while ((bytesRead = deflateStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            outputStream.Write(buffer, 0, bytesRead);
                        }
                    }
                    return outputStream.ToArray();
                }
            }
        }

        public static bool Is64Bit
        {
            get { return IntPtr.Size == 8; }
        }

        public static string ConvertFileLengthForDisplay(long size)
        {
            string result = size.ToString();

            if (size < 1024) { result = $"{size}b"; }
            else if (size > 1024 && size <= 1048576) { result = $"{size / 1024}kb"; }
            else if (size > 1048576 && size <= 1073741824) { result = $"{size / 1048576}mb"; }
            else if (size > 1073741824 && size <= 1099511627776) { result = $"{size / 1073741824}gb"; }
            else if (size > 1099511627776) { result = $"{size / 1099511627776}tb"; }

            return result;
        }


		/// <summary>
        /// Old Utilities for SharpShell and PassTheHash
        /// </summary>
        public static ushort DataLength(int length_start, byte[] string_extract_data)
        {
            byte[] bytes = { string_extract_data[length_start], string_extract_data[length_start + 1] };
            ushort string_length = BitConverter.ToUInt16(GetByteRange(string_extract_data, length_start, length_start + 1), 0);
            return string_length;
        }

        public static byte[] GetByteRange(byte[] array, int start, int end)
        {
            var newArray = array.Skip(start).Take(end - start + 1).ToArray();
            return newArray;
        }

		public static byte[] ConvertStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static byte[] ConvertFromPacketOrderedDictionary(OrderedDictionary packet_ordered_dictionary)
        {
            List<byte[]> byte_list = new List<byte[]>();
            foreach (DictionaryEntry de in packet_ordered_dictionary)
            {
                byte_list.Add(de.Value as byte[]);
            }

            var flattenedList = byte_list.SelectMany(bytes => bytes);
            byte[] byte_Array = flattenedList.ToArray();

            return byte_Array;
        }


    }
}
