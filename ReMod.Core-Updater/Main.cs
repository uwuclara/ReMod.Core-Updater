using System;
using MelonLoader;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;

namespace ReMod.CoreUpdater
{
    public class Main : MelonPlugin
    {
        /* ----------------------------------  Settings ---------------------------------- */
        private const string fileName     =  "ReMod.Core.dll";                                                                        // File name + extension of the mod/plugin
        private const string directory    =  "UserLibs";                                                                              // dir where to save it in vrc folder
        private const string downloadLink =  "https://github.com/Cyril-Xd/ReMod.Core-Quest/releases/latest/download/ReMod.Core.dll";  // Download link
        /* ----------------------------------  Settings ---------------------------------- */
        
        
        private static readonly string filePath =  Path.Combine(Environment.CurrentDirectory, directory, fileName);
            
        public override void OnApplicationStarted()
        {
            MelonCoroutines.Start(download());
        }

        private static IEnumerator download()
        {
            var request = UnityWebRequest.Get(downloadLink);
            yield return request.Send();

            if (!request.isHttpError || !request.isNetworkError)
            {
                var byteArray = request.downloadHandler.data;
                handle(byteArray);
            }
            else
                MelonLogger.Msg("Failed downloading latest " + fileName + " " + request.GetError());
        }

        private static void handle(byte[] bytes)
        {
            if (File.Exists(filePath))
            {
                if (calculateHash(File.ReadAllBytes(filePath)) != calculateHash(bytes))
                    save(bytes);
                else
                    MelonLogger.Msg(fileName + " is up to date!");
            }
            else
                save(bytes);
        }

        private static void save(byte[] bytes)
        {
            var temp = File.Create(filePath); temp.Close(); temp.Dispose();
            File.WriteAllBytes(filePath, bytes);
            MelonLogger.Msg(fileName + " got updated!");
            stopapp();
        }
        
        private static string calculateHash(byte[] byteArray)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = new MemoryStream(byteArray))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        private static void stopapp()
        {
            MelonLogger.Msg(ConsoleColor.Cyan, "DOWNLOADED NEW " + fileName + " UPDATE AND VRCHAT NEEDS TO BE RESTARTED!");
            Application.Quit();
        }
    }
}
