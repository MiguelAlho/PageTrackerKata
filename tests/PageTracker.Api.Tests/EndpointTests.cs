using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using PageTracker.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PageTracker.Api.Tests
{
    public class EndpointTests
    { 
        [Fact]
        public async void GettingCountForUntrackedUrlReturnsOkWithZero()
        {
            var app = new TrackerApplication();
            using var client = app.CreateClient();

            using var _response = await client.GetAsync($"/counter/visitors/?uri={Uris.ValidUri1}");

            _response.StatusCode.Should().Be(HttpStatusCode.OK);

            var payload = JsonConvert.DeserializeObject<int>(await _response.Content.ReadAsStringAsync());
            payload.Should().Be(0);
        }

        [Fact]
        public async void GettingCountForTrackedUrlReturnsOkWithVisitorCount()
        {
            var app = new TrackerApplication();
            //add 2 visitors
            app.SetupVisit(Uris.ValidUri1, "abc");
            app.SetupVisit(Uris.ValidUri1, "def");

            using var client = app.CreateClient();            

            using var _response = await client.GetAsync($"/counter/visitors/?uri={Uris.ValidUri1}");

            _response.StatusCode.Should().Be(HttpStatusCode.OK);

            var payload = JsonConvert.DeserializeObject<int>(await _response.Content.ReadAsStringAsync());
            payload.Should().Be(2);
        }

        [Fact]
        public async void InvalidUriInQueryReturnsBadRequest()
        {
            var app = new TrackerApplication();

            using var client = app.CreateClient();

            using var _response = await client.GetAsync($"/counter/visitors/?uri=invalidUri");

            _response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void CanPostVisitInfo()
        {
            var app = new TrackerApplication();

            var request = new Visit(Uris.ValidUri1, "abc");

            using var client = app.CreateClient();

            using var _response = await client.PostAsync($"/", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            _response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var count = app.GetVisitorCount(Uris.ValidUri1);
            count.Should().Be(1);
        }

        [Fact]
        public async void InvalidUriInPostReturnsBadRequest()
        {
            var app = new TrackerApplication();

            var request = new { Uri = "invalidURI", VisitorId = "abc" };

            using var client = app.CreateClient();

            using var _response = await client.PostAsync($"/", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            _response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void InvalidVisitorIdInPostReturnsBadRequest()
        {
            var app = new TrackerApplication();

            var request = new { Uri = Uris.ValidUri1, VisitorId = " " };

            using var client = app.CreateClient();

            using var _response = await client.PostAsync($"/", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            _response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }



        [Fact]
        public async void CanHandleWriteConcurrency()
        {
            //Setup sample data - 100 visits to 2 pages from 2 users
            var visits = new List<StringContent>();
            for(int i = 0; i < 100; i++)
            {
                visits.Add(new StringContent(
                    JsonConvert.SerializeObject(
                        new Visit(
                            new Random().Next(2) % 2 == 0 ? Uris.ValidUri1 : Uris.ValidUri2,
                            new Random().Next(2) % 2 == 0 ? "abc" : "def"
                    )),
                    Encoding.UTF8,
                    "application/json"));
            }

            var app = new TrackerApplication();
            using var client = app.CreateClient();

            var responses = new List<HttpStatusCode>();
            await Parallel.ForEachAsync(visits, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (visit, token) =>
            {
                responses.Add((await client.PostAsync("/", visit)).StatusCode);
            });

            responses.All(x => x == HttpStatusCode.NoContent).Should().BeTrue();

            //statisticly, each url should end up with 2 visits
            app.GetVisitorCount(Uris.ValidUri1).Should().Be(2);
            app.GetVisitorCount(Uris.ValidUri2).Should().Be(2);
        }
    }

    class TrackerApplication : WebApplicationFactory<Program>
    {
        override protected IHost CreateHost(IHostBuilder builder)
        {
            
            return base.CreateHost(builder);
        }

        internal void SetupVisit(Uri validUri1, string visitorId)
        {
            var tracker = Services.GetService(typeof(Tracker)) as Tracker;
            tracker!.RegisterVisit(validUri1, visitorId);
        }

        internal int GetVisitorCount(Uri validUri1)
        {
            var tracker = Services.GetService(typeof(Tracker)) as Tracker;
            return tracker!.GetUniqueVisitorCount(validUri1);
        }
    }
}
