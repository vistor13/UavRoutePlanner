using ErrorOr;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Flight;
using UavRoutePlanner.BLL.Models.Geo;
using Errors = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.BLL.Algorithm;


public class AntColonyRoutePlanner(ITimeMatrixBuilder matrixBuilder) : IRoutePlanner
{
    public ErrorOr<RoutePlan> Plan(RoutePlanRequest request, CancellationToken ct = default)
    {
        var settings = request.Settings;
        var n = request.Objects.Count;
        var finish = n + 1;

       
        var points = new List<GeoPoint> { request.Start };
        points.AddRange(request.Objects);
        points.Add(request.Finish);

        var matrix = matrixBuilder.Build(points, request.AirspeedMs, request.Wind);
        if (matrix.IsError)
            return matrix.Errors;

        var times = matrix.Value.Times;
        var legs = matrix.Value.Legs;

        
        if (times[0, finish] > request.MaxTimeS)
            return Error.Validation("TimeBudgetTooSmall", Errors.TimeBudgetTooSmall);

        var random = request.Seed is { } seed ? new Random(seed) : new Random();

      
        var etaBeta = new double[n + 2, n + 2];
        for (var i = 0; i < n + 2; i++)
        {
            for (var j = 0; j < n + 2; j++)
            {
                if (i != j && times[i, j] > 0)
                    etaBeta[i, j] = Math.Pow(1.0 / times[i, j], settings.Beta);
            }
        }

    
        var tau = new double[n + 2, n + 2];
        for (var i = 0; i < n + 2; i++)
            for (var j = 0; j < n + 2; j++)
                tau[i, j] = settings.InitialPheromone;

       
        List<int> bestRoute = [0, finish];
        var bestCount = 0;
        var bestTime = times[0, finish];

        var routes = new List<int>[settings.AntsCount];
        var counts = new int[settings.AntsCount];
        var routeTimes = new double[settings.AntsCount];

        var visited = new bool[n + 2];
        var candidates = new List<int>(n);
        var weights = new double[n + 2];

        for (var iteration = 0; iteration < settings.IterationsCount; iteration++)
        {
            ct.ThrowIfCancellationRequested();

            
            for (var k = 0; k < settings.AntsCount; k++)
            {
                Array.Clear(visited);
                var route = new List<int>(n + 2) { 0 };
                var current = 0;
                var elapsed = 0.0;

                while (true)
                {
                    
                    candidates.Clear();
                    for (var j = 1; j <= n; j++)
                    {
                        if (!visited[j] && elapsed + times[current, j] + times[j, finish] <= request.MaxTimeS)
                            candidates.Add(j);
                    }

                    if (candidates.Count == 0) break;

                    var next = ChooseNext(current, candidates, tau, etaBeta, settings.Alpha, weights, random);

                    route.Add(next);
                    visited[next] = true;
                    elapsed += times[current, next];
                    current = next;
                }

                elapsed += times[current, finish];
                route.Add(finish);

                routes[k] = route;
                counts[k] = route.Count - 2;
                routeTimes[k] = elapsed;
            }

           
            var iterBest = 0;
            for (var k = 1; k < settings.AntsCount; k++)
            {
                if (counts[k] > counts[iterBest]
                    || (counts[k] == counts[iterBest] && routeTimes[k] < routeTimes[iterBest]))
                    iterBest = k;
            }

            if (counts[iterBest] > bestCount
                || (counts[iterBest] == bestCount && routeTimes[iterBest] < bestTime))
            {
                bestRoute = routes[iterBest];
                bestCount = counts[iterBest];
                bestTime = routeTimes[iterBest];
            }

           
            UpdatePheromone(tau, routes, counts, settings);
        }

     
        var plannedLegs = new List<PlannedLeg>();
        for (var m = 0; m < bestRoute.Count - 1; m++)
        {
            var leg = legs[bestRoute[m], bestRoute[m + 1]];
            if (leg is not null)
                plannedLegs.Add(new PlannedLeg(bestRoute[m], bestRoute[m + 1], leg));
        }

        return new RoutePlan(
            bestRoute.Skip(1).Take(bestCount).ToList(),
            plannedLegs,
            bestTime);
    }

   
    private static int ChooseNext(
        int current, List<int> candidates, double[,] tau, double[,] etaBeta, double alpha,
        double[] weights, Random random)
    {
        var sum = 0.0;
        foreach (var j in candidates)
        {
            var w = (alpha == 1.0 ? tau[current, j] : Math.Pow(tau[current, j], alpha)) * etaBeta[current, j];
            weights[j] = w;
            sum += w;
        }

        
        if (!(sum > 0) || double.IsInfinity(sum))
            return candidates[random.Next(candidates.Count)];

        var threshold = random.NextDouble() * sum;
        var cumulative = 0.0;
        foreach (var j in candidates)
        {
            cumulative += weights[j];
            if (threshold <= cumulative) return j;
        }

        return candidates[^1];
    }

  
    private static void UpdatePheromone(
        double[,] tau, List<int>[] routes, int[] counts, AntColonySettings settings)
    {
        var size = tau.GetLength(0);
        var keep = 1.0 - settings.EvaporationRate;

        for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
                tau[i, j] *= keep;

        for (var k = 0; k < routes.Length; k++)
        {
            var deposit = settings.PheromoneDeposit * counts[k];
            var route = routes[k];

            for (var m = 0; m < route.Count - 1; m++)
                tau[route[m], route[m + 1]] += deposit;
        }
    }
}
