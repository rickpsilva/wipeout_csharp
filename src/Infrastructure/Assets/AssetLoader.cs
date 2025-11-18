using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

namespace WipeoutRewrite.Infrastructure.Assets
{
    /// <summary>
    /// Implementação de arquivo do IAssetLoader.
    /// </summary>
    public class AssetLoader : IAssetLoader
    {
        private readonly ILogger<AssetLoader> _logger;
        private string _basePath = "";
        private Dictionary<string, string> _assetCache = new();

        public AssetLoader(ILogger<AssetLoader> logger)
        {
            _logger = logger;
        }

        public void Initialize(string basePath)
        {
            _basePath = basePath;
            _logger.LogInformation("AssetLoader initialized with base path: {BasePath}", _basePath);
            
            if (!Directory.Exists(_basePath))
            {
                _logger.LogWarning("Asset path does not exist: {BasePath}", _basePath);
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
                    _logger.LogWarning("File not found: {FullPath}", fullPath);
                    return null;
                }
                return File.ReadAllText(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading text file {RelativePath}", relativePath);
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
                    _logger.LogWarning("File not found: {FullPath}", fullPath);
                    return null;
                }
                return File.ReadAllBytes(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading binary file {RelativePath}", relativePath);
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
                    _logger.LogWarning("Directory not found: {FullPath}", fullPath);
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
                _logger.LogError(ex, "Error listing files in {RelativePath}", relativePath);
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
                _logger.LogInformation("Loaded {TrackCount} tracks", tracks.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading track list");
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
