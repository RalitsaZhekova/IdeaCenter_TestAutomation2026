using System;
using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;

using ExamPrepIdeaCenter.Models;

namespace ExamPrepIdeaCenter
{
    [TestFixture]
    public class Tests
    {
        private RestClient _client;
        private static string? lastIdeaID;

        private const string baseURL = "http://144.91.123.158:82/api";

        private const string loginEmail = "user@example.com";
        private const string loginPass = "myPassword";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken(loginEmail, loginPass);

            var options = new RestClientOptions(baseURL)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this._client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestClient(baseURL);
            var request = new RestRequest("/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });

            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                string token = content.GetProperty("accessToken").ToString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Failed to retrieve token");
                }
                return token;
            }

            throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
        }

        [Order(1)]
        [Test]
        public void CreateIdea_WithRequiredFields_Returns200Success()
        {
            var ideaBody = new IdeaDTO
            {
                Title = "My new Idea",
                Description = "Detailed Description...",
                Url = ""
            };

            var request = new RestRequest("/Idea/Create", Method.Post);
            request.AddJsonBody(ideaBody);

            var response = this._client.Execute(request);
            var responseItem = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
        
            Assert.That(responseItem, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expect status code 200 OK");
                Assert.That(responseItem.Msg, Is.EqualTo("Successfully created!"));
            });
        }

        [Order(2)]
        [Test]
        public void ListAllIdeas_Returns200Success()
        {
            var request = new RestRequest("/Idea/All", Method.Get);
        
            var response = this._client.Execute(request);
            var responseItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(responseItems, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expect status code 200 OK");
                Assert.That(responseItems, Is.Not.Null);
                Assert.That(responseItems, Is.Not.Empty);
            });

            lastIdeaID = responseItems.Last().IdeaId;
        }

        [Order(3)]
        [Test]
        public void EditIdea_Returns200Success()
        {
            var editedIdeaBody = new IdeaDTO
            {
                Title = "My edited Idea",
                Description = "Edited Description...",
                Url = ""
            };

            var request = new RestRequest("/Idea/Edit", Method.Put);
            request.AddQueryParameter("ideaId", lastIdeaID);
            request.AddJsonBody(editedIdeaBody);

            var response = this._client.Execute(request);
            var responseItem = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(responseItem, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expect status code 200 OK");
                Assert.That(responseItem.Msg, Is.EqualTo("Edited successfully"));
            });
        }

        [Order(4)]
        [Test]
        public void DeleteIdea_Returns200Success_MsgAsString()
        {
            var request = new RestRequest("/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaId", lastIdeaID);

            var response = this._client.Execute(request);
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expect status code 200 OK");
                Assert.That(response.Content, Is.EqualTo("\"The idea is deleted!\""));
            });
        }

        [Order(5)]
        [Test]
        public void CreateIdea_WithMissingFields_Returns400BadRequest()
        {
            var ideaBody = new IdeaDTO
            {
                Title = "",
                Description = "",
                Url = ""
            };

            var request = new RestRequest("/Idea/Create", Method.Post);
            request.AddJsonBody(ideaBody);

            var response = this._client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expect status code 400 BadRequest");
        }

        [Order(6)]
        [Test]
        public void EditIdea_WithInvalidID_Returns400BadRequest()
        {
            var editedIdeaBody = new IdeaDTO
            {
                Title = "My edited Idea",
                Description = "Edited Description...",
                Url = ""
            };

            var request = new RestRequest("/Idea/Edit", Method.Put);
            request.AddQueryParameter("ideaId", -1);
            request.AddJsonBody(editedIdeaBody);

            var response = this._client.Execute(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expect status code 400 BadRequest");
                Assert.That(response.Content, Is.EqualTo("\"There is no such idea!\""));
            });
        }

        [Order(7)]
        [Test]
        public void DeleteIdea_WithInvalidID_Returns400BadRequest()
        {
            var request = new RestRequest("/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaId", -1);

            var response = this._client.Execute(request);
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expect status code 400 BadRequest");
                Assert.That(response.Content, Is.EqualTo("\"There is no such idea!\""));
            });
        }

        [OneTimeTearDown]
        public void TearDown() 
        {
            this._client?.Dispose();
        }
    }
}