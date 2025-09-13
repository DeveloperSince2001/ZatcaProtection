using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace ProtectionKeyDepartment
{
    public static class StegoObfuscator
    {
        public const int DefaultCoverLength = 5000; // زودنا الطول لدعم بيانات أكبر
        public const int ChunkSize = 24;
        private const int HeaderLen = 8;

        private static readonly char[] CoverAlphabet =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 .,;:!?-'\"()[]{}<>/@#&*+=_%$^|~".ToCharArray();

        private static string GenerateRandomCover(int coverLen)
        {
            var sb = new StringBuilder(coverLen);
            var bytes = new byte[4];
            using (var rng = RandomNumberGenerator.Create())
            {
                for (int i = 0; i < coverLen; i++)
                {
                    rng.GetBytes(bytes);
                    uint v = BitConverter.ToUInt32(bytes, 0);
                    sb.Append(CoverAlphabet[v % (uint)CoverAlphabet.Length]);
                }
            }
            return sb.ToString();
        }

        private static Random CreateDeterministicRng(string stegoKey)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(stegoKey));
                int seed = BitConverter.ToInt32(hash, 0)
                         ^ BitConverter.ToInt32(hash, 4)
                         ^ BitConverter.ToInt32(hash, 8)
                         ^ BitConverter.ToInt32(hash, 12)
                         ^ BitConverter.ToInt32(hash, 16)
                         ^ BitConverter.ToInt32(hash, 20)
                         ^ BitConverter.ToInt32(hash, 24)
                         ^ BitConverter.ToInt32(hash, 28);
                return new Random(seed);
            }
        }

        private static List<int> GeneratePositions(string stegoKey, int coverLen, int chunksCount)
        {
            var rng = CreateDeterministicRng(stegoKey);
            int posHeader = rng.Next(0, coverLen + 1);
            var positions = new SortedSet<int> { posHeader };
            while (positions.Count < chunksCount)
            {
                int p = rng.Next(posHeader + 1, coverLen + 1);
                positions.Add(p);
            }
            return positions.ToList();
        }

        private static List<string> Chunkify(string text, int size)
        {
            var list = new List<string>((text.Length + size - 1) / size);
            for (int i = 0; i < text.Length; i += size)
                list.Add(text.Substring(i, Math.Min(size, text.Length - i)));
            return list;
        }

        private static string InsertAt(string baseStr, int index, string insert)
        {
            return baseStr.Substring(0, index) + insert + baseStr.Substring(index);
        }

        public static string Hide(string finalEncryptedBase64, string stegoKey, int coverLen = DefaultCoverLength)
        {
            string header = finalEncryptedBase64.Length.ToString("X8");
            string combined = header + finalEncryptedBase64;

            var chunks = Chunkify(combined, ChunkSize);
            int n = chunks.Count;

            string cover = GenerateRandomCover(coverLen);
            var positions = GeneratePositions(stegoKey, coverLen, n);

            for (int i = n - 1; i >= 0; i--)
            {
                cover = InsertAt(cover, positions[i], chunks[i]);
            }
            return cover;
        }

        public static string Extract(string stegoText, string stegoKey, int coverLen = DefaultCoverLength)
        {
            var rng = CreateDeterministicRng(stegoKey);
            int posHeader = rng.Next(0, coverLen + 1);

            string firstChunk = stegoText.Substring(posHeader, ChunkSize);
            string headerHex = firstChunk.Substring(0, HeaderLen);
            int payloadLen = int.Parse(headerHex, System.Globalization.NumberStyles.HexNumber);

            int combinedLen = HeaderLen + payloadLen;
            int n = (combinedLen + ChunkSize - 1) / ChunkSize;

            var positions = GeneratePositions(stegoKey, coverLen, n);

            var sb = new StringBuilder(combinedLen);
            int consumed = 0;
            for (int i = 0; i < n; i++)
            {
                int remaining = combinedLen - consumed;
                int thisChunkLen = Math.Min(ChunkSize, remaining);
                int actualIndex = positions[i] + (i * ChunkSize);
                string part = stegoText.Substring(actualIndex, thisChunkLen);
                sb.Append(part);
                consumed += thisChunkLen;
            }

            string combined = sb.ToString();
            return combined.Substring(HeaderLen, payloadLen);
        }
    }

    public static class MultiLayerEncryptor
    {
        public static string EncryptTriple(string number1, string number2, string jsonData,
                                           string password1, string password2, string password3)
        {
            string plain = number1 + "|" + number2 + "|" + jsonData;

            string step1 = SecureNumberEncryptor.EncryptText(plain, password1);
            string step2 = SecureNumberEncryptor.EncryptText(step1, password2);
            string step3 = SecureNumberEncryptor.EncryptText(step2, password3);
            return step3;
        }

        public static (string, string, string) DecryptTriple(string encrypted,
                                                     string password1, string password2, string password3)
        {
            string s2 = SecureNumberEncryptor.DecryptText(encrypted, password3);
            string s1 = SecureNumberEncryptor.DecryptText(s2, password2);
            string plain = SecureNumberEncryptor.DecryptText(s1, password1);

            string[] parts = plain.Split('|');
            if (parts.Length < 3) throw new FormatException("Invalid decrypted format.");
            return (parts[0], parts[1], string.Join("|", parts.Skip(2)));
        }
    }

    public static class SecureNumberEncryptor
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100000;

        public static string EncryptText(string plain, string password)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plain);

            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            byte[] keyMaterial;
            using (var kdf = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                keyMaterial = kdf.GetBytes(KeySize * 2);

            byte[] aesKey = keyMaterial.Take(KeySize).ToArray();
            byte[] hmacKey = keyMaterial.Skip(KeySize).Take(KeySize).ToArray();

            byte[] iv = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(iv);

            byte[] cipherBytes;
            using (var aes = Aes.Create())
            {
                aes.Key = aesKey;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                using (var encryptor = aes.CreateEncryptor())
                    cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            }

            byte[] combined = salt.Concat(iv).Concat(cipherBytes).ToArray();

            byte[] hmac;
            using (var hmacSha = new HMACSHA256(hmacKey))
                hmac = hmacSha.ComputeHash(combined);

            byte[] finalData = combined.Concat(hmac).ToArray();
            return Convert.ToBase64String(finalData);
        }

        public static string DecryptText(string encryptedBase64, string password)
        {
            byte[] allBytes = Convert.FromBase64String(encryptedBase64);
            if (allBytes.Length < SaltSize + 16 + 32)
                throw new ArgumentException("Invalid encrypted data.");

            byte[] salt = allBytes.Take(SaltSize).ToArray();
            byte[] iv = allBytes.Skip(SaltSize).Take(16).ToArray();
            byte[] cipher = allBytes.Skip(SaltSize + 16).Take(allBytes.Length - SaltSize - 16 - 32).ToArray();
            byte[] hmac = allBytes.Skip(allBytes.Length - 32).Take(32).ToArray();

            byte[] keyMaterial;
            using (var kdf = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                keyMaterial = kdf.GetBytes(KeySize * 2);

            byte[] aesKey = keyMaterial.Take(KeySize).ToArray();
            byte[] hmacKey = keyMaterial.Skip(KeySize).Take(KeySize).ToArray();

            byte[] combined = salt.Concat(iv).Concat(cipher).ToArray();
            byte[] computedHmac;
            using (var hmacSha = new HMACSHA256(hmacKey))
                computedHmac = hmacSha.ComputeHash(combined);

            if (!CryptographicEquals(hmac, computedHmac))
                throw new CryptographicException("Invalid HMAC - data corrupted or wrong password.");

            using (var aes = Aes.Create())
            {
                aes.Key = aesKey;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                using (var decryptor = aes.CreateDecryptor())
                {
                    byte[] plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
        }

        private static bool CryptographicEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int result = 0;
            for (int i = 0; i < a.Length; i++)
                result |= a[i] ^ b[i];
            return result == 0;
        }
    }
}

