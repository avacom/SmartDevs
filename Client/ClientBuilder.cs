using Common;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class ClientBuilder<T> : IDisposable
    {
        T proxy;
        ILogger log;

        ChannelFactory<T> channelFactory;

        public ClientBuilder(ILogger logger,  string serviceID, object callbackObj, string port, string host)
        {
            Initialize(logger, serviceID, callbackObj, port, host);
        }

        public ClientBuilder(ILogger logger, string serviceID, object callbackObj)
        {
            Initialize(logger, serviceID, callbackObj, Constants.DEFAULT_PORT, Constants.DEFAULT_HOST);
        }

        public ClientBuilder(ILogger logger, string serviceID, string port, string host)
        {
            Initialize(logger, serviceID, null, port, host);
        }

        public ClientBuilder(ILogger logger, string serviceID)
        {
            Initialize(logger, serviceID, null, Constants.DEFAULT_PORT, Constants.DEFAULT_HOST);
        }

        private bool Initialize(ILogger logger, string serviceID, object callbackObj, string port, string host)
        {
            log = logger;
            bool ret = true;
            try
            {
                
                string msg = callbackObj == null ? "Initializing the client channel for the service {0}" : "Initializing the client duplex channel for the service {0}";
                log.TraceMessage(string.Format(msg, serviceID));
                string path = string.Format("http://{0}:{1}/{2}", host, port, serviceID);

                log.TraceMessage(string.Format("Initialization parameters: port = {0}, host = {1}, url = {2}", port, host, path));
                ServiceEndpointCollection endpoints = MetadataResolver.Resolve(typeof(T), new Uri(string.Format("{0}?wsdl", path)), MetadataExchangeClientMode.HttpGet);
                if (endpoints != null && endpoints.Count > 0)
                {
                    ServiceEndpoint endpoint = endpoints[0];

                    log.TraceMessage(string.Format("Creating the channel factory for the service {0}", serviceID));
                    if (callbackObj != null)
                    {
                        channelFactory = new DuplexChannelFactory<T>(
                            callbackObj,
                            endpoint.Binding,
                            new EndpointAddress(path));
                    }
                    else
                    {
                        channelFactory = new ChannelFactory<T>(
                            endpoint.Binding,
                            new EndpointAddress(path));
                    }

                    if (channelFactory != null)
                    {
                        log.TraceMessage(string.Format("Channel factory for the service {0} created", serviceID));
                        log.TraceMessage(string.Format("Generating the proxy for the service {0}", serviceID));
                        proxy = channelFactory.CreateChannel();
                        log.TraceMessage(string.Format("The proxy for the service {0} created", serviceID));
                    }
                    else
                    {
                        log.TraceMessage(string.Format("Failed to create the channel factory for the service {0}!!!", serviceID));
                        ret = false;
                    }
                }
                else
                {
                    log.TraceMessage(string.Format("Service {0} contains no end points", serviceID));
                    ret = false;
                }
            }
            catch (Exception ex)
            {
                log.TraceMessage(string.Format("Unable to initialize the client channel for the service {0}!!!", serviceID));
                log.TraceException(ex);
                ret = false;
            }
            return ret;
        }

        public void Dispose()
        {
            channelFactory.Close();
        }

        public T Proxy
        {
            get
            {
                return proxy;
            }
        }
    }
}
