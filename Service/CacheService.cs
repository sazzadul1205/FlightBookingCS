using FlightBookingCS.Service.Cache;
using FlightBookingCS.Service.Interface;
using FlightBookingCS.ViewModel;
using FlightBookingCS.ViewModel.ApiModels;
using System.Collections;
using System.Text.Json;

namespace FlightBookingCS.Service
{
    public class CacheService : ICacheService
    {
        private readonly string _cacheDirectory;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(15);
        private readonly ILogger<CacheService> _logger;
        private readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

        public CacheService(ILogger<CacheService> logger)
        {
            _logger = logger;

            // Get cache directory
            _cacheDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Cache", "FlightSearch");

            // Create cache directory
            Directory.CreateDirectory(_cacheDirectory);

            _logger.LogInformation("Flight search cache directory: {CacheDirectory}", _cacheDirectory);

            // Start cleanup task
            Task.Run(async () => await CleanupAsync());

        }


        // Get cache data
        public async Task<CachedFlightData?> GetAsync(string igxKey)
        {
            // check if igxKey is null
            if (string.IsNullOrEmpty(igxKey))
            {
                _logger.LogWarning("Cannot get cache with null or empty IGXKey");
                return null;
            }

            // acquire lock
            await _fileLock.WaitAsync();

            try
            {
                // Get cache file path
                var filePath = GetCacheFilePath(igxKey);

                // check if file exists
                if (!File.Exists(filePath))
                {
                    _logger.LogInformation("Cache miss for IGXKey: {IGXKey}", igxKey);
                    return null;
                }

                // Read file
                var count = await File.ReadAllTextAsync(filePath);

                // Deserialize
                var cacheData = JsonSerializer.Deserialize<CachedFlightData>(count);

                // Check if deserialization was successful
                if (cacheData == null)
                {
                    _logger.LogWarning("Failed to deserialize cache for IGXKey: {IGXKey}", igxKey);
                    return null;
                }

                // Check if cache is expired
                if (cacheData.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogInformation("Cache expired for IGXKey: {IGXKey}, Expired at: {ExpiresAt}", igxKey, cacheData.ExpiresAt);

                    File.Delete(filePath);
                    return null;
                }

                _logger.LogInformation("Cache hit for IGXKey: {IGXKey}, File: {FilePath}", igxKey, filePath);
                return cacheData;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading cache for IGXKey: {IGXKey}", igxKey);
                return null;
            }
            finally
            {
                // release lock
                _fileLock.Release();
            }
        }

