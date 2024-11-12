namespace WebApplication1.Settings
{
    // Interface pour définir les paramètres MongoDB
    public interface IMongoDBSettings
    {
        string ConnectionString { get; set; }  // Chaîne de connexion MongoDB
        string DatabaseName { get; set; }      // Nom de la base de données
        string UsersCollectionName { get; set; }  // Nom de la collection des utilisateurs
    }

    // Classe qui implémente l'interface IMongoDBSettings
    public class MongoDBSettings : IMongoDBSettings
    {
        public required string ConnectionString { get; set; }  // Chaîne de connexion MongoDB
        public required string DatabaseName { get; set; }      // Nom de la base de données
        public required string UsersCollectionName { get; set; }  // Nom de la collection des utilisateurs
    }
}
