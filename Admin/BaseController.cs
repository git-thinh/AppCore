using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace Admin
{
    public class BaseController<TCache, TApi> : ApiController
    {
        protected readonly oApiCacheClient _api;
        public BaseController() { }
        public BaseController(string project, string name, string version) :base()
        {
            _api = new oApiCacheClient(project, name, version);
        }

        #region [ GET ]

        HttpResponseMessage _getCacheResultQuery(int pageNumber = 0, int pageSize = 0, string condition = "") => _api.getItems<TCache>(new oCacheRequest() { PageSize = pageSize, PageNumber = pageNumber, Conditions = condition }, null);

        [AttrApiInfo("Lấy về các cấu hình của model")]
        public HttpResponseMessage get_setting() => _api.get_setting();

        [AttrApiInfo("Lấy về tất cả đối tượng")]
        public HttpResponseMessage get_all() => _getCacheResultQuery(0, 0);

        [AttrApiInfo("Lấy về top đối tượng đầu tiên")]
        public HttpResponseMessage get_top([FromUri]int top = 10) => _getCacheResultQuery(1, top);

        [AttrApiInfo("Lấy về 1 đối tượng theo ID")]
        public HttpResponseMessage get_item([FromUri]long id) => _api.get_item<TCache>(id);

        [AttrApiInfo("Lấy get_lookup về cac đối tượng theo {id:.., name:..., state:...}")]
        public HttpResponseMessage get_lookup([FromUri]string fieldName = "name") => _api.get_lookup(fieldName);

        [AttrApiInfo("Lấy về 1 đối tượng theo ID")]
        public HttpResponseMessage get_ids([FromUri]string ids = "") => _api.get_items<TCache>(ids);

        [AttrApiInfo("Lấy về pageSize đối tượng của trang pageNumber với điều kiện condition", Description = "[1] /api/get_items | [2] /api/get_items?pageSize=...&pageNumber=...&condition=...")]
        public HttpResponseMessage get_items([FromUri]int pageNumber = 0, [FromUri]int pageSize = 0, [FromUri]string condition = "") => _getCacheResultQuery(pageNumber, pageSize, condition);


        [AttrApiInfo("Lấy về 1 đối tượng test random của lớp Cache")]
        public HttpResponseMessage get_object_random_cache() => _api.createRandomItem<TCache>();

        [AttrApiInfo("Lấy về 1 đối tượng test random của lớp DTO")]
        public HttpResponseMessage get_object_random_dto() => _api.createRandomItem<TApi>();


        [AttrApiInfo("Lấy về 1 đối tượng blank test của lớp Cache")]
        public HttpResponseMessage get_object_blank_cache() => _api.createObjectBlank<TCache>();

        [AttrApiInfo("Lấy về 1 đối tượng blank test của lớp DTO")]
        public HttpResponseMessage get_object_blank_dto() => _api.createObjectBlank<TApi>();

        #endregion

        #region [ POST ]

        HttpResponseMessage _postCacheResult(string storeName, TApi dto) => _api.updateItem<TApi>(storeName, dto);
        HttpResponseMessage _postCacheResultByID(string storeName, long id) => _api.updateItemByID(storeName, id);

        [AttrApiInfo("Thêm mới 1 đối tượng")]
        public HttpResponseMessage post_addNew([FromBody] TApi dto) => _postCacheResult("addNew", dto);

        [AttrApiInfo("Cập nhật, thay đổi dữ liệu 1 đối tượng")]
        public HttpResponseMessage post_updateItem([FromBody] TApi dto) => _postCacheResult("updateItem", dto);

        [AttrApiInfo("Xóa 1 đối tượng")]
        public HttpResponseMessage post_removeItem([FromBody] TApi dto) => _postCacheResult("removeItem", dto);

        [AttrApiInfo("Xóa 1 đối tượng theo ID")]
        public HttpResponseMessage post_removeItemID([FromUri] long id) => _postCacheResultByID("removeItem", id);

        [AttrApiInfo("Cập nhật, thay đổi dữ liệu 1 đối tượng theo tên hành động")]
        public HttpResponseMessage post_updateByAction([FromUri]string storeAction, [FromBody] TApi dto) => _postCacheResult(storeAction, dto);

        [AttrApiInfo("Cập nhật, thay đổi dữ liệu 1 đối tượng theo tên store trong DB")]
        public HttpResponseMessage post_updateByStore([FromUri]string storeName, [FromBody] TApi dto) => _postCacheResult(storeName, dto);

        #endregion

    }
}
