using System;
using Core.Entities;

namespace Core.Interfaces;

public interface IUniteOfWork : IDisposable
{
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
    Task<bool> Complete();
}
