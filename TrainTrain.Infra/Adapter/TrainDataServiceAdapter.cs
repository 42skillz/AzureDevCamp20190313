using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TrainTrain.Domain;
using TrainTrain.Domain.Port;

namespace TrainTrain.Infra.Adapter
{
    public class TrainDataServiceAdapter :IProvideTrainTopology, IProvideBookedSeats
    {
        private readonly string _uriTrainDataService;

        public TrainDataServiceAdapter(string uriTrainDataService)
        {
            _uriTrainDataService = uriTrainDataService;
        }

        public async Task<Train> GetTrain(TrainId trainId)
        {
            using (var client = new HttpClient())
            {
                var value = new MediaTypeWithQualityHeaderValue("application/json");
                client.BaseAddress = new Uri(_uriTrainDataService);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(value);

                // HTTP GET
                var response = await client.GetAsync($"api/data_for_train/{trainId}");
                response.EnsureSuccessStatusCode();
                return new Train(trainId, AdaptTrainTopology(await response.Content.ReadAsStringAsync()));
            }
        }

        public async Task<Reservation> BookSeats(ReservationAttempt reservationAttempt)
        {
            using (var client = new HttpClient())
            {
                var value = new MediaTypeWithQualityHeaderValue("application/json");
                client.BaseAddress = new Uri(_uriTrainDataService);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(value);

                // HTTP POST
                HttpContent resJson = new StringContent(
                    BuildPostContent(reservationAttempt.TrainId.Id, reservationAttempt.BookingReference.Id, reservationAttempt.Seats), Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync("reserve", resJson);

                response.EnsureSuccessStatusCode();

                return new Reservation(reservationAttempt.TrainId, reservationAttempt.BookingReference, reservationAttempt.Seats);
            }
        }

        private static string BuildPostContent(string trainId, string bookingRef, IEnumerable<Seat> seats)
        {
            return $"{{\r\n\t\"train_id\": \"{trainId}\",\r\n\t\"seats\": {AdaptSeats(seats)},\r\n\t\"booking_reference\": \"{bookingRef}\"\r\n}}";
        }

        private static string AdaptSeats(IEnumerable<Seat> seats)
        {
            var jsonSeats = new StringBuilder("[");
            jsonSeats.Append(string.Join(", ", seats));
            jsonSeats.Append("]");
            return jsonSeats.ToString();
        }

        public static Dictionary<string, Coach> AdaptTrainTopology(string trainTopology)
        {
            var coaches = new Dictionary<string, Coach>();
            //var sample =
            //"{\"seats\": {\"1A\": {\"booking_reference\": \"\", \"seat_number\": \"1\", \"coach\": \"A\"}, \"2A\": {\"booking_reference\": \"\", \"seat_number\": \"2\", \"coach\": \"A\"}}}";

            // Forced to workaround with dynamic parsing since the received JSON is invalid format ;-(
            dynamic parsed = JsonConvert.DeserializeObject(trainTopology);

            foreach (var token in ((Newtonsoft.Json.Linq.JContainer) parsed))
            {
                var allStuffs = ((Newtonsoft.Json.Linq.JObject) ((Newtonsoft.Json.Linq.JContainer) token).First);

                foreach (var stuff in allStuffs)
                {
                    var jsonSeat = stuff.Value.ToObject<SeatJsonPoco>();
                    var seat = new Seat(jsonSeat.coach, int.Parse(jsonSeat.seat_number), new BookingReference(jsonSeat.booking_reference));
                    if (!coaches.ContainsKey(jsonSeat.coach))
                    {
                        coaches[jsonSeat.coach] = new Coach(jsonSeat.coach);
                    }
                    coaches[jsonSeat.coach]  = coaches[jsonSeat.coach].AddSeat(seat);
                }
            }
            return coaches;
        }
    }
}