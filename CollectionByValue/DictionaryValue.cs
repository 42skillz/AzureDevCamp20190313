using System.Collections.Generic;
using Value;
using Value.Shared;

namespace CollectionByValue
{
    public class DictionaryValue<TK, TV>: ValueType<DictionaryValue<TK, TV>>
    {
        public IDictionary<TK,TV> Item { get; }

        public DictionaryValue(IDictionary<TK,TV> item)
        {
            Item = item;
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            return new object[] {new DictionaryByValue<TK, TV>(Item)};
        }
    }
}