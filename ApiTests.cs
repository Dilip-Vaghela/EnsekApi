using System.Net;
using System.Text.Json.Nodes;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;

namespace EnsekApi;

[TestFixture]
public class Tests
{
    // The base URL for our example tests
    private const string BaseUrl = "https://qacandidatetest.ensek.io/";

    // The RestSharp client we'll use to make our requests
    private RestClient _client;

    [OneTimeSetUp]
    public void SetupRestSharpClient()
    {
        _client = new RestClient(BaseUrl);
    }

    [Test]
    public void TestResetTestData()
    {
        // setup login request
        var requestLogin = new RestRequest("/ENSEK/login", Method.Post);
        requestLogin.AddBody(new
        {
            password = "testing",
            username = "test"
        });

        // send request
        var responseLogin = _client.ExecutePost(requestLogin);
        Assert.That(responseLogin.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // deserialize json string response to JsonNode object
        var dataLogin = JsonSerializer.Deserialize<JsonNode>(responseLogin.Content!)!;
        var bearerToken = dataLogin["access_token"];

        Assert.That(dataLogin["message"].ToString().Equals("Success"));
        Console.WriteLine($"""{dataLogin}""");

        // setup reset request with bearer token 
        var requestReset = new RestRequest("/ENSEK/reset", Method.Post);
        requestReset.AddOrUpdateHeader("Authorization", $"Bearer {bearerToken}");

        // send request
        var responseReset = _client.ExecutePost(requestReset);
        Assert.That(responseLogin.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var dataReset = JsonSerializer.Deserialize<JsonNode>(responseReset.Content!)!;

        Assert.That(dataReset["message"].ToString().Equals("Success"));
        Console.WriteLine($"""{dataReset}""");
    }

    [TestCase(1, 1)]
    [TestCase(2, 1)]
    [TestCase(3, 1)]
    [TestCase(4, 1)]
    public void TestBuyAQuantityOfEachFuel(int id, int quantity)
    {
        var requestAddFuel = new RestRequest($"/ENSEK/buy/{id}/{quantity}", Method.Put);
        var responseAddFuel = _client.ExecutePut(requestAddFuel);
        var dataAddFuel = JsonSerializer.Deserialize<JsonNode>(responseAddFuel.Content!)!;
        Assert.That(dataAddFuel["message"].ToString().Contains("You have purchased "));
        var orderId = dataAddFuel["message"].ToString().Substring(dataAddFuel["message"].ToString().Length - 37);
        Console.WriteLine($"""{dataAddFuel}   asas  {orderId}""");

        var requestOrder = new RestRequest($"/ENSEK/orders", Method.Get);
        var responseOrder = _client.ExecuteGet(requestOrder);
        var dataOrder = JsonSerializer.Deserialize<JsonNode>(responseOrder.Content!)!;
        //Assert.That(dataOrder["Id"].ToString().Contains(orderId));
        Console.WriteLine($"""{dataOrder}   VS  {orderId}""");
    }
}