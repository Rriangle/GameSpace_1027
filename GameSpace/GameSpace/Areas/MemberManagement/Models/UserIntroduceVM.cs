using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MemberManagement.Models
{
		public class UserIntroduceVM
		{
			[Display(Name = "使用者編號")]
			public int UserId { get; set; }
			[Display(Name = "暱稱")]
			public string UserNickName { get; set; } = null!;
			[Display(Name = "性別")]
			public string Gender { get; set; } = null!;
			[Display(Name = "身分證字號")]
			public string IdNumber { get; set; } = null!;
			[Display(Name = "手機號碼")]
			public string Cellphone { get; set; } = null!;
			[Display(Name = "電子信箱")]
			public string Email { get; set; } = null!;
			[Display(Name = "住址")]
			public string Address { get; set; } = null!;
			[Display(Name = "生日")]
			public DateOnly DateOfBirth { get; set; }
			[Display(Name = "帳號創建時間")]
			public DateTime CreateAccount { get; set; }
			[Display(Name = "玩家照片")]
			public byte[]? UserPicture { get; set; }
			[Display(Name = "簡介")]
			public string? UserIntroduce1 { get; set; }

			public virtual User User { get; set; } = null!;
		}
	
}
