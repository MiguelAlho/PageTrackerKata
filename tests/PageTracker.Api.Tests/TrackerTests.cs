using FluentAssertions;
using PageTracker.API;
using System;
using Xunit;

namespace PageTracker.Api.Tests;

public class TrackerTests
{
    static Uri ValidUri1 = new Uri("http://google.com");
    static Uri ValidUri2 = new Uri("http://google.com/?search=x");

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

            tracker.GetUniqueVisitorCount(ValidUri1).Should().Be(0);
        }

        [Fact]
        public void CanRecordANewVistorToUntrackedUrl()
        {
            var tracker = new Tracker();
            var visitorId = Guid.NewGuid().ToString();

            tracker.RegisterVisit(ValidUri1, visitorId);

            tracker.GetUniqueVisitorCount(ValidUri1).Should().Be(1);
        }

        [Fact]
        public void RepeatVistorCountsAsOneVisit()
        {
            var tracker = new Tracker();
            var visitorId = Guid.NewGuid().ToString();

            tracker.RegisterVisit(ValidUri1, visitorId);
            tracker.RegisterVisit(ValidUri1, visitorId);

            tracker.GetUniqueVisitorCount(ValidUri1).Should().Be(1);
        }

        [Fact]
        public void SameVistor2UrlsCountsAsOneVisitPerUrl()
        {
            var tracker = new Tracker();
            var visitorId = Guid.NewGuid().ToString();

            tracker.RegisterVisit(ValidUri1, visitorId);
            tracker.RegisterVisit(ValidUri2, visitorId);

            tracker.GetUniqueVisitorCount(ValidUri1).Should().Be(1);
            tracker.GetUniqueVisitorCount(ValidUri2).Should().Be(1);
        }

        [Fact]
        public void MultipleVistorsToSameSums()
        {
            var tracker = new Tracker();
            var visitorId1 = Guid.NewGuid().ToString();
            var visitorId2 = Guid.NewGuid().ToString();

            tracker.RegisterVisit(ValidUri1, visitorId1);
            tracker.RegisterVisit(ValidUri1, visitorId2);
            tracker.RegisterVisit(ValidUri1, visitorId1);
            tracker.RegisterVisit(ValidUri1, visitorId2);

            tracker.GetUniqueVisitorCount(ValidUri1).Should().Be(2);
        }

    }
}
