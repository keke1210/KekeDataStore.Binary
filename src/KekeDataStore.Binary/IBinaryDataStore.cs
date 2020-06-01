using System;
using System.Collections.Generic;

namespace KekeDataStore.Binary
{
    /// <summary>
    /// File data store contract
    /// </summary>
    /// <typeparam name="TEntity">Value Entity Object in Dictionary</typeparam>
    public interface IBinaryDataStore<TEntity> : IDisposable where TEntity : IBaseEntity
    {
        /// <summary>
        /// returns all items in the store
        /// </summary>
        /// <returns>List of elements</returns>
        IEnumerable<TEntity> GetAll();

        /// <summary>
        /// returns items with the predicate you apply to it 
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>List of elements</returns>
        IEnumerable<TEntity> Get(Predicate<TEntity> predicate);

        /// <summary>
        /// returns single specified item with the predicate you apply to it 
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>The element or null</returns>
        TEntity GetSingle(Predicate<TEntity> predicate);

        /// <summary>
        /// Finds a TEntity by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The item</returns>
        TEntity GetById(string id);

        /// <summary>
        /// Creates a new item into store
        /// </summary>
        /// <param name="entity">the element that needs to be created</param>
        /// <returns>The created element</returns>
        TEntity Create(TEntity entity);

        /// <summary>
        /// Deletes item by the specified id
        /// </summary>
        /// <param name="id">Id of the item we want to delete</param>
        /// <returns>true if item deletes succesfully</returns>
        bool Delete(string id);

        /// <summary>
        ///  Update an Entity
        /// </summary>
        /// <param name="id">Id of the item to be updated.</param>
        /// <param name="entity">Object value we want to update</param>
        /// <returns>the updated item</returns>
        TEntity Update(string id, TEntity entity);

        /// <summary>
        ///  Saves changes
        /// </summary>
        /// <returns>True if data is saved successfully in the store</returns>
        bool SaveChanges();

        /// <summary>
        /// Clears all items in store
        /// </summary>
        /// <returns>True if data is truncated succesfully in the store</returns>
        bool Truncate();

        /// <summary>
        /// Get all items in store as IQueryable
        /// </summary>
        /// <returns>All items as linq IQueryable</returns>
        System.Linq.IQueryable<TEntity> AsQueryable();

        /// <summary>
        /// Number of items in the store
        /// </summary>
        int Count { get; }
    }
}
