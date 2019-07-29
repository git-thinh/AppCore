using Dapper; 
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Microsoft.ServiceModel.WebSockets;
using Microsoft.Win32;
using Newtonsoft.Json;
using Owin;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Caching;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Unity;

namespace Admin
{
    //public interface IDataflow
    //{
    //    string IP { get; }
    //    int f_user_Authentication(string sessionId);


    //    int cache_Port { set; get; }
    //    void cache_Start();
    //    void cache_Stop();
    //    string[] cache_List();
    //    void cache_Restart();
    //    ICacheService cache_Get(string modelName);
    //    string cache_executeQueries(oCacheRequest[] requests);
    //    Dictionary<string, bool> cacheByID(oCachePostResult result);
    //}

    public class Dataflow : IDataflow
    {
        public const string _NOTIFY_CHANNEL = "5B5AD37D-B512-49A1-AF6F-EA6855B954AD";
        public const string _NOTIFY_EVENT_SESSION = "B2B9337A-277C-4A49-A6E7-20725E58E9AE";
        public const string _NOTIFY_EVENT_MESSAGE_CLIENT = "DD34A871-86E6-47A3-946D-A973EF15ECC5";
        public static readonly IIpcChannelRegistrar _registrar = new IpcChannelRegistrar(Registry.CurrentUser, "581173E2-68FD-4AED-A8B8-204196CC65BA");
        static readonly IpcEventChannel _notify_Host = new IpcEventChannel(_registrar, _NOTIFY_CHANNEL);

        static ConcurrentQueue<string> _notifyMessages = new ConcurrentQueue<string>();
        static readonly ManualResetEvent _notifyQueueLock = new ManualResetEvent(false);

        public const int _ENCRYPT_KEY_NOT_LOGIN = 10000;
        static readonly ConcurrentDictionary<string, int> _clientEncryptKey = new ConcurrentDictionary<string, int>();

        public string IP { get; }
        public Dataflow(string ip = "") { this.IP = ip; }

        public void Stop()
        {
            _notify_Host[_NOTIFY_EVENT_SESSION].OnEvent -= ___notify_eventSession_receiveMessage;
            _notify_Host[_NOTIFY_EVENT_MESSAGE_CLIENT].OnEvent -= ___notify_eventClient_receiveMessage;
            _notify_Host.StopListening();
        }

        public void Start()
        {
            _notify_Host.StartListening(_NOTIFY_CHANNEL);
            _notify_Host[_NOTIFY_EVENT_SESSION].OnEvent += ___notify_eventSession_receiveMessage;
            _notify_Host[_NOTIFY_EVENT_MESSAGE_CLIENT].OnEvent += ___notify_eventClient_receiveMessage;

            var server = WebApp.Start("http://*:80", (app) =>
            {
                //use cors on server level
                app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

                HttpConfiguration config = new HttpConfiguration();
                //--------------------------------------------------------------
                var container = new UnityContainer();
                //container.RegisterType<IProductRepository, ProductRepository>(new HierarchicalLifetimeManager());
                container.RegisterInstance<IDataflow>(this);
                config.DependencyResolver = new UnityResolver(container);

                //--------------------------------------------------------------

                config.Filters.Add(new BasicAuthenticationAttribute(this));

                //--------------------------------------------------------------
                // Web API configuration and services
                var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
                config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);
                config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

                //app.Use(typeof(LoginOwinMiddleware), this);
                //app.Use(typeof(CacheOwinMiddleware), this);

                //--------------------------------------------------------------
                // Routing staic
                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{action}",
                    defaults: new
                    {
                        action = RouteParameter.Optional
                    }
                );
                app.UseWebApi(config);

                //--------------------------------------------------------------
                // Webserver static at folder
                //var physicalFileSystem = new PhysicalFileSystem(@"./");
                //var physicalFileSystem = new PhysicalFileSystem(@"../MessageUI");
                var physicalFileSystem = new PhysicalFileSystem(@"../wwwRoot/WebUI");
                var options = new FileServerOptions
                {
                    EnableDirectoryBrowsing = true,
                    EnableDefaultFiles = true,
                    FileSystem = physicalFileSystem
                };
                options.StaticFileOptions.FileSystem = physicalFileSystem;
                options.StaticFileOptions.ServeUnknownFileTypes = true;
                options.DefaultFilesOptions.DefaultFileNames = new[] { "index.html" };

                app.UseFileServer(options);
            });

