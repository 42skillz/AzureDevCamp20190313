using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NFluent;
using NSubstitute;
using TechTalk.SpecFlow;
using TrainTrain.Domain;
using TrainTrain.Domain.Port;
using TrainTrain.Infra.Adapter;
using TrainTrain.Test.TDD.Acceptance;

namespace TrainTrain.Test.Specification.StepDefinition
{
    [Binding]
    public class TrainReservationSteps
    {
        private readonly BookingReference _bookingReference = new BookingReference("341RTFA");
        private readonly TrainId _trainId = new TrainId("9043-2019-03-13");
        private IProvideTrainTopology _provideTrainTopology;
        private int _seatsCountRequested;

        [Given(@"one coach empty made of (.*) seats")]
        public void GivenOneCoachEmptyMadeOfSeats(int availableSeats)
        {
            _provideTrainTopology = BuildTrainTopology(_trainId,
                TrainTopologyGenerator.With_n_seats_and_m_already_reserved(availableSeats, 1));
        }

        [Given(@"(.*) coaches of (.*) seats and (.*) already reserved")]
        public void GivenCoachesOfSeatsAndAlreadyReserved(int coachesCount, int seatsCount, int alreadyReservedSeats)
        {
             _provideTrainTopology = BuildTrainTopology(_trainId,
                TrainTopologyGenerator.With_n_seats_and_m_already_reserved(seatsCount, coachesCount,
                    alreadyReservedSeats, _bookingReference.Id));

        }

        [Given(@"(.*) coaches of (.*) seats and (.*) seats already reserved in the coach (.*)")]
        public void GivenCoachesOfSeatsAndSeatsAlreadyReservedInTheCoach(int coachesCount, int seatsCount,
            int alreadyReservedSeats, int coachNumber)
        {
            _provideTrainTopology = BuildTrainTopology(_trainId,
                TrainTopologyGenerator.With_n_seats_and_m_already_reserved(seatsCount, coachesCount,
                    alreadyReservedSeats, _bookingReference.Id, coachNumber));
        }

        [When(@"(.*) seats are requested")]
        public void WhenSeatsAreRequested(int seatsCountRequested)
        {
            _seatsCountRequested = seatsCountRequested;
        }

        [Then(@"should assign the reservation these seats ""(.*)""")]
        public async Task ThenTheReservationShouldBeAssignedTheseSeats(string expectedSeats)
        {
            var bookingReferenceService = BuildBookingReferenceService(_bookingReference);
            var provideReservationService = BuildMakeReservation(_trainId, _bookingReference, expectedSeats);

            var ticketOffice =
                new TicketOfficeService(_provideTrainTopology, provideReservationService, bookingReferenceService);

            var reservation = await ticketOffice.Reserve(_trainId, new SeatsRequested(_seatsCountRequested));

            var seatsAsJsonString = AdaptSeatsToJson(expectedSeats);

            Check.That(SeatsReservationAdapter.AdaptReservation(reservation))
                .IsEqualTo(
                    $"{{\"train_id\": \"{_trainId}\", \"booking_reference\": \"{_bookingReference}\", \"seats\": [{seatsAsJsonString}]}}");
        }

        [Then(@"the reservation should be failed")]
        public async Task ThenTheReservationShouldBeFailed()
        {
            var bookingReferenceService = BuildBookingReferenceService(_bookingReference);
            var provideReservationService = BuildMakeReservation(_trainId, _bookingReference, string.Empty);

            var ticketOffice =
                new TicketOfficeService(_provideTrainTopology, provideReservationService, bookingReferenceService);

            var reservation = await ticketOffice.Reserve(_trainId, new SeatsRequested(_seatsCountRequested));

            Check.That(SeatsReservationAdapter.AdaptReservation(reservation))
                .IsEqualTo($"{{\"train_id\": \"{_trainId}\", \"booking_reference\": \"\", \"seats\": []}}");
        }

        private static IProvideBookingReference BuildBookingReferenceService(BookingReference bookingReference)
        {
            var bookingReferenceService = Substitute.For<IProvideBookingReference>();
            bookingReferenceService.GetBookingReference().Returns(Task.FromResult(bookingReference));
            return bookingReferenceService;
        }

        private static IProvideTrainTopology BuildTrainTopology(TrainId trainId, string trainTopology)
        {
            var trainDataService = Substitute.For<IProvideTrainTopology>();
            trainDataService.GetTrain(trainId)
                .Returns(Task.FromResult(new Train(trainId,
                    TrainDataServiceAdapter.AdaptTrainTopology(trainTopology))));

            return trainDataService;
        }

        private static IProvideReservation BuildMakeReservation(TrainId trainId,
            BookingReference bookingReference, string expectedSeats)
        {
            var trainDataService = Substitute.For<IProvideReservation>();

            trainDataService.BookSeats(Arg.Any<ReservationAttempt>())
                .Returns(new Reservation(trainId, bookingReference, ToSeats(expectedSeats).ToList()));

            return trainDataService;
        }

        private static IEnumerable<Seat> ToSeats(string expectedSeats)
        {
            var seats = new List<Seat>();
            
            if (expectedSeats == string.Empty) return seats;

            foreach (var seatName in expectedSeats.Split(','))
            {
                var (coachName, seatNumber) = ToSeat(seatName.Trim());
                seats.Add(new Seat(coachName, seatNumber));
            }
            return seats;
        }

        private static (string coachName, int seatNumber) ToSeat(string seatAsString)
        {
            var value = seatAsString[0].ToString();
            return (seatAsString[1].ToString(), Convert.ToInt32(value));
        }

        private static string AdaptSeatsToJson(string expectedSeats)
        {
            return string.Join(", ", expectedSeats.Split(",").Select(s => $"\"{s.Trim()}\""));
        }
    }
}