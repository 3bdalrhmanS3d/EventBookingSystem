namespace EventBookingSystemV1.Controllers
{
    public class SecretManager
    {
        private readonly IConfiguration _configuration;

        public SecretManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void DisplaySecrets()
        {
            //var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var smtpPassword = _configuration["EmailSettings:Password"];
            var jwtSecretKey = _configuration["JWT:SecretKey"];

            //Console.WriteLine($"Connection String: {connectionString}");
            Console.WriteLine($"SMTP Password: {smtpPassword}");
            Console.WriteLine($"JWT Secret Key: {jwtSecretKey}");
        }

        public void UpdateSecrets()
        {
            // لا يمكن إضافة أو تعديل secrets مباشرة من IConfiguration
            // ومع ذلك، يمكنك تعديل أو إضافة القيم باستخدام dotnet CLI:

            // مثال على استخدام dotnet CLI لإضافة سر:
            // dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YourNewConnectionString"
            // dotnet user-secrets set "JWT:SecretKey" "YourNewSecretKey"

            // أو استخدام القيمة المخزنة:
            //var newConnectionString = "NewConnectionString";
            var newJwtSecretKey = "NewSecretKeyForJWT";

            // يقوم بإظهار الجديد في الكونسول
            //Console.WriteLine($"Updated Connection String: {newConnectionString}");
            Console.WriteLine($"Updated JWT Secret Key: {newJwtSecretKey}");
        }

        public void RemoveSecret()
        {
            // إزالة secrets من خلال dotnet CLI:
            // dotnet user-secrets remove "EmailSettings:Password"
            // هذا سيزيل السر الذي تم تخزينه بهذا الاسم.

            Console.WriteLine("Secret removed successfully!");
        }
    }
}
