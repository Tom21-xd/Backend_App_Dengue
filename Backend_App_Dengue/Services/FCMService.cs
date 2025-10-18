using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend_App_Dengue.Services
{
    public class FCMService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FCMService> _logger;
        private static bool _isInitialized = false;
        private static readonly object _lock = new object();

        public FCMService(IConfiguration configuration, ILogger<FCMService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            InitializeFirebase();
        }

        private void InitializeFirebase()
        {
            if (_isInitialized) return;

            lock (_lock)
            {
                if (_isInitialized) return;

                try
                {
                    var credentialsPath = _configuration["Firebase:CredentialsPath"];

                    if (string.IsNullOrEmpty(credentialsPath))
                    {
                        _logger.LogError("Firebase credentials path not configured in appsettings.json");
                        throw new InvalidOperationException("Firebase:CredentialsPath no está configurado");
                    }

                    if (!File.Exists(credentialsPath))
                    {
                        _logger.LogError("Firebase credentials file not found at: {Path}", credentialsPath);
                        throw new FileNotFoundException($"Archivo de credenciales no encontrado: {credentialsPath}");
                    }

                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(credentialsPath)
                    });

                    _isInitialized = true;
                    _logger.LogInformation("Firebase Admin SDK initialized successfully from {Path}", credentialsPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize Firebase Admin SDK");
                    throw;
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
                _logger.LogInformation("Notification sent successfully to device. Response: {Response}", response);
                return response;
            }
            catch (FirebaseMessagingException fex)
            {
                _logger.LogError(fex, "Firebase messaging error sending notification to token: {Token}. Error code: {ErrorCode}",
                    fcmToken, fex.MessagingErrorCode);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to device");
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
                _logger.LogInformation("{SuccessCount} of {TotalCount} notifications sent successfully",
                    response.SuccessCount, fcmTokens.Count);

                if (response.FailureCount > 0)
                {
                    _logger.LogWarning("{FailureCount} notifications failed to send", response.FailureCount);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending multiple notifications");
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
                _logger.LogInformation("Notification sent to topic '{Topic}' successfully. Response: {Response}",
                    topic, response);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to topic: {Topic}", topic);
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
                _logger.LogInformation("{SuccessCount} devices subscribed to topic '{Topic}'",
                    response.SuccessCount, topic);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to topic: {Topic}", topic);
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
                _logger.LogInformation("{SuccessCount} devices unsubscribed from topic '{Topic}'",
                    response.SuccessCount, topic);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing from topic: {Topic}", topic);
                throw;
            }
        }
    }
}
