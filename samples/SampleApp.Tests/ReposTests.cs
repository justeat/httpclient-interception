﻿// Copyright (c) Just Eat, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using JustEat.HttpClientInterception;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace SampleApp.Tests
{
    [Collection(HttpServerCollection.Name)] // Use the shared HTTP server fixture
    public sealed class ReposTests : IDisposable
    {
        public ReposTests(HttpServerFixture fixture, ITestOutputHelper outputHelper)
        {
            Fixture = fixture;
            Fixture.SetOutputHelper(outputHelper);
        }

        private HttpServerFixture Fixture { get; }

        [Fact]
        public async Task Can_Get_Organization_Repositories()
        {
            // Arrange - use a scope to clean-up registrations
            using (Fixture.Interceptor.BeginScope())
            {
                // Setup an expected response from the GitHub API
                var builder = new HttpRequestInterceptionBuilder()
                    .HavingId("GET Weyland Yutani repositories")
                    .Requests()
                    .ForHttps()
                    .ForHost("api.github.com")
                    .ForPath("orgs/weyland-yutani/repos")
                    .ForQuery("per_page=2")
                    .Responds()
                    .WithSystemTextJsonContent(
                        new[]
                        {
                            new { id = 1, name = "foo" },
                            new { id = 2, name = "bar" },
                        })
                    .RegisterWith(Fixture.Interceptor);

                string[] actual;

                // Act - Perform the HTTP request against our application and deserialize the response
                using (var httpClient = Fixture.CreateClient())
                {
                    string json = await httpClient.GetStringAsync("api/repos?count=2");
                    actual = JsonConvert.DeserializeObject<string[]>(json);
                }

                // Assert - Our application should have parsed the stub-names
                actual.ShouldBe(new[] { "bar", "foo" });
            }
        }

        public void Dispose()
        {
            Fixture.ClearOutputHelper();
        }
    }
}
