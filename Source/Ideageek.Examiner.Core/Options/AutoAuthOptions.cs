namespace Ideageek.Examiner.Core.Options;

public class AutoAuthOptions
{
    public string Username { get; set; } = "superadmin@examiner.com";
    public string Password { get; set; } = "SuperAdmin@123";
    public string CookieName { get; set; } = "ExaminerAuth";
    public bool SetCookie { get; set; } = true;
}
