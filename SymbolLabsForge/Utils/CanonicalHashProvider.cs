//===============================================================
// File: CanonicalHashProvider.cs
// Author: Gemini
// Date: 2025-11-14
// Purpose: Provides a deterministic hashing mechanism for images
//          by operating on raw pixel data and a canonical header.
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Text;

namespace SymbolLabsForge.Utils
{
    public static class CanonicalHashProvider
    {
        private const byte HASH_VERSION = 1;

        /// <summary>
        /// Computes a deterministic SHA256 hash of an image by serializing its
        /// metadata and raw pixel data in a canonical format.
        /// </summary>
                    public static string ComputeSha256(Image<L8> image)
                    {
                        using var sha256 = SHA256.Create();
                        using var ms = new MemoryStream();
                
                        // 1. Write canonical header
                        ms.Write(Encoding.ASCII.GetBytes("SL"));
                        ms.WriteByte(HASH_VERSION);
                        ms.WriteByte((byte)PixelType.L8);
                        ms.Write(BitConverter.GetBytes(image.Width));
                        ms.Write(BitConverter.GetBytes(image.Height));
                
                        // 2. Implement the "Copy-Local" pattern for thread-safe hashing.
                        
                        // 2a. Allocate a local managed buffer.
                        int pixelDataSize = image.Width * image.Height; // L8 is 1 byte per pixel
                        var pixelData = new byte[pixelDataSize];
                        int offset = 0;
                
                        // 2b. Copy pixel values manually into the local buffer.
                        image.ProcessPixelRows(accessor =>
                        {
                            for (int y = 0; y < accessor.Height; y++)
                            {
                                var row = accessor.GetRowSpan(y);
                                for (int x = 0; x < row.Length; x++)
                                {
                                    pixelData[offset++] = row[x].PackedValue;
                                }
                            }
                        });
                
                        // 2c. Write the safe, local copy to the MemoryStream.
                        ms.Write(pixelData, 0, pixelData.Length);
                
                        // 3. Compute hash
                        ms.Position = 0;
                        var hashBytes = sha256.ComputeHash(ms);
                        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                    }        private enum PixelType : byte
        {
            L8 = 1,
            Rgba32 = 2
        }
    }
}