            //Thread t = new Thread(new ParameterizedThreadStart(___notify_onProcessPushMessage));
            //t.Start(this);
        }

        void ___notify_eventClient_receiveMessage(object sender, IpcSignalEventArgs e)
        {
            if (e.Arguments.Length > 0)
                _notifyMessages.Enqueue(e.Arguments[0]);
        }

        void ___notify_eventSession_receiveMessage(object sender, IpcSignalEventArgs e)
        {
            if (e.Arguments.Length > 0)
            {
                string sessionId;
                switch (e.Arguments[0])
                {
                    case "[SESSION_KEY]":
                        if (e.Arguments.Length > 2)
                        {
                            sessionId = e.Arguments[1];
                            int key = _ENCRYPT_KEY_NOT_LOGIN;
                            if (int.TryParse(e.Arguments[2], out key))
                            {
                                if (_clientEncryptKey.ContainsKey(sessionId) == false)
                                    _clientEncryptKey.TryAdd(sessionId, key);
                                else
                                    _clientEncryptKey[sessionId] = key;
                            }
                            else
                            {
                                //error here
                            }
                        }
                        break;
                    case "[SESSION_OPEN]":
                        if (e.Arguments.Length > 1)
                        {
                            sessionId = e.Arguments[1];
                            if (_clientEncryptKey.ContainsKey(sessionId) == false)
                                _clientEncryptKey.TryAdd(sessionId, _ENCRYPT_KEY_NOT_LOGIN);
                        }
                        break;
                    case "[SESSION_CLOSE]":
                        if (e.Arguments.Length > 1)
                        {
                            sessionId = e.Arguments[1];
                            if (_clientEncryptKey.ContainsKey(sessionId))
                                _clientEncryptKey.TryRemove(sessionId, out int enid);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public int f_user_Authentication(string sessionId)
        {
            if (_clientEncryptKey.ContainsKey(sessionId)) return _clientEncryptKey[sessionId];
            return 0;
        }


        #region [ CACHE ]

        static readonly string _path_Startup = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        static readonly string _path_wwwRoot = Path.Combine(Path.GetDirectoryName(_path_Startup), "wwwRoot");

        //static readonly string _pathModel_Running = @"C:\Projects\F88.Mobility\ModelBuild";
        static readonly string _pathModel_Running = Path.Combine(_path_wwwRoot, "_Model_Running");
        static readonly string _pathModel_Release = Path.Combine(_path_wwwRoot, "_Model_Release");
        static readonly string _pathModel_Backup = Path.Combine(_path_wwwRoot, "_Model_Backup");

        //static readonly string _path_AdminUI = Path.Combine(_path_wwwRoot, "WebUI");

        static ConcurrentDictionary<string, ServiceHost> _storeCache = new ConcurrentDictionary<string, ServiceHost>() { };
        static ConcurrentDictionary<string, ICacheService> _storeClient = new ConcurrentDictionary<string, ICacheService>() { };
        static ConcurrentDictionary<string, ChannelFactory<ICacheService>> _storeFactories = new ConcurrentDictionary<string, ChannelFactory<ICacheService>>() { };

        const int _count_for_random = 9;
        const string _prefix_model = "m_";
        public int cache_Port { set; get; }

        public void cache_Start()
        {
            if (!Directory.Exists(_path_wwwRoot)) Directory.CreateDirectory(_path_wwwRoot);
            if (!Directory.Exists(_pathModel_Running)) Directory.CreateDirectory(_pathModel_Running);
            if (!Directory.Exists(_pathModel_Release)) Directory.CreateDirectory(_pathModel_Release);
            if (!Directory.Exists(_pathModel_Backup)) Directory.CreateDirectory(_pathModel_Backup);
            //----------------------------------------------------------------------------------------------
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            cache_Port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            //----------------------------------------------------------------------------------------------

            string[] fsDLL_Run = Directory.GetFiles(_pathModel_Running, "*.dll")
                .Select(x => Path.GetFileName(x))
                .Select(x => x.Substring(0, x.Length - 4).ToLower())
                .ToArray();

            string[] a;
            string modelName = "", serviceName = "", servicePath, url;
            long model_ID = 0;

            for (int i = 0; i < fsDLL_Run.Length; i++)
            {
                a = fsDLL_Run[i].Split('.');
                if (a.Length < 3 || fsDLL_Run[i].StartsWith(_prefix_model) == false || fsDLL_Run[i].Contains(".") == false) continue;

                modelName = a[a.Length - 2];
                serviceName = string.Join("_", a);

                if (long.TryParse(a[a.Length - 1], out model_ID) && model_ID > 0)
                {
                    servicePath = modelName + "/" + model_ID;
                    if (_storeCache.ContainsKey(servicePath)) continue;

                    string fileDll = Path.Combine(_pathModel_Running, fsDLL_Run[i] + ".dll");
                    var assembly = Assembly.Load(File.ReadAllBytes(fileDll));
                    //var assembly = Assembly.LoadFile(fileDll);
                    var types = assembly.GetTypes();

                    var typeModel = types.FirstOrDefault(x => x.Name == serviceName);
                    var typeService = types.FirstOrDefault(x => x.Name == serviceName + "_Service");
                    var typeBehavior = types.FirstOrDefault(x => x.Name == serviceName + "_Behavior");

                    object instanceService = Activator.CreateInstance(typeService, _pathModel_Running, this);
                    IServiceBehavior instanceBehavior = Activator.CreateInstance(typeBehavior, instanceService) as IServiceBehavior;

                    url = "http://127.0.0.1:" + cache_Port + "/" + servicePath;
                    ServiceHost host = new ServiceHost(typeService, new Uri(url));
                    host.AddServiceEndpoint(typeof(ICacheService), new BasicHttpBinding(), "");
                    host.Description.Behaviors.Add(instanceBehavior);
                    host.Open();
                    _storeCache.TryAdd(servicePath, host);

                    ChannelFactory<ICacheService> factory = new ChannelFactory<ICacheService>(new BasicHttpBinding(), new EndpointAddress(url));
                    ICacheService proxy = factory.CreateChannel();
                    //proxy.createRandomItems(_count_for_random);
                    _storeClient.TryAdd(servicePath, proxy);
                    _storeFactories.TryAdd(servicePath, factory);
                }
            }
        }

        public void cache_Stop()
        {
            ServiceHost[] hosts = _storeCache.Values.ToArray();
            foreach (var host in hosts) host.Close();

            ChannelFactory<ICacheService>[] factories = _storeFactories.Values.ToArray();
            foreach (var factory in factories) factory.Close();

            _storeCache.Clear();
            _storeClient.Clear();
            _storeFactories.Clear();
        }

        public string[] cache_List()
        {
            string[] a = _storeCache.Keys.ToArray();
            a = a.OrderBy(x => x).ToArray();
            return a;
        }

        public void cache_Restart()
        {
            cache_Stop();
            cache_Start();
        }

        public ICacheService cache_Get(string modelName)
        {
            string url = _storeFactories.Keys.Where(x => x.StartsWith(modelName + "/")).SingleOrDefault();
            if (!string.IsNullOrEmpty(url))
            {
                url = "http://127.0.0.1:" + cache_Port + "/" + url;
                if (!string.IsNullOrEmpty(url))
                {
                    ChannelFactory<ICacheService> factory = new ChannelFactory<ICacheService>(new BasicHttpBinding(), new EndpointAddress(url));
                    var _api = factory.CreateChannel();
                    return _api;
                }
            }
            return null;
        }

        public string cache_executeQueries(oCacheRequest[] requests)
        {
            string json = "";

            ICacheService api = null;

            for (int i = 0; i < requests.Length; i++)
            {
                if (requests[i].PageNumber == 0) requests[i].PageNumber = 1;
                if (requests[i].PageSize == 0) requests[i].PageSize = 10;

                api = this.cache_Get(requests[i].ServiceName);
                if (api != null)
                {
                    json = api.getItems(requests[i], null);
                    break;
                }
            }

            return json;
        }

        //public Dictionary<string, bool> cacheByID<T, Dto>(oCachePostResult result, ICacheSynchronized<T, Dto> self)
        public Dictionary<string, bool> cacheByID(oCachePostResult result)
        {
            Dictionary<string, bool> dic = new Dictionary<string, bool>() { };

            if (result.Ok)
            {
                if (!string.IsNullOrEmpty(result.ServiceArray)
                    && !string.IsNullOrEmpty(result.KeyArray))
                {
                    try
                    {
                        string[] aService = result.ServiceArray.Split(',');
                        long[] aKey = result.KeyArray.Split(',').Select(x => long.Parse(x)).ToArray();
                        string[] paths = _storeCache.Keys.ToArray();

                        oCacheReload[] ar = new oCacheReload[aKey.Length];
                        for (int i = 0; i < aKey.Length; i++) ar[i] = new oCacheReload()
                        {
                            ModelName = aService[i],
                            Id = aKey[i],
                            CacheKey = paths.Where(x => x.StartsWith(aService[i] + "/")).SingleOrDefault()
                        };

                        // Must be check store update output wrong ServiceName
                        ar = ar.Where(x => !string.IsNullOrEmpty(x.CacheKey)).ToArray();

                        DynamicParameters param = null;
                        string storeName;
                        ICacheService cache = null;
                        string ProjectCode = result.ProjectCode;
                        if (string.IsNullOrEmpty(ProjectCode)) ProjectCode = "f88m";

                        using (IDbConnection cnn = new SqlConnection(_DB_CONST.get_connectString_Mobility()))
                        {
                            cnn.Open();
                            for (int i = 0; i < ar.Length; i++)
                            {
                                if (_storeClient.ContainsKey(ar[i].CacheKey))
                                {
                                    try
                                    {
                                        cache = _storeClient[ar[i].CacheKey];
                                        param = new DynamicParameters();
                                        param.Add("@id", ar[i].Id);
                                        storeName = string.Format("{0}_{1}_{2}", ProjectCode, ar[i].ModelName, "cacheByID");

                                        dynamic obj = cnn.Query<dynamic>(storeName, param, commandType: CommandType.StoredProcedure);
                                        if (obj != null)
                                        {
                                            string jsonItem = JsonConvert.SerializeObject(obj);

                                            DB_ACTION_TYPE type = DB_ACTION_TYPE.DB_INSERT;
                                            switch (result.ActionType)
                                            {
                                                case "DB_INSERT":
                                                    type = DB_ACTION_TYPE.DB_INSERT;
                                                    break;
                                                case "DB_UPDATE":
                                                    type = DB_ACTION_TYPE.DB_UPDATE;
                                                    break;
                                                case "DB_REMOVE":
                                                    type = DB_ACTION_TYPE.DB_REMOVE;
                                                    break;
                                            }

                                            if (!string.IsNullOrEmpty(jsonItem))
                                            {
                                                bool ok = cache.cacheUpdate(type, ar[i].Id, jsonItem);
                                                dic.Add(ar[i].ModelName, ok);
                                            }

                                        }
                                    }
                                    catch (Exception exx)
                                    {
                                        dic.Add(ar[i].ModelName, false);
                                    }
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            return dic;
        }

        public class oCacheReload
        {
            public long Id { set; get; }
            public string ModelName { set; get; }
            public string CacheKey { set; get; }
        }

        #endregion
    }


}
