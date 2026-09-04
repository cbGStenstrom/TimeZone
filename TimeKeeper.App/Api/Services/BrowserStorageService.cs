using System.Text.Json;
using Microsoft.JSInterop;

namespace TimeKeeper.App.Api.Services
{
    public class BrowserStorageService
    {
        #region properties

        public IJSRuntime JsRuntime { get; }

        #endregion properties

        #region ctor

        public BrowserStorageService(IJSRuntime jsRuntime)
        {
            JsRuntime = jsRuntime;
        }

        #endregion ctor

        #region public

        /// <summary>
        /// Returns an instance of an object stored in the browser local storage.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T?> ReadFromStorage<T>(string key)
        {
            T? result = default(T);

            var data = await this.JsRuntime.InvokeAsync<string>("window.localStorage.getItem", key);
            if(!String.IsNullOrEmpty(data))
            {
                result = JsonSerializer.Deserialize<T>(data);
            }

            return result;
        }

        /// <summary>
        /// Writes a serialized version of the <paramref name="arg"/> to the browser local storage
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public async Task WriteToStorage<T>(string key, T arg)
        {
            await this.JsRuntime.InvokeVoidAsync("window.localStorage.setItem", key, JsonSerializer.Serialize<T>(arg));
        }

        #endregion public
    }
}
