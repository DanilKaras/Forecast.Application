﻿using System;
using System.Configuration;
using AymanMVCProject.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RestSharp;

namespace DataCoin.Utility
{
    public static class StaticUtility
    {
        public static IRestResponse GenerateRestUrl(string url, string apiKey)
        {
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);

            request.AddHeader("X-CoinAPI-Key", apiKey);
            var response = client.Execute(request);
            return response.StatusDescription == "Too Many Requests" ? null : response;            
        } 

        public static DateTime TimeConverter(string time)
        {
            if (time.IndexOf('.') != -1)
            {
                return DateTime.Parse(time.Remove(time.IndexOf('.'), time.Length - time.IndexOf('.')));
            }

            return DateTime.MinValue;
        }
    }
}