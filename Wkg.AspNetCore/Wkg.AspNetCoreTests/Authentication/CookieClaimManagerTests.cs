using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wkg.AspNetCore.Authentication.Tests;

[TestClass]
public class CookieClaimManagerTests
{
    [TestMethod]
    public void SerializationTest()
    {
        CookieClaimManager<TestClaim> claimManager = new(new HttpContextAccessor(), new ClaimValidationOptions("podracer"), new SessionKeyStore());
        IClaimManager<TestClaim> manager = claimManager;
        DateTime now = DateTime.UtcNow;
        string base64 = manager.Serialize(new ClaimScopeData<TestClaim>(new TestClaim("blah"), now.AddDays(1), []));
        string base642 = manager.Serialize(new ClaimScopeData<TestClaim>(new TestClaim("blah"), now.AddDays(1), []));
        Assert.AreEqual(base64, base642);
        Assert.IsTrue(manager.TryDeserialize(base64, out ClaimScopeData<TestClaim>? data));
    }
}

file class TestClaim(string rawValue) : IdentityClaim(rawValue);