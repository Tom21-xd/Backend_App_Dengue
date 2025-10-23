using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend_App_Dengue.Model
{
    public class CertificatePdfModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("pdf_data")]
        public byte[] PdfData { get; set; } = Array.Empty<byte>();

        [BsonElement("file_name")]
        public string FileName { get; set; } = string.Empty;

        [BsonElement("content_type")]
        public string ContentType { get; set; } = "application/pdf";

        [BsonElement("uploaded_at")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("certificate_id")]
        public int CertificateId { get; set; }
    }
}
