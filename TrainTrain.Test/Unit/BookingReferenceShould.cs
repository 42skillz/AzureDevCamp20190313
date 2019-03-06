﻿using System;
using NFluent;
using NUnit.Framework;
using TrainTrain.Domain;

namespace TrainTrain.Test.Unit
{
    internal class BookingReferenceShould
    {
        [Test]
        public void Be_value_object()
        {
            var bookingReference = new BookingReference("AK23H");
            var sameBookingReference = new BookingReference("AK23H");

            Check.That(bookingReference).IsEqualTo(sameBookingReference);
        }

        [Test]
        public void Raise_exception_when_id_length_is_invalid()
        {
            Check.That(BookingReference.MaxLength).IsEqualTo(7);
            Check.ThatCode(() => new BookingReference("12345678")).Throws<ArgumentException>();
        }
    }
}