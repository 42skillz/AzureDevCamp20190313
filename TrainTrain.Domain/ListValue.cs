using System.Collections.Generic;
using Value;

namespace TrainTrain.Domain
{
    public class ListValue<T>: ValueType<ListValue<T>>
    {
        public List<T> Item { get; }

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