using System;
using System.IO;
using System.Text.Json;
using System.Text.Encodings.Web;
using MemoryPack;

namespace MemoryPackRepacker;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 5)
        {
            Console.WriteLine("Usage: <server: jp|gl> <mode: deserialize|serialize> <type: table|media|bundle> <input> <output>");
            return;
        }

        string server = args[0].ToLower();
        string mode = args[1].ToLower();
        string type = args[2].ToLower();
        string inputPath = args[3];
        string outputPath = args[4];

        Console.WriteLine($"[Config] Server: {server}, Mode: {mode}, Type: {type}");
        Console.WriteLine($"[IO] Input: {inputPath}");
        Console.WriteLine($"[IO] Output: {outputPath}");

        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var context = new AppJsonContext(options);

            if (mode == "deserialize")
            {
                Console.WriteLine(">>> Starting Deserialization (Bin -> JSON)");
                byte[] data = File.ReadAllBytes(inputPath);
                string json = string.Empty;

                if (server == "jp")
                {
                    if (type == "table")
                    {
                        var obj = MemoryPackSerializer.Deserialize<TableCatalog>(data);
                        json = JsonSerializer.Serialize(obj, context.TableCatalog);
                    }
                    else if (type == "media")
                    {
                        var catalog = MemoryPackSerializer.Deserialize<MediaCatalog>(data);
                        if (catalog != null)
                        {
                            foreach (var m in catalog.Table.Values)
                            {
                                m.Path = m.Path.Replace("\\", "/");
                            }
                        }
                        json = JsonSerializer.Serialize(catalog, context.MediaCatalog);
                    }
                    else if (type == "bundle")
                    {
                        var obj = MemoryPackSerializer.Deserialize<BundlePatchPackInfo>(data);
                        json = JsonSerializer.Serialize(obj, context.BundlePatchPackInfo);
                    }
                }
                else if (server == "gl")
                {
                    if (type == "table")
                    {
                        var obj = MemoryPackSerializer.Deserialize<TableCatalogGL>(data);
                        json = JsonSerializer.Serialize(obj, context.TableCatalogGL);
                    }
                    else if (type == "media")
                    {
                        var catalog = MemoryPackSerializer.Deserialize<MediaCatalogGL>(data);
                        if (catalog != null)
                        {
                            foreach (var m in catalog.Table.Values) m.Path = m.Path.Replace("\\", "/");
                            foreach (var m in catalog.Catalog.Values) m.Path = m.Path.Replace("\\", "/");
                        }
                        json = JsonSerializer.Serialize(catalog, context.MediaCatalogGL);
                    }
                }

                if (!string.IsNullOrEmpty(json))
                {
                    File.WriteAllText(outputPath, json);
                    Console.WriteLine("<<< Deserialization Complete.");
                }
            }
            else if (mode == "serialize")
            {
                Console.WriteLine(">>> Starting Serialization (JSON -> Bin)");
                string json = File.ReadAllText(inputPath);
                byte[]? bin = null;

                if (server == "jp")
                {
                    if (type == "table")
                    {
                        var obj = JsonSerializer.Deserialize(json, context.TableCatalog);
                        bin = MemoryPackSerializer.Serialize(obj);
                    }
                    else if (type == "media")
                    {
                        var catalog = JsonSerializer.Deserialize(json, context.MediaCatalog);
                        if (catalog != null)
                        {
                            foreach (var m in catalog.Table.Values)
                            {
                                m.Path = m.Path.Replace("/", "\\");
                            }
                        }
                        bin = MemoryPackSerializer.Serialize(catalog);
                    }
                    else if (type == "bundle")
                    {
                        var obj = JsonSerializer.Deserialize(json, context.BundlePatchPackInfo);
                        bin = MemoryPackSerializer.Serialize(obj);
                    }
                }
                else if (server == "gl")
                {
                    if (type == "table")
                    {
                        var obj = JsonSerializer.Deserialize(json, context.TableCatalogGL);
                        bin = MemoryPackSerializer.Serialize(obj);
                    }
                    else if (type == "media")
                    {
                        var catalog = JsonSerializer.Deserialize(json, context.MediaCatalogGL);
                        if (catalog != null)
                        {
                            foreach (var m in catalog.Table.Values) m.Path = m.Path.Replace("/", "\\");
                            foreach (var m in catalog.Catalog.Values) m.Path = m.Path.Replace("/", "\\");
                        }
                        bin = MemoryPackSerializer.Serialize(catalog);
                    }
                }

                if (bin != null)
                {
                    File.WriteAllBytes(outputPath, bin);
                    Console.WriteLine("<<< Serialization Complete.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[Inner Error] {ex.InnerException.Message}");
            }
        }
    }
}
