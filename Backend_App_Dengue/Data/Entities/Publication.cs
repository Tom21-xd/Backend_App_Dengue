using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("publicacion")]
    public class Publication
    {
        [Key]
        [Column("ID_PUBLICACION")]
        [JsonPropertyName("ID_PUBLICACION")]
        public int Id { get; set; }

        [Required]
        [Column("TITULO_PUBLICACION")]
        [MaxLength(200)]
        [JsonPropertyName("TITULO_PUBLICACION")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("DESCRIPCION_PUBLICACION")]
        [MaxLength(1000)]
        [JsonPropertyName("DESCRIPCION_PUBLICACION")]
        public string Description { get; set; } = string.Empty;

        [Column("FECHA_PUBLICACION")]
        [JsonPropertyName("FECHA_PUBLICACION")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("FK_ID_USUARIO")]
        [JsonPropertyName("FK_ID_USUARIO")]
        public int UserId { get; set; }

        // MongoDB GridFS Image ID
        [Column("ID_IMAGEN")]
        [MaxLength(255)]
        [JsonPropertyName("ID_IMAGEN")]
        public string? ImageId { get; set; }

        [Column("ESTADO_PUBLICACION")]
        [JsonPropertyName("ESTADO_PUBLICACION")]
        public bool IsActive { get; set; } = true;

        // New fields - Category
        [Column("FK_ID_CATEGORIA")]
        [JsonPropertyName("FK_ID_CATEGORIA")]
        public int? CategoryId { get; set; }

        // New fields - Priority
        [Column("NIVEL_PRIORIDAD")]
        [MaxLength(50)]
        [JsonPropertyName("NIVEL_PRIORIDAD")]
        public string Priority { get; set; } = "Normal"; // Baja, Normal, Alta, Urgente

        [Column("FIJADA")]
        [JsonPropertyName("FIJADA")]
        public bool IsPinned { get; set; } = false;

        [Column("FECHA_EXPIRACION")]
        [JsonPropertyName("FECHA_EXPIRACION")]
        public DateTime? ExpirationDate { get; set; }

        // New fields - Geolocation
        [Column("LATITUD")]
        [JsonPropertyName("LATITUD")]
        public decimal? Latitude { get; set; }

        [Column("LONGITUD")]
        [JsonPropertyName("LONGITUD")]
        public decimal? Longitude { get; set; }

        [Column("FK_ID_CIUDAD")]
        [JsonPropertyName("FK_ID_CIUDAD")]
        public int? CityId { get; set; }

        [Column("FK_ID_DEPARTAMENTO")]
        [JsonPropertyName("FK_ID_DEPARTAMENTO")]
        public int? DepartmentId { get; set; }

        [Column("DIRECCION")]
        [MaxLength(255)]
        [JsonPropertyName("DIRECCION")]
        public string? Address { get; set; }

        // New fields - Notifications
        [Column("ENVIAR_NOTIFICACION_PUSH")]
        [JsonPropertyName("ENVIAR_NOTIFICACION_PUSH")]
        public bool SendPushNotification { get; set; } = false;

        [Column("NOTIFICACION_ENVIADA")]
        [JsonPropertyName("NOTIFICACION_ENVIADA")]
        public bool NotificationSent { get; set; } = false;

        [Column("FECHA_ENVIO_NOTIFICACION")]
        [JsonPropertyName("FECHA_ENVIO_NOTIFICACION")]
        public DateTime? NotificationSentAt { get; set; }

        // New fields - Scheduling
        [Column("FECHA_PUBLICACION_PROGRAMADA")]
        [JsonPropertyName("FECHA_PUBLICACION_PROGRAMADA")]
        public DateTime? ScheduledPublishDate { get; set; }

        [Column("PUBLICADA")]
        [JsonPropertyName("PUBLICADA")]
        public bool IsPublished { get; set; } = true;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(CategoryId))]
        [JsonIgnore]
        public virtual PublicationCategory? Category { get; set; }

        [ForeignKey(nameof(CityId))]
        [JsonIgnore]
        public virtual City? City { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        [JsonIgnore]
        public virtual Department? Department { get; set; }

        [JsonIgnore]
        public virtual ICollection<PublicationReaction> Reactions { get; set; } = new List<PublicationReaction>();

        [JsonIgnore]
        public virtual ICollection<PublicationComment> Comments { get; set; } = new List<PublicationComment>();

        [JsonIgnore]
        public virtual ICollection<PublicationView> Views { get; set; } = new List<PublicationView>();

        [JsonIgnore]
        public virtual ICollection<PublicationTagRelation> PublicationTags { get; set; } = new List<PublicationTagRelation>();

        [JsonIgnore]
        public virtual ICollection<SavedPublication> SavedByUsers { get; set; } = new List<SavedPublication>();
    }
}
