using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.Caching;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Admin
{
    public class BaseServiceCache<TCache, TApi> : ICacheService
    {
        static CacheSynchronized<TCache, TApi> _store = null;
        readonly string _project_Code;
        readonly long _model_ID;
        readonly string _model_Name;
        readonly string _path;
        readonly IDataflow _dataflow;
        public BaseServiceCache(string projectCode, long modelID, string modelName, string modelPath, IDataflow dataflow)
        {
            this._path = modelPath;
            this._model_ID = modelID;
            this._model_Name = modelName;
            this._dataflow = dataflow;
            this._project_Code = projectCode;
            _store = new CacheSynchronized<TCache, TApi>(projectCode,  modelID,  modelName,  modelPath,  dataflow);
        }

        public IDataflow getDataflow() => _dataflow; 
        public long getModelID() => _model_ID; 
        public string getProjectCode() => _project_Code; 
        public string getModelName() => _model_Name; 
        public string getModelPath() => _path; 
        public bool checkOpen()=> true; 

        public string createObjectBlank() => _store.createObjectBlank();

        public string createRandomItems(int count) => _store.createRandomItems(count);
        public void cacheAll() => _store.cacheAll();
        public string getLookupJson(string fieldName) => _store.getLookupJson(fieldName);
        public string getItemJson(long keyID) => _store.getItemJson(keyID);
        public string getItemsJson(long[] IDs) => _store.getItemsJson(IDs);
        public string getItems(oCacheRequest request, byte[] predicate) => JsonConvert.SerializeObject(_store.getItems(request, predicate));    
        public string updateItem(string storeName, string jsonItem)
        {
            oCachePostResult result = _store.updateItem(storeName, jsonItem);
            return JsonConvert.SerializeObject(result);
        }   
        public string updateItemByID(string storeName, long id)
        {
            oCachePostResult result = _store.updateItemByID(storeName, id);
            return JsonConvert.SerializeObject(result);
        }

        public bool cacheUpdate(DB_ACTION_TYPE db_type, long id, string jsonItemArray) => _store.cacheUpdate(db_type, id, jsonItemArray);
    }

    public class BaseServiceCacheBehavior : IServiceBehavior, IInstanceProvider
    {
        private readonly object _instance;
        public BaseServiceCacheBehavior(object instance)
        {
            _instance = instance;
        }
        public object GetInstance(InstanceContext instanceContext) { return _instance; }
        //public object GetInstance(InstanceContext instanceContext) { return new oUserService(_dataflow, _cacheFields); }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) { }
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }
        public void ReleaseInstance(InstanceContext instanceContext, object instance) { }
        public object GetInstance(InstanceContext instanceContext, Message message) => this.GetInstance(instanceContext);
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher cd in serviceHostBase.ChannelDispatchers)
                foreach (EndpointDispatcher ed in cd.Endpoints)
                    ed.DispatchRuntime.InstanceProvider = this;
        }
    }
}
