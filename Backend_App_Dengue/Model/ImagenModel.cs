using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Backend_App_Dengue.Model
{
    public class ImagenModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Imagen { get; set; }
        public string Name { get; set; }
    }
}
