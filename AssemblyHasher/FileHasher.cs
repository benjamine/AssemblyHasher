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
            using (var hashService = Murmur.MurmurHash.Create128())
            {
                foreach (var filename in filenames)
                {
                    var extension = Path.GetExtension(filename).ToLowerInvariant();
                    if (extension == ".dll" || extension == ".exe")
                    {
                        var disassembled = Disassembler.Disassemble(filename);
                        AddFileToHash(disassembled.ILFilename, hashService,
                            AssemblySourceCleanup.GetFilter(AssemblySourceCleanup.FileTypes.IL, ignoreVersions));
                        foreach (var resource in disassembled.Resources)
                        {
                            AddFileToHash(resource, hashService,
                            AssemblySourceCleanup.GetFilter(resource, ignoreVersions));
                        }
                        disassembled.Delete();
                    }
                    else
                    {
                        AddFileToHash(filename, hashService,
                            AssemblySourceCleanup.GetFilter(filename, ignoreVersions));
                    }
                }
                hashService.TransformFinalBlock(new byte[0], 0, 0);
                return Convert.ToBase64String(hashService.Hash);
            }
        }

        private static void AddFileToHash(string filename, HashAlgorithm hashService, StreamFilter filter = null, Encoding encoding = null)
        {
            if (filter == null || filter == StreamFilter.None)
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
            else
            {
                if (encoding == null)
                {
                    if (Path.GetExtension(filename).Equals(".res", StringComparison.InvariantCultureIgnoreCase))
                    {
                        encoding = Encoding.Unicode;
                    }
                    else
                    {
                        encoding = Encoding.Default;
                    }
                }
                using (var stream = File.OpenRead(filename))
                {
                    using (var reader = new StreamReader(stream, encoding))
                    {
                        foreach (var line in filter.ReadAllLines(reader))
                        {
                            var lineBuffer = encoding.GetBytes(line);
                            hashService.TransformBlock(lineBuffer, 0, lineBuffer.Length, lineBuffer, 0);
                        }
                    }
                }
            }
        }
    }
}
