using System.Security.Claims;

namespace EduCore.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>The signed-in user's id (Teacher.ID or Student.ID), or 0 if not signed in.</summary>
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(id, out var value) ? value : 0;
        }
    }
}
