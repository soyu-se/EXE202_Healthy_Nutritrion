﻿using HealthyNutritionApp.Application.DatabaseContext;
using HealthyNutritionApp.Application.Interfaces;
using MongoDB.Driver;

namespace HealthyNutritionApp.Infrastructure.Services
{
    public class UnitOfWork(HealthyNutritionDbContext dbContext) : IUnitOfWork
    {


        private readonly IMongoDatabase _database = dbContext.GetDatabase();
        private bool disposedValue;

        public IMongoCollection<TDocument> GetCollection<TDocument>() where TDocument : class
        {
            return _database.GetCollection<TDocument>(typeof(TDocument).Name);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
