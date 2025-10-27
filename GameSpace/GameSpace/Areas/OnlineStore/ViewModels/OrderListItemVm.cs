namespace GameSpace.Models.ViewModels
{
	public class OrderListItemVm
	{
		public int OrderId { get; set; }
		public string? OrderCode { get; set; }
		public int UserId { get; set; }
		public DateTime OrderDate { get; set; }
		public decimal? GrandTotal { get; set; }

		// 來自 SO_Shipments
		public string? ShipmentStatusCode { get; set; }  // READY/SHIPPED/DELIVERED/LOST/RETURNED

		public string ShipmentStatusText => ShipmentStatusCode switch
		{
			"READY" => "待出貨",
			"SHIPPED" => "已出貨",
			"DELIVERED" => "已送達",
			"LOST" => "遺失",
			"RETURNED" => "退回",
			_ => "—"
		};

		public string ShipmentBadgeClass => ShipmentStatusCode switch
		{
			"READY" => "bg-secondary",
			"SHIPPED" => "bg-info",
			"DELIVERED" => "bg-success",
			"LOST" => "bg-danger",
			"RETURNED" => "bg-warning text-dark",
			_ => "bg-light text-dark"
		};
	}
}
