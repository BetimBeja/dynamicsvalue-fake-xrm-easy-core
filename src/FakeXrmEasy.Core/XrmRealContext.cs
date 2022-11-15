﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xrm.Sdk;
using System.Configuration;
using System.IO;

using System.Xml.Linq;
using System.Linq;

using System.IO.Compression;
using System.Runtime.Serialization;

using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Plugins;
using FakeXrmEasy.Abstractions.Enums;

#if FAKE_XRM_EASY_NETCORE
using Microsoft.Powerplatform.Cds.Client;
#elif FAKE_XRM_EASY_2016 || FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
using Microsoft.Xrm.Tooling.Connector;
#else 
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
#endif

namespace FakeXrmEasy
{
    /// <summary>
    /// Reuse unit test syntax to test against a real CRM organisation
    /// It uses a real CRM organisation service instance
    /// </summary>
    public class XrmRealContext : IXrmRealContext
    {
        /// <summary>
        /// 
        /// </summary>
        public FakeXrmEasyLicense? LicenseContext { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ConnectionStringName { get; set; } = "fakexrmeasy-connection";

        /// <summary>
        /// Use these user to impersonate calls
        /// </summary>
        public ICallerProperties CallerProperties { get; set; }

        public IXrmFakedPluginContextProperties PluginContextProperties { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected IOrganizationService _service;

        private readonly Dictionary<string, object> _properties;

        /// <summary>
        /// 
        /// </summary>
        public XrmRealContext()
        {
            //Don't setup fakes in this case.
            _properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionStringName"></param>
        public XrmRealContext(string connectionStringName)
        {
            ConnectionStringName = connectionStringName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="organizationService"></param>
        public XrmRealContext(IOrganizationService organizationService)
        {
            _service = organizationService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool HasProperty<T>()
        {
            return _properties.ContainsKey(typeof(T).FullName);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="TypeAccessException"></exception>
        public T GetProperty<T>() 
        {
            if(!_properties.ContainsKey(typeof(T).FullName)) 
            {
                throw new TypeAccessException($"Property of type '{typeof(T).FullName}' doesn't exists");  
            }

            return (T) _properties[typeof(T).FullName];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        public void SetProperty<T>(T property) 
        {
            if(!_properties.ContainsKey(typeof(T).FullName)) 
            {
                _properties.Add(typeof(T).FullName, property);
            }
            else 
            {
                _properties[typeof(T).FullName] = property;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IOrganizationService GetOrganizationService()
        {
            if (_service != null)
                return _service;

            _service = GetOrgService();
            return _service;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected IOrganizationService GetOrgService()
        {
            var connection = ConfigurationManager.ConnectionStrings[ConnectionStringName];

            // In case of missing connection string in configuration,
            // use ConnectionStringName as an explicit connection string
            var connectionString = connection == null ? ConnectionStringName : connection.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception("The ConnectionStringName property must be either a connection string or a connection string name");
            }

            // Connect to the CRM web service using a connection string.
#if FAKE_XRM_EASY_NETCORE
            var client = new CdsServiceClient(connectionString);
#elif FAKE_XRM_EASY_2016 || FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
            var client = new CrmServiceClient(connectionString);
#else
            CrmConnection crmConnection = CrmConnection.Parse(connectionString);
            var client = new OrganizationService(crmConnection);
#endif
            return client;
        }

        public IXrmFakedTracingService GetTracingService()
        {
            throw new NotImplementedException();
        }
    }
}
