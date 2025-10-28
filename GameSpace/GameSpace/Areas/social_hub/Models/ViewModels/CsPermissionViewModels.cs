// /Areas/social_hub/Models/ViewModels/CsPermissionViewModels.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace GameSpace.Areas.social_hub.Models.ViewModels
{
	/// <summary>
	/// 清單用：每筆客服資料 + 權限旗標
	/// </summary>
	public sealed class CsPermListItemVM
	{
		public int AgentId { get; set; }
		public int ManagerId { get; set; }
		public string ManagerName { get; set; } = "-";
		public bool IsActive { get; set; }
		public byte MaxConcurrent { get; set; }
		public DateTime CreatedAt { get; set; }

		public bool CanAssign { get; set; }
		public bool CanTransfer { get; set; }
		public bool CanAccept { get; set; }
		public bool CanEditMuteAll { get; set; }
	}

	/// <summary>
	/// 客服人員（Agent）編輯頁 VM（含 UI 控制旗標）。
	/// 非主管一律唯讀；主管可調 IsActive / MaxConcurrent。
	/// </summary>
	public sealed class CsAgentEditVM
	{
		// 基本識別
		[HiddenInput] public int AgentId { get; set; }
		[HiddenInput] public int ManagerId { get; set; }

		// 顯示用
		[Display(Name = "管理員名稱")]
		public string? ManagerName { get; set; }

		// 可編欄位
		[Display(Name = "啟用")]
		public bool IsActive { get; set; }

		[Display(Name = "同時處理上限")]
		[Range(1, 50, ErrorMessage = "同時處理上限需介於 1~50")]
		public byte MaxConcurrent { get; set; } = 5;

		// 只讀顯示
		[Display(Name = "建立時間")] public DateTime? CreatedAt { get; set; }
		[Display(Name = "最後更新")] public DateTime? UpdatedAt { get; set; }

		// UI 控制旗標
		public bool IsReadOnly { get; set; } = true;
		public bool AllowToggleActive { get; set; } = false;
		public bool AllowEditMaxConcurrent { get; set; } = false;
	}

	/// <summary>
	/// 客服權限（Permission）編輯頁 VM（含 UI 控制旗標）。
	/// 非主管一律唯讀；主管可改所有權限（含最高權限 CanAssign）。
	/// </summary>
	public sealed class CsPermEditVM
	{
		// 基本識別
		[HiddenInput] public int AgentId { get; set; }
		[HiddenInput] public int ManagerId { get; set; }

		// 顯示用
		[Display(Name = "管理員名稱")]
		public string? ManagerName { get; set; }

		// 權限旗標
		[Display(Name = "可派單/幫轉單/結單（最高權限）")]
		public bool CanAssign { get; set; }

		[Display(Name = "可轉單")]
		public bool CanTransfer { get; set; }

		[Display(Name = "可接單")]
		public bool CanAccept { get; set; }

		[Display(Name = "穢語可編修全部（否則僅能編修自己建立）")]
		public bool CanEditMuteAll { get; set; }

		// UI 控制旗標
		public bool IsReadOnly { get; set; } = true;
		public bool EditableCanAssign { get; set; } = false;
	}

	/// <summary>
	/// 加入客服專用權限頁使用的 VM：
	/// - ManagerId：要加入的管理員（下拉）。
	/// - EligibleManagers：下拉清單來源（只列 CustomerService=1 且尚未擁有專用權限者）。
	/// - Init*：建立/補建時的初始設定（Agent 與 Permission）。
	/// </summary>
	public sealed class CsAgentAddVM
	{
		[Display(Name = "管理員ID")]
		[Required(ErrorMessage = "請選擇管理員")]
		public int? ManagerId { get; set; }

		public IEnumerable<SelectListItem>? EligibleManagers { get; set; }

		// 初始 Agent 狀態
		[Display(Name = "啟用")]
		public bool InitIsActive { get; set; } = true;

		[Display(Name = "同時處理上限")]
		[Range(1, 50, ErrorMessage = "同時處理上限需介於 1~50")]
		public byte InitMaxConcurrent { get; set; } = 5;

		// 初始 Permission 旗標
		[Display(Name = "可派單/幫轉單/結單（最高權限）")]
		public bool InitCanAssign { get; set; } = false;

		[Display(Name = "可轉單")]
		public bool InitCanTransfer { get; set; } = false;

		[Display(Name = "可接單")]
		public bool InitCanAccept { get; set; } = false;

		[Display(Name = "穢語可編修全部")]
		public bool InitCanEditMuteAll { get; set; } = false;
	}
}
