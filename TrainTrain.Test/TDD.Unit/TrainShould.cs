using NFluent;
using TrainTrain.Domain;
using TrainTrain.Infra.Adapter;
using TrainTrain.Test.TDD.Acceptance;
using Xunit;

namespace TrainTrain.Test.TDD.Unit
{
    public class TrainShould
    {
        private readonly TrainId _trainId = new TrainId("9043-2019-03-13");

        [Fact]
        public void Be_value_object()
        {
            var train = new Train(_trainId,
                TrainDataServiceAdapter.AdaptTrainTopology(TrainTopologyGenerator
                    .With_2_coaches_and_9_seats_already_reserved_in_the_first_coach()));

            var sameTrainTopology = new Train(_trainId,
                TrainDataServiceAdapter.AdaptTrainTopology(TrainTopologyGenerator
                    .With_2_coaches_and_9_seats_already_reserved_in_the_first_coach()));

            Check.That(train).IsEqualTo(sameTrainTopology);
        }

        /*
        [Fact]
        public void Expose_Coaches()
        {
            var train = new Train(_trainId,
                TrainDataServiceAdapter.AdaptTrainTopology(TrainTopologyGenerator
                    .With_2_coaches_and_9_seats_already_reserved_in_the_first_coach()));

            Check.That(train.Coaches).HasSize(2);
            Check.That(train.Coaches["A"].Seats).HasSize(10);
            Check.That(train.Coaches["B"].Seats).HasSize(10);
        }
        */
    }
}