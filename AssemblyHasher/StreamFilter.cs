using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AssemblyHasher
{
    public class StreamFilter
    {
        public abstract class Item
        {
            public bool OnlyOnce { get; set; }
            public int SkipLines { get; set; }
            public Item SkipUntil { get; set; }

            public Item(bool onlyOnce = true, int skipLines = 1, Item skipUntil = null)
            {
                OnlyOnce = onlyOnce;
                SkipLines = skipLines;
                SkipUntil = skipUntil;
            }

            public abstract bool IsMatch(string input);
            public abstract string Remove(string input);
        }

        public class RegexItem : Item
        {
            public Regex Regex { get; set; }

            public RegexItem(Regex regex, bool onlyOnce = true, int skipLines = 1, Item skipUntil = null)
                : base(onlyOnce, skipLines, skipUntil)
            {
                Regex = regex;
            }

            public override bool IsMatch(string input)
            {
                return Regex.IsMatch(input);
            }

            public override string Remove(string input)
            {
                return Regex.Replace(input, string.Empty);
            }
        }

        public class StartsWithItem : Item
        {
            public string Value { get; set; }

            public StartsWithItem(string value, bool onlyOnce = true, int skipLines = 1, Item skipUntil = null)
                : base(onlyOnce, skipLines, skipUntil)
            {
                Value = value;
            }

            public override bool IsMatch(string input)
            {
                return input.StartsWith(Value);
            }

            public override string Remove(string input)
            {
                return input.Replace(Value, string.Empty);
            }
        }

        public class ContainsItem : Item
        {
            public string Value { get; set; }

            public ContainsItem(string value, bool onlyOnce = true, int skipLines = 1, Item skipUntil = null)
                : base(onlyOnce, skipLines, skipUntil)
            {
                Value = value;
            }

            public override bool IsMatch(string input)
            {
                return input.Contains(Value);
            }

            public override string Remove(string input)
            {
                return input.Replace(Value, string.Empty);
            }
        }

        public static StreamFilter None = new StreamFilter();

        private readonly Item[] _items;

        public StreamFilter(Item[] items = null)
        {
            _items = items;
        }

        public IEnumerable<string> ReadAllLines(StreamReader reader)
        {
            var skipNextLines = 0;
            var skipCount = 0;
            var totalSkipCount = 0;
            var totalLines = 0;
            Item skipUntil = null;
            var line = reader.ReadLine();
            var filterItems = _items == null ? null : _items.ToList();
            while (line != null)
            {
                var lineOut = line;
                if (skipNextLines > 0)
                {
                    skipCount++;
                    totalSkipCount++;
                    skipNextLines--;
                }
                else if (skipUntil != null && !skipUntil.IsMatch(lineOut))
                {
                    skipCount++;
                    totalSkipCount++;
                    skipNextLines = 0;
                }
                else
                {
                    if (skipCount > 0)
                    {
                        skipCount = 0;
                    }
                    skipUntil = null;
                    if (filterItems != null && filterItems.Count > 0)
                    {
                        for (var i = 0; i < filterItems.Count; i++)
                        {
                            var filterItem = filterItems[i];
                            if (filterItem.SkipUntil != null)
                            {
                                if (filterItem.IsMatch(lineOut))
                                {

                                    skipUntil = filterItem.SkipUntil;
                                    skipNextLines = 0;
                                    skipCount = 1;
                                    totalSkipCount++;
                                    if (filterItem.OnlyOnce)
                                    {
                                        filterItems.RemoveAt(i);
                                    }
                                    break;
                                }
                            }
                            else if (filterItem.SkipLines > 0)
                            {
                                if (filterItem.IsMatch(lineOut))
                                {
                                    skipNextLines = filterItem.SkipLines;
                                    skipUntil = null;
                                    skipCount = 1;
                                    totalSkipCount++;
                                    if (filterItem.OnlyOnce)
                                    {
                                        filterItems.RemoveAt(i);
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                lineOut = filterItem.Remove(lineOut);
                                if (filterItem.OnlyOnce && line != lineOut)
                                {
                                    filterItems.RemoveAt(i);
                                    i--;
                                }
                            }
                        }
                    }
                    if (skipUntil != null && skipUntil.IsMatch(lineOut))
                    {
                        skipUntil = null;
                        skipCount = 0;
                    }
                    if (skipNextLines == 0 && skipUntil == null)
                    {
                        skipCount = 0;
                        yield return lineOut;
                    }
                }
                line = reader.ReadLine();
                totalLines++;
            }
        }

    }
}
