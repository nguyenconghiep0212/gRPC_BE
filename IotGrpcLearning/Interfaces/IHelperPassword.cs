namespace IotGrpcLearning.Interfaces
{
    public interface IHelperPassword
    {
		(string hash, string salt) HashPassword(string password);
		bool VerifyPassword(string password, string storedHash, string storedSalt);

	}
}
