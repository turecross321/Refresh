using System.Security.Cryptography;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace RefreshTests.GameServer.Tests.Assets;

public class AssetUploadTests : GameServerTest
{
    [Test]
    public void CanUploadAsset()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddService<ImportService>();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        ReadOnlySpan<byte> data = "TEX a"u8;
        
        string hash = BitConverter.ToString(SHA1.HashData(data))
            .Replace("-", "")
            .ToLower();

        HttpResponseMessage response = client.PostAsync("/lbp/upload/" + hash, new ByteArrayContent(data.ToArray())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));
    }
    
    [Test]
    public void CanUploadAssetPsp()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddService<ImportService>();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        client.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");

        ReadOnlySpan<byte> data = "TEX a"u8;
        
        string hash = BitConverter.ToString(SHA1.HashData(data))
            .Replace("-", "")
            .ToLower();

        HttpResponseMessage response = client.PostAsync("/lbp/upload/" + hash, new ByteArrayContent(data.ToArray())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));
    }
    
    [Test]
    public void CantUploadAssetWithInvalidHash()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddService<ImportService>();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        ReadOnlySpan<byte> data = "TEX a"u8;
        
        HttpResponseMessage response = client.PostAsync("/lbp/upload/6e4d252f247e3aa99ef846df8c65493393e79f4f", new ByteArrayContent(data.ToArray())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void CantUploadBlockedAsset()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddService<ImportService>();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        ReadOnlySpan<byte> data = "FSHbthsa"u8;
        
        string hash = BitConverter.ToString(SHA1.HashData(data))
            .Replace("-", "")
            .ToLower();

        HttpResponseMessage response = client.PostAsync("/lbp/upload/" + hash, new ByteArrayContent(data.ToArray())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(Unauthorized));
    }
    
    [Test]
    public void DataStoreWriteFailReturnsInternalServerError()
    {
        using TestContext context = this.GetServer(true, new WriteFailingDataStore());
        context.Server.Value.Server.AddService<ImportService>();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        ReadOnlySpan<byte> data = "TEX a"u8;
        
        string hash = BitConverter.ToString(SHA1.HashData(data))
            .Replace("-", "")
            .ToLower();

        HttpResponseMessage response = client.PostAsync("/lbp/upload/" + hash, new ByteArrayContent(data.ToArray())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(InternalServerError));
    }
    
    [Test]
    public void CantUploadDuplicateAssets()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddService<ImportService>();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        ReadOnlySpan<byte> data = "TEX a"u8;
        
        string hash = BitConverter.ToString(SHA1.HashData(data))
            .Replace("-", "")
            .ToLower();

        HttpResponseMessage response = client.PostAsync("/lbp/upload/" + hash, new ByteArrayContent(data.ToArray())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));
        
        response = client.PostAsync("/lbp/upload/" + hash, new ByteArrayContent(data.ToArray())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(Conflict));
    }
    
    [Test]
    public void CantUploadTooLarge()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddService<ImportService>();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        //create 5mb array
        ReadOnlySpan<byte> data = new byte[5000000];
        
        string hash = BitConverter.ToString(SHA1.HashData(data))
            .Replace("-", "")
            .ToLower();

        HttpResponseMessage response = client.PostAsync("/lbp/upload/" + hash, new ByteArrayContent(data.ToArray())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(RequestEntityTooLarge));
    }
    
    [Test]
    public void InvalidAssetHashUploadFails()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddService<ImportService>();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        ReadOnlySpan<byte> data = "TEX a"u8;
        
        string hash = BitConverter.ToString(SHA1.HashData(data))
            .Replace("-", "")
            .ToLower();

        HttpResponseMessage response = client.PostAsync("/lbp/upload/I_AM_NOT_REAL", new ByteArrayContent(data.ToArray())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(BadRequest));
    }

    [Test]
    public void CanRetrieveAsset()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddService<ImportService>();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        ReadOnlySpan<byte> data = "TEX a"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data))
            .Replace("-", "")
            .ToLower();

        client.PostAsync("/lbp/upload/" + hash, new ByteArrayContent(data.ToArray())).Wait();
        HttpResponseMessage response = client.GetAsync("/lbp/r/" + hash).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));
        byte[] returnedData = response.Content.ReadAsByteArrayAsync().Result;
        
        Assert.That(data.SequenceEqual(returnedData), Is.True);
    }

    [Test]
    public void InvalidHashFails()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddService<ImportService>();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        HttpResponseMessage response = client.GetAsync("/lbp/r/asdf").Result;
        Assert.That(response.StatusCode, Is.EqualTo(BadRequest));
        
        response = client.GetAsync("/lbp/r/..%2Frpc.json").Result;
        Assert.That(response.StatusCode, Is.EqualTo(BadRequest));
    }
}