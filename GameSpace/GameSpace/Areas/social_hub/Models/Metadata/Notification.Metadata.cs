using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;

namespace GameSpace.Models;

[ModelMetadataType(typeof(NotificationMetadata))]
public partial class Notification { }

public class NotificationMetadata
{
	[ValidateNever] public NotificationAction Action { get; set; } = null!;
	[ValidateNever] public NotificationSource Source { get; set; } = null!;
	[ValidateNever] public User? Sender { get; set; }
	[ValidateNever] public ManagerDatum? SenderManager { get; set; }
	[ValidateNever] public Group? Group { get; set; }
	[ValidateNever] public ICollection<NotificationRecipient> NotificationRecipients { get; set; } = default!;
}
