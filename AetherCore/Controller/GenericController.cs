using Microsoft.AspNetCore.Mvc;
using AetherCore.Service;

namespace AetherCore.Controller
{
    // 泛型 Controller，用來抽象出基本的 CRUD 操作邏輯
    public abstract class GenericController<TRequest, TResponse, TService>
        : ControllerBase where TService : IService<TRequest, TResponse>
    {
        // 注入對應的 Service 實作
        protected readonly TService _service;

        public GenericController(TService service)
        {
            _service = service;
        }

        // C - Create：建立新資料
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TRequest requestDto)
        {
            return Ok(await OnCreate(requestDto));
        }

        // R - Read All：取得所有資料
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await OnGetAll());
        }

        // R - Read by Id：透過主鍵取得單筆資料
        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            return Ok(await OnGet(key));
        }

        // U - Update：更新指定主鍵的資料
        [HttpPut("{key}")]
        public async Task<IActionResult> Update(string key, [FromBody] TRequest requestDto)
        {
            await OnUpdate(key, requestDto);

            return Ok();
        }

        // D - Delete：刪除指定主鍵的資料
        [HttpDelete("{key}")]
        public async Task<IActionResult> Delete(string key)
        {
            await OnDelete(key);

            return Ok();
        }

        // 實際處理建立邏輯，可被覆寫
        protected virtual async Task<TResponse> OnCreate(TRequest request)
        {
            return await _service.CreateAsync(request);
        }

        // 實際處理查詢單筆邏輯，可被覆寫
        protected virtual async Task<TResponse> OnGet(string key)
        {
            return await _service.GetAsync(key);
        }

        // 實際處理查詢全部資料邏輯，可被覆寫
        protected virtual async Task<List<TResponse>> OnGetAll()
        {
            return await _service.GetAllAsync();
        }

        // 實際處理更新邏輯，可被覆寫
        protected virtual async Task OnUpdate(string key, TRequest request)
        {
            await _service.UpdateAsync(key, request);
        }

        // 實際處理刪除邏輯，可被覆寫
        protected virtual async Task OnDelete(string key)
        {
            await _service.DeleteAsync(key);
        }
    }
}
