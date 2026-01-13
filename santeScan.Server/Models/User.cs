namespace santeScan.Server.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } // Pour la sécurité

        // Informations utiles pour l'IA lors de l'analyse
        public int Age { get; set; }
        public string Gender { get; set; }
    }
}
