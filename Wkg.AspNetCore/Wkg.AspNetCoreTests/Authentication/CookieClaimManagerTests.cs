using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wkg.AspNetCore.Authentication.Jwt.Claims;
using Wkg.AspNetCore.Authentication.Jwt;
using Wkg.AspNetCore.Authentication.Jwt.Implementations.CookieBased;
using Wkg.AspNetCore.Authentication.Jwt.Internals;
using Wkg.AspNetCore.Authentication.Jwt.SigningFunctions;
using Wkg.AspNetCore.Authentication.Jwt.SigningFunctions.Implementations;

namespace Wkg.AspNetCore.Authentication.Tests;

[TestClass]
public class CookieClaimManagerTests
{
    private static CookieClaimManager<TestIdentityClaim, NoDecryptionKeys> CreateClaimManager() => new
    (
        new HttpContextAccessor(), 
        new CookieClaimOptions(false, new ClaimValidationOptions(TimeSpan.FromHours(12))), 
        new SessionKeyStore<NoDecryptionKeys>(TimeSpan.FromHours(12)),
        new HmacSha512SigningFunction(new HmacOptions("secret-key"))
    );

    [TestMethod]
    public void TestDeterministic1()
    {
        IClaimManager<TestIdentityClaim, NoDecryptionKeys> manager = CreateClaimManager();
        DateTime now = DateTime.UtcNow;
        ClaimRepositoryData<TestIdentityClaim, NoDecryptionKeys> originalData = new(new TestIdentityClaim("blah"), now.AddDays(1), []);
        string base64 = manager.Serialize(originalData);
        string base64_2 = manager.Serialize(originalData);
        Assert.AreEqual(base64, base64_2);
    }

    [TestMethod]
    public void TestDeterministic2()
    {
        IClaimManager<TestIdentityClaim, NoDecryptionKeys> manager = CreateClaimManager();
        DateTime now = DateTime.UtcNow;
        ClaimRepositoryData<TestIdentityClaim, NoDecryptionKeys> data1 = new(new TestIdentityClaim("blah"), now.AddDays(1), []);
        ClaimRepositoryData<TestIdentityClaim, NoDecryptionKeys> data2 = new(new TestIdentityClaim("blah"), now.AddDays(1), []);
        string base64 = manager.Serialize(data1);
        string base64_2 = manager.Serialize(data2);
        Assert.AreEqual(base64, base64_2);
    }

    [TestMethod]
    public void TestDeterministic3()
    {
        IClaimManager<TestIdentityClaim, NoDecryptionKeys> manager = CreateClaimManager();
        DateTime now = DateTime.UtcNow;
        Claim<int> claim = new("bloo", 1);
        ClaimRepositoryData<TestIdentityClaim, NoDecryptionKeys> originalData = new(new TestIdentityClaim("blah"), now.AddDays(1), [claim]);
        string base64 = manager.Serialize(originalData);
        string base64_2 = manager.Serialize(originalData);
        Assert.AreEqual(base64, base64_2);
    }

    [TestMethod]
    public void TestDeterministic4()
    {
        IClaimManager<TestIdentityClaim, NoDecryptionKeys> manager = CreateClaimManager();
        DateTime now = DateTime.UtcNow;

        Claim<int> claim1 = new("bloo", 1);
        Claim<int> claim2 = new("bloo", 1);
        ClaimRepositoryData<TestIdentityClaim, NoDecryptionKeys> data1 = new(new TestIdentityClaim("blah"), now.AddDays(1), [claim1]);
        ClaimRepositoryData<TestIdentityClaim, NoDecryptionKeys> data2 = new(new TestIdentityClaim("blah"), now.AddDays(1), [claim2]);
        string base64 = manager.Serialize(data1);
        string base64_2 = manager.Serialize(data2);
        Assert.AreEqual(base64, base64_2);
    }

    [TestMethod] 
    public void TestDeserialization()
    {
        IClaimManager<TestIdentityClaim, NoDecryptionKeys> manager = CreateClaimManager();
        DateTime now = DateTime.UtcNow;
        Claim<int> claim = new("bloo", 1);
        ClaimRepositoryData<TestIdentityClaim, NoDecryptionKeys> originalData = new(new TestIdentityClaim("blah"), now.AddDays(1), [claim]);
        string base64 = manager.Serialize(originalData);
        Assert.IsTrue(manager.TryDeserialize(base64, out ClaimRepositoryData<TestIdentityClaim, NoDecryptionKeys>? data, out ClaimRepositoryStatus status));
        Assert.AreEqual(originalData.IdentityClaim.RawValue, data.IdentityClaim.RawValue);
        Assert.AreEqual(originalData.ExpirationDate, data.ExpirationDate);
        Assert.AreEqual(originalData.Claims.Length, data.Claims.Length);
        Assert.IsTrue(data.Claims.All(original => data.Claims.Any(actual => actual.Subject == original.Subject && actual.RawValue == original.RawValue)));
        Assert.AreEqual(ClaimRepositoryStatus.Valid, status);
    }

    [TestMethod]
    public void SerializationTest()
    {
        IClaimManager<TestIdentityClaim, NoDecryptionKeys> manager = CreateClaimManager();
        DateTime now = DateTime.UtcNow;
        ClaimRepositoryData<TestIdentityClaim, NoDecryptionKeys> originalData = new(new TestIdentityClaim("blah"), now.AddDays(1), []);
        string base64 = manager.Serialize(originalData);
        string base64_2 = manager.Serialize(originalData);
        Assert.AreEqual(base64, base64_2);
        Assert.IsTrue(manager.TryDeserialize(base64, out ClaimRepositoryData<TestIdentityClaim, NoDecryptionKeys>? data, out ClaimRepositoryStatus status));
        Assert.AreEqual(originalData.IdentityClaim.RawValue, data.IdentityClaim.RawValue);
        Assert.AreEqual(originalData.ExpirationDate, data.ExpirationDate);
        Assert.AreEqual(originalData.Claims.Length, data.Claims.Length);
        Assert.AreEqual(ClaimRepositoryStatus.Valid, status);
    }
}

internal class TestIdentityClaim(string rawValue) : IdentityClaim(rawValue);