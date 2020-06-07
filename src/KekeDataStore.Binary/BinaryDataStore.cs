using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace KekeDataStore.Binary
{
    /// <summary>
    /// Thread safe Data Store that saves data into binary files.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class BinaryDataStore<T> : IBinaryDataStore<T> where T : IBaseEntity
    {
        private readonly string _dataFilePath;
        private readonly Lazy<Dictionary<string, T>> _data;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Creates a file at the at specified directory with the specified filename
        /// </summary>
        /// <param name="filepath">Directory of the folder</param>
        /// <param name="dbName">The name of the binary file</param>
        public BinaryDataStore(string dirPath = "", string dbName = "")
        {
            _dataFilePath = SetFilePath(dirPath, dbName);

            _data = new Lazy<Dictionary<string, T>>(() => ReadFromBinaryFile(_dataFilePath), true);
        }

        public int Count
        {
            get => ReadLocked(() => _data.Value.Count);
        }

        public IQueryable<T> AsQueryable()
        {
            IQueryable<T> ReadFunc() => _data.Value.Select(x => x.Value).AsQueryable();
            
            var query = ReadLocked(ReadFunc);
            
            return query;
        }

        public IEnumerable<T> GetAll() 
        {
            IEnumerable<T> ReadFunc() => _data.Value.Select(x => x.Value);

            var elements = ReadLocked(ReadFunc);

            return elements;
        }

        //public IEnumerable<T> GetAll() => ReadLocked(() => _data.Value.Select(x => x.Value));

        public IEnumerable<T> Get(Predicate<T> predicate) 
        {
            IEnumerable<T> ReadFunc() => _data.Value.Where(t => predicate(t.Value)).Select(x => x.Value);
            
            var elements = ReadLocked(ReadFunc);

            return elements;
        } 

        public T GetSingle(Predicate<T> predicate)
        {
            T ReadFunc()  => _data.Value.Where(t => predicate(t.Value)).Select(x => x.Value).SingleOrDefault();

            var item = ReadLocked(ReadFunc);

            return item;
        }

        public T Create(T entity)
        {
            void ReadAction()
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));

                if (_data.Value.ContainsKey(entity.Id.ToString()))
                    throw new KekeDataStoreException($"Id: {entity.Id.ToString()}, already exists on the collection!");
            }

            T WriteFunc()
            {
                if (entity.Id.ToString().IsEmptyGuid())
                    entity.Id = Guid.NewGuid();

                // Sets entity id as key and object as value
                _data.Value.Add(entity.Id.ToString(), entity);

                return entity;
            }

            var createdItem = UpgradeableReadLocked<T>(ReadAction, WriteFunc);

            return createdItem;
        }

        public bool Delete(string id)
        {
            void ReadAction()
            {
                if (id.IsEmptyGuid()) throw new ArgumentNullException(nameof(id));

                T element;
                var elementExists = _data.Value.TryGetValue(id, out element);

                if (!elementExists) throw new KekeDataStoreException("Element doesn't exists on the collection!");
            }
                    
            bool WriteFunc() => _data.Value.Remove(id);
            
            var deleted = UpgradeableReadLocked<bool>(ReadAction, WriteFunc);

            return deleted;
        }

        public T GetById(string id)
        {
            T ReadFunc()
            {
                if (id.IsEmptyGuid()) throw new ArgumentNullException(nameof(id));

                T element;
                _data.Value.TryGetValue(id, out element);
                
                return element;
            }

            var item = ReadLocked(ReadFunc);

            return item;
        }

        public T Update(string id, T entity)
        {
            void ReadAction()
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));
                
                T item;
                var userExists = _data.Value.TryGetValue(id, out item);
                
                if (!userExists) 
                    throw new KekeDataStoreException($"Object of type '{typeof(T).Name}' with id: '{id}', doesn't exists on collection!");
            }

            T WriteFunc()
            {
                entity.Id = new Guid(id);
                _data.Value[id] = entity;
                return entity;
            }

            var updatedItem = UpgradeableReadLocked(ReadAction, WriteFunc);

            return updatedItem;
        }

        public bool Truncate()
        {
            bool WriteFunc() 
            {
                _data.Value.Clear();
                return true;
            } 

            var truncated = WriteLocked<bool>(WriteFunc);

            return truncated;
        }

        public bool SaveChanges()
        {
            bool WriteFunc()
            {
                WriteToBinaryFile(_data.Value, _dataFilePath);
                return true;
            }

            var changesSaved = (bool?)WriteLocked(WriteFunc) ?? false;

            return changesSaved;
        }

        /// <summary>
        /// Serializes the _data collectuin to the binary file
        /// </summary>
        private static void WriteToBinaryFile(Dictionary<string, T> data, string dbPath)
        {
            BinaryFileUtils.WriteToFile(data, dbPath);
        }

        /// <summary>
        /// Reads dictionary object from the binary file. If file doesn't exists it creates a new empty dictionary
        /// </summary>
        /// <param name="dbFile"></param>
        /// <returns></returns>
        private static Dictionary<string, T> ReadFromBinaryFile(string dbFile)
        {
            if (File.Exists(dbFile))
            {
                var result = BinaryFileUtils.LoadFile<T>(dbFile);
                return result;
            }
            else
            {
                var emptyData = new Dictionary<string, T>();
                BinaryFileUtils.WriteToFile(emptyData, dbFile);
                return emptyData;
            }
        }

        /// <summary>
        /// Sets the path of the binary file, if it doesn't exists it creates a new one
        /// </summary>
        /// <param name="directory">directory folder of the file</param>
        /// <param name="fileName">file name without type extension</param>
        /// <returns></returns>
        private static string SetFilePath(string directory, string fileName) 
        {
            if (!Directory.Exists(directory))
            {
                var defaultDirectory = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "Datafiles");

                if (!Directory.Exists(defaultDirectory))
                    Directory.CreateDirectory(defaultDirectory);

                directory = defaultDirectory;
            }

            if (!fileName.IsValidFilename())
            {
                // Name of the T class in plural
                var defaultFileName = $"{typeof(T).Name}s";
                fileName = defaultFileName;
            }

            return Path.Combine(directory, $"{fileName}.bin");
        }

        /// <summary>
        /// Wrapper for Enter/ExitReadLock
        /// </summary>
        /// <typeparam name="TResp">Type of object you want to return</typeparam>
        /// <param name="func">callback function which will be invoked inside ReaderLockSlim</param>
        /// <returns>Response object we read.</returns>
        private TResult ReadLocked<TResult>(Func<TResult> ReadFunc)
        {
            _lock.EnterReadLock();
            try
            {
                return ReadFunc.Invoke();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Wrapper for Enter/ExitUpgradeableReadLock. readAction is invoked first then the writeFunc.
        /// </summary>
        /// <typeparam name="TResp">Type of object you want to return after writting.</typeparam>
        /// <param name="readAction">void callback function for read</param>
        /// <param name="writeFunc">callback function that writes and returns value</param>
        /// <returns>Response object we write.</returns>
        private TResult UpgradeableReadLocked<TResult>(Action ReadAction, Func<TResult> WriteFunc)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                ReadAction.Invoke();

                _lock.EnterWriteLock();
                try
                {
                    return WriteFunc.Invoke();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Wrapper for Enter/ExitWriteLock
        /// </summary>
        /// <typeparam name="TResp">Type of object you want to return after writting.</typeparam>
        /// <param name="func">callback function that writes and returns value</param>
        /// <returns>Response object we write.</returns>
        private TResult WriteLocked<TResult>(Func<TResult> WriteFunc)
        {
            _lock.EnterWriteLock();
            try
            {
                return WriteFunc.Invoke();
            }
            catch(Exception)
            {
                throw;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            if (_lock != null) _lock.Dispose();
        }

        ~BinaryDataStore()
        {
            if (_lock != null) _lock.Dispose();
        }
    }
}