using System.Collections.Generic;
using Value;

namespace CollectionByValue
{
    public class ListValue<T>: ValueType<ListValue<T>>
    {
        public readonly IList<T> Item;

        public ListValue(IList<T> item)
        {
            Item = item;
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            return new object[] {new ListByValue<T>(Item)};
        }
    }
}