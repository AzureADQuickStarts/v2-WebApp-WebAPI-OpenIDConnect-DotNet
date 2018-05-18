﻿using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TodoList_WebApp.Utils;

namespace TodoList_WebApp.Controllers
{
    [Authorize]
    public class TodoListController : Controller
    {
        private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private static string todoListServiceUrl = ConfigurationManager.AppSettings["ida:TodoServiceUrl"];
        private ConfidentialClientApplication app = null;

        // A sample 


        // GET: TodoList
        public async Task<ActionResult> Index()
        {
            AuthenticationResult result = null;

            try
            {
                string userObjectId = ClaimsPrincipal.Current.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

                ClientCredential credential = new ClientCredential(Startup.clientSecret);

                // Here you ask for a token using the web app's clientId as the scope, since the web app and service share the same clientId.
                app = new ConfidentialClientApplication(Startup.clientId, redirectUri, credential, new NaiveSessionCache(userObjectId, this.HttpContext));

                //Request an access token to access your Web API using api://{WebAPIClientId}/{Scope}
                result = await app.AcquireTokenSilentAsync(new string[]
                {
                    Startup.TodoListServiceScope,
                });

                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, todoListServiceUrl + "/api/todolist");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);

                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    String responseString = await response.Content.ReadAsStringAsync();
                    JArray todoList = JArray.Parse(responseString);
                    ViewBag.TodoList = todoList;
                    return View();
                }
                else
                {
                    // If the call failed with access denied, then drop the current access token from the cache, 
                    // and show the user an error indicating they might need to sign-in again.
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        app.AppTokenCache.Clear(Startup.clientId);

                        return new RedirectResult("/Error?message=Error: " + response.ReasonPhrase + " You might need to sign in again.");
                    }
                }

                return new RedirectResult("/Error?message=An Error Occurred Reading To Do List: " + response.StatusCode);
            }
            catch (MsalException ee)
            {
                // If MSAL could not get a token silently, show the user an error indicating they might need to sign in again.
                return new RedirectResult("/Error?message=An Error Occurred Reading To Do List: " + ee.Message + " You might need to log out and log back in.");
            }
            catch (Exception ex)
            {
                return new RedirectResult("/Error?message=An Error Occurred Reading To Do List: " + ex.Message);
            }
        }

        // POST: TodoList/Create
        [HttpPost]
        public async Task<ActionResult> Create(string description)
        {
            AuthenticationResult result = null;

            try
            {
                string userObjectId = ClaimsPrincipal.Current.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

                ClientCredential credential = new ClientCredential(Startup.clientSecret);

                // Here you ask for a token using the web app's clientId as the scope, since the web app and service share the same clientId.
                app = new ConfidentialClientApplication(Startup.clientId, redirectUri, credential, new NaiveSessionCache(userObjectId, this.HttpContext));

                //Request an access token to access your Web API using api://{WebAPIClientId}/{Scope}
                result = await app.AcquireTokenSilentAsync(new string[]
                {
                    Startup.TodoListServiceScope,
                });

                HttpContent content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("Description", description) });
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, todoListServiceUrl + "/api/todolist");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
                request.Content = content;
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return new RedirectResult("/TodoList");
                }
                else
                {
                    // If the call failed with access denied, then drop the current access token from the cache, 
                    // and show the user an error indicating they might need to sign-in again.
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        app.AppTokenCache.Clear(Startup.clientId);

                        return new RedirectResult("/Error?message=Error: " + response.ReasonPhrase + " You might need to sign in again.");
                    }
                }

                return new RedirectResult("/Error?message=Error reading your To-Do List.");
            }
            catch (MsalException ex)
            {
                // If MSAL could not get a token silently, show the user an error indicating they might need to sign in again.
                return new RedirectResult("/Error?message=Error: " + ex.Message + " You might need to sign in again.");
            }
            catch (Exception ex)
            {
                return new RedirectResult("/Error?message=Error reading your To-Do List.  " + ex.Message);
            }
        }

        // POST: /TodoList/Delete
        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            AuthenticationResult result = null;

            try
            {
                string userObjectId = ClaimsPrincipal.Current.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

                ClientCredential credential = new ClientCredential(Startup.clientSecret);

                // Here you ask for a token using the web app's clientId as the scope, since the web app and service share the same clientId.
                app = new ConfidentialClientApplication(Startup.clientId, redirectUri, credential, new NaiveSessionCache(userObjectId, this.HttpContext));

                //Request an access token to access your Web API using api://{WebAPIClientId}/{Scope}
                result = await app.AcquireTokenSilentAsync(new string[]
                {
                    Startup.TodoListServiceScope,
                });
                
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, todoListServiceUrl + "/api/todolist/" + id);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return new RedirectResult("/TodoList");
                }
                else
                {
                    // If the call failed with access denied, then drop the current access token from the cache, 
                    // and show the user an error indicating they might need to sign-in again.
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        app.AppTokenCache.Clear(Startup.clientId);

                        return new RedirectResult("/Error?message=Error: " + response.ReasonPhrase + " You might need to sign in again.");
                    }
                }

                return new RedirectResult("/Error?message=Error deleting your To-Do Item.");
            }
            catch (MsalException ex)
            {
                // If MSAL could not get a token silently, show the user an error indicating they might need to sign in again.
                return new RedirectResult("/Error?message=Error: " + ex.Message + " You might need to sign in again.");
            }
            catch (Exception ex)
            {
                return new RedirectResult("/Error?message=Error deleting your To-Do Item.  " + ex.Message);
            }
        }
    }
}