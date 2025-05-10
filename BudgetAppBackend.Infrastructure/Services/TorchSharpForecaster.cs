using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Application.DTOs.AiAnalysisDTOS;
using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;

namespace BudgetAppBackend.Infrastructure.Services;

public class TorchSharpForecaster : IDailySpendingForecaster
{
    public async Task<DailySpendingForecastResult> ForecastAsync(float[] dailyAmounts, DateTime lastKnownDate, int daysToPredict)
    {
        const int sequenceLength = 7;
        var sequences = CreateSequences(dailyAmounts, sequenceLength);

        var device = torch.cuda.is_available() ? CUDA : CPU;
        var model = new SpendingLstmModel(1, 32, 1).to(device);

        var lossFn = nn.MSELoss();
        var optimizer = torch.optim.Adam(model.parameters(), lr: 0.001);

        for (int epoch = 1; epoch <= 50; epoch++)
        {
            foreach (var seq in sequences)
            {
                optimizer.zero_grad();
                var input = seq.Input.to(device);
                var target = seq.Target.to(device);

                var output = model.forward(input);
                var loss = lossFn.forward(output, target);
                loss.backward();
                optimizer.step();
            }
        }

        var forecast = Forecast(model, dailyAmounts, sequenceLength, daysToPredict);

        return await Task.FromResult(new DailySpendingForecastResult
        {
            Forecast = forecast.ToList(),
            StartDate = lastKnownDate.AddDays(1)
        });
    }

    private float[] Forecast(SpendingLstmModel model, float[] history, int sequenceLength, int daysToForecast)
    {
        var input = history.TakeLast(sequenceLength).ToArray();
        var forecast = new List<float>();

        for (int i = 0; i < daysToForecast; i++)
        {
            var tensor = torch.tensor(input).reshape(sequenceLength, 1, 1);
            var predicted = model.forward(tensor).to(CPU).data<float>()[0];

            forecast.Add(predicted);
            input = input.Skip(1).Append(predicted).ToArray();
        }

        return forecast.ToArray();
    }

    private List<SpendingSequence> CreateSequences(float[] data, int sequenceLength)
    {
        var sequences = new List<SpendingSequence>();

        for (int i = 0; i < data.Length - sequenceLength; i++)
        {
            var input = data.Skip(i).Take(sequenceLength).ToArray();
            var target = data[i + sequenceLength];

            sequences.Add(new SpendingSequence
            {
                Input = torch.tensor(input).reshape(sequenceLength, 1, 1),
                Target = torch.tensor(new float[] { target }).reshape(1, 1)
            });
        }

        return sequences;
    }

    public class SpendingSequence
    {
        public Tensor Input { get; set; }
        public Tensor Target { get; set; }
    }

    public class SpendingLstmModel : Module
    {
        private readonly LSTM lstm;
        private readonly Linear linear;

        public SpendingLstmModel(long inputSize, long hiddenSize, long outputSize, long numLayers = 1)
            : base("SpendingLstm")
        {
            lstm = nn.LSTM(inputSize, hiddenSize, numLayers);
            linear = nn.Linear(hiddenSize, outputSize);
            RegisterComponents();
        }

        public Tensor forward(Tensor input)
        {
            var (output, _, _) = lstm.forward(input);
            return linear.forward(output[output.shape[0] - 1]);
        }
    }
}
