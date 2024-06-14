using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wkg.AspNetCore.Authentication.Tests;

[TestClass]
public class CookieClaimManagerTests
{
    [TestMethod]
    public void SerializationTest()
    {
        // TODO: more tests
        CookieClaimManager<TestClaim> claimManager = new(new HttpContextAccessor(), new ClaimValidationOptions("podracer"), new SessionKeyStore());
        IClaimManager<TestClaim> manager = claimManager;
        DateTime now = DateTime.UtcNow;
        ClaimScopeData<TestClaim> originalData = new(new TestClaim("blah"), now.AddDays(1), []);
        string base64 = manager.Serialize(originalData);
        string base64_2 = manager.Serialize(originalData);
        Assert.AreEqual(base64, base64_2);
        Assert.IsTrue(manager.TryDeserialize(base64, out ClaimScopeData<TestClaim>? data));
        Assert.AreEqual(originalData.IdentityClaim.RawValue, data.IdentityClaim.RawValue);
        Assert.AreEqual(originalData.ExpirationDate, data.ExpirationDate);
        Assert.AreEqual(originalData.Claims.Length, data.Claims.Length);
    }
}

file class TestClaim(string rawValue) : IdentityClaim(rawValue);