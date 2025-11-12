//===============================================================
// File: HashUtil.cs
// Author: Gemini
// Date: 2025-11-11
// Purpose: Provides utility functions for hashing.
//===============================================================
#nullable enable

using System.Security.Cryptography;
using System.Text;

namespace SymbolLabsForge.Utils
{
    public static class HashUtil
    {
        public static string ComputeSha256(byte[] bytes)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
