namespace DRLib.MathUtils;

public class Matrix
{
    public readonly double[] Data;
    public readonly int NumRow;
    public readonly int NumCol;
    public bool GroupRows = true; // determines if Data groups rows or column info together.

    public Matrix(int numRows, int numCols)
    {
        NumRow = numRows;
        NumCol = numCols;
        Data = new double[numRows * numCols];
    }

    public double[] this[int row] => Data[(row * NumCol) .. (row * NumCol + NumCol)];

    public double this[int row, int col] => Data[row * NumCol + col];

    public void ColumnSet(int colIndex, Func<double> setter)
    {
        Parallel.For(0, NumRow, r => Data[r * NumCol + colIndex] = setter());
    }

    public double ColumnAvg(int colIndex, Func<double, double> valueGetter)
    {
        return Enumerable.Range(0, NumRow)
            .AsParallel()
            .Average(r => valueGetter(Data[r * NumCol + colIndex]));
    }

    public void CrossApply(Func<(double Val, int Index), double> applyToIndex, int offset = 0)
    {
        Parallel.For(0, NumRow, r => {
            var startIndex = r * NumCol + offset;
            for (int c = startIndex; c < startIndex + NumCol - 1; c++)
                Data[c] = applyToIndex((Data[c], c));
        });
    }
}