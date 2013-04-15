using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyHasher
{
    public static class FileHasher
    {
        public static string Hash(params string[] filenames)
        {
            return Hash(filenames);
        }

        public static string Hash(bool ignoreVersions, params string[] filenames)
        {
            using (var hashService = new SHA1CryptoServiceProvider())
            {
                foreach (var filename in filenames)
                {
                    var extension = Path.GetExtension(filename).ToLowerInvariant();
                    if (extension == ".dll" || extension == ".exe")
                    {
                        var disassembled = Disassembler.Disassemble(filename, ignoreVersions);
                        AddFileToHash(disassembled.ILFilename, hashService);
                        foreach (var resource in disassembled.Resources)
                        {
                            AddFileToHash(resource, hashService);
                        }
                        disassembled.Delete();
                    }
                    else
                    {
                        AddFileToHash(filename, hashService);
                    }
                }
                hashService.TransformFinalBlock(new byte[0], 0, 0);
                return Convert.ToBase64String(hashService.Hash);
            }
        }

        private static void AddFileToHash(string filename, HashAlgorithm hashService)
        {
            using (var stream = File.OpenRead(filename))
            {
                var buffer = new byte[1200000];
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                while (bytesRead > 1)
                {
                    hashService.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                }
            }
        }
    }
}
