﻿using Common;
using Configuration;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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
            log.TraceMessage("Initializing services");
            bool ret = true;
            config = new Config();
            if (config.Load(Constants.CONFIG_FILENAME))
            {
                foreach(Service s in config.Services)
                {
                    Type serviceType = Type.GetType(string.Format(s.Type));
                    Type contractType = Type.GetType(string.Format(s.Contract));

                    log.TraceMessage(string.Format("Loading the service \"{0}\"", s.Type));

                    try
                    {
                        if (serviceType != null && contractType != null)
                        {
                            Uri baseAddress = new Uri(string.Format("http://{0}:{1}/{2}", config.Host, config.Port, s.Name));
                            ServiceHost host = new ServiceHost(serviceType, baseAddress);

                            WSDualHttpBinding binding = new WSDualHttpBinding();
                            binding.Security.Mode = WSDualHttpSecurityMode.Message;
                            binding.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.Basic256;
                            binding.MessageEncoding = WSMessageEncoding.Mtom;

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
    }
}