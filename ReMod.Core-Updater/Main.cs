using System;
using MelonLoader;
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
        private const string downloadLink =  "https://github.com/Cyril-Xd/ReMod.Core-Quest/releases/latest/download/ReMod.Core.dll";  // Download link
        /* ----------------------------------  Settings ---------------------------------- */
        
        
        private static readonly string modsfilePath =  Path.Combine(Environment.CurrentDirectory, "Mods", fileName);
        private static readonly string userLibsfilePath =  Path.Combine(Environment.CurrentDirectory, "UserLibs", fileName); 
        // UserLibs are loaded before plugins therefore it won't load new one. If u know a way how to unload assembly from UserLibs, tell me.

        public override void OnApplicationEarlyStart()
        {
            if (!File.Exists(userLibsfilePath))
                download();
            else
                removeAndStop();
        }

        private static void download()
        {
            var request = UnityWebRequest.Get(downloadLink);
            request.Send();
            
            do
            {
                if (!request.isDone) continue;
                if (!request.isHttpError || !request.isNetworkError)
                {
                    var byteArray = request.downloadHandler.data;
                    handle(byteArray);
                }
                else
                    MelonLogger.Msg(ConsoleColor.Red, "Failed downloading latest " + fileName + " " + request.GetError());
                break;
            } 
            while (true);
        }

        private static void handle(byte[] bytes)
        {
            if (File.Exists(modsfilePath))
            {
                if (calculateHash(File.ReadAllBytes(modsfilePath)) != calculateHash(bytes))
                    save(bytes);
                else
                    MelonLogger.Msg(ConsoleColor.Green, fileName + " is up to date!");
            }
            else
                save(bytes);
        }

        private static void save(byte[] bytes)
        {
            var temp = File.Create(modsfilePath); temp.Close(); temp.Dispose();
            File.WriteAllBytes(modsfilePath, bytes);
            MelonLogger.Msg(ConsoleColor.Green, fileName + " got updated!");
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

        private static void removeAndStop()
        {
            File.Delete(userLibsfilePath);
            MelonLogger.Msg(ConsoleColor.Yellow, 
                "FOUND " + fileName + " IN USERLIBS AND VRCHAT NEEDS TO BE RESTARTED TO MAKE AUTOUPDATER WORK. PLEASE DON'T PUT " + fileName + " in USERLIBS FOLDER!");
            Application.Quit();
        }
    }
}
