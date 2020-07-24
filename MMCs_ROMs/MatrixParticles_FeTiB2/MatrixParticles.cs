//Mixture laws for metallic matrix composites
// From Ashby - 10.1016/0956-7151(93)90242-K
// From Shercliff and Ashby - 10.1179/mst.1994.10.6.443

using System;
using System.ComponentModel.Composition;
using Granta.HybridBase;
using Granta.Framework.DataAnnotations;
using System.ComponentModel.DataAnnotations;

#region Steps to create a hybrid model

// Make sure you have a reference to both:
// System.ComponentModel.Composition (part of .Net framework)
// and
// Granta.HybridBase (found in the Selector install folder, eg c:\program files\CES Selector 2010)
//
// Create two public classes:
// * One to hold source material values (the 'Source Material')
// * One to return calculated values (the 'Model')
//
// To deploy your model, drop the built library (dll) into the plugins folder in the CES install folder.
// CES will automatically find your model and offer it as an option in the synthesizer user interface.

#endregion

namespace UserModel
{
    #region List of all properties used in model calculations

    // You must create a public class to store source material data for your synthesis.
    // To specify how these properties map to the database, mark up data values with the [Data] attribute, 
    // which requires a name and optional unit.

    #endregion

    public class SourceData
    {
        [Data("TiB2 (titanium boride)", "%")]
        public double TiB2;

        [Data("Density", "kg/m^3")]
        public double Density;

        [Data("Young's modulus", "GPa")]
        public double YoungsModulus;

        [Data("Poisson's ratio", "")]
        public double PoissonCoeff;
    }

    #region Model name, images and description

    // In order to calculated a synthesis, you must create a public class that defines your model.
    // Mark the class with the [Export("Granta.HybridModel")] attribute
    // Add the [Model] attribute to provide a name and description.

    #endregion

    [Export("Granta.HybridModel")]
    [BindableDisplay(Name = "Fe-TiB2", Description = "Example models for metallic matrix composites.", GroupName = "Examples")]
    [Image("UserModel.model.png", "UserModel.thumbnail.png")]
    public class ExampleModel
    {
        public enum ModelType
        {
            [Display(Description = "Voigt or Upper bound")] Voigt,
            [Display(Description = "Reuss or Lower bound")] Reuss,
            [Display(Description = "Voigt-Reuss-Hill or Mean")] VoigtReussHill,
            [Display(Description = "Hashin")] Hashin,
            [Display(Description = "Halpin-Tsai")] HalpinTsai
        }

        [Option]
        [Display(Name = "Mixture laws", Description = "Pick a model")]
        [Group("Mixture laws", 1)]
        public ModelType mixtureLaws = ModelType.Hashin;

        #region Details of Source Material inputs

        // Create one or more instances of the source material.
        // Make sure the materials are marked as public.
        // Use the [Material] attribute give the source material a name.
        // Here we create two materials  - a matrix and a reinforcement.

        #endregion

        [Material]
        [Display(Name = "Matrix", Description = "Matrix material")]
        [Group("Matrix", 2)]
        public SourceData matrix;

        [Material]
        [Display(Name = "Reinforcement", Description = "Reinforcement material")]
        [Group("Reinforcement", 3)]
        public SourceData reinforcement;

        #region Details of Model Variables and Model Parameter inputs

        // Variables are of type public double, and must be marked with the 
        // [Variable] attribute, which requires a name (used in the UI) and unit.
        // 
        // You can optionally specify default values by using either the 
        // [RangeValues] or [Values] attributes.
        // 
        // You can optionally specify an upper or lower bound by using the 
        // [Bounds] attribute

        #endregion

        [Variable("%")]
        [Display(Name = "Reinforcement percentage", Description = "Specify the volume fraction")]
        [RangeValues(Start = 15, End = 25, Number = 3, Logarithmic = false)]
        [Bounds(0, 100)]
        [Group("Reinforcement percentage", 4)]
        public double percentage;

        [Variable("s")]
        [Display(Name = "Aspect Ratio", Description = "Specify the mean particles aspect ratio")]
        [RangeValues(Start = 1, End = 5, Number = 3, Logarithmic = false)]
        [Bounds(1, 1000)]
        [Group("Reinforcement description", 5)]
        public double aspectRatio;

        #region  Model Calculations

        // Create a method for each attribute you wish to calculate.
        // Each method should be public and return a double.
        //
        // Mark the method with the 
        // [CalculatedData] attribute, specifying the name and unit
        // of the data value you wish to calculate.
        // 
        // You can add as many methods as you require.

        #endregion

        [CalculatedData("TiB2 (titanium boride)", "%")]
        public double TiB2()
        {
            return percentage;
        }

        [CalculatedData("Density", "kg/m^3")]
        public double Density()
        {
            return (reinforcement.Density + matrix.Density) / 2;
        }
        [CalculatedData("Young's modulus", "GPa")]
        public double YoungsModulus()
        {
            switch (mixtureLaws)
            {
                case ModelType.Voigt:
                    return VoigtRuleOfMixture(reinforcement.YoungsModulus, matrix.YoungsModulus);
                case ModelType.Reuss:
                    return ReussRuleOfMixture(reinforcement.YoungsModulus, matrix.YoungsModulus);
                case ModelType.VoigtReussHill:
                    var a = VoigtRuleOfMixture(reinforcement.YoungsModulus, matrix.YoungsModulus);
                    var b = ReussRuleOfMixture(reinforcement.YoungsModulus, matrix.YoungsModulus);
                    return VRHRuleOfMixture(a, b);
                case ModelType.Hashin:
                    return HashinRuleOfMixture(matrix.YoungsModulus, reinforcement.YoungsModulus, matrix.PoissonCoeff, reinforcement.PoissonCoeff);
                case ModelType.HalpinTsai:
                    return HTRuleOfMixture(matrix.YoungsModulus, reinforcement.YoungsModulus);
                default:
                    return double.NaN;
            }

        }
        private double VoigtRuleOfMixture(double a, double b)
        {
            var f = percentage / 100;
            return f * a + (1.0 - f) * b;
        }
        private double ReussRuleOfMixture(double a, double b)
        {
            var f = percentage / 100;
            return 1 / (((f / a) + ((1.0 - f) / b)));
        }
        private double VRHRuleOfMixture(double a, double b)
        {
            return (a + b) / 2;
        }
        private double HashinRuleOfMixture(double a, double b, double c, double d)
        {
            var f = percentage / 100;
            var LM = (1 - c) - 2 * (Math.Pow(c, 2)); 
            var LR = (1 - d) - 2 * (Math.Pow(d, 2));

            return (b * f + a * (1.0 - f) + ((2 * (Math.Pow((d-c), 2)) * f * (1.0 - d)) / (a * (1.0 - c) * LR + (LM * (1.0 - f) + (1.0 - c) * b)))); ;
        }
        private double HTRuleOfMixture(double a, double b)
        {
            var f = percentage / 100;
            var q = ((b / a) - 1) / ((b / a) + 2 * aspectRatio);
            return a * (1 + 2 * aspectRatio * q * f) / (1 - q * f);
        }
    }
}