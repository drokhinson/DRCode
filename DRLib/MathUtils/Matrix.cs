using Index = System.Index;

namespace DRLib.MathUtils;

public class Matrix<T>(int numRows, int numCols)
{
    public readonly T[] Data = new T[numRows * numCols];
    public readonly int NumRow = numRows;
    public readonly int NumCol = numCols;

    public T[] this[int row] => Data[(row * NumCol) .. (row * NumCol + NumCol)];

    public T this[int row, int col] => Data[row * NumCol + col];

    public void ColumnSet(Index index, Func<T> setter)
    {
        var colIndex = GetColIndex(index);

        Parallel.For(0, NumRow, r => Data[r * NumCol + colIndex] = setter());
    }

    public T[] ColumnGet(Index index)
    {
        var colIndex = GetColIndex(index);

        return Enumerable.Range(0, NumRow)
            .AsParallel()
            .Select(r => Data[r * NumCol + colIndex])
            .ToArray();
    }

    public void CrossApply(Func<(T Val, int Index), T> applyToIndex, int offset = 0)
    {
        Parallel.For(0, NumRow, r => {
            var startIndex = r * NumCol + offset;
            for (int c = startIndex; c < startIndex + NumCol; c++)
                Data[c] = applyToIndex((Data[c], c));
        });
    }

    protected int GetColIndex(Index index) => index.IsFromEnd ? NumCol - index.Value : index.Value;
}

public class DoubleMatrix(int numRows, int numCols) : Matrix<double>(numRows, numCols)
{
    public double ColumnAvg(Index index, Func<double, double> valueGetter)
    {
        var colIndex = GetColIndex(index);

        return Enumerable.Range(0, NumRow)
            .AsParallel()
            .Average(r => valueGetter(Data[r * NumCol + colIndex]));
    }
}