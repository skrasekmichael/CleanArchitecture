﻿using TeamUp.Common.Abstractions;

namespace TeamUp.Infrastructure.Core;

internal sealed class DateTimeProvider : IDateTimeProvider
{
	public DateTime UtcNow => DateTime.UtcNow;

	public DateTimeOffset DateTimeOffsetUtcNow => DateTimeOffset.UtcNow;
}
