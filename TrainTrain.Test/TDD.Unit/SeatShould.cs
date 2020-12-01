using NFluent;
using TrainTrain.Domain;
using Xunit;

namespace TrainTrain.Test.TDD.Unit
{
    public class SeatShould
    {
        [Fact]
        public void Be_value_object()
        {
            var seat = new Seat("A", 1);
            var sameSeat = new Seat("A", 1);

            Check.That(seat).IsEqualTo(sameSeat);
        }
    }
}