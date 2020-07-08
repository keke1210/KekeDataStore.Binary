using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace KekeDataStore.Binary
{
    internal static class BinaryFileUtils
    {
        /// <summary>
        /// Writes content to binary file
        /// </summary>
        /// <typeparam name="T">Values of Dictionary</typeparam>
        /// <param name="obj">Object you want to serialize</param>
        /// <param name="filename">name of file where you want to serialize content</param>
        internal static void WriteToFile<T>(Dictionary<string, T> obj, string filename) where T : IBaseEntity
        {
            if (!typeof(T).IsSerializable)
                throw new KekeDataStoreException($"Type '{typeof(T).Name}' is not marked serializable!");

            Stopwatch sw = null;
            BinaryWriter writer = null;

            while (true)
            {
                try
                {
                    var id = obj.Values.FirstOrDefault()?.Id.ToString();
                    AppendData(filename, id, obj.ToByteArray());
                    break;
                }
                catch (IOException e) when (e.Message.Contains("because it is being used by another process"))
                {
                    // If some other process is using this file, retry operation unless elapsed times is greater than 10sec
                    sw = sw ?? Stopwatch.StartNew();
                    if (sw.ElapsedMilliseconds > 10000)
                        throw;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    writer?.Dispose();
                }
            }
        }


        /// <summary>
        /// Reads content of a binary file. 
        /// </summary>
        /// <typeparam name="T">Dictionary</typeparam>
        /// <param name="filename">name of the file from where you want to deserialize content</param>
        /// <returns>The deserialized object</returns>
        internal static object LoadFile<T>(string filename)
        {
            if (!typeof(T).IsSerializable)
                throw new KekeDataStoreException($"Type '{typeof(T).Name}' is not marked serializable!");

            Stopwatch sw = null;
            Stream stream = null;
            IFormatter formatter = new BinaryFormatter();

            while (true)
            {
                try
                {
                    stream = Stream.Synchronized(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true));
                    return (true, formatter.Deserialize(stream));
                }
                catch (IOException e) when (e.Message.Contains("because it is being used by another process"))
                {
                    // If some other process is using this file, retry operation unless elapsed times is greater than 10sec
                    sw = sw ?? Stopwatch.StartNew();
                    if (sw.ElapsedMilliseconds > 10000)
                        throw;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    stream?.Dispose();
                }
            }
        }

        // Takes the user Id and _data of the user
        private static void AppendData(string filename, string userId, byte[] data)
        {
            using (Stream fileStream = Stream.Synchronized(new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.None)))
            using (var bw = new BinaryWriter(fileStream))
            {
                bw.Write(userId);
                bw.Write(data);
            }
        }


        private static byte[] ToByteArray(this object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
