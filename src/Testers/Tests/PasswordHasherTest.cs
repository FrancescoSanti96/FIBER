using Template.Cryptography;

namespace Testers.Tests
{
    public class PasswordHasherTest : ITest
    {
        public string Name => "PasswordHasher";

        public string Hash(string password)
            => PasswordHasher.Hash(password);

        public void Run()
        {
            Console.Write("Inserisci la password da hashare: ");
            string password = Console.ReadLine() ?? "";

            string hash = PasswordHasher.Hash(password);
            Console.WriteLine($"Hash generato:\n{hash}");
        }
    }
}
