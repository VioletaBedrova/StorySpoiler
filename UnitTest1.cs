using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoiler.Models;
using System.Net;
using System.Text.Json;

namespace Story
{
    [TestFixture]
    public class StoryTests
    {
        private RestClient client;
        private static string createdStoryId;
        private static string baseURL = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("reex", "qwerty123");

            
            var options = new RestClientOptions(baseURL)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        
        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseURL);

            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString();
        }

        [Test]
        [Order(1)]
        public void CreateStory_ShouldReturnCreated()
        {
            var storyRequest = new StoryDTO
            {
                Title = "New Story",
                Description = "Test story description",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(storyRequest);

            var response =this. client.Execute(request);
            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(createResponse.Msg, Is.EqualTo("Successfully created!"));


            createdStoryId = createResponse.Id;



        }

       

        [Test]
        [Order(2)]
        public void EditStoryTitle_ShouldReturnOk()
        {
            var editrequest = new StoryDTO
            {
                Title = "Edited Title",
                Description = "Some Description",
                Url = ""
            };
            var request = new RestRequest($"/api/Story/Edit/storyId", Method.Put);
            var response = this.client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            
        }
        [Test]
        [Order(3)]
        public void Test_GetAllStorySpoilers_ShouldReturn200AndList()
        {
            var request = new RestRequest($"/api/Story/All", Method.Get);
            var response = this.client.Execute(request);

            var responseItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseItems, Is.Not.Null);
            Assert.That(responseItems, Is.Not.Empty);
        }
        [Test]
        [Order(4)]

       
        public void DeleteStorySpoiler_ShouldReturn200()
        {
            var request = new RestRequest($"Story/Delete/{createdStoryId}", Method.Delete);
        
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));

        }

        [Test]  
        [Order(5)]
        public void CreateStory_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var storyRequest = new StoryDTO
            {
                Title = "",
                Description = "",
                
            };
            var request = new RestRequest($"/api/Story/Create", Method.Post);
            request.AddJsonBody(storyRequest);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        [Order(6)]
        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            string nonExistingStoryId = "524";
            var editRequest = new StoryDTO
            {
                Title = "jahsjhsjhjs",
                Description = "ushuwqu",
                Url = ""
            };
            var request = new RestRequest($"/api/Story/Edit/storyId", Method.Put);
            request.AddQueryParameter("storyId", nonExistingStoryId);
            request.AddJsonBody(editRequest);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No spoilers..."));
            


        }

        [Test]
        [Order(7)]
        public void DeleteNonExistingStory_ShouldReturnBadRequest()
        {
            string nonExistingStoryId = "42442";
            var request = new RestRequest($"/api/Story/Delete/storyId", Method.Delete);
            request.AddQueryParameter("storyId", nonExistingStoryId);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));

        }







        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}