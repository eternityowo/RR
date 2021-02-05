using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Common
{
    public interface IWebContentService
    {
        Task<Texture2D> LoadTexture(string url, int height, int weight, CancellationToken token);
    }

    public class WebContentService : IWebContentService
    {
        private static readonly Regex IDRegex = new Regex(@"(\/id\/)([0-9]+)", RegexOptions.Compiled);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="height"></param>
        /// <param name="weight"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Texture2D> LoadTexture(string url, int height, int weight, CancellationToken token)
        {
            using (var uwr = UnityWebRequestTexture.GetTexture($"{url}/{height}/{weight}"))
            {
                await uwr.SendWebRequest();
                
                token.ThrowIfCancellationRequested();

                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(uwr.error);
                }
                else
                {
                    var texture = DownloadHandlerTexture.GetContent(uwr);
                    
                    var matches = IDRegex.Matches(uwr.url);
                    texture.name = matches[0].Groups[2].Value;
                    
                    return texture;
                }
            }
            return null;
        }
    }
}