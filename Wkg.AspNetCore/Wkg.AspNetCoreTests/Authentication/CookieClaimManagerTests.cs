using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wkg.AspNetCore.Authentication.Claims;
using Wkg.AspNetCore.Authentication.CookieBased;
using Wkg.AspNetCore.Authentication.Internals;

namespace Wkg.AspNetCore.Authentication.Tests;

[TestClass]
public class CookieClaimManagerTests
{
    private static CookieClaimManager<TestIdentityClaim> CreateClaimManager() => 
        new(new HttpContextAccessor(), new ClaimValidationOptions("podracer"), new SessionKeyStore());

    [TestMethod]
    public void TestDeterministic1()
    {
        IClaimManager<TestIdentityClaim> manager = CreateClaimManager();
        DateTime now = DateTime.UtcNow;
        ClaimRepositoryData<TestIdentityClaim> originalData = new(new TestIdentityClaim("blah"), now.AddDays(1), []);
        string base64 = manager.Serialize(originalData);
        string base64_2 = manager.Serialize(originalData);
        Assert.AreEqual(base64, base64_2);
    }

    [TestMethod]
    public void TestDeterministic2()
    {
        IClaimManager<TestIdentityClaim> manager = CreateClaimManager();
        DateTime now = DateTime.UtcNow;
        ClaimRepositoryData<TestIdentityClaim> data1 = new(new TestIdentityClaim("blah"), now.AddDays(1), []);
        ClaimRepositoryData<TestIdentityClaim> data2 = new(new TestIdentityClaim("blah"), now.AddDays(1), []);
        string base64 = manager.Serialize(data1);
        string base64_2 = manager.Serialize(data2);
        Assert.AreEqual(base64, base64_2);
    }

    [TestMethod]
    public void TestDeterministic3()
    {
        IClaimManager<TestIdentityClaim> manager = CreateClaimManager();
        DateTime now = DateTime.UtcNow;
        Claim<int> claim = new("bloo", 1);
        ClaimRepositoryData<TestIdentityClaim> originalData = new(new TestIdentityClaim("blah"), now.AddDays(1), [claim]);
        string base64 = manager.Serialize(originalData);
        string base64_2 = manager.Serialize(originalData);
        Assert.AreEqual(base64, base64_2);
    }

    [TestMethod]
    public void TestDeterministic4()
    {
        IClaimManager<TestIdentityClaim> manager = CreateClaimManager();
        DateTime now = DateTime.UtcNow;

        Claim<int> claim1 = new("bloo", 1);
        Claim<int> claim2 = new("bloo", 1);
        ClaimRepositoryData<TestIdentityClaim> data1 = new(new TestIdentityClaim("blah"), now.AddDays(1), [claim1]);
        ClaimRepositoryData<TestIdentityClaim> data2 = new(new TestIdentityClaim("blah"), now.AddDays(1), [claim2]);
        string base64 = manager.Serialize(data1);
        string base64_2 = manager.Serialize(data2);
        Assert.AreEqual(base64, base64_2);
    }

    [TestMethod] 
    public void TestDeserialization()
    {
        IClaimManager<TestIdentityClaim> manager = CreateClaimManager();
        DateTime now = DateTime.UtcNow;
        Claim<int> claim = new("bloo", 1);
        ClaimRepositoryData<TestIdentityClaim> originalData = new(new TestIdentityClaim("blah"), now.AddDays(1), [claim]);
        string base64 = manager.Serialize(originalData);
        Assert.IsTrue(manager.TryDeserialize(base64, out ClaimRepositoryData<TestIdentityClaim>? data));
        Assert.AreEqual(originalData.IdentityClaim.RawValue, data.IdentityClaim.RawValue);
        Assert.AreEqual(originalData.ExpirationDate, data.ExpirationDate);
        Assert.AreEqual(originalData.Claims.Length, data.Claims.Length);
        Assert.IsTrue(data.Claims.All(original => data.Claims.Any(actual => actual.Subject == original.Subject && actual.RawValue == original.RawValue)));
    }

    [TestMethod]
    public void SerializationTest()
    {
        IClaimManager<TestIdentityClaim> manager = CreateClaimManager();
        DateTime now = DateTime.UtcNow;
        ClaimRepositoryData<TestIdentityClaim> originalData = new(new TestIdentityClaim("blah"), now.AddDays(1), []);
        string base64 = manager.Serialize(originalData);
        string base64_2 = manager.Serialize(originalData);
        Assert.AreEqual(base64, base64_2);
        Assert.IsTrue(manager.TryDeserialize(base64, out ClaimRepositoryData<TestIdentityClaim>? data));
        Assert.AreEqual(originalData.IdentityClaim.RawValue, data.IdentityClaim.RawValue);
        Assert.AreEqual(originalData.ExpirationDate, data.ExpirationDate);
        Assert.AreEqual(originalData.Claims.Length, data.Claims.Length);
    }
}

internal class TestIdentityClaim(string rawValue) : IdentityClaim(rawValue);