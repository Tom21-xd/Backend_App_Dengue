using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Backend_App_Dengue.Model;

namespace Backend_App_Dengue.Data
{
    public class ConexionMongo
    {
        private IMongoDatabase cnm;

        public ConexionMongo()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = config.GetConnectionString("MongoDbConnection")
                ?? throw new InvalidOperationException("MongoDB connection string not found");

            var databaseName = config.GetConnectionString("MongoDbDatabase")
                ?? throw new InvalidOperationException("MongoDB database name not found");

            var client = new MongoClient(connectionString);
            cnm = client.GetDatabase(databaseName);
        }

        public string UploadImage(ImagenModel img)
        {
            try
            {
                var imageCollection = cnm.GetCollection<ImagenModel>("Imagen");
                imageCollection.InsertOne(img);
                return img.Id;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al subir la imagen a MongoDB", ex);
            }
        }

        public ImagenModel GetImage(string id)
        {
            try
            {
                var imageCollection = cnm.GetCollection<ImagenModel>("Imagen");
                return imageCollection.Find(new BsonDocument("_id", new ObjectId(id))).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener la imagen con ID {id}", ex);
            }
        }

        public void DeleteImage(string id)
        {
            try
            {
                var imageCollection = cnm.GetCollection<ImagenModel>("Imagen");
                imageCollection.DeleteOne(new BsonDocument("_id", new ObjectId(id)));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar la imagen con ID {id}", ex);
            }
        }

        public void UpdateImage(ImagenModel img, string id)
        {
            try
            {
                var imageCollection = cnm.GetCollection<ImagenModel>("Imagen");
                var filtro = Builders<ImagenModel>.Filter.Eq("Id", id);
                var update = Builders<ImagenModel>.Update.Set("Imagen", img.Imagen);
                imageCollection.UpdateOne(filtro, update);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar la imagen con ID {id}", ex);
            }
        }
    }
}
