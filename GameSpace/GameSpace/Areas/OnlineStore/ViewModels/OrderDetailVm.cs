namespace GameSpace.Models.ViewModels
{
	public class OrderDetailVm
	{
		// 訂單本體
		public int OrderId { get; set; }
		public string? OrderCode { get; set; }
		public int UserId { get; set; }
		public DateTime OrderDate { get; set; }

		public decimal? Subtotal { get; set; }
		public decimal? ShippingFee { get; set; }
		public decimal? Discount { get; set; }
		public decimal? GrandTotal { get; set; }

		// 地址快照（SO_OrderAddresses）
		public string? Recipient { get; set; }
		public string? Phone { get; set; }
		public string? AddressFull { get; set; }

		// 最新出貨狀態（SO_Shipments）
		public string? ShipmentStatusCode { get; set; } // READY/SHIPPED/DELIVERED/LOST/RETURNED
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

		// 明細
		public List<OrderItemRow> Items { get; set; } = new();
		public class OrderItemRow
		{
			public int LineNo { get; set; }
			public int ProductId { get; set; }
			public string? ProductName { get; set; } // 之後要 JOIN S_ProductInfo 時填入
			public decimal UnitPrice { get; set; }
			public int Quantity { get; set; }
			public decimal Subtotal => UnitPrice * Quantity;
		}
	}
}
