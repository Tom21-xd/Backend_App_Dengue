using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend_App_Dengue.Services
{
    public class FCMService
    {
        private static bool _isInitialized = false;

        public FCMService()
        {
            InitializeFirebase();
        }

        private void InitializeFirebase()
        {
            if (!_isInitialized)
            {
                try
                {
                    // Inicializar Firebase Admin SDK con el archivo de credenciales
                    // El archivo debe estar en la raíz del proyecto
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile("firebase-adminsdk.json")
                    });
                    _isInitialized = true;
                    Console.WriteLine("Firebase Admin SDK inicializado correctamente");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al inicializar Firebase: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Enviar notificación push a un solo dispositivo
        /// </summary>
        public async Task<string> SendNotificationToDevice(string fcmToken, string title, string body, Dictionary<string, string> data = null)
        {
            try
            {
                var message = new Message()
                {
                    Token = fcmToken,
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data,
                    Android = new AndroidConfig()
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification()
                        {
                            ChannelId = "dengue_notifications",
                            Sound = "default"
                        }
                    }
                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                Console.WriteLine($"Notificación enviada exitosamente: {response}");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar notificación: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Enviar notificación push a múltiples dispositivos
        /// </summary>
        public async Task<BatchResponse> SendNotificationToMultipleDevices(List<string> fcmTokens, string title, string body, Dictionary<string, string> data = null)
        {
            try
            {
                var message = new MulticastMessage()
                {
                    Tokens = fcmTokens,
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data,
                    Android = new AndroidConfig()
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification()
                        {
                            ChannelId = "dengue_notifications",
                            Sound = "default"
                        }
                    }
                };

                BatchResponse response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
                Console.WriteLine($"{response.SuccessCount} notificaciones enviadas exitosamente de {fcmTokens.Count}");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar notificaciones múltiples: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Enviar notificación a un topic (grupo de usuarios suscritos)
        /// </summary>
        public async Task<string> SendNotificationToTopic(string topic, string title, string body, Dictionary<string, string> data = null)
        {
            try
            {
                var message = new Message()
                {
                    Topic = topic,
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data,
                    Android = new AndroidConfig()
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification()
                        {
                            ChannelId = "dengue_notifications",
                            Sound = "default"
                        }
                    }
                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                Console.WriteLine($"Notificación enviada al topic '{topic}': {response}");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar notificación al topic: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Suscribir un dispositivo a un topic
        /// </summary>
        public async Task<TopicManagementResponse> SubscribeToTopic(List<string> fcmTokens, string topic)
        {
            try
            {
                var response = await FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(fcmTokens, topic);
                Console.WriteLine($"{response.SuccessCount} dispositivos suscritos al topic '{topic}'");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al suscribir al topic: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Desuscribir un dispositivo de un topic
        /// </summary>
        public async Task<TopicManagementResponse> UnsubscribeFromTopic(List<string> fcmTokens, string topic)
        {
            try
            {
                var response = await FirebaseMessaging.DefaultInstance.UnsubscribeFromTopicAsync(fcmTokens, topic);
                Console.WriteLine($"{response.SuccessCount} dispositivos desuscritos del topic '{topic}'");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al desuscribir del topic: {ex.Message}");
                throw;
            }
        }
    }
}
