using System;
using System.Collections.Generic;
using System.IO;

namespace WipeoutRewrite.Infrastructure.Assets
{
    /// <summary>
    /// Implementação de arquivo do IAssetLoader.
    /// </summary>
    public class AssetLoader : IAssetLoader
    {
        private string _basePath = "";
        private Dictionary<string, string> _assetCache = new();

        public void Initialize(string basePath)
        {
            _basePath = basePath;
            Console.WriteLine($"AssetLoader initialized with base path: {_basePath}");
            
            if (!Directory.Exists(_basePath))
            {
                Console.WriteLine($"⚠ Warning: Asset path does not exist: {_basePath}");
            }
        }

        /// <summary>
        /// Carregar um ficheiro de texto (configuração, dados)
        /// </summary>
        public string? LoadTextFile(string relativePath)
        {
            try
            {
                string fullPath = Path.Combine(_basePath, relativePath);
                if (!File.Exists(fullPath))
                {
                    Console.WriteLine($"⚠ File not found: {fullPath}");
                    return null;
                }
                return File.ReadAllText(fullPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error loading text file {relativePath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Carregar dados binários (modelos, dados de track)
        /// </summary>
        public byte[]? LoadBinaryFile(string relativePath)
        {
            try
            {
                string fullPath = Path.Combine(_basePath, relativePath);
                if (!File.Exists(fullPath))
                {
                    Console.WriteLine($"⚠ File not found: {fullPath}");
                    return null;
                }
                return File.ReadAllBytes(fullPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error loading binary file {relativePath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Listar ficheiros numa pasta
        /// </summary>
        public List<string> ListFiles(string relativePath, string pattern = "*")
        {
            var files = new List<string>();
            try
            {
                string fullPath = Path.Combine(_basePath, relativePath);
                if (!Directory.Exists(fullPath))
                {
                    Console.WriteLine($"⚠ Directory not found: {fullPath}");
                    return files;
                }

                var fileEntries = Directory.GetFiles(fullPath, pattern);
                foreach (var file in fileEntries)
                {
                    files.Add(Path.GetFileName(file));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error listing files in {relativePath}: {ex.Message}");
            }
            return files;
        }

        /// <summary>
        /// Carregar ficheiros de configuração (config.txt, tracks list, etc.)
        /// </summary>
        public List<string> LoadTrackList()
        {
            var tracks = new List<string>();
            try
            {
                var trackDirs = Directory.GetDirectories(Path.Combine(_basePath, "wipeout"));
                foreach (var dir in trackDirs)
                {
                    string trackName = Path.GetFileName(dir);
                    if (trackName.StartsWith("track"))
                    {
                        tracks.Add(trackName);
                    }
                }
                Console.WriteLine($"✓ Loaded {tracks.Count} tracks");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error loading track list: {ex.Message}");
            }
            return tracks;
        }

        /// <summary>
        /// Carregar imagem PNG para textura
        /// </summary>
        public byte[]? LoadImage(string relativePath)
        {
            return LoadBinaryFile(relativePath);
        }

        /// <summary>
        /// Verificar se um asset existe
        /// </summary>
        public bool AssetExists(string relativePath)
        {
            try
            {
                string fullPath = Path.Combine(_basePath, relativePath);
                return File.Exists(fullPath);
            }
            catch
            {
                return false;
            }
        }
    }
}
