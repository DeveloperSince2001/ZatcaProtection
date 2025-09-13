using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ProtectionLicenceDepartment
{
    public static class StegoObfuscator
    {
        public const int DefaultCoverLength = 2000; // طول سلسلة الغطاء ≥ 2000 لزيادة الأمان
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
            if (stegoKey == null) throw new ArgumentNullException(nameof(stegoKey));
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
            if (chunksCount <= 0) throw new ArgumentOutOfRangeException(nameof(chunksCount));
            var rng = CreateDeterministicRng(stegoKey);

            int posHeader = rng.Next(0, coverLen + 1);
            var positions = new SortedSet<int> { posHeader };

            while (positions.Count < chunksCount)
            {
                int p = rng.Next(posHeader + 1, coverLen + 1);
                positions.Add(p);
            }
            return new List<int>(positions);
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
            if (string.IsNullOrEmpty(finalEncryptedBase64)) throw new ArgumentNullException(nameof(finalEncryptedBase64));
            if (coverLen < 1000) throw new ArgumentOutOfRangeException(nameof(coverLen), "coverLen must be ≥ 1000");
            if (ChunkSize < HeaderLen) throw new InvalidOperationException("ChunkSize must be ≥ HeaderLen (8).");

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
            if (stegoText == null) throw new ArgumentNullException(nameof(stegoText));
            if (coverLen < 1000) throw new ArgumentOutOfRangeException(nameof(coverLen));

            var rng = CreateDeterministicRng(stegoKey);
            int posHeader = rng.Next(0, coverLen + 1);

            if (stegoText.Length < posHeader + ChunkSize)
                throw new ArgumentException("Stego text too short or wrong cover length / key.");

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

                if (stegoText.Length < actualIndex + thisChunkLen)
                    throw new ArgumentException("Stego text inconsistent (wrong key/coverLen or corrupted).");

                string part = stegoText.Substring(actualIndex, thisChunkLen);
                sb.Append(part);
                consumed += thisChunkLen;
            }

            string combined = sb.ToString();
            string payload = combined.Substring(HeaderLen, payloadLen);
            return payload;
        }
    }

    // ===== التشفير ثلاثي الطبقات AES + إضافة التاريخ =====
    public static class MultiLayerEncryptor
    {
        public static string EncryptTriple(string number1, string number2, DateTime date,
                                           string password1, string password2, string password3)
        {
            string step1 = SecureNumberEncryptor.EncryptData(number1, number2, date, password1);
            string step2 = SecureNumberEncryptor.EncryptData(step1, "", DateTime.MinValue, password2);
            string step3 = SecureNumberEncryptor.EncryptData(step2, "", DateTime.MinValue, password3);
            return step3;
        }

        public static (string, string, DateTime) DecryptTriple(string encrypted,
                                                     string password1, string password2, string password3)
        {
            var (s2, _, _) = SecureNumberEncryptor.DecryptData(encrypted, password3);
            var (s1, _, _) = SecureNumberEncryptor.DecryptData(s2, password2);
            var (n1, n2, date) = SecureNumberEncryptor.DecryptData(s1, password1);
            return (n1, n2, date);
        }
    }

    public static class SecureNumberEncryptor
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100000;

        public static string EncryptData(string number1, string number2, DateTime date, string password)
        {
            if (number1 == null) throw new ArgumentNullException(nameof(number1));
            if (number2 == null) throw new ArgumentNullException(nameof(number2));
            if (password == null) throw new ArgumentNullException(nameof(password));

            string dateStr = date == DateTime.MinValue ? "" : date.ToString("yyyy-MM-dd");
            string plain = number1 + "|" + number2 + "|" + dateStr;
            byte[] plainBytes = Encoding.UTF8.GetBytes(plain);

            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] keyMaterial;
            using (var kdf = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                keyMaterial = kdf.GetBytes(KeySize * 2);
            }

            byte[] aesKey = new byte[KeySize];
            byte[] hmacKey = new byte[KeySize];
            Array.Copy(keyMaterial, 0, aesKey, 0, KeySize);
            Array.Copy(keyMaterial, KeySize, hmacKey, 0, KeySize);

            byte[] iv = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }

            byte[] cipherBytes;
            using (var aes = Aes.Create())
            {
                aes.Key = aesKey;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                {
                    cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                }
            }

            byte[] combined = new byte[salt.Length + iv.Length + cipherBytes.Length];
            Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
            Buffer.BlockCopy(iv, 0, combined, salt.Length, iv.Length);
            Buffer.BlockCopy(cipherBytes, 0, combined, salt.Length + iv.Length, cipherBytes.Length);

            byte[] hmac;
            using (var hmacSha = new HMACSHA256(hmacKey))
            {
                hmac = hmacSha.ComputeHash(combined);
            }

            byte[] finalData = new byte[combined.Length + hmac.Length];
            Buffer.BlockCopy(combined, 0, finalData, 0, combined.Length);
            Buffer.BlockCopy(hmac, 0, finalData, combined.Length, hmac.Length);

            return Convert.ToBase64String(finalData);
        }

        public static (string number1, string number2, DateTime date) DecryptData(string encryptedBase64, string password)
        {
            if (encryptedBase64 == null) throw new ArgumentNullException(nameof(encryptedBase64));
            if (password == null) throw new ArgumentNullException(nameof(password));

            byte[] allBytes = Convert.FromBase64String(encryptedBase64);

            if (allBytes.Length < SaltSize + 16 + 32)
                throw new ArgumentException("Invalid encrypted data.");

            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(allBytes, 0, salt, 0, SaltSize);

            byte[] iv = new byte[16];
            Buffer.BlockCopy(allBytes, SaltSize, iv, 0, 16);

            int hmacSize = 32;
            int cipherLen = allBytes.Length - SaltSize - iv.Length - hmacSize;
            if (cipherLen <= 0) throw new ArgumentException("Invalid cipher length.");

            byte[] cipher = new byte[cipherLen];
            Buffer.BlockCopy(allBytes, SaltSize + iv.Length, cipher, 0, cipherLen);

            byte[] hmac = new byte[hmacSize];
            Buffer.BlockCopy(allBytes, allBytes.Length - hmacSize, hmac, 0, hmacSize);

            byte[] keyMaterial;
            using (var kdf = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                keyMaterial = kdf.GetBytes(KeySize * 2);
            }

            byte[] aesKey = new byte[KeySize];
            byte[] hmacKey = new byte[KeySize];
            Array.Copy(keyMaterial, 0, aesKey, 0, KeySize);
            Array.Copy(keyMaterial, KeySize, hmacKey, 0, KeySize);

            byte[] combined = new byte[salt.Length + iv.Length + cipher.Length];
            Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
            Buffer.BlockCopy(iv, 0, combined, salt.Length, iv.Length);
            Buffer.BlockCopy(cipher, 0, combined, salt.Length + iv.Length, cipher.Length);

            byte[] computedHmac;
            using (var hmacSha = new HMACSHA256(hmacKey))
            {
                computedHmac = hmacSha.ComputeHash(combined);
            }

            if (!CryptographicEquals(hmac, computedHmac))
                throw new CryptographicException("Invalid HMAC - data corrupted or wrong password.");

            byte[] plainBytes;
            using (var aes = Aes.Create())
            {
                aes.Key = aesKey;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor())
                {
                    plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
                }
            }

            string combinedText = Encoding.UTF8.GetString(plainBytes);
            string[] parts = combinedText.Split('|');
            if (parts.Length != 3) throw new FormatException("Invalid decrypted format.");

            DateTime date = DateTime.MinValue;
            if (!string.IsNullOrEmpty(parts[2]))
                date = DateTime.ParseExact(parts[2], "yyyy-MM-dd", null);

            return (parts[0], parts[1], date);
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


