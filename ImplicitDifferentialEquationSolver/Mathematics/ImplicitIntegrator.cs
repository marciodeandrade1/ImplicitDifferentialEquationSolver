using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImplicitDifferentialEquationSolver.Mathematics
{
    public class ImplicitIntegrator
    {
        public decimal CalculateStep(Func<decimal, decimal, decimal> df, decimal x0, decimal t, decimal dt)
        {
            // g(x) for Implicit Euler calculations
            decimal g(decimal x) { return x - x0 - dt * df(x, t + dt); };

            //  g'(x) approximated with the secant method
            decimal gdot(decimal y1, decimal y0) { return 1 - dt * ((df(y1, t + dt) - df(y0, t + dt)) / (y1 - y0)); };

            // Starting point for the error
            decimal error = 1.0M;
            // Error tolerance
            decimal tol = 1e-10M;
            // Max number of iterations
            int maxIterations = 10;
            int iteration = 0;

            // Starting point
            decimal y0 = x0;

            // First estimate using forward Euler
            decimal y1 = x0 + dt * df(x0, t + dt);

            // Newton-Raphson iterations
            while (error > tol && iteration < maxIterations)
            {
                decimal y2 = y1 - (decimal)(g(y1) / gdot(y1, y0));
                error = (decimal)Math.Abs((double)(y2 - y1));
                y0 = y1;
                y1 = y2;
                iteration++;
            }

            // Return estimate within range or from max number of iterations
            return y1;
        }


    }
}
