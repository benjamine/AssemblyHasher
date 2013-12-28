using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AssemblyHasher
{
    class AssemblySourceCleanup
    {
        public enum FileTypes
        {
            IL,
            Res
        }

        public static Regex regexVsVersionInfoRes = new Regex("VS_VERSION_INFO.*VarFileInfo", RegexOptions.Compiled);
        public static Regex regexFileVersionRes = new Regex("FileVersion[0-9\\.\0 ]*", RegexOptions.Compiled);
        public static Regex regexProductVersionRes = new Regex("ProductVersion[0-9\\.\0 ]*", RegexOptions.Compiled);
        public static Regex regexAssemblyVersionRes = new Regex("Assembly Version[0-9\\.\0 ]*", RegexOptions.Compiled);

        private static IDictionary<Tuple<FileTypes, bool>, StreamFilter> _filters = new Dictionary<Tuple<FileTypes, bool>, StreamFilter>();

        public static StreamFilter GetFilter(string filename, bool removeAssemblyVersion = false)
        {
            FileTypes fileType;
            var ext = Path.GetExtension(filename) ?? "";
            if (ext.Equals(".il", StringComparison.InvariantCultureIgnoreCase))
            {
                fileType = FileTypes.IL;
            } 
            else if (ext.Equals(".res", StringComparison.InvariantCultureIgnoreCase))
            {
                fileType = FileTypes.Res;
            } 
            else
            {
                return StreamFilter.None;
            }
            return GetFilter(fileType, removeAssemblyVersion);
        }

        public static StreamFilter GetFilter(FileTypes fileType, bool removeAssemblyVersion = false)
        {
            StreamFilter filter;
            var key = new Tuple<FileTypes, bool>(fileType, removeAssemblyVersion);
            if (!_filters.TryGetValue(key, out filter))
            {
                filter = _filters[key] = CreateFilter(fileType, removeAssemblyVersion);
            }
            return filter;
        }

        private static StreamFilter CreateFilter(FileTypes fileType, bool removeAssemblyVersion = false)
        {
            var filterItems = new List<StreamFilter.Item>();
            if (fileType == FileTypes.Res)
            {
                if (!removeAssemblyVersion)
                {
                    return StreamFilter.None;
                }
                filterItems.Add(new StreamFilter.RegexItem(regexVsVersionInfoRes, true, 0));
                filterItems.Add(new StreamFilter.RegexItem(regexProductVersionRes, true, 0));
                filterItems.Add(new StreamFilter.RegexItem(regexFileVersionRes, true, 0));
                filterItems.Add(new StreamFilter.RegexItem(regexAssemblyVersionRes, true, 0));
            }
            else
            {
                filterItems.Add(new StreamFilter.StartsWithItem("// Entry point code", true, 2));
                filterItems.Add(new StreamFilter.StartsWithItem(".imagebase"));
                filterItems.Add(new StreamFilter.StartsWithItem("//", false));
                filterItems.Add(new StreamFilter.ContainsItem("<PrivateImplementationDetails>", false));
                filterItems.Add(new StreamFilter.StartsWithItem("  .custom", false,
                    skipUntil: new StreamFilter.ContainsItem(" )")));
                filterItems.Add(new StreamFilter.StartsWithItem("    .custom", false,
                    skipUntil: new StreamFilter.ContainsItem(" )")));

                if (removeAssemblyVersion)
                {
                    filterItems.Add(new StreamFilter.ContainsItem("AssemblyFileVersionAttribute", false));
                    filterItems.Add(new StreamFilter.StartsWithItem("  .ver", false));
                }
            }
            return new StreamFilter(filterItems.ToArray());
        }
    }
}
