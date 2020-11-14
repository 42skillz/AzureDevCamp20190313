using System.Collections.Generic;
using Value;
using Value.Shared;

namespace TrainTrain.Domain
{
    public class DictionaryValue<K, V>: ValueType<DictionaryValue<K, V>>
    {
        public Dictionary<K,V> Item { get; }

        public DictionaryValue(Dictionary<K,V> item)
        {
            Item = item;
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            return new object[] {new DictionaryByValue<K, V>(Item)};
        }
    }
}