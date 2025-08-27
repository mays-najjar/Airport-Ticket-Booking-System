using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AirportBooking.Repositories
{
    public class FileRepository<T> : IRepository<T> where T : class
    {
        private readonly string _filePath;
        private readonly List<T> _entities;

        public FileRepository(string fileName)
        {
            var dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDirectory);
            _filePath = Path.Combine(dataDirectory, $"{fileName}.json");
            _entities = LoadEntities().Result.ToList();
        }

        private async Task<IEnumerable<T>> LoadEntities()
        {
            if (!File.Exists(_filePath))
            {
                await SaveChangesAsync();
                return new List<T>();
            }

            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
            }
            catch
            {
                return new List<T>();
            }
        }

        public IEnumerable<T> GetAll() => _entities;

        public T? GetById(string id)
        {
            var property = typeof(T).GetProperty("Id");
            if (property == null) return null;

            return _entities.FirstOrDefault(e => (string?)property.GetValue(e) == id);
        }

        public async Task AddAsync(T entity)
        {
            _entities.Add(entity);
            await SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            var property = typeof(T).GetProperty("Id");
            if (property == null) return;

            var id = (string?)property.GetValue(entity);
            if (id == null) return;

            var existing = GetById(id);
            if (existing != null)
            {
                var index = _entities.IndexOf(existing);
                _entities[index] = entity;
                await SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(string id)
        {
            var entity = GetById(id);
            if (entity != null)
            {
                _entities.Remove(entity);
                await SaveChangesAsync();
            }
        }

        public async Task SaveChangesAsync()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_entities, options);
            await File.WriteAllTextAsync(_filePath, json);
        }

        Task<IEnumerable<T>> IRepository<T>.GetAll()
        {
            throw new NotImplementedException();
        }

        Task<T?> IRepository<T>.GetById(string id)
        {
            throw new NotImplementedException();
        }
    }

}