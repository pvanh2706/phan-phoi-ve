using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PpvRecon.Infrastructure.Persistence;

public sealed class PpvDateOnlyConverter()
    : ValueConverter<DateOnly, string>(
        value => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        value => DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture));
