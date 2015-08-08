using Common;
using Interfaces;
using Interfaces.Common;
using Logger;
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

        ChannelFactory<T> channelFactory;

        public ClientBuilder(string serviceID, object callbackObj, string port, string host)
        {
            Initialize(serviceID, callbackObj, port, host);
        }

        public ClientBuilder(string serviceID, object callbackObj)
        {
            Initialize(serviceID, callbackObj, Constants.DEFAULT_PORT, Constants.DEFAULT_HOST);
        }

        public ClientBuilder(string serviceID, string port, string host)
        {
            Initialize(serviceID, null, port, host);
        }

        public ClientBuilder(string serviceID)
        {
            Initialize(serviceID, null, Constants.DEFAULT_PORT, Constants.DEFAULT_HOST);
        }

        private bool Initialize(string serviceID, object callbackObj, string port, string host)
        {
            bool ret = true;
            try
            {
                
                string msg = callbackObj == null ? "Initializing the client channel for the service {0}" : "Initializing the client duplex channel for the service {0}";
                LoggerManager.Log.TraceMessage(string.Format(msg, serviceID));
                string path = string.Format("http://{0}:{1}/{2}", host, port, serviceID);

                LoggerManager.Log.TraceMessage(string.Format("Initialization parameters: port = {0}, host = {1}, url = {2}", port, host, path));
                ServiceEndpointCollection endpoints = MetadataResolver.Resolve(typeof(T), new Uri(string.Format("{0}?wsdl", path)), MetadataExchangeClientMode.HttpGet);
                if (endpoints != null && endpoints.Count > 0)
                {
                    ServiceEndpoint endpoint = endpoints[0];

                    LoggerManager.Log.TraceMessage(string.Format("Creating the channel factory for the service {0}", serviceID));
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
                        LoggerManager.Log.TraceMessage(string.Format("Channel factory for the service {0} created", serviceID));
                        LoggerManager.Log.TraceMessage(string.Format("Generating the proxy for the service {0}", serviceID));
                        proxy = channelFactory.CreateChannel();
                        LoggerManager.Log.TraceMessage(string.Format("The proxy for the service {0} created", serviceID));
                    }
                    else
                    {
                        LoggerManager.Log.TraceMessage(string.Format("Failed to create the channel factory for the service {0}!!!", serviceID));
                        ret = false;
                    }
                }
                else
                {
                    LoggerManager.Log.TraceMessage(string.Format("Service {0} contains no end points", serviceID));
                    ret = false;
                }
            }
            catch (Exception ex)
            {
                LoggerManager.Log.TraceMessage(string.Format("Unable to initialize the client channel for the service {0}!!!", serviceID));
                LoggerManager.Log.TraceException(ex);
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
