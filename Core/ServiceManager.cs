﻿using Client;
using Common;
using Configuration;
using Interfaces;
using Interfaces.Common;
using Interfaces.DataContracts;
using Interfaces.Services;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class ServiceManager
    {
        ILogger log;
        Config config;
        List<ServiceHost> services;

        public ServiceManager(ILogger logger)
        {
            log = logger;
            services = new List<ServiceHost>();
        }

        public void Initialize()
        {
            Deinitialize();
            if (InitConfig())
            {
                log.TraceMessage("Initializing services");
                IConfigurationService cs = new ClientBuilder<IConfigurationService>(log, "ConfigurationService").Proxy;
                Device currDev = cs.GetCurrentDevice();
                if (currDev != null)
                {
                    foreach (Service s in currDev.Services)
                    {
                        Type serviceType = Type.GetType(string.Format(s.Type));
                        Type contractType = Type.GetType(string.Format(s.Contract));

                        log.TraceMessage(string.Format("Loading the service \"{0}\"", s.Type));

                        try
                        {
                            if (serviceType != null && contractType != null)
                            {
                                Uri baseAddress = new Uri(string.Format("http://{0}:{1}/{2}", currDev.Host, currDev.Port, s.Name));
                                ServiceHost host = new ServiceHost(serviceType, baseAddress);

                                Binding binding;
                                if (s.Advanced)
                                {
                                    binding = new WSDualHttpBinding();
                                    ((WSDualHttpBinding)binding).Security.Mode = WSDualHttpSecurityMode.None;
                                }
                                else
                                {
                                    binding = new BasicHttpBinding();
                                }

                                host.AddServiceEndpoint(contractType, binding, "");

                                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                                smb.HttpGetEnabled = true;
                                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                                host.Description.Behaviors.Add(smb);

                                host.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding(), "mex");

                                host.Open();

                                services.Add(host);
                            }
                            else
                            {
                                log.TraceMessage(string.Format("ERROR: cannot load the service \"{0}\". The type cannot be resolved", s.Type));
                            }
                        }
                        catch (Exception ex)
                        {
                            log.TraceMessage(string.Format("ERROR: cannot load the service \"{0}\"", s.Type));
                            log.TraceException(ex);
                        }
                    }
                }
                log.TraceMessage("Services initialized");
            }
        }

        public void Deinitialize()
        {
            if (services != null && services.Count > 0)
            {
                log.TraceMessage("Deinitializing services");
                foreach (ServiceHost h in services)
                {
                    try
                    {
                        h.Close();
                    }
                    catch (Exception ex)
                    {
                        log.TraceMessage("ERROR during the service deinitialization");
                        log.TraceException(ex);
                    }
                }
                services.Clear();
                log.TraceMessage("Services deinitialized");
            }
        }

        private bool InitConfig()
        {
            bool ret = true;
            log.TraceMessage("Initializing the configuration service");
            try
            {
                Uri baseAddress = new Uri("http://localhost:9876/ConfigurationService");
                ServiceHost host = new ServiceHost(typeof(ConfigurationService), baseAddress);

                BasicHttpBinding binding = new BasicHttpBinding();

                host.AddServiceEndpoint(typeof(IConfigurationService), binding, "");

                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                host.Description.Behaviors.Add(smb);

                host.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding(), "mex");

                host.Open();

                services.Add(host);
            }
            catch (Exception ex)
            {
                log.TraceMessage("ERROR: cannot load the configuration service");
                log.TraceException(ex);
            }

            return ret;
        }
    }
}
