using FluentAssertions;
using PageTracker.API;
using System;
using Xunit;

namespace PageTracker.Api.Tests;

public class Uris
{
    public static Uri ValidUri1 => new("http://google.com");
    public static Uri ValidUri2 => new("http://google.com/?search=x");
}

public class TrackerTests
{
    

    [Fact]
    public void UrisAreMatchable()
    {
        var uri1 = new Uri("http://google.com");
        var uri2 = new Uri("http://google.com");

        uri1.Should().Be(uri2);
    }

    public class TrackingUrlVistorCounts
    {
        [Fact]
        public void EmptyTrackerForUrlReturnsZeroVisitors()
        {
            var tracker = new Tracker();

            tracker.GetUniqueVisitorCount(Uris.ValidUri1).Should().Be(0);
        }

        [Fact]
        public void CanRecordANewVistorToUntrackedUrl()
        {
            var tracker = new Tracker();
            var visitorId = Guid.NewGuid().ToString();

            tracker.RegisterVisit(Uris.ValidUri1, visitorId);

            tracker.GetUniqueVisitorCount(Uris.ValidUri1).Should().Be(1);
        }

        [Fact]
        public void RepeatVistorCountsAsOneVisit()
        {
            var tracker = new Tracker();
            var visitorId = Guid.NewGuid().ToString();

            tracker.RegisterVisit(Uris.ValidUri1, visitorId);
            tracker.RegisterVisit(Uris.ValidUri1, visitorId);

            tracker.GetUniqueVisitorCount(Uris.ValidUri1).Should().Be(1);
        }

        [Fact]
        public void SameVistor2UrlsCountsAsOneVisitPerUrl()
        {
            var tracker = new Tracker();
            var visitorId = Guid.NewGuid().ToString();

            tracker.RegisterVisit(Uris.ValidUri1, visitorId);
            tracker.RegisterVisit(Uris.ValidUri2, visitorId);

            tracker.GetUniqueVisitorCount(Uris.ValidUri1).Should().Be(1);
            tracker.GetUniqueVisitorCount(Uris.ValidUri2).Should().Be(1);
        }

        [Fact]
        public void MultipleVistorsToSameSums()
        {
            var tracker = new Tracker();
            var visitorId1 = Guid.NewGuid().ToString();
            var visitorId2 = Guid.NewGuid().ToString();

            tracker.RegisterVisit(Uris.ValidUri1, visitorId1);
            tracker.RegisterVisit(Uris.ValidUri1, visitorId2);
            tracker.RegisterVisit(Uris.ValidUri1, visitorId1);
            tracker.RegisterVisit(Uris.ValidUri1, visitorId2);

            tracker.GetUniqueVisitorCount(Uris.ValidUri1).Should().Be(2);
        }

    }
}
