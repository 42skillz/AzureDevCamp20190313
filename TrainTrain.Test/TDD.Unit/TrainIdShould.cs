using System;
using NFluent;
using TrainTrain.Domain;
using Xunit;

namespace TrainTrain.Test.TDD.Unit
{
    public class TrainIdShould
    {
        [Fact]
        public void Be_value_object()
        {
            const string trainIdPrimitive = "9043-2019-03-13";
            var trainId = new TrainId(trainIdPrimitive);
            var sameTrainId = new TrainId(trainIdPrimitive);

            Check.That(trainId).IsEqualTo(sameTrainId);
            Check.That(trainId.TrainNumber).IsEqualTo(sameTrainId.TrainNumber);
            Check.That(trainId.Date).IsEqualTo(sameTrainId.Date);
        }

        [Fact]
        public void Raise_exception_when_argument_is_illegal()
        {
            const string trainIdPrimitive = "express 2000";
            Check.ThatCode(() => new TrainId(trainIdPrimitive)).Throws<ArgumentException>()
                .WithMessage("id should be formatted like TrainNumber[4]-yyyy-MM-dd");
        }
    }
}