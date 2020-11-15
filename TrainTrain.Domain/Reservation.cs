using System.Collections.Generic;
using CollectionByValue;

namespace TrainTrain.Domain
{
    public record Reservation
    {
        private readonly ListValue<Seat> _seats;
        public TrainId TrainId { get; }
        public BookingReference BookingReference { get; }
        public IEnumerable<Seat> Seats => _seats.Item;

        public Reservation(TrainId trainId, BookingReference bookingReference, IEnumerable<Seat> seats)
        {
            TrainId = trainId;
            BookingReference = bookingReference;
            _seats = new ListValue<Seat>(new List<Seat>(seats));
        }
    }
}