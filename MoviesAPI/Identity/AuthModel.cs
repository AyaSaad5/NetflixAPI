﻿namespace MoviesAPI.Identity
{
    public class AuthModel
    {
        public string Message { get; set; }
        public bool IsAuthentcated { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public string Token { get; set; }
        public DateTime ExpiredOn { get; set; }





    }
}
