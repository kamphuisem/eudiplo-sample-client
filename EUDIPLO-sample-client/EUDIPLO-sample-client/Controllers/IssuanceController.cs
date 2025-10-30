using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System.Text.Json;

namespace EUDIPLO_sample_client.Controllers;

public class IssuanceController : Controller
{
	private readonly RestClient _client = new();

	public IActionResult GetCredentialOffer(string issuanceConfigurationId)
	{
		var authUrl = Environment.GetEnvironmentVariable("AuthUrl") ?? "";
		var clientId = Environment.GetEnvironmentVariable("IssuerClientId") ?? "";
		var clientSecret = Environment.GetEnvironmentVariable("IssuerClientSecret") ?? "";

		var accessToken = GetAccessToken(authUrl, clientId, clientSecret);
		string? offer = null;
		if (accessToken != null)
		{
			offer = GetCredentialOffer(accessToken, issuanceConfigurationId);
		}

		return Ok(new
		{
			offer,
		});
	}

	public IActionResult CredentialOffer(string issuanceConfigurationId)
	{
		var authUrl = Environment.GetEnvironmentVariable("AuthUrl") ?? "";
		var clientId = Environment.GetEnvironmentVariable("IssuerClientId") ?? "";
		var clientSecret = Environment.GetEnvironmentVariable("IssuerClientSecret") ?? "";

		var accessToken = GetAccessToken(authUrl, clientId, clientSecret);
		if (accessToken != null)
		{
			ViewBag.CredentialOffer = GetCredentialOffer(accessToken, issuanceConfigurationId);
		}

		return View();
	}

	private string? GetCredentialOffer(string accessToken, string issuanceConfigurationId)
	{
		var issuerUrl = Environment.GetEnvironmentVariable("IssuerUrl") ?? "";

		var request = new RestRequest(issuerUrl, Method.Post);
		request.AddHeader("Accept", "application/json");
		request.AddHeader("Content-Type", "application/json");
		request.AddHeader("Authorization", "Bearer " + accessToken);
		request.AddJsonBody("{\"response_type\": \"uri\", \"issuanceId\": \"" + issuanceConfigurationId + "\"}");

		var response = _client.Execute(request);
		if (response != null && response.StatusCode == System.Net.HttpStatusCode.Created)
		{
			var x = JsonDocument.Parse(response.Content!);
			return x.RootElement.GetProperty("uri").Deserialize<string>();
		}

		return null;
	}

	private string? GetAccessToken(string authUrl, string clientId, string clientSecret)
	{
		var request = new RestRequest(authUrl, Method.Post);
		request.AddHeader("Cache-Control", "no-cache");
		request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
		request.AddParameter("application/x-www-form-urlencoded", $"grant_type=client_credentials&client_id={clientId}&client_secret={clientSecret}", ParameterType.RequestBody);
		var response = _client.Execute(request);
		if (response != null && response.StatusCode == System.Net.HttpStatusCode.Created)
		{
			var x = JsonDocument.Parse(response.Content!);
			return x.RootElement.GetProperty("access_token").Deserialize<string>();
		}

		return null;
	}
}
