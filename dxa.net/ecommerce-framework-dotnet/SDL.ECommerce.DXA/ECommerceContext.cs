﻿using Sdl.Web.Mvc.Configuration;

using SDL.ECommerce.Api;
using SDL.ECommerce.Api.Model;
//using SDL.ECommerce.OData;
using SDL.ECommerce.Rest;

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Sdl.Web.Common;
using Sdl.Web.Common.Configuration;

namespace SDL.ECommerce.DXA
{
    
    /// <summary>
    /// E-Commerce Context
    /// </summary>
    public class ECommerceContext
    {
        public const string CURRENT_QUERY = "CurrentQuery";
        public const string QUERY_RESULT = "QueryResult";
        public const string URL_PREFIX = "UrlPrefix";
        public const string FACETS = "Facets";
        public const string PRODUCT = "Product";
        public const string CATEGORY = "Category";
        public const string ROOT_TITLE = "RootTitle";
        public const string SEARCH_PHRASE = "SearchPhrase";

        private static IDictionary<string, IECommerceClient> clients = new ConcurrentDictionary<string,IECommerceClient>();
        private static IECommerceLinkResolver linkResolver = new DXALinkResolver();

        private const int DEFAULT_CATEGORY_EXPIRY_TIMEOUT = 3600000;


        /// <summary>
        /// Client to E-Commerce services
        /// </summary>
        public static IECommerceClient Client
        {
            get
            {
                string locale = WebRequestContext.Localization.Culture;
                IECommerceClient client = null;
                if (!clients.TryGetValue(locale, out client))
                {
                    client = Create(locale);
                    clients.Add(locale, client);
                }
                return client;
            }
            
        }

        /// <summary>
        /// GEt client to E-Commerce services via locale
        /// </summary>
        public static IECommerceClient GetClient(Localization localization)
        {
            string locale = localization.Culture;
            IECommerceClient client = null;
            if (!clients.TryGetValue(locale, out client))
            {
                client = Create(locale);
                clients.Add(locale, client);
            }
            return client;


        }

        /// <summary>
        /// Link resolver for E-Commerce items
        /// </summary>
        public static IECommerceLinkResolver LinkResolver
        {
            get
            {
                return linkResolver;
            }
        }

        /// <summary>
        /// Create E-Commerce client for specified locale
        /// </summary>
        /// <param name="locale"></param>
        /// <returns></returns>
        private static IECommerceClient Create(string locale)
        {
            var endpointAddress = WebConfigurationManager.AppSettings["ecommerce-service-uri"];
            var clientType = WebConfigurationManager.AppSettings["ecommerce-service-client-type"] ?? "odata";
            var categoryExpiryTimeoutStr = WebConfigurationManager.AppSettings["ecommerce-category-expiry-timeout"];
            int categoryExpiryTimeout = categoryExpiryTimeoutStr != null ? Int32.Parse(categoryExpiryTimeoutStr) : DEFAULT_CATEGORY_EXPIRY_TIMEOUT;
            if (clientType.Equals("odata"))
            {

                // TODO: Get token service data here as well
                return new SDL.ECommerce.OData.ECommerceClient(endpointAddress, locale, DependencyResolver.Current.GetService);
            }
            else if (clientType.Equals("rest"))
            {

                // TODO: Add support for dependency injection on the REST client as well!!
                //

                return new ECommerceClient(endpointAddress, locale, new DXACacheProvider(locale), categoryExpiryTimeout);
            }
            throw new DxaException("Invalid client type configured for the E-Commerce service: " + clientType);
        }

        /// <summary>
        /// Get request E-Commerce property
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object Get(string key)
        {
            return HttpContext.Current == null ? null : HttpContext.Current.Items["ECOM-" + key];
        }

        /// <summary>
        /// Set request E-Commerce property
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object Set(string key, object value)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items["ECOM-" + key] = value;
            }
            return value;
        }

        /// <summary>
        /// Localize path
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string LocalizePath(string url)
        {
            string path = WebRequestContext.Localization.Path;
            if (!String.IsNullOrEmpty(path))
            {
                if (path.EndsWith("/"))
                {
                    url = path + (url.StartsWith("/") ? url.Substring(1) : url);
                }
                else
                {
                    url = path + (url.StartsWith("/") ? url : '/' + url);
                }
            }
            return url;
        }

        public static bool ShowEditUrl(IEditable editableItem)
        {
            return WebRequestContext.IsPreview && editableItem != null && !String.IsNullOrEmpty(editableItem.EditUrl);
        }

        public static ICart Cart
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current.Session["ECOM-Cart"] as ICart;
                }
                return null;
            }
            set
            {
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Session["ECOM-Cart"] = value;
                }
            }
        }

        /// <summary>
        /// Get edit URL for an editable item. All requests are passed through the edit proxy. 
        /// </summary>
        /// <param name="editableItem"></param>
        /// <returns></returns>
        public static string EditUrl(IEditable editableItem)
        {
            return "/edit-proxy" + editableItem.EditUrl;
        }

        /// <summary>
        /// Get edit menu
        /// </summary>
        public static IEditMenu EditMenu
        {
            get
            {
                var currentQuery = Get(CURRENT_QUERY) as Query;
                if (currentQuery != null && WebRequestContext.IsPreview )
                {
                    return Client.EditService.GetInContextMenuItems(currentQuery);
                }
                return null;
            }
        }
    }
}
