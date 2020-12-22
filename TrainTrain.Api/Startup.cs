using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TrainTrain.Domain;
using TrainTrain.Infra.Adapter;

namespace TrainTrain.Api
{
    public class Startup
    {
        private const string UriTrainDataService = "http://localhost:50680";
        private const string UriBookingReferenceService = "http://localhost:51691/";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var trainDataServiceAdapter = new TrainDataServiceAdapter(UriTrainDataService);
            var bookingReferenceServiceAdapter = new BookingReferenceServiceAdapter(UriBookingReferenceService);

            var ticketOffice = new TicketsOfficeService(
                trainDataServiceAdapter,
                trainDataServiceAdapter, 
                bookingReferenceServiceAdapter);

            var seatsReservationAdapter = new ReservationAdapter(ticketOffice);
            services.AddSingleton(seatsReservationAdapter);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
        }
    }
}
