using Domain.Quota;

namespace Application.Extensions;

public static class QuotaCellExtensions
{
    public static IEnumerable<IEnumerable<QuotaCell>> BatchCells(this IEnumerable<QuotaCell> quotaCells, int batchSize)
    {
        var batch = new List<QuotaCell>(batchSize);

        foreach (var cell in quotaCells)
        {
            batch.Add(cell);

            if (batch.Count != batchSize)
            {
                continue;
            }

            yield return batch;
            batch = [];
        }
    }
}