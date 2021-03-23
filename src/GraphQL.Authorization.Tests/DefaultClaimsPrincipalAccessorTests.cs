using System.Collections.Generic;
using System.Security.Claims;
using GraphQL.Validation;
using Shouldly;
using Xunit;

namespace GraphQL.Authorization.Tests
{
    public class DefaultClaimsPrincipalAccessorTests
    {
        [Fact]
        public void returns_null_from_null_user_context()
        {
            var accessor = new DefaultClaimsPrincipalAccessor();
            var context = new ValidationContext();
            accessor.GetClaimsPrincipal(context).ShouldBeNull();
        }

        [Fact]
        public void returns_null_from_empty_user_context()
        {
            var accessor = new DefaultClaimsPrincipalAccessor();
            var context = new ValidationContext { UserContext = new Dictionary<string, object>() };
            accessor.GetClaimsPrincipal(context).ShouldBeNull();
        }

        [Fact]
        public void returns_null_from_typed_user_context()
        {
            var accessor = new DefaultClaimsPrincipalAccessor();
            var context = new ValidationContext { UserContext = new TestContext1() };
            accessor.GetClaimsPrincipal(context).ShouldBeNull();
        }

        [Fact]
        public void returns_principal_from_typed_user_context()
        {
            var accessor = new DefaultClaimsPrincipalAccessor();
            var context = new ValidationContext { UserContext = new TestContext2() };
            accessor.GetClaimsPrincipal(context).ShouldNotBeNull();
        }

        private class TestContext1 : Dictionary<string, object>, IProvideClaimsPrincipal
        {
            public ClaimsPrincipal? User => null;
        }

        private class TestContext2 : Dictionary<string, object>, IProvideClaimsPrincipal
        {
            public ClaimsPrincipal? User => new ClaimsPrincipal();
        }
    }
}
