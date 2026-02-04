using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WipeoutRewrite.Infrastructure.Graphics;

namespace WipeoutRewrite.Tools.UI
{
    public static class TextureLoader
    {
        // Loads all textures from a single CMP file
        public static int[] LoadTexturesFromCmpFile(ITextureManager textureManager, string cmpPath)
        {
            if (!File.Exists(cmpPath))
                return Array.Empty<int>();
            return textureManager.LoadTexturesFromCmp(cmpPath);
        }

        // Loads all textures from all CMP files in a folder
        public static int[] LoadTexturesFromFolder(ITextureManager textureManager, string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return Array.Empty<int>();
            var cmpFiles = Directory.GetFiles(folderPath, "*.cmp", SearchOption.TopDirectoryOnly);
            var allTextures = new List<int>();
            foreach (var cmp in cmpFiles)
            {
                var textures = textureManager.LoadTexturesFromCmp(cmp);
                if (textures != null && textures.Length > 0)
                    allTextures.AddRange(textures);
            }
            return allTextures.ToArray();
        }

        // Loads a single texture from a TIM file
        public static int LoadTextureFromTimFile(ITextureManager textureManager, string timPath)
        {
            if (!File.Exists(timPath))
                return 0;
            return textureManager.LoadTextureFromTim(timPath);
        }

        // Loads all textures from all TIM files in a folder
        public static Dictionary<string, int> LoadTexturesFromTimFolder(ITextureManager textureManager, string folderPath)
        {
            var result = new Dictionary<string, int>();
            
            if (!Directory.Exists(folderPath))
                return result;
                
            var timFiles = Directory.GetFiles(folderPath, "*.tim", SearchOption.TopDirectoryOnly);
            
            foreach (var tim in timFiles)
            {
                var handle = textureManager.LoadTextureFromTim(tim);
                if (handle > 0)
                {
                    result[Path.GetFileName(tim)] = handle;
                }
            }
            
            return result;
        }
    }
}
