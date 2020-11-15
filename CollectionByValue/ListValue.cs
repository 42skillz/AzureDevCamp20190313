using System.Collections.Generic;
using Value;

namespace CollectionByValue
{
    public class ListValue<T>: ValueType<ListValue<T>>
    {
        public List<T> Item { get; }

        public ListValue(IEnumerable<T> item):this(new List<T>(item))
        {
        }

        public ListValue(List<T> item)
        {
            Item = item;
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            return new object[] {new ListByValue<T>(Item)};
        }
    }
}