        // Store cache
        public async Task StoreAsyc(string igxKey, FlightSearchApiResponse response, FlightSearchRequest request)
        {
            // Check if parameters are valid
            if (string.IsNullOrEmpty(igxKey) || response == null || !response.Success)
            {
                _logger.LogWarning("Cannot store cache: Invalid parameters. IGXKey: {IGXKey}, Success: {Success}", igxKey, response?.Success);
                return;
            }

            // Acquire lock
            await _fileLock.WaitAsync();

            try
            {
                // Create cache data
                var cacheData = new CachedFlightData
                {
                    IGXKey = igxKey,
                    ApiResponse = response,
                    Request = request,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.Add(_cacheDuration),
                    Id = Guid.NewGuid().ToString()
                };

                // Serialize
                var json = JsonSerializer.Serialize(cacheData);

                // Get cache file path
                var filePath = GetCacheFilePath(igxKey);

                // Write
                await File.WriteAllTextAsync(filePath, json);

                // Get file info
                var fileInfo = new FileInfo(filePath);
                cacheData.FilePath = filePath;
                cacheData.FileSize = fileInfo.Length;

                _logger.LogInformation("Cache stored for IGXKey: {IGXKey}, File: {FilePath}, Size: {FileSize} bytes, Expires: {ExpiresAt}", igxKey, filePath, fileInfo.Length, cacheData.ExpiresAt);

                // Update metadata file
                await UpdateCacheMetadataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing cache for IGXKey: {IGXKey}", igxKey);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        // Delete cache
        public async Task<bool> DeleteAsync(string igxKey)
        {
            // Check if parameters are valid
            if (string.IsNullOrEmpty(igxKey)) return false;

            // Acquire lock
            await _fileLock.WaitAsync();

            try
            {
                // Get cache file path
                var filePath = GetCacheFilePath(igxKey);

                // Check if file exists
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("Cannot delete cache - file not found: {FilePath}", filePath);
                    return false;
                }

                // Delete File
                File.Delete(filePath);

                _logger.LogInformation("Cache deleted for IGXKey: {IGXKey}, File: {FilePath}", igxKey, filePath);

                // Update metadata
                await GetCacheMetadataAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing cache for IGXKey: {IGXKey}", igxKey);
                return false;
            }
            finally
            {
                _fileLock.Release();
            }

        }

        // Cleanup cache
        public async Task CleanupAsync()
        {
            // Acquire lock
            await _fileLock.WaitAsync();

            try
            {
                _logger.LogInformation("Starting cache cleanup...");

                // Get files
                var files = Directory.GetFiles(_cacheDirectory, "*.json");

                // Initialize counters
                int deleteCount = 0;
                long freeBytes = 0;

                // Process files
                foreach (var file in files)
                {
                    try
                    {
                        // If metadata file, skip
                        if (Path.GetFileName(file) == "metadata.json") continue;

                        // Read file
                        var content = await File.ReadAllTextAsync(file);

                        // Deserialize
                        var cacheData = JsonSerializer.Deserialize<CachedFlightData>(content);

                        // Check if deserialization was successful & file is expired
                        if (cacheData != null && cacheData.ExpiresAt < DateTime.UtcNow)
                        {
                            // Get file info
                            var fileInfo = new FileInfo(file);

                            // Add to free bytes
                            freeBytes += fileInfo.Length;

                            // Delete
                            File.Delete(file);

                            deleteCount++;

                            _logger.LogDebug("Deleted expired cache: {File}, Created: {CreatedAt}, Expired: {ExpiresAt}", file, cacheData.CreatedAt, cacheData.ExpiresAt);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing cache file during cleanup: {File}", file);
                    }
                }

                // Update metadata after cleanup
                await UpdateCacheMetadataAsync();

                _logger.LogInformation("Cache cleanup completed. Deleted {DeletedCount} expired files, Freed {FreedKB} KB",
                    deleteCount, freeBytes / 1024);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        // Get cache count
        public async Task<int> GetCacheCountAsync()
        {
            try
            {
                // Get files in cache directory
                var files = Directory.GetFiles(_cacheDirectory, "*.json");

                // Get count
                var count = files.Where(f => Path.GetFileName(f) != "metadata.json").Count();

                // Return
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache count");
                return 0;
            }
        }

        public async Task<IEnumerable<CacheMetadata>> GetCacheMetadataAsync()
        {
            try
            {
                // Get metadata File path
                var metadataFile = Path.Combine(_cacheDirectory, "metadata.json");

                // Check if file exists Return empty
                if (!File.Exists(metadataFile)) return Enumerable.Empty<CacheMetadata>();

                // Read file
                var json = await File.ReadAllTextAsync(metadataFile);

                // Deserialize
                var metadata = JsonSerializer.Deserialize<List<CacheMetadata>>(json);

                // Return metadata
                return metadata ?? Enumerable.Empty<CacheMetadata>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache metadata");
                return Enumerable.Empty<CacheMetadata>();
            }
        }


        // Get cache file path
        private string GetCacheFilePath(string igxKey)
        {
            // replace invalid characters
            var safeKey = string.Join("_", igxKey.Split(Path.GetInvalidFileNameChars()));

            // return file path
            return Path.Combine(_cacheDirectory, $"{safeKey}.json");
        }

        private async Task UpdateCacheMetadataAsync()
        {
            try
            {
                // Get files in cache directory
                var files = Directory.GetFiles(_cacheDirectory, "*.json");

                // Create metadata
                var metadataList = new List<CacheMetadata>();

                // Process files
                foreach (var file in files)
                {
                    // If metadata file, skip
                    if (Path.GetFileName(file) == "metadata.json") continue;

                    // try to deserialize
                    try
                    {
                        // Read file
                        var content = await File.ReadAllTextAsync(file);

                        // Deserialize
                        var cacheData = JsonSerializer.Deserialize<CachedFlightData>(content);

                        // Check if deserialization was successful
                        if (cacheData != null)
                        {
                            // Get file info
                            var fileInfo = new FileInfo(file);

                            // Add to metadata
                            metadataList.Add(new CacheMetadata
                            {
                                IGXKey = cacheData.IGXKey,
                                CreatedAt = cacheData.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                                ExpiresAt = cacheData.ExpiresAt,
                                FileSize = fileInfo.Length,
                                FilePath = file
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing file for metadata: {File}", file);
                    }
                }

                // Update metadata
                var metadataFile = Path.Combine(_cacheDirectory, "metadata.json");

                // Serialize metadata and Sort
                var json = JsonSerializer.Serialize(metadataList.OrderBy(x => x.CreatedAt));

                // Write metadata
                await File.WriteAllTextAsync(metadataFile, json);


                _logger.LogDebug("Updated cache metadata with {Count} items", metadataList.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cache metadata");
            }

        }
    }
}
