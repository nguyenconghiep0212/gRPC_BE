using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace IotGrpcLearning.Infrastructure
{
	public static class JsonFileLoader
	{
		public static T[] LoadFromJson<T>(string relativePath, string? basePath = null)
		{
			try
			{
				var baseDir = basePath ?? AppContext.BaseDirectory;
				var path = Path.Combine(baseDir, relativePath);

				if (!File.Exists(path))
				{
					Console.WriteLine($"File not found: {path}");
					throw new Exception($"{relativePath} does not exist");
				}

				var json = File.ReadAllText(path);
				var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
				var arr = JsonSerializer.Deserialize<T[]>(json, opts) ?? Array.Empty<T>();
				if (arr.Length  == 0)
				{
					throw new Exception($"JSON file at {relativePath} is empty or does not contain an array of {typeof(T).Name}");
				}
				return arr;
			}
			catch(Exception ex)
			{
				Console.WriteLine($"Error loading JSON from {relativePath}: {ex.Message}");
				return Array.Empty<T>();
			}
		}

		public static async Task<T[]> LoadFromJsonAsync<T>(string relativePath, string? basePath = null, CancellationToken ct = default)
		{
			try
			{
				var baseDir = basePath ?? AppContext.BaseDirectory;
				var path = Path.Combine(baseDir, relativePath);

				if (!File.Exists(path))
				{
					Console.WriteLine($"File not found: {path}");
					throw new Exception($"{relativePath} does not exist");
				}

				await using var fs = File.OpenRead(path);
				var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
				var arr = JsonSerializer.Deserialize<T[]>(fs, opts) ?? Array.Empty<T>();
				if (arr.Length == 0)
				{
					throw new Exception($"JSON file at {relativePath} is empty or does not contain an array of {typeof(T).Name}");
				}
				return arr;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error loading JSON from {relativePath}: {ex.Message}");
				return Array.Empty<T>();
			}
		}
	}
}