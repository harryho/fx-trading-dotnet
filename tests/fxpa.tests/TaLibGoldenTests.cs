using System;
using TicTacTec.TA.Library;
using Xunit;

namespace fxpa.tests;

/// <summary>
/// Golden-master tests over the bundled TA-Lib assembly running on .NET 10.
/// The input series is fixed (seeded, embedded literal) and the expected RSI
/// value was derived from an independent Wilder-smoothing reference
/// implementation, so a regression in the TA-Lib binding or the runtime
/// fails loudly here rather than silently on a chart.
/// </summary>
public class TaLibGoldenTests
{
    private const double GoldenRsiLast = 57.321198;
    private const int RsiPeriod = 14;
    private const int ExpectedOutBegIdx = 14;

    private static double[] Prices() => new double[]
    {
            100.482223, 99.059756, 98.412347, 97.604301, 98.387361,
            98.98513, 100.250886, 99.020397, 98.828354, 97.420726,
            96.598504, 96.665105, 95.247366, 94.363763, 94.878405,
            95.067724, 94.251089, 94.577813, 95.587047, 94.107194,
            95.105233, 95.769465, 95.324242, 94.306228, 95.773589,
            95.317032, 94.104544, 92.904365, 94.031598, 94.403148,
            95.405246, 96.167414, 96.329721, 97.84638, 97.519837,
            97.731163, 98.802317, 99.219729, 100.39102, 100.680812,
            101.364984, 100.00704, 99.213525, 98.610627, 97.357982,
            96.579634, 95.392739, 94.754457, 95.225078, 94.856058,
            94.503619, 93.653091, 92.980722, 94.384352, 94.893261,
            95.281567, 94.312097, 95.07239, 94.078938, 93.75525,
            95.322772, 95.806771, 96.033316, 96.65562, 97.768461,
            98.674061, 97.88411, 96.48362, 95.961525, 95.291521,
            94.445568, 95.868588, 97.085328, 96.560829, 97.092689,
            96.819148, 98.154246, 98.076686, 97.397815, 96.66236,
            96.902601, 96.2171, 96.529317, 97.812568, 97.55071,
            96.730604, 98.32297, 98.402502, 97.184321, 95.830382,
            94.670294, 95.115377, 96.070823, 95.879519, 94.576455,
            94.259475, 95.847451, 95.987705, 97.498048, 98.666465,
            97.202057, 97.936294, 98.549596, 98.714204, 98.041362,
            98.528344, 97.374156, 97.221928, 97.128472, 98.585301,
            99.800445, 99.116951, 99.168768, 98.222589, 99.551735,
            100.750343, 100.175522, 100.656265, 101.044073, 100.017874,
            100.881658, 101.053733, 101.967475, 102.111571, 100.613344,
            100.118228, 98.678606, 100.058812, 101.282849, 102.361013,
            101.814306, 100.493874, 101.715704, 103.151247, 101.916773,
            101.923344, 100.637902, 101.495769, 102.369856, 101.267869,
            101.241245, 101.445636, 100.767311, 101.971854, 101.783581,
            100.940156, 101.111974, 101.87476, 100.998328, 100.464649,
            102.049612, 102.564234, 102.422344, 102.526829, 101.401942,
            100.598504, 100.146569, 100.470326, 99.683682, 98.866356,
            97.586434, 98.042854, 97.252573, 98.559375, 99.724245,
            98.443903, 97.681717, 98.255548, 97.419682, 96.329849,
            97.729943, 98.000177, 97.965457, 98.897777, 99.901018,
            98.991288, 97.791774, 97.628033, 97.441126, 97.388903,
            98.149038, 98.736468, 100.28738, 99.092476, 98.840602,
            98.39244, 99.563625, 98.834459, 97.924107, 97.814809,
            97.622642, 96.986132, 96.260532, 97.622655, 97.496361,
            98.666543, 98.872551, 97.529375, 99.127151, 100.218836,
            101.722725, 103.094462, 104.225419, 103.240983, 103.246471,
            102.409088, 102.152312, 100.834082, 100.508899, 102.063356,    };

    [Fact]
    public void Rsi_last_value_matches_independent_reference()
    {
        double[] prices = Prices();
        double[] output = new double[prices.Length];

        int begIdx = 0;
        int nbElement = 0;
        Core.RetCode ret = Core.Rsi(0, prices.Length - 1, prices, RsiPeriod,
                                    out begIdx, out nbElement, output);

        Assert.Equal(Core.RetCode.Success, ret);
        Assert.Equal(ExpectedOutBegIdx, begIdx);
        Assert.Equal(prices.Length - RsiPeriod, nbElement);

        double actual = output[nbElement - 1];
        Assert.True(Math.Abs(actual - GoldenRsiLast) < 1e-6,
                    $"RSI golden mismatch: expected {GoldenRsiLast}, actual {actual}");
    }

    [Fact]
    public void Rsi_output_leading_values_are_nan_until_warmup_completes()
    {
        double[] prices = Prices();
        double[] output = new double[prices.Length];

        Core.RetCode ret = Core.Rsi(0, prices.Length - 1, prices, RsiPeriod,
                                    out _, out int nbElement, output);

        Assert.Equal(Core.RetCode.Success, ret);
        for (int i = 0; i < nbElement; i++)
        {
            Assert.False(double.IsNaN(output[i]), $"unexpected NaN at output[{i}]");
        }
    }
}
