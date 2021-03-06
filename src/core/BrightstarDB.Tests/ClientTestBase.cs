﻿using System.Xml.Linq;
using NUnit.Framework;
#if !PORTABLE
using System;
using BrightstarDB.Client;
using BrightstarDB.Server.Modules;
using BrightstarDB.Server.Modules.Authentication;
using BrightstarDB.Server.Modules.Permissions;
using Nancy.Hosting.Self;


namespace BrightstarDB.Tests
{
    public class ClientTestBase
    {
        private static NancyHost _serviceHost;
        private static bool _closed;
        private static readonly object HostLock = new object();

        protected static void StartService()
        {
            StartServer();
        }

        protected static void CloseService()
        {
            lock (HostLock)
            {
                _serviceHost.Stop();
                _closed = true;
            }
        }

        private static void StartServer()
        {
            lock (HostLock)
            {
#if SDK_TESTS
    // We assume that the test framework starts up the service for us.
#else
                if (_serviceHost == null || _closed)
                {
                    _serviceHost = new NancyHost(new BrightstarBootstrapper(
                                                     BrightstarService.GetClient(),
                                                     new IAuthenticationProvider[] {new NullAuthenticationProvider()},
                                                     new FallbackStorePermissionsProvider(StorePermissions.All, StorePermissions.All),
                                                     new FallbackSystemPermissionsProvider(SystemPermissions.All, SystemPermissions.All)),
                                                     new HostConfiguration { AllowChunkedEncoding = false },
                                                 new Uri("http://localhost:8090/brightstar/"));
                    _serviceHost.Start();
                }
#endif
            }
        }

        protected static void AssertTriplePatternInGraph(IBrightstarService client, string storeName, string triplePattern,
            string graphUri)
        {
            var sparql = "ASK { GRAPH <" + graphUri + "> {" + triplePattern + "}}";
            var resultsDoc = XDocument.Load(client.ExecuteQuery(storeName, sparql));
            Assert.IsTrue(resultsDoc.SparqlBooleanResult());
        }

        protected static void AssertTriplePatternInDefaultGraph(IBrightstarService client, string storeName,
            string triplePattern)
        {
            var sparql = "ASK {{" + triplePattern + "}}";
            var resultsDoc = XDocument.Load(client.ExecuteQuery(storeName, sparql));
            Assert.IsTrue(resultsDoc.SparqlBooleanResult());
        }

        protected static void AssertTriplePatternNotInGraph(IBrightstarService client, string storeName, string triplePattern,
            string graphUri)
        {
            var sparql = "ASK { GRAPH <" + graphUri + "> {" + triplePattern + "}}";
            var resultsDoc = XDocument.Load(client.ExecuteQuery(storeName, sparql));
            Assert.IsFalse(resultsDoc.SparqlBooleanResult());
        }

        protected static void AssertTriplePatternNotInDefaultGraph(IBrightstarService client, string storeName,
            string triplePattern)
        {
            var sparql = "ASK {{" + triplePattern + "}}";
            var resultsDoc = XDocument.Load(client.ExecuteQuery(storeName, sparql));
            Assert.IsFalse(resultsDoc.SparqlBooleanResult());
        }
    }
}

#else

namespace BrightstarDB.Tests
{
    public class ClientTestBase
    {
        protected static void StartService()
        {
            // The test framework will have to control the service for us
        }

        protected static void CloseService()
        {
            // The test framework will have to control the service for us
        }
    }
}

#endif
