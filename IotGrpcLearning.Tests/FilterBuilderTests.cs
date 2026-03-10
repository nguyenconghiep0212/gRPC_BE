using IotGrpcLearning.Infrastructure;
using System.Collections.Generic;
using Xunit;

namespace IotGrpcLearning.Tests
{
    public class FilterBuilderTests
    {
        [Fact]
        public void BuildFilterQuery_EmptyFilters_ReturnsEmpty()
        {
            var helper = new Helper();
            var (query, parameters) = helper.BuildFilterQuery("Projects", new 
                Dictionary<string, string[]>());
            Assert.Equal(string.Empty, query);
            Assert.Empty(parameters);
        }

        [Fact]
        public void BuildFilterQuery_SingleColumnMultipleValues_GeneratesParameters()
        {
            var helper = new Helper();
            var filters = new Dictionary<string, string[]>
            {
                ["name"] = new[] { "alpha", "beta" }
            };

            var (query, parameters) = helper.BuildFilterQuery("Projects", filters);

            // We expect a WHERE clause (phase 0: tests assert non-empty and parameter count)
            Assert.Contains("WHERE", query.ToUpperInvariant());
            Assert.Equal(2, parameters.Count);
        }
    }
}