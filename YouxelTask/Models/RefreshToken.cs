namespace YouxelTask.FileStorage.Api.Models
{
	public class RefreshToken
	{
		public string Token { get; set; }
		public string Username { get; set; }
		public DateTime ExpiryDate { get; set; }
	}
}
