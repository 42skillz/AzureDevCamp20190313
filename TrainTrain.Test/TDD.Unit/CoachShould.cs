using System.Collections.Generic;
using NFluent;
using NUnit.Framework;
using TrainTrain.Domain;

namespace TrainTrain.Test.TDD.Unit
{
    internal class CoachShould
    {
        [Test]
        public void Be_value_object()
        {
            var coach = new Coach("A", new List<Seat> {new Seat("A", 1), new Seat("A", 2)});
            var sameCoach = new Coach("A", new List<Seat> {new Seat("A", 1), new Seat("A", 2)});

            Check.That(coach.Name).IsEqualTo(sameCoach.Name);
            Check.That(coach).IsEqualTo(sameCoach);
        }
    }
}