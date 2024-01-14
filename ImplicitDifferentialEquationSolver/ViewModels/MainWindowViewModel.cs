using ImplicitDifferentialEquationSolver.Mathematics;
using OxyPlot;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImplicitDifferentialEquationSolver.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Implicit differential equation solver";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public ImplicitIntegrator Integrator { get; set; } = new ImplicitIntegrator();



        private int pNumberOfSteps = 11;
        /// <summary>
        /// Number of time steps
        /// </summary>
        public int NumberOfSteps
        {
            get { return pNumberOfSteps; }
            set { SetProperty(ref pNumberOfSteps, value); }
        }

        private decimal pdt = 0.5M;
        /// <summary>
        /// Time step
        /// </summary>
        public decimal dt
        {
            get { return pdt; }
            set { SetProperty(ref pdt, value); }
        }

        private decimal pt = 0;
        /// <summary>
        /// Current time
        /// </summary>
        public decimal t
        {
            get { return pt; }
            set { SetProperty(ref pt, value); }
        }

        private decimal pInitialValue = 1M;
        /// <summary>
        /// Initial angle in radians pi/2 is 90 degrees
        /// </summary>
        public decimal InitialValue
        {
            get { return pInitialValue; }
            set
            {
                pInitialValue = value;
            }
        }

        private PlotModel pPlotModel = new PlotModel();
        public PlotModel PlotModel
        {
            get { return pPlotModel; }
            set { SetProperty(ref pPlotModel, value); }
        }
        private DelegateCommand pCalculateCommand;
        public DelegateCommand CalculateCommand => pCalculateCommand;

        public MainWindowViewModel()
        {
            pCalculateCommand = new DelegateCommand(ExecuteCalculateCommand, CanExecuteCalculateCommand);
            List<decimal[]> res = new List<decimal[]>();
        
            PlotArray(res.ToArray());
        }

        async void ExecuteCalculateCommand()
        {

            PlotModel = new PlotModel();

            // Initial pendulum conditions 
            decimal CurrentValue = InitialValue;

            // Create new graph for plotting
            LineSeries IntegrationSeries = new LineSeries();
            IntegrationSeries.Title = "Implicit calculation";
            IntegrationSeries.MarkerType = MarkerType.None;

            LineSeries ExactSeries = new LineSeries();
            ExactSeries.Title = "Exact solution";
            ExactSeries.MarkerType = MarkerType.None;
            // Keep the plotmodel
            PlotModel.Series.Add(IntegrationSeries);
            PlotModel.Series.Add(ExactSeries);

            IntegrationSeries.Points.Add(new DataPoint((double)0d, (double)CurrentValue));
            ExactSeries.Points.Add(new DataPoint((double)0d, (double)ExcactSolution(0M)));

            for (int i = 1; i < NumberOfSteps; i++)
            {
                // Due to floating point arethmitic multiplication is better than summation
                // Since its a small value (dt) that is added multiple times.
                // Large value + value less than epsilton is equal to the large value in floating point 
                t = dt * i;

                // Run the RK integrator in as an asyncronous process. 
                var CurrentValuesFromAnotherThread = await Task.Run(() => Integrator.CalculateStep(
                                                     // The integration step, it needs a pointer to all the derivatives at current x and t
                                                     (x, t) => ydot(x, t),
                                                     // the initial condition for integration step
                                                     CurrentValue,
                                                     // the current time
                                                     t,
                                                     // the step size in time
                                                     dt
                                                   ));
                // Store the caluclated values from local 
                CurrentValue = CurrentValuesFromAnotherThread;

                // Store the values in linesereis 
                IntegrationSeries.Points.Add(new DataPoint((double)t, (double)CurrentValue));
                ExactSeries.Points.Add(new DataPoint((double)t, (double)ExcactSolution(t)));
            }

            PlotModel.Legends.Add(new Legend()
            {
                LegendTitle = "Legend",
                LegendPosition = LegendPosition.TopRight,
                LegendBackground = OxyColor.FromRgb(255, 255, 255),
                LegendBorder = OxyColor.FromRgb(0, 0, 0),
                LegendBorderThickness = 1
            });

            PlotModel.InvalidatePlot(true);
        }

        bool CanExecuteCalculateCommand()
        {
            return true;
        }



        public void PlotArray(decimal[][] array)
        {

            PlotModel newMyModel = new PlotModel();

            newMyModel.Legends.Add(new Legend()
            {
                LegendTitle = "Legend",
                LegendPosition = LegendPosition.TopRight,
                LegendBackground = OxyColor.FromRgb(255, 255, 255),
                LegendBorder = OxyColor.FromRgb(0, 0, 0),
                LegendBorderThickness = 1
            });


            LineSeries IntegrationSeries = new LineSeries();
            IntegrationSeries.Title = "Implicit calcualtion";
            IntegrationSeries.MarkerType = MarkerType.None;
            double ind = 0;
            foreach (var item in array)
            {
                ind = (double)item[2];
                IntegrationSeries.Points.Add(new DataPoint(ind, (double)item[0]));
            }
            if (newMyModel.Series.Count == 0)
                newMyModel.Series.Add(IntegrationSeries);
            else
                newMyModel.Series[0] = IntegrationSeries;

            PlotModel = newMyModel;

        }

        /// <summary>
        /// The differential equation y' = -y^2
        /// </summary>
        /// <param name="y"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public decimal ydot(decimal y, decimal t)
        {
            return -y * y ;
        }

        /// <summary>
        /// Exact solution to y' = -y^2
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public decimal ExcactSolution(decimal t) 
        {
            return 1 / (InitialValue + t);
        }
    }
}
