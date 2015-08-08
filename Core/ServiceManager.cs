using Client;
using Common;
using Configuration;
using Interfaces;
using Interfaces.Common;
using Interfaces.DataContracts;
using Interfaces.Services;
using Logger;
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
        
        List<ServiceHost> services;

        public ServiceManager()
        {
            services = new List<ServiceHost>();
        }

        public void Initialize()
        {
            Deinitialize();
            
            LoggerManager.Log.TraceMessage("Initializing services");

            Device currDev = ConfigManager.Configuration.CurrentDevice;
            if (currDev != null)
            {
                foreach (Service s in currDev.Services)
                {
                    Type serviceType = Type.GetType(string.Format(s.Type));
                    Type contractType = Type.GetType(string.Format(s.Contract));

                    LoggerManager.Log.TraceMessage(string.Format("Loading the service \"{0}\"", s.Type));

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
                            LoggerManager.Log.TraceMessage(string.Format("ERROR: cannot load the service \"{0}\". The type cannot be resolved", s.Type));
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerManager.Log.TraceMessage(string.Format("ERROR: cannot load the service \"{0}\"", s.Type));
                        LoggerManager.Log.TraceException(ex);
                    }
                    
                }
                LoggerManager.Log.TraceMessage("Services initialized");
            }
        }

        public void Deinitialize()
        {
            if (services != null && services.Count > 0)
            {
                LoggerManager.Log.TraceMessage("Deinitializing services");
                foreach (ServiceHost h in services)
                {
                    try
                    {
                        h.Close();
                    }
                    catch (Exception ex)
                    {
                        LoggerManager.Log.TraceMessage("ERROR during the service deinitialization");
                        LoggerManager.Log.TraceException(ex);
                    }
                }
                services.Clear();
                LoggerManager.Log.TraceMessage("Services deinitialized");
            }
        }
    }
}
