using System;

namespace Comet.Web.Areas.Profile.Models
{
    public class ProfileModel
    {
        public string CurrentPage { get; set; } // for pagination

        public int Id { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }

        public string BackgroundImage = "~/assets/images/parallax/trojan1_1920x1200.jpg";
        public string ProfileImage = "~/assets/images/avatar.png";

        public string Locality { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }

        public int CommentCount { get; set; }
        public int Reputation { get; set; }

        public string Name { get; set; }
        public string SocialName { get; set; }
        public string Pronoun { get; set; }
        public string AboutMe { get; set; }

        public bool EmailConfirmed { get; set; }
        public bool PhoneConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }

        public bool LoadedByOwner { get; set; }
        public bool IsFriend { get; set; }
    }
}